using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using MDBLib;

namespace RPiVendApp
{
    public static class CashDeskHelper
    {
        /// <summary>
        /// Инициализирует последовательный порт фискального регистратора
        /// </summary>
        public static async void CashDesk_Init()
        {
            try
            {
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.UART1LED, Colors.Yellow);
                StartPage.UpdateProgress(5);
                StartPage.AddItemToLogBox("Поиск последовательного порта ККТ...");
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                foreach (var item in dis)
                {
                    if (item.Name.Contains("FT232"))
                    {
                        CashDesk.CashDeskDeviceSerialPort = await SerialDevice.FromIdAsync(item.Id);
                        StartPage.CashDeskDeviceID = item.Id;
                        break;
                    }
                }
                if (CashDesk.CashDeskDeviceSerialPort == null) return;
                CashDesk.CashDeskDeviceSerialPort.WriteTimeout = TimeSpan.FromMilliseconds(100);
                CashDesk.CashDeskDeviceSerialPort.ReadTimeout = TimeSpan.FromMilliseconds(100);
                CashDesk.CashDeskDeviceSerialPort.BaudRate = 9600;
                CashDesk.CashDeskDeviceSerialPort.Parity = SerialParity.None;
                CashDesk.CashDeskDeviceSerialPort.StopBits = SerialStopBitCount.One;
                CashDesk.CashDeskDeviceSerialPort.DataBits = 8;
                CashDesk.DeviceTypeReceived += CashDesk_DeviceTypeReceived;
                CashDesk.KKTPrinterStateReceived += CashDesk_KKTPrinterStateReceived;
                CashDesk.KKTStateReceived += CashDesk_KKTStateReceived;
                CashDesk.CashDeskCurrentStageParametersReceived += CashDesk_CashDeskCurrentStageParametersReceived;
                CashDesk.StageStateReceived += CashDesk_StageStateReceived;
                CashDesk.CurrentModeChanged += CashDesk_CurrentModeChanged;
                CashDesk.CloseStageResult += CashDesk_CloseStageResult;
                CashDesk.OpenStageResult += CashDesk_OpenStageResult;
                CashDesk.OpenReceiptResult += CashDesk_OpenReceiptResult;
                CashDesk.CancelReceiptResult += CashDesk_CancelReceiptResult;
                CashDesk.StartReceiptEntryResult += CashDesk_StartReceiptEntryResult;
                CashDesk.AddEntryDataResult += CashDesk_AddEntryDataResult;
                CashDesk.ReceiptPaymentResult += CashDesk_ReceiptPaymentResult;
                CashDesk.CloseReceiptResult += CashDesk_CloseReceiptResult;
                CashDesk.AllTasksCancelled += CashDesk_AllTasksCancelled;
                string a = "Последовательный порт ККТ успешно настроен: ";
                a += CashDesk.CashDeskDeviceSerialPort.BaudRate + "-";
                a += CashDesk.CashDeskDeviceSerialPort.DataBits + "-";
                a += CashDesk.CashDeskDeviceSerialPort.Parity.ToString() + "-";
                a += CashDesk.CashDeskDeviceSerialPort.StopBits;
                StartPage.AddItemToLogBox(a);
                while (CashDesk.CashDeskDeviceSerialPort == null)
                {
                    await Task.Delay(100);
                }
                StartPage.AddItemToLogBox("Порт ККТ открыт, ожидание данных...");
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.UART1LED, Colors.Green);
                CashDesk.StartCommunication();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(CashDesk.IncomingKKTDataWatcher, StartPage.GlobalCancellationTokenSource.Token);
                Task.Run(CashDesk.KKTTaskWatcher, StartPage.GlobalCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception ex)
            {
                StartPage.AddItemToLogBox(ex.Message);
            }
        }

        private static void CashDesk_AllTasksCancelled()
        {
            if (StartPage.CurrentState == StartPage.States.Init)
            {
                StartPage.AddItemToLogBox("Успешная отмена заданий ККТ");
            }
        }

        private static void CashDesk_CashDeskCurrentStageParametersReceived(CurrentStageParameters StageParameters)
        {
            StartPage.SystemState.KKTStageNumber = StageParameters.StageNumber.ToString();
            StartPage.SystemState.KKTStageOpened = StageParameters.StageOpened;
            StartPage.SystemState.KKTReceiptNextNumber = StageParameters.LastReceiptNumber + 1;
        }

        /// <summary>
        /// результат закрытия чека
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_CloseReceiptResult(string ResultString)
        {
            if (ResultString == "OK")
            {
                StartPage.SystemState.KKTReceiptOpened = false;
            }
        }

        /// <summary>
        /// результат расчета по чеку
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_ReceiptPaymentResult(string ResultString)
        {

        }

        /// <summary>
        /// результат добавления данных позиции в чек
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_AddEntryDataResult(string ResultString)
        {

        }

