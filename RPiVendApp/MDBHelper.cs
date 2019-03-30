using System;
using Windows.Devices.Gpio;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Threading;
using Windows.Storage;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using MDBLib;

namespace RPiVendApp
{
    /// <summary>
    /// Класс-прокладка для работы с устройствами приема наличных
    /// </summary>
    public static class MDBHelper
    {
        /// <summary>
        /// Прогресс инициализации: 0 - выкл, 1 - адаптер стартанул, 2,3,4 - инициализация устройств, 5 - готов к работе, 6 - ошибка
        /// </summary>
        public static int MDBInitStep = 0;
        /// <summary>
        /// Подключаем обработчики событий от устройств, включаем питание на шине MDB
        /// </summary>
        /// <returns></returns>
        public static void StartMDB()
        {
            try
            {
                MDB.MDBAdapterStarted += CashDevices_MDBStarted;
                MDB.MDBCoinChangerTubesStatus += CashDevices_MDBCCTubesStatus;
                MDB.MDBChangeDispensed += CashDevices_MDBChangeDispensed;
                MDB.MDBInsertedBill += CashDevices_MDBInsertedBill;
                MDB.MDBInsertedCoinRoutedToCashBox += CashDevices_MDBInsertedCoinRoutedToCashBox;
                MDB.MDBInsertedCoinRoutedToCoinChangerTube += CashDevices_MDBInsertedCoinRoutedToCCTube;
                MDB.MDBDataProcessingError += CashDevices_MDBDataProcessingError;
                MDB.MDBError += CashDevices_MDBError;
                MDB.MDBCoinChangerReseted += CashDevices_MDBCCReseted;
                MDB.MDBBillValidatorReseted += CashDevices_MDBBAReseted;
                MDB.MDBCoinChangerPayoutStarted += CashDevices_MDBCCPayOutStarted;
                MDB.MDBInformationMessageReceived += CashDevices_MDBInformationMessageReceived;
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
        private static void CashDevices_MDBCCPayOutStarted()
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
                    if ((!StartPage.bareset) && (MDBInitStep == 3 || MDBInitStep == 4))
                    {
                        MDBInitStep++;
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
                            MDBInitStep = 5;
                            StartPage.AddItemToLogBox("Устройства приема наличных обнаружены и настроены");
                        }
                    }
                    if ((!StartPage.bainit) && (MDBInitStep == 1 || MDBInitStep == 2))
                    {
                        MDBInitStep++;
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        MDB.ResetBillValidatorAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
                    if ((!StartPage.ccreset) && (MDBInitStep == 3 || MDBInitStep == 4))
                    {
                        MDBInitStep++;
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
                            MDBInitStep = 5;
                            StartPage.AddItemToLogBox("Устройства приема наличных обнаружены и настроены");
                        }
                    }
                    if ((!StartPage.ccinit) && (MDBInitStep == 1 || MDBInitStep == 2))
                    {
                        MDBInitStep++;
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        MDB.ResetCoinChangerAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
                StartPage.UpdateProgress(15);
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.MDBLED, Colors.Green);
                StartPage.UpdateStartLEDs(StartPage.StartPageInstance.UART0LED, Colors.Green);
                StartPage.AddItemToLogBox("Найден адаптер MDB-RS232");
                int count = 0;
                while (count < 10)
                {
                    Task.Delay(100).Wait();
                    count++;
                }
                MDBInitStep = 1;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                MDB.ResetCashDevicesAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
                MDBInitStep = 6;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                MDB.DisableCashDevicesAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
        private static void CashDevices_MDBChangeDispensed(List<MDB.CoinsRecord> CoinsRecords)
        {
            StartPage.AddItemToLogBox("Выдана сдача, руб: " + CoinsRecords.Sum(x => x.CoinValue * x.CoinsDispensed).ToString("N2"));
        }

        /// <summary>
        /// Обрабатываем сведения о состоянии трубок монетоприемника
        /// </summary>
        private static void CashDevices_MDBCCTubesStatus(List<MDB.CoinChangerTubeRecord> CoinChangerTubeRecords)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServiceTasks.UpdateCashCounter(0, 0, 0, 0, CoinChangerTubeRecords.First(x => (int)x.CoinValue == 1).CoinsCount , CoinChangerTubeRecords.First(x => (int)x.CoinValue == 2).CoinsCount, CoinChangerTubeRecords.First(x => (int)x.CoinValue == 5).CoinsCount, CoinChangerTubeRecords.First(x => (int)x.CoinValue == 10).CoinsCount);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
