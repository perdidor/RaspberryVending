using System;
using Windows.Devices.Gpio;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace RPiVendApp
{
    public static class MDBHelper
    {
        /// <summary>
        /// Инициализируем устройства приема наличных, включаем питание на шине MDB
        /// </summary>
        /// <returns></returns>
        public static async Task Init_MDB()
        {
            try
            {
                while (MDB.MDBSerialPort == null)
                {
                    await Task.Delay(100);
                }
                MDB.MDBStarted += CashDevices_MDBStarted;
                MDB.MDBCCTubesStatus += CashDevices_MDBCCTubesStatus;
                MDB.MDBChangeDispensed += CashDevices_MDBChangeDispensed;
                MDB.MDBInsertedBill += CashDevices_MDBInsertedBill;
                MDB.MDBInsertedCoinRoutedToCashBox += CashDevices_MDBInsertedCoinRoutedToCashBox;
                MDB.MDBInsertedCoinRoutedToCCTube += CashDevices_MDBInsertedCoinRoutedToCCTube;
                MDB.MDBDataProcessingError += CashDevices_MDBDataProcessingError;
                MDB.MDBError += CashDevices_MDBError;
                MDB.MDBCCReseted += CashDevices_MDBCCReseted;
                MDB.MDBBAReseted += CashDevices_MDBBAReseted;
                MDB.MDBCCPayOutBusy += CashDevices_MDBCCPayOutBusy;
                MDB.MDBInformationMessageReceived += CashDevices_MDBInformationMessageReceived;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(MDB.DispensedCoinsInfoTask, StartPage.GlobalCancellationTokenSource.Token);
                Task.Run(MDB.SendCommandTask, StartPage.GlobalCancellationTokenSource.Token);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBLED, Colors.Yellow);
                StartPage.AddItemToLogBox("Включаем питание шины MDB...");
                StartPage.MDBPin.Write(GpioPinValue.Low);
            }
            catch
            {
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBLED, Colors.Red);
            }
        }

        /// <summary>
        /// Монетоприемник занят выдачей сдачи
        /// </summary>
        private static void CashDevices_MDBCCPayOutBusy()
        {
            //можно чем-нибудь моргнуть или выдать звуковое сопровождение
        }

        /// <summary>
        /// произведен сброс купюроприемника
        /// </summary>
        private static void CashDevices_MDBBAReseted()
        {
            Task.Run(() =>
            {
                Mutex m2 = new Mutex(true, "MDBDeviceResetHandlerMutex", out bool mutexWasCreated);
                if (!mutexWasCreated)
                {
                    try
                    {
                        m2.WaitOne();
                    }
                    catch (AbandonedMutexException)
                    {

                    }
                }
                try
                {
                    if ((!StartPage.bareset) && (MDB.MDBInitStep == 3 || MDB.MDBInitStep == 4))
                    {
                        MDB.MDBInitStep++;
                        StartPage.UpdateProgress(5);
                        StartPage.bareset = true;
                        StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBBALED, Colors.Green);
                        StartPage.AddItemToLogBox("Купюроприемник готов к работе");
                        if (StartPage.ccreset)
                        {
                            string oosfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".031";
                            StartPage.CurrentState = StartPage.States.OutOfService;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                var frame = Window.Current.Content as Frame;
                                frame.Navigate(typeof(OutOfServicePage));
                            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            StartPage.AddItemToLogBox("Ожидание полной инициализации...");
                            int count = 0;
                            while (count < 100)
                            {
                                Task.Delay(100).Wait();
                                count++;
                            }
                            MDB.MDBInitStep = 5;
                            StartPage.AddItemToLogBox("Устройства приема наличных обнаружены и настроены");
                        }
                    }
                    if ((!StartPage.bainit) && (MDB.MDBInitStep == 1 || MDB.MDBInitStep == 2))
                    {
                        MDB.MDBInitStep++;
                        StartPage.UpdateProgress(5);
                        StartPage.bainit = true;
                        StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBBALED, Colors.Yellow);
                        StartPage.AddItemToLogBox("Купюроприемник инициализирован");
                        StartPage.UpdateProgress(5);
                        int count = 0;
                        while (count < 20)
                        {
                            Task.Delay(100).Wait();
                            count++;
                        }
                        MDB.ResetBA();
                    }
                }
                catch (Exception ex)
                {
                    StartPage.AddItemToLogBox("Ошибка: " + ex.Message);
                }
                finally
                {

                }
                m2.ReleaseMutex();
                m2.Dispose();
            });
        }

        /// <summary>
        /// произведен сброс монетоприемника
        /// </summary>
        private static void CashDevices_MDBCCReseted()
        {
            Task.Run(() =>
            {
                Mutex m2 = new Mutex(true, "MDBDeviceResetHandlerMutex", out bool mutexWasCreated);
                if (!mutexWasCreated)
                {
                    try
                    {
                        m2.WaitOne();
                    }
                    catch (AbandonedMutexException)
                    {

                    }
                }
                try
                {
                    if ((!StartPage.ccreset) && (MDB.MDBInitStep == 3 || MDB.MDBInitStep == 4))
                    {
                        MDB.MDBInitStep++;
                        StartPage.UpdateProgress(5);
                        StartPage.ccreset = true;
                        StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBCCLED, Colors.Green);
                        StartPage.AddItemToLogBox("Монетоприемник готов к работе");
                        if (StartPage.bareset)
                        {
                            string oosfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".031";
                            StartPage.CurrentState = StartPage.States.OutOfService;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                var frame = Window.Current.Content as Frame;
                                frame.Navigate(typeof(OutOfServicePage));
                            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            StartPage.AddItemToLogBox("Ожидание полной инициализации...");
                            int count = 0;
                            while (count < 100)
                            {
                                Task.Delay(100).Wait();
                                count++;
                            }
                            MDB.MDBInitStep = 5;
                            StartPage.AddItemToLogBox("Устройства приема наличных обнаружены и настроены");
                        }
                    }
                    if ((!StartPage.ccinit) && (MDB.MDBInitStep == 1 || MDB.MDBInitStep == 2))
                    {
                        MDB.MDBInitStep++;
                        StartPage.UpdateProgress(5);
                        StartPage.ccinit = true;
                        StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBCCLED, Colors.Yellow);
                        StartPage.AddItemToLogBox("Монетоприемник инициализирован");
                        int count = 0;
                        while (count < 20)
                        {
                            Task.Delay(100).Wait();
                            count++;
                        }
                        MDB.ResetCC();
                    }
                }
                catch (Exception ex)
                {
                    StartPage.AddItemToLogBox("Ошибка: " + ex.Message);
                }
                finally
                {

                }
                m2.ReleaseMutex();
                m2.Dispose();
            });
        }

        /// <summary>
        /// Монета помещена в трубку монетоприемника
        /// </summary>
        /// <param name="CoinValue"></param>
        private static void CashDevices_MDBInsertedCoinRoutedToCCTube(double CoinValue)
        {
            StartPage.AddItemToLogBox("Монета помещена в трубку монетоприемника ₽" + CoinValue.ToString("N2"));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServiceTasks.UpdateCashCounter(1, CoinValue, 0, 0);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if ((StartPage.CurrentState == StartPage.States.ReadyToServe) || (StartPage.CurrentState == StartPage.States.ReadyToDispenseWater))
            {
                StartPage.UserDeposit += CoinValue;
            }
        }

        /// <summary>
        /// MDB устройства включены
        /// </summary>
        private static void CashDevices_MDBStarted()
        {
            Task.Run(() =>
            {
                StartPage.UpdateProgress(10);
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBLED, Colors.Green);
                StartPage.AddItemToLogBox("Найден адаптер MDB-RS232");
                int count = 0;
                while (count < 10)
                {
                    Task.Delay(100).Wait();
                    count++;
                }
                MDB.MDBInitStep = 1;
                MDB.ResetCashDevices();
            });
        }

        /// <summary>
        /// Обрабатываем текстовую информацию от устройств приема наличных
        /// </summary>
        /// <param name="MDBInformationMessage"></param>
        private static void CashDevices_MDBInformationMessageReceived(string MDBInformationMessage)
        {
            StartPage.AddItemToLogBox(MDBInformationMessage);
        }

        /// <summary>
        /// Критическая ошибка устройств приема наличных
        /// </summary>
        /// <param name="ErrorMessage"></param>
        private static void CashDevices_MDBError(string ErrorMessage)
        {
            if ((StartPage.CurrentState == StartPage.States.ReadyToServe) || (StartPage.CurrentState == StartPage.States.ReadyToDispenseWater) || (StartPage.CurrentState == StartPage.States.DispenseChange))
            {
                StartPage.CurrentState = StartPage.States.OutOfService;
                MDB.MDBInitStep = 6;
                MDB.DisableCashDevices();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(OutOfServicePage));
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                StartPage.AddItemToLogBox("Устройства приема наличных принудительно отключены из-за ошибки.");
            }
        }

        /// <summary>
        /// Ошибка при обработке данных от устройств приема наличных
        /// </summary>
        /// <param name="DataProcessingErrorMessage"></param>
        private static void CashDevices_MDBDataProcessingError(string DataProcessingErrorMessage)
        {
            StartPage.AddItemToLogBox(DataProcessingErrorMessage);
        }

        /// <summary>
        /// Монета помещена в кэшбокс
        /// </summary>
        /// <param name="CoinValue"></param>
        private static void CashDevices_MDBInsertedCoinRoutedToCashBox(double CoinValue)
        {
            StartPage.AddItemToLogBox("Монета помещена в кэшбокс ₽" + CoinValue.ToString("N2"));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServiceTasks.UpdateCashCounter(1, CoinValue, 0, 0);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if ((StartPage.CurrentState == StartPage.States.ReadyToServe) || (StartPage.CurrentState == StartPage.States.ReadyToDispenseWater))
            {
                StartPage.UserDeposit += CoinValue;
            }
        }

        /// <summary>
        /// Вставлена купюра
        /// </summary>
        /// <param name="BillValue"></param>
        private static void CashDevices_MDBInsertedBill(double BillValue)
        {
            StartPage.AddItemToLogBox("Прием купюры ₽" + BillValue.ToString("N2"));
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServiceTasks.UpdateCashCounter(0, 0, 1, (int)BillValue);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if ((StartPage.CurrentState == StartPage.States.ReadyToServe) || (StartPage.CurrentState == StartPage.States.ReadyToDispenseWater))
            {
                StartPage.UserDeposit += BillValue;
            }
        }

        /// <summary>
        /// Обрабатыввем сведения о выданной сдаче
        /// </summary>
        /// <param name="DispensedSum"></param>
        private static void CashDevices_MDBChangeDispensed(int DispensedSum)
        {
            StartPage.AddItemToLogBox("Выдана сдача, руб: " + DispensedSum.ToString("N2"));
        }

        /// <summary>
        /// Обрабатываем сведения о состоянии трубок монетоприемника
        /// </summary>
        /// <param name="RUR1"></param>
        /// <param name="RUR2"></param>
        /// <param name="RUR5"></param>
        /// <param name="RUR10"></param>
        private static void CashDevices_MDBCCTubesStatus(int RUR1, int RUR2, int RUR5, int RUR10)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServiceTasks.UpdateCashCounter(0, 0, 0, 0, RUR10, RUR5, RUR2, RUR1);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