        /// <summary>
        /// результат добавления позиции в чек
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_StartReceiptEntryResult(string ResultString)
        {

        }

        /// <summary>
        /// результат отмены чека
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_CancelReceiptResult(string ResultString)
        {
            if (ResultString == "OK")
            {
                StartPage.SystemState.KKTReceiptOpened = false;
            }
        }

        /// <summary>
        /// результат открытия чека
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_OpenReceiptResult(string ResultString)
        {
            if (ResultString == "OK")
            {
                StartPage.SystemState.KKTReceiptOpened = true;
            }
        }

        /// <summary>
        /// результат открытия смены
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_OpenStageResult(string ResultString)
        {
            if (ResultString == "OK")
            {
                StartPage.SystemState.KKTStageOpened = true;
            }
        }

        /// <summary>
        /// результат закрытия смены
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_CloseStageResult(string ResultString)
        {
            if (ResultString == "OK")
            {
                StartPage.SystemState.KKTStageOpened = false;
            }
        }

        /// <summary>
        /// результат смены режима
        /// </summary>
        /// <param name="ResultString"></param>
        private static void CashDesk_CurrentModeChanged(string ResultString)
        {
            StartPage.AddItemToLogBox(ResultString);
        }

        /// <summary>
        /// информация о состоянии смены
        /// </summary>
        /// <param name="StageStateString"></param>
        private static void CashDesk_StageStateReceived(StageProperties StageState)
        {
            try
            {
                //SystemState.KKTStageOpened = (StageState.CurrentStageState == StageProperties.StageState.Opened);
                StartPage.SystemState.KKTStageOver24h = (StageState.CurrentStageState == StageProperties.StageState.Over24h);
                StartPage.SystemState.LastStageClosedDateTime = Convert.ToInt64(StageState.StageClosedDateTime.ToString("yyyyMMddHHmmss"));
                StartPage.SystemState.LastStageClosedDateTimeStr = StageState.StageClosedDateTime.ToString("dd.MM.yyyy HH:mm:ss");
                if (StartPage.CurrentState != StartPage.States.Init && StartPage.CurrentState != StartPage.States.OutOfService && StartPage.CurrentState != StartPage.States.ServiceMode && StartPage.SystemState.KKTStageOver24h)
                {
                    StartPage.CurrentState = StartPage.States.OutOfService;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    MDB.DisableCashDevices();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var frame = Window.Current.Content as Frame;
                        frame.Navigate(typeof(OutOfServicePage));
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    StartPage.AddItemToLogBox("Длительность смены превысила 24ч, устройство переведено в режим \"Не обслуживает\"");
                }
            }
            catch
            {

            }
        }

        /// <summary>
        /// Информация о состоянии ККТ
        /// </summary>
        /// <param name="CashDeskKKTState"></param>
        private static void CashDesk_KKTStateReceived(CashDeskKKTState CashDeskKKTState)
        {
            try
            {
                StartPage.SystemState.KKTMfgNumber = CashDeskKKTState.KKTMfgNumber;
                //SystemState.KKTStageNumber = CashDeskKKTState.StageNumber;
                //SystemState.KKTStageOpened = CashDeskKKTState.StageOpened;
                StartPage.SystemState.KKTReceiptOpened = (CashDeskKKTState.ReceiptState == 2);
                //SystemState.KKTReceiptNextNumber = Convert.ToInt32(CashDeskKKTState.NextReceiptNumber);
                StartPage.SystemState.KKTCurrentMode = (int)BCDHelper.BCDByteToInt(new byte[] { Convert.ToByte((int)CashDeskKKTState.CurrentMode) });
            }
            catch
            {

            }
        }

        /// <summary>
        /// Информация о состоянии принтера ККТ
        /// </summary>
        /// <param name="KKTPrinterState"></param>
        private static void CashDesk_KKTPrinterStateReceived(KKTPrinterState KKTPrinterState)
        {
            try
            {
                StartPage.SystemState.KKTPrinterConnected = KKTPrinterState.PrinterConnected;
                StartPage.SystemState.KKTPrinterCutterError = KKTPrinterState.PrinterCutterError;
                StartPage.SystemState.KKTPrinterNonRecoverableError = KKTPrinterState.PrinterNonRecoverableError;
                StartPage.SystemState.KKTPrinterOverHeated = KKTPrinterState.PrinterOverHeated;
                StartPage.SystemState.KKTPrinterPaperEmpty = KKTPrinterState.PrinterPaperEmpty;
                StartPage.SystemState.KKTPrinterPaperEnding = KKTPrinterState.PrinterPaperEnding;
                StartPage.SystemState.KKTPrinterPaperJammed = KKTPrinterState.PrinterPaperJammed;
            }
            catch
            {

            }
        }

        /// <summary>
        /// Информация о типе ККТ
        /// </summary>
        /// <param name="KKTDeviceTypeString"></param>
        private static void CashDesk_DeviceTypeReceived(string KKTDeviceTypeString)
        {

        }
    }
}
