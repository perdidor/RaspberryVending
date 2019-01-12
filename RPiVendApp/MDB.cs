using System;
using Windows.UI;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using System.Globalization;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Text;

namespace RPiVendApp
{
    public class MDB
    {
        public delegate void MDBStartedDelegate();
        public delegate void MDBChangerPayOutBusydDelegate();
        public delegate void MDBBAResetedDelegate();
        public delegate void MDBCCResetedDelegate();
        public delegate void MDBInsertedBillDelegate(double BillValue);
        public delegate void MDBInsertedCoinRoutedToCashBoxDelegate(double CoinValue);
        public delegate void MDBInsertedCoinRoutedToCCTubeDelegate(double CoinValue);
        public delegate void MDBErrorDelegate(string ErrorMessage);
        public delegate void MDBDataProcessingErrorDelegate(string DataProcessingErrorMessage);
        public delegate void MDBChangeDispensedDelegate(int DispensedSum);
        public delegate void MDBCCTubesStatusDelegate(int RUR1, int RUR2, int RUR5, int RUR10);
        public delegate void MDBInformationMessageReceivedDelegate(string MDBInformationMessage);

        public static MDB MDBInstance = null;
        public static SerialDevice MDBSerialPort = null;
        private static List<byte[]> CommandList = new List<byte[]> { };
        public static int MDBInitStep = 0;
        private static bool baalreadydisabled = false;
        private static bool CheckBAStatus = false;
        private static bool CheckCCTubeStatus = false;
        private static DataReader MDBSerialDataReaderObject = null;
        public static bool DispenseInProgress = false;
        public static bool CheckDispenseResult = false;
        public static DateTime Dispensetimeout = new DateTime();

        public static event MDBStartedDelegate MDBStarted;
        public static event MDBInsertedBillDelegate MDBInsertedBill;
        public static event MDBErrorDelegate MDBError;
        public static event MDBDataProcessingErrorDelegate MDBDataProcessingError;
        public static event MDBInsertedCoinRoutedToCashBoxDelegate MDBInsertedCoinRoutedToCashBox;
        public static event MDBInsertedCoinRoutedToCCTubeDelegate MDBInsertedCoinRoutedToCCTube;
        public static event MDBChangeDispensedDelegate MDBChangeDispensed;
        public static event MDBCCTubesStatusDelegate MDBCCTubesStatus;
        public static event MDBInformationMessageReceivedDelegate MDBInformationMessageReceived;
        public static event MDBBAResetedDelegate MDBBAReseted;
        public static event MDBCCResetedDelegate MDBCCReseted;
        public static event MDBChangerPayOutBusydDelegate MDBCCPayOutBusy;

        private static SemaphoreSlim MDBCommandsListSemaphore = new SemaphoreSlim(0, 1);


        /// <summary>
        /// Инициализирует последовательный порт шины MDB
        /// </summary>
        /// <param name="cashdeskseroalportid">Исключить конкретный порт из поиска</param>
        public static async void ConnectMDBSerialPort(string cashdeskseroalportid)
        {
            try
            {
                MDBCommandsListSemaphore.Release();
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.UART0LED, Colors.Yellow);
                StartPage.UpdateProgress(5);
                StartPage.AddItemToLogBox("Поиск последовательного порта шины MDB...");
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                foreach (var item in dis)
                {
                    if (item.Id != cashdeskseroalportid)
                    {
                        MDBSerialPort = await SerialDevice.FromIdAsync(item.Id);
                    }
                }
                if (MDBSerialPort == null) return;
                MDBSerialPort.WriteTimeout = TimeSpan.FromMilliseconds(1000);
                MDBSerialPort.ReadTimeout = TimeSpan.FromMilliseconds(1000);
                MDBSerialPort.BaudRate = 9600;
                MDBSerialPort.Parity = SerialParity.None;
                MDBSerialPort.StopBits = SerialStopBitCount.One;
                MDBSerialPort.DataBits = 8;
                string a = "Порт шины MDB успешно настроен: ";
                a += MDBSerialPort.BaudRate + "-";
                a += MDBSerialPort.DataBits + "-";
                a += MDBSerialPort.Parity.ToString() + "-";
                a += MDBSerialPort.StopBits;
                StartPage.AddItemToLogBox(a);
                ListenMDBSerialPort();
            }
            catch (Exception ex)
            {
                StartPage.AddItemToLogBox(ex.Message);
            }
        }

        /// <summary>
        /// Считывает данные с ппоследовательного порта шины MDB
        /// </summary>
        private static async void ListenMDBSerialPort()
        {
            StartPage.UpdateProgress(5);
            while (true)
            {
                try
                {
                    while (MDBSerialPort == null)
                    {
                        await Task.Delay(100);
                    }
                    StartPage.UpdateStartLEDs(StartPage.StartPageInstance.UART0LED, Colors.Green);
                    StartPage.AddItemToLogBox("Порт MDB открыт, ожидание данных...");
                    MDBSerialDataReaderObject = new DataReader(MDBSerialPort.InputStream)
                    {
                        InputStreamOptions = InputStreamOptions.Partial
                    };
                    List<byte> tmpres = new List<byte> { };
                    while (true)
                    {
                        Task<UInt32> loadAsyncTask = MDBSerialDataReaderObject.LoadAsync(64).AsTask(StartPage.GlobalCancellationTokenSource.Token);
                        UInt32 bytesRead = 0;
                        bytesRead = await loadAsyncTask;
                        if (bytesRead > 0)
                        {
                            byte[] tmpbyte = new byte[bytesRead];
                            MDBSerialDataReaderObject.ReadBytes(tmpbyte);
                            for (int i = 0; i < tmpbyte.Length; i++)
                            {
                                if ((tmpbyte[i] == '\n')/* || (tmpbyte[tmpbyte.Length - 1] == 10)*/)
                                {
                                    byte[] b = tmpres.ToArray();
                                    //Task.Factory.StartNew(() => { ProcessIncomingMDB(b); });
                                    //ProcessIncomingMDB(b);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    Task.Run(() =>
                                    {
                                        Mutex mut = new Mutex(true, "DataProcessMutex", out bool created);
                                        if (!created)
                                        {
                                            try
                                            {
                                                mut.WaitOne();
                                            }
                                            catch
                                            {

                                            }
                                        }
                                        string tmpstr = Encoding.UTF8.GetString(b).Trim();
                                        if (tmpstr == "00 00 00") return;
                                        tmpstr = tmpstr.Replace("\r", "");
                                        CurrenzaC2Green_ICTA7V7_DataProcess(tmpstr);
                                        mut.ReleaseMutex();
                                        mut.Dispose();
                                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    tmpres = new List<byte> { };
                                }
                                else
                                {
                                    tmpres.Add(tmpbyte[i]);
                                }
                            }
                            //CollectMDBData(tmpbyte);
                            //Task.Factory.StartNew(() => { CollectMDBData(tmpbyte); });
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    CloseMDBSerialDevice();
                }
                catch (Exception ex)
                {
                    StartPage.AddItemToLogBox(ex.Message);
                }
                finally
                {
                    if (MDBSerialDataReaderObject != null)
                    {
                        MDBSerialDataReaderObject.DetachStream();
                        MDBSerialDataReaderObject = null;
                    }
                }
            }
        }

        /// <summary>
        /// Закрывает последовательный порт шины MDB
        /// </summary>
        private static void CloseMDBSerialDevice()
        {
            if (MDBSerialPort != null)
            {
                MDBSerialPort.Dispose();
            }
            MDBSerialPort = null;
        }


        /// <summary>
        /// Запускается в начале выдачи сдачи монетами (при первом сообщении "Changer Payout Busy").
        /// </summary>
        /// <returns></returns>
        private static void CurrenzaC2Green_DispenseWatch()
        {
            while (Dispensetimeout > DateTime.Now)
            {
                Task.Delay(1000).Wait();
            }
            DispenseInProgress = false;
            if (StartPage.UserDeposit == 0) CheckDispenseResult = true;
        }

        public static Task DispensedCoinsInfoTask()
        {
            Task.Delay(1000).Wait();
            StartPage.AddItemToLogBox("Старт отслеживания выданной сдачи...");
            while (true)
            {
                try
                {
                    if (CheckDispenseResult)
                    {
                        GetDispensedInfo();
                        StartPage.AddItemToLogBox("Ожидаем данные о выданной сдаче...");
                        int RetryCount = 0;
                        while (CheckDispenseResult && RetryCount < 51)//ждать ответа от монетоприемника будем 5 секунд, потом забъем хуй и продолжим работать
                        {
                            Task.Delay(100).Wait();//пауза на 0.1сек
                            RetryCount++;
                        }
                        CheckDispenseResult = false;
                    }
                }
                catch (Exception ex)
                {
                    StartPage.AddItemToLogBox(ex.Message);
                }
                Task.Delay(1000).Wait();
            }
        }

        /// <summary>
        /// Defines command set to control MDB cash devices
        /// </summary>
        public static MDBCommands CommandSet = MDBCommandSets.CurrenzaC2Blue_ICTA7V7;

        /// <summary>
        /// Currenza C2 Green coin changer and ICT A7/V7 bill validator data processing method.
        /// Must fire all MDB events, all events must be handled externally as well.
        /// </summary>
        /// <param name="dt">String representation if incoming MDB serial data</param>
        /// <returns>none</returns>
        public static void CurrenzaC2Green_ICTA7V7_DataProcess(string dt)
        {
            string tmpmdbmsg = "";
            if (dt.Length >= 500)
            {
                MDBInformationMessageReceived?.Invoke("Слишком большой пакет (>512 байт)");
                return;
            }
            string ResponseData = "";
            try
            {
                ResponseData = dt;
                if ((ResponseData == "00") || (ResponseData == "FF"))
                {
                    //m2.ReleaseMutex();
                    return;
                }
                if (ResponseData == "MDB-UART PLC ready") //MDB-UART adapter PLC started
                {
                    MDBStarted?.Invoke();
                    //m2.ReleaseMutex();
                    return;
                }
            }
            catch
            {
                MDBDataProcessingError?.Invoke("Ошибка при обработке данных MDB");
                //m2.ReleaseMutex();
                return;
            }
            int mult = 0;
            double CoinValue = 0;
            if (ResponseData.StartsWith("30"))
            {
                try
                {
                    string data = ResponseData.Substring(3);
                    if (data == "81 09")
                    {
                        MDBInsertedBill?.Invoke(50.00);
                    }
                    if (data == "82 09")
                    {
                        MDBInsertedBill?.Invoke(100.00);
                    }
                    if ((data == "01") || (data == "01 09"))
                    {
                        MDBError?.Invoke("Ошибка двигателя купюроприемника");
                    }
                    if ((data == "02") || (data == "02 09"))
                    {
                        MDBError?.Invoke("Ошибка датчика купюроприемника");
                    }
                    if ((data == "03") || (data == "03 09"))
                    {
                        MDBError?.Invoke("Валидатор занят обработкой данных");
                    }
                    if ((data == "04") || (data == "04 09"))
                    {
                        MDBError?.Invoke("Ошибка микропрограммы купюроприемника");
                    }
                    if ((data == "05") || (data == "05 09"))
                    {
                        MDBError?.Invoke("Замятие в купюроприемнике");
                    }
                    if ((data == "06") || (data == "06 09"))
                    {
                        MDBBAReseted?.Invoke();
                    }
                    if ((data == "07") || (data == "07 09"))
                    {
                        tmpmdbmsg = "BA Bill Removed";
                    }
                    if ((data == "08") || (data == "08 09"))
                    {
                        tmpmdbmsg = "BA Cash Box Out of Position";
                    }
                    if ((data == "09"))
                    {
                        tmpmdbmsg = "BA Unit Disabled";
                    }
                    if ((data == "0A") || (data == "0A 09"))
                    {
                        tmpmdbmsg = "BA Invalid Escrow Request";
                    }
                    if ((data == "0B") || (data == "0B 09"))
                    {
                        tmpmdbmsg = "BA Bill Rejected";
                    }
                    if ((data == "14") || (data == "14 09"))
                    {
                        tmpmdbmsg = "BA Bill Not Recognized";
                    }
                }
                catch (Exception ex)
                {
                    MDBDataProcessingError?.Invoke(ex.Message);
                }
                finally
                {
                    if (((tmpmdbmsg == "BA Unit Disabled") && !(baalreadydisabled)) || (tmpmdbmsg != "BA Unit Disabled"))
                    {
                        MDBInformationMessageReceived?.Invoke(tmpmdbmsg);
                        if (tmpmdbmsg == "BA Unit Disabled") baalreadydisabled = true; else baalreadydisabled = false;
                        //m2.ReleaseMutex();
                    }
                }
                return;
            }
            if (ResponseData.StartsWith("08"))
            {
                try
                {
                    if (((int.Parse(ResponseData.Substring(3, 2), NumberStyles.HexNumber)) > 13) && !((int.Parse(ResponseData.Substring(3, 2), NumberStyles.HexNumber)) == 33))
                    {
                        int i = int.Parse(ResponseData.Substring(3).Substring(0, 2), NumberStyles.HexNumber);
                        string FirstByte = Convert.ToString(i, 2).PadLeft(8,'0');
                        if (FirstByte.Substring(4, 4) == "0000") CoinValue = 0.5;
                        if (FirstByte.Substring(4, 4) == "0010") CoinValue = 1;
                        if (FirstByte.Substring(4, 4) == "0011") CoinValue = 2;
                        if (FirstByte.Substring(4, 4) == "0100") CoinValue = 5;
                        if (FirstByte.Substring(4, 4) == "0110") CoinValue = 10;
                        double dep = 0.00;
                        if (FirstByte.Substring(2, 2) == "00")
                        {
                            //"Кэшбокс";
                            mult = 1;
                            dep = (CoinValue * mult);
                            MDBInsertedCoinRoutedToCashBox?.Invoke(dep);
                            //m2.ReleaseMutex();
                            return;
                        }
                        if (FirstByte.Substring(2, 2) == "01")
                        {
                            //"Трубка монетоприемника";
                            mult = 1;
                            dep = (CoinValue * mult);
                            MDBInsertedCoinRoutedToCCTube?.Invoke(dep);
                            //m2.ReleaseMutex();
                            return;
                        }
                        if (FirstByte.Substring(2, 2) == "10")
                        {
                            //"Неизвестно";
                            mult = 0;
                        }
                        if (FirstByte.Substring(2, 2) == "11")
                        {
                            //"Возврат";
                            mult = 0;
                        }
                    }
                    else
                    {
                        string data = ResponseData.Substring(3, 2);
                        if ((data == "01"))
                        {
                            tmpmdbmsg = "CC Escrow Request";
                        }
                        if ((data == "02"))
                        {
                            tmpmdbmsg = "CC Changer Payout Busy";
                            Dispensetimeout = DateTime.Now.AddSeconds(3);
                            MDBCCPayOutBusy?.Invoke();
                            if (!DispenseInProgress)
                            {
                                CurrenzaC2Green_DispenseWatch();
                            }
                            //m2.ReleaseMutex();
                            return;
                        }
                        if ((data == "03"))
                        {
                            tmpmdbmsg = "CC No Credit";
                        }
                        if ((data == "04"))
                        {
                            MDBError?.Invoke("Ошибка датчика в трубе монетоприемника");
                        }
                        if ((data == "05"))
                        {
                            tmpmdbmsg = "CC Double Arrival";
                        }
                        if ((data == "06"))
                        {
                            MDBError?.Invoke("Монетоприемник открыт");
                        }
                        if ((data == "07"))
                        {
                            MDBError?.Invoke("Застревание монеты в трубе монетоприемника при выдаче сдачи");
                        }
                        if ((data == "08"))
                        {
                            MDBError?.Invoke("Ошибка микропрограммы монетоприемника");
                        }
                        if ((data == "09"))
                        {
                            MDBError?.Invoke("Ошибка направляющих механизмов монетоприемника");
                        }
                        if ((data == "0A"))
                        {
                            tmpmdbmsg = "CC Changer Busy";
                        }
                        if ((data == "0B"))
                        {
                            MDBCCReseted?.Invoke();
                        }
                        if ((data == "0C"))
                        {
                            MDBError?.Invoke("Застревание монеты");
                        }
                        if ((data == "21"))
                        {
                            tmpmdbmsg = "Монета не распознана, возвращена";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MDBDataProcessingError?.Invoke(ex.Message);
                }
                finally
                {
                    MDBInformationMessageReceived?.Invoke(tmpmdbmsg);
                    baalreadydisabled = false;
                }
                //m2.ReleaseMutex();
                return;
            }
            if ((CheckDispenseResult) && (ResponseData.Length == 47))
            {
                string[] bytes = ResponseData.Split(' ');
                int i1 = int.Parse(bytes[2], NumberStyles.HexNumber);
                int i2 = int.Parse(bytes[3], NumberStyles.HexNumber);
                int i5 = int.Parse(bytes[4], NumberStyles.HexNumber);
                int i10 = int.Parse(bytes[6], NumberStyles.HexNumber);
                int TotalCoins = int.Parse(bytes[bytes.Length - 1], NumberStyles.HexNumber);
                int TotalDispensed = i1 + i2 * 2 + i5 * 5 + i10 * 10;
                CheckDispenseResult = false;
                MDBChangeDispensed?.Invoke(TotalDispensed);
                //m2.ReleaseMutex();
                return;
            }
            if ((CheckCCTubeStatus) && (ResponseData.Length == 56))
            {
                CheckCCTubeStatus = false;
                string[] bytes = ResponseData.Substring(7).Split(' ');
                int i1 = int.Parse(bytes[2], NumberStyles.HexNumber);
                int i2 = int.Parse(bytes[3], NumberStyles.HexNumber);
                int i5 = int.Parse(bytes[4], NumberStyles.HexNumber);
                int i10 = int.Parse(bytes[6], NumberStyles.HexNumber);
                int TotalCoinsInTubesValue = i1 + i2 * 2 + i5 * 5 + i10 * 10;
                MDBCCTubesStatus?.Invoke(i1, i2, i5, i10);
                //m2.ReleaseMutex();
                return;
            }
            if ((CheckBAStatus) && (ResponseData.Length == 8))
            {
                int i = int.Parse(ResponseData.Substring(1, 1) + ResponseData.Substring(3, 2), NumberStyles.HexNumber);
                string AnswerBytes = Convert.ToString(i, 2).PadLeft(16, '0');
                bool isstackerfull = (AnswerBytes.StartsWith("1"));
                int stackerbillscount = Convert.ToInt32(AnswerBytes.Substring(1), 2);
                //Here must be invoke to MDBBAStatus, but we will handle bills count manually on server side
                CheckBAStatus = false;
                //m2.ReleaseMutex();
                return;
            }
        }

        /// <summary>
        /// Возвращаем купюру из Escrow
        /// </summary>
        public static void ReturnBill()
        {
            AddCommand(CommandSet.ReturnBill);
        }

        /// <summary>
        /// Выдаем сдачу монетами
        /// </summary>
        /// <param name="PayOutSum"></param>
        public static void PayoutCoins(int PayOutSum)
        {
            byte[] payouttmpcmd = MDBCommandSets.CurrenzaC2Green_PayoutCoins(PayOutSum);
            AddCommand(payouttmpcmd);
            //MDBInstance.AddCommand(new byte[3] { 0x0f, 0x02, 0xFE});
        }

        /// <summary>
        /// Запрашиваем информацию о выданной сдаче
        /// </summary>
        public static void GetDispensedInfo()
        {
            AddCommand(CommandSet.GetDispensedCoinsInfo);
        }

        /// <summary>
        /// Отсылает команды на шину MDB
        /// </summary>
        /// <returns></returns>
        public static async Task SendCommandTask()
        {
            while (true)
            {
                MDBCommandsListSemaphore.Wait();
                if (CommandList.Count > 0)
                {
                    try
                    {
                        foreach (var cmd in CommandList)
                        {
                            DataWriter MDBSerialDataWriteObject = new DataWriter(MDBSerialPort.OutputStream);
                            MDBSerialDataWriteObject.WriteBytes(cmd);
                            await MDBSerialDataWriteObject.StoreAsync();
                            StringBuilder hex = new StringBuilder();
                            //foreach (byte b in cmd) hex.AppendFormat("{0:x2} ", b);
                            //StartPage.AddItemToLogBox("Запись в порт MDB: " + hex.ToString());
                            if (MDBSerialDataWriteObject != null)
                            {
                                MDBSerialDataWriteObject.DetachStream();
                                MDBSerialDataWriteObject = null;
                            }
                            await Task.Delay(500);
                        }
                        CommandList.Clear();
                    }
                    catch /*(Exception ex)*/
                    {
                        //StartPage.AddItemToLogBox("Ошибка записи в порт MDB: " + ex.Message);
                    }
                    finally
                    {

                    }
                }
                MDBCommandsListSemaphore.Release();
                await Task.Delay(500);
            }
        }


        /// <summary>
        /// Добавляет элемент в список команд для отправки по шине MDB
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dispense"></param>
        private static void AddCommand(byte[] cmd, bool dispense = false)
        {
            MDBCommandsListSemaphore.Wait();
            CommandList.Add(cmd);
            MDBCommandsListSemaphore.Release();
        }

        /// <summary>
        /// Добавляет элемент в список команд для отправки по шине MDB
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="dispense"></param>
        private static void AddCommand(byte[][] cmds, bool dispense = false)
        {
            MDBCommandsListSemaphore.Wait();
            foreach (var cmd in cmds)
            {
                CommandList.Add(cmd);
            }
            MDBCommandsListSemaphore.Release();
        }

        /// <summary>
        /// Запрещаем прием монет
        /// </summary>
        public static void DisableAcceptCoins()
        {
            AddCommand(CommandSet.DisableAcceptCoins);//Configure to accept 0.5, 1, 2, 5 and 10 RUR coins
        }

        /// <summary>
        /// Запрещаем прием купюр
        /// </summary>
        public static void DisableAcceptBills()
        {
            AddCommand(CommandSet.DisableAcceptBills); //Configure Bill Validator to accept only 50 and 100 RUR bills
        }

        /// <summary>
        /// разрешаем прием монет
        /// </summary>
        public static void EnableAcceptCoins()
        {
            AddCommand(CommandSet.EnableAcceptCoins);//Configure to accept 0.5, 1, 2, 5 and 10 RUR coins
        }

        /// <summary>
        /// Запрещаем выдачу монет
        /// </summary>
        public static void EnableDispenseCoins()
        {
            AddCommand(CommandSet.EnableDispenseCoins);
        }

        /// <summary>
        /// разрешаем прием наличных
        /// </summary>
        public static void EnableCashDevices()
        {
            AddCommand(new byte[][] { CommandSet.EnableAcceptCoins, CommandSet.EnableAcceptBills });
        }

        /// <summary>
        /// запрещаем прием наличных
        /// </summary>
        public static void DisableCashDevices()
        {
            AddCommand(new byte[][] {  CommandSet.DisableAcceptCoins, CommandSet.DisableAcceptBills });
        }

        /// <summary>
        /// Разрешаем прием купюр
        /// </summary>
        public static void EnableAcceptBills()
        {
            AddCommand(CommandSet.EnableAcceptBills); //Configure Bill Validator to accept only 50 and 100 RUR bills
        }

        /// <summary>
        /// Выполняет сброс монетоприемника
        /// </summary>
        public static void ResetCC()
        {
            AddCommand(CommandSet.ResetCC);//Reset CC
        }

        /// <summary>
        /// Выполняет сброс купюроприемника
        /// </summary>
        public static void ResetBA()
        {
            AddCommand(CommandSet.ResetBA);//Reset Bill Validator
        }

        /// <summary>
        /// Выполняет сброс купюроприемника
        /// </summary>
        public static void ResetCashDevices()
        {
            AddCommand(new byte[][] { CommandSet.ResetBA, CommandSet.ResetCC } );//Reset Bill Validator
        }

        /// <summary>
        /// Запрашиваем состояние стекера купюроприемника
        /// </summary>
        public static void GetBAStatus()
        {
            CheckBAStatus = true;
            AddCommand(CommandSet.GetBAStatus); //Request Stacker Status
        }

        /// <summary>
        /// Запрашиваем состояние монетоприемника
        /// </summary>
        public static void GetCCStatus()
        {
            CheckCCTubeStatus = true;
            AddCommand(CommandSet.GetCCStatus); //Request Stacker Status
        }
    }
}
