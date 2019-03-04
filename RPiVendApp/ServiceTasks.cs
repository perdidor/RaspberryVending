using System;
using Windows.Storage.Streams;
using Windows.Devices.Gpio;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Windows.UI;
using Windows.UI.Xaml;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using System.Threading;
using Windows.Web.Http;
using Windows.Storage;
using Windows.System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace RPiVendApp
{
    /// <summary>
    /// Служебные задачи
    /// </summary>
    public static class ServiceTasks
    {
        public static void StartAll()
        {
            Task.Run(GetDistance, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(HoursCounter, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(CashCounter, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(GetAmbientVars, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(PowerSocketsWatcher, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(SystemStateLog, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(ExternalLightSwitcher, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(MDBBAStatus, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(MDBCCStatus, StartPage.GlobalCancellationTokenSource.Token);
            if (StartPage.CurrentDeviceSettings.UseKKT) Task.Run(KKTStatusWatchTask, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(ReceiveCommands, StartPage.GlobalCancellationTokenSource.Token);
            Task.Run(InitialStartupWaiter, StartPage.GlobalCancellationTokenSource.Token);
        }

        /// <summary>
        /// Последние 10 значений уровня воды, для фильтрации показаний ультразвукового датчика расстояния
        /// </summary>
        private static double[] DistanceArray = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        /// <summary>
        /// Ожидание старта и переход в интерфейс продажи
        /// </summary>
        /// <returns></returns>
        private static async Task InitialStartupWaiter()
        {
            try
            {
                await Task.Delay(30000);
                while (StartPage.SystemState.MDBInitStep != 5 || (StartPage.CurrentDeviceSettings.UseKKT && (StartPage.SystemState.KKTCurrentMode != 1 || !StartPage.SystemState.KKTStageOpened || StartPage.SystemState.KKTStageOver24h)))
                {
                    await Task.Delay(5000);
                }
                StartPage.CurrentState = StartPage.States.ReadyToServe;
                MDB.EnableCashDevices();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(MainPage));
                });
            }
            catch
            {

            }
        }

        /// <summary>
        /// Запрашивает команды с сервера системы управления
        /// </summary>
        /// <returns></returns>
        private static async Task ReceiveCommands()
        {
            StartPage.AddItemToLogBox("Старт получения команд из системы управления...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.SYSTEMSTATELED.Fill = mySolidColorBrush;
            });
            await Task.Delay(30000);
            while (true)
            {
                try
                {
                    ClientRequest tmpreq = RequestEncoder.EncodeRequestData(GlobalVars.RegID);
                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "Request", tmpreq.Request },
                        { "Signature", tmpreq.Signature },
                        { "AData", tmpreq.AData },
                        { "BData", tmpreq.BData }
                    };
                    HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(data);
                    HttpClient httpClient = StartPage.StartPageInstance.NewWebClient();
                    Uri requestUri = new Uri(GlobalVars.SERVER_ENDPOINT + GlobalVars.RECEIVE_COMMANDS_PATH);
                    HttpResponseMessage httpResponse = new HttpResponseMessage();
                    httpResponse = await httpClient.PostAsync(requestUri, content);
                    httpResponse.EnsureSuccessStatusCode();
                    string tmpres = await httpResponse.Content.ReadAsStringAsync();
                    string oosfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".031";
                    switch (tmpres)
                    {
                        case "SalesMode":
                            {
                                if (StartPage.CurrentState == StartPage.States.OutOfService || StartPage.CurrentState == StartPage.States.ServiceMode)
                                {
                                    File.Delete(oosfilename);
                                    ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0));
                                }
                                break;
                            }
                        case "KKTCloseStage":
                            {
                                while (CashDesk.PendingTasks.Count != 0)
                                {
                                    await Task.Delay(100);
                                }
                                if (StartPage.SystemState.KKTCurrentMode != 0) CashDesk.ExitCurrentMode();
                                CashDesk.ChangeMode(DeviceStateMode.StageReports_WaitingForCommand);
                                CashDesk.CloseStage();
                                break;
                            }
                        case "KKTOpenStage":
                            {
                                while (CashDesk.PendingTasks.Count != 0)
                                {
                                    await Task.Delay(100);
                                }
                                if (StartPage.SystemState.KKTCurrentMode != 0) CashDesk.ExitCurrentMode();
                                CashDesk.ChangeMode(DeviceStateMode.Registration_WaitingForCommand);
                                CashDesk.OpenStage();
                                break;
                            }
                        case "KKTRegistrationMode":
                            {
                                while (CashDesk.PendingTasks.Count != 0)
                                {
                                    await Task.Delay(100);
                                }
                                if (StartPage.SystemState.KKTCurrentMode != 0) CashDesk.ExitCurrentMode();
                                CashDesk.ChangeMode(DeviceStateMode.Registration_WaitingForCommand);
                                break;
                            }
                        case "KKTCancelReceipt":
                            {
                                while (CashDesk.PendingTasks.Count != 0)
                                {
                                    await Task.Delay(100);
                                }
                                CashDesk.CancelReceipt();
                                break;
                            }
                        case "Shutdown":
                            {
                                ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0));
                                break;
                            }
                        case "Reboot":
                            {
                                ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0));
                                break;
                            }
                        case "OOSMode":
                            {
                                File.WriteAllText(oosfilename, "oospersists");
                                while (StartPage.CurrentState != StartPage.States.ReadyToServe && StartPage.CurrentState != StartPage.States.ServiceMode)
                                {
                                    await Task.Delay(1000);
                                }
                                StartPage.CurrentState = StartPage.States.OutOfService;
                                MDB.DisableCashDevices();
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    var frame = Window.Current.Content as Frame;
                                    frame.Navigate(typeof(OutOfServicePage));
                                });
                                StartPage.AddItemToLogBox("Временно не обслуживает");
                                break;
                            }
                        case "ServiceMode":
                            {
                                StartPage.CurrentState = StartPage.States.ServiceMode;
                                MDB.DisableAcceptBills();
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    var frame = Window.Current.Content as Frame;
                                    frame.Navigate(typeof(ServiceModePage));
                                });
                                break;
                            }
                        case "Incasso":
                            {
                                StartPage.CurrentState = StartPage.States.ServiceMode;
                                MDB.DisableCashDevices();
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    var frame = Window.Current.Content as Frame;
                                    frame.Navigate(typeof(ServiceModePage));
                                });
                                StartPage.StartPageInstance.MakeIncassation();
                                break;
                            }
                        case "Unregister":
                            {
                                string keypairfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".001";
                                string regfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".002";
                                File.Delete(keypairfilename);
                                File.Delete(regfilename);
                                ShutdownManager.BeginShutdown(ShutdownKind.Restart, TimeSpan.FromSeconds(0));
                                break;
                            }
                    }
                }
                catch /*(Exception ex)*/
                {
                    //AddItemToLogBox(ex.Message);
                }
                await Task.Delay(30000);
            }
        }

        /// <summary>
        /// Запрашивает данные о состоянии купюроприемника
        /// </summary>
        /// <returns></returns>
        private static async Task MDBBAStatus()
        {
            await Task.Delay(35000);
            StartPage.AddItemToLogBox("Старт отслеживания состояния купюроприемника...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.MDBBASTATUSLED.Fill = mySolidColorBrush;
            });
            while (true)
            {
                if (MDB.MDBInitStep == 5 && !MDB.DispenseInProgress && MDB.CheckDispenseResult)
                {
                    MDB.GetBAStatus();
                }
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.MDBBASTATUSLED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.MDBBASTATUSLED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(30000);
            }
        }

        /// <summary>
        /// Запрашивает данные о состоянии ККТ
        /// </summary>
        /// <returns></returns>
        private static async Task MDBCCStatus()
        {
            await Task.Delay(30000);
            StartPage.AddItemToLogBox("Старт отслеживания состояния монетоприемника...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.MDBCCSTATUSLED.Fill = mySolidColorBrush;
            });
            while (true)
            {
                if (MDB.MDBInitStep == 5 && !MDB.DispenseInProgress && !MDB.CheckDispenseResult)
                {
                    MDB.GetCCStatus();
                }
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.MDBCCSTATUSLED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.MDBCCSTATUSLED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(30000);
            }
        }

        /// <summary>
        /// Запрашивает данные о состоянии монетоприемника
        /// </summary>
        /// <returns></returns>
        private static async Task KKTStatusWatchTask()
        {
            StartPage.AddItemToLogBox("Старт отслеживания состояния ККТ...");
            CashDesk.GetKKTInformation();
            await Task.Delay(20000);
            if (StartPage.SystemState.KKTCurrentMode != 1 && StartPage.SystemState.KKTStageOpened && !StartPage.SystemState.KKTStageOver24h)
            {
                if (StartPage.SystemState.KKTCurrentMode != 0) CashDesk.ExitCurrentMode();
                CashDesk.ChangeMode(DeviceStateMode.Registration_WaitingForCommand);
            }
            await Task.Delay(120000);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.KKTSTATELED.Fill = mySolidColorBrush;
            });
            while (true)
            {
                if (CashDesk.CashDeskDeviceSerialPort != null && StartPage.CurrentState != StartPage.States.DispenseChange && StartPage.CurrentState != StartPage.States.ReadyToDispenseWater)
                {
                    while (CashDesk.PendingTasks.Count != 0)
                    {
                        await Task.Delay(100);
                    }
                    CashDesk.GetKKTInformation();
                }
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.KKTSTATELED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.KKTSTATELED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(120000);
            }
        }

        /// <summary>
        /// Отправляет на сервер данные о состоянии устройста
        /// </summary>
        /// <returns></returns>
        private static async Task SystemStateLog()
        {
            StartPage.AddItemToLogBox("Старт отслеживания состояния системы...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.SYSTEMSTATELED.Fill = mySolidColorBrush;
            });
            await Task.Delay(60000);
            while (true)
            {
                try
                {
                    StartPage.SystemState.VMCMode = ((int)StartPage.CurrentState).ToString();
                    StartPage.SystemState.MDBInitStep = MDB.MDBInitStep;
                    StartPage.CashCounterAccessFlag.Wait();
                    ClientRequest tmpreq = RequestEncoder.EncodeRequestData(StartPage.SystemState);
                    StartPage.CashCounterAccessFlag.Release();
                    Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "Request", tmpreq.Request },
                        { "Signature", tmpreq.Signature },
                        { "AData", tmpreq.AData },
                        { "BData", tmpreq.BData }
                    };
                    HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(data);
                    HttpClient httpClient = StartPage.StartPageInstance.NewWebClient();
                    Uri requestUri = new Uri(GlobalVars.SERVER_ENDPOINT + GlobalVars.SYSTEM_STATE_LOG_PATH);
                    HttpResponseMessage httpResponse = new HttpResponseMessage();
                    httpResponse = await httpClient.PostAsync(requestUri, content);
                    httpResponse.EnsureSuccessStatusCode();
                }
                catch /*(Exception ex)*/
                {
                    //AddItemToLogBox(ex.Message);
                }
                ChangePage.ChangeModeSemaphore.Wait();
                string watersalesfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".006";
                try
                {
                    List<WaterSales> WaterSalesList = StartPage.Deserialize<List<WaterSales>>(File.ReadAllText(watersalesfilename));
                    if (WaterSalesList.Count > 0)
                    {
                        ClientRequest tmpreq = RequestEncoder.EncodeRequestData(WaterSalesList);
                        Dictionary<string, string> data = new Dictionary<string, string>
                        {
                            { "Request", tmpreq.Request },
                            { "Signature", tmpreq.Signature },
                            { "AData", tmpreq.AData },
                            { "BData", tmpreq.BData }
                        };
                        HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(data);
                        HttpClient httpClient = StartPage.StartPageInstance.NewWebClient();
                        Uri requestUri = new Uri(GlobalVars.SERVER_ENDPOINT + GlobalVars.SALES_LOG_PATH);
                        HttpResponseMessage httpResponse = new HttpResponseMessage();
                        httpResponse = await httpClient.PostAsync(requestUri, content);
                        httpResponse.EnsureSuccessStatusCode();
                        string tmpres = await httpResponse.Content.ReadAsStringAsync();
                        if (tmpres == WaterSalesList.Count.ToString())
                        {
                            WaterSalesList.Clear();
                            while (WaterSalesList.Count != 0)
                            {
                                Task.Delay(100).Wait();
                            }
                            var xs = new XmlSerializer(WaterSalesList.GetType());
                            var xml = new Utf8StringWriter();
                            xs.Serialize(xml, WaterSalesList);
                            File.WriteAllText(watersalesfilename, xml.ToString());
                        }
                    }
                }
                catch /*(Exception ex)*/
                {

                }
                finally
                {
                    ChangePage.ChangeModeSemaphore.Release();
                }
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.SYSTEMSTATELED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.SYSTEMSTATELED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(180000);
            }
        }


        /// <summary>
        /// Вычисляет текущее положение Солнца относительно горизонта для определения начала и конца темного времени суток.
        /// для вычисления используются координаты текущего положения устройства, заданные в классе GlobalVars
        /// </summary>
        /// <param name="azimuthdegrees"></param>
        /// <param name="altitudedegrees"></param>
        private static void CalculateSunPosition(out double azimuthdegrees, out double altitudedegrees)
        {
            DateTime dateTime = DateTime.Now;
            double Deg2Rad = 0.01745329251994329576923690768489;
            double Rad2Deg = 1 / Deg2Rad;
            // Convert to UTC
            dateTime = dateTime.ToUniversalTime();
            // Number of days from J2000.0.
            double julianDate = 367 * dateTime.Year - (int)((7.0 / 4.0) * (dateTime.Year + (int)((dateTime.Month + 9.0) / 12.0)))
                      + (int)((275.0 * dateTime.Month) / 9.0) + dateTime.Day - 730531.5;
            double julianCenturies = julianDate / 36525.0;
            // Sidereal Time
            double siderealTimeHours = 6.6974 + 2400.0513 * julianCenturies;
            double siderealTimeUT = siderealTimeHours + (366.2422 / 365.2422) * dateTime.TimeOfDay.TotalHours;
            double siderealTime = siderealTimeUT * 15 + StartPage.CurrentDeviceSettings.Longitude;
            // Refine to number of days (fractional) to specific time.
            julianDate += dateTime.TimeOfDay.TotalHours / 24.0;
            julianCenturies = julianDate / 36525.0;
            // Solar Coordinates
            double meanLongitude = CorrectAngle(Deg2Rad * (280.466 + 36000.77 * julianCenturies));
            double meanAnomaly = CorrectAngle(Deg2Rad * (357.529 + 35999.05 * julianCenturies));
            double equationOfCenter = Deg2Rad * ((1.915 - 0.005 * julianCenturies) * Math.Sin(meanAnomaly) + 0.02 * Math.Sin(2 * meanAnomaly));
            double elipticalLongitude = CorrectAngle(meanLongitude + equationOfCenter);
            double obliquity = (23.439 - 0.013 * julianCenturies) * Deg2Rad;
            // Right Ascension
            double rightAscension = Math.Atan2(Math.Cos(obliquity) * Math.Sin(elipticalLongitude), Math.Cos(elipticalLongitude));
            double declination = Math.Asin(Math.Sin(rightAscension) * Math.Sin(obliquity));
            // Horizontal Coordinates
            double hourAngle = CorrectAngle(siderealTime * Deg2Rad) - rightAscension;
            if (hourAngle > Math.PI)
            {
                hourAngle -= 2 * Math.PI;
            }
            altitudedegrees = Math.Asin(Math.Sin(StartPage.CurrentDeviceSettings.Latitude * Deg2Rad) * Math.Sin(declination) + Math.Cos(StartPage.CurrentDeviceSettings.Latitude * Deg2Rad) * Math.Cos(declination) * Math.Cos(hourAngle));
            // Nominator and denominator for calculating Azimuth
            // angle. Needed to test which quadrant the angle is in.
            double aziNom = -Math.Sin(hourAngle);
            double aziDenom = Math.Tan(declination) * Math.Cos(StartPage.CurrentDeviceSettings.Latitude * Deg2Rad) - Math.Sin(StartPage.CurrentDeviceSettings.Latitude * Deg2Rad) * Math.Cos(hourAngle);
            azimuthdegrees = Math.Atan(aziNom / aziDenom);
            if (aziDenom < 0) // In 2nd or 3rd quadrant
            {
                azimuthdegrees += Math.PI;
            }
            else if (aziNom < 0) // In 4th quadrant
            {
                azimuthdegrees += 2 * Math.PI;
            }
            azimuthdegrees *= Rad2Deg;
            altitudedegrees *= Rad2Deg;
        }

        /// <summary>
        /// Определяет, в какой четверти координатной плоскости находится угол
        /// </summary>
        /// <param name="angleInRadians"></param>
        /// <returns></returns>
        private static double CorrectAngle(double angleInRadians)
        {
            if (angleInRadians < 0)
            {
                return 2 * Math.PI - (Math.Abs(angleInRadians) % (2 * Math.PI));
            }
            if (angleInRadians > 2 * Math.PI)
            {
                return angleInRadians % (2 * Math.PI);
            }
            return angleInRadians;
        }

        /// <summary>
        /// Включает и выключает внешнее освещение в зависимости от времени суток
        /// </summary>
        /// <returns></returns>
        private static async Task ExternalLightSwitcher()
        {
            await Task.Delay(12000);
            StartPage.AddItemToLogBox("Старт управления внешним освещением...");
            StartPage.UpdateProgress(5);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.EXTERNALLIGHTLED.Fill = mySolidColorBrush;
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            while (true)
            {
                await Task.Run(() =>
                {
                    double azimuthdegrees = 0.00;
                    double altitudedegrees = 0.00;
                    try
                    {
                        CalculateSunPosition(out azimuthdegrees, out altitudedegrees);
                        if ((altitudedegrees <= -1) && (!StartPage.SystemState.IsExternalLightOn))
                        {
                            StartPage.ExternalLightPin.Write(GpioPinValue.Low);
                            StartPage.AddItemToLogBox("Внешнее освещение включено");
                        }
                        if ((altitudedegrees >= 0) && (StartPage.SystemState.IsExternalLightOn))
                        {
                            StartPage.ExternalLightPin.Write(GpioPinValue.High);
                            StartPage.AddItemToLogBox("Внешнее освещение отключено");
                        }
                    }
                    catch (Exception ex)
                    {
                        StartPage.AddItemToLogBox(ex.Message);
                    }
                });
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.EXTERNALLIGHTLED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.EXTERNALLIGHTLED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(30000);
            }
        }

        /// <summary>
        /// Измеряет уровень воды в баке
        /// </summary>
        /// <returns></returns>
        private static async Task GetDistance()
        {
            await Task.Delay(10000);
            StartPage.AddItemToLogBox("Старт измерения уровня воды в баке...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.MEASUREDISTANCELED.Fill = mySolidColorBrush;
            });
            while (true)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var mre = new ManualResetEventSlim(false);
                        StartPage.TriggerPin.Write(GpioPinValue.High);
                        mre.Wait(TimeSpan.FromMilliseconds(0.01));
                        StartPage.TriggerPin.Write(GpioPinValue.Low);
                        var sw = new Stopwatch();
                        var dw = new Stopwatch();
                        dw.Start();
                        while (StartPage.EchoPin.Read() != GpioPinValue.High)
                        {
                            if (dw.ElapsedMilliseconds > 100) throw new Exception("Таймаут датчика расстояния!");
                        }
                        sw.Start();
                        while (StartPage.EchoPin.Read() == GpioPinValue.High)
                        {
                        }
                        sw.Stop();
                        var distance = sw.Elapsed.TotalSeconds * 17000;
                        //StartPageInstance.AddItemToLogBox("Distance = " + distance.ToString());
                        double sumrange = 0;
                        for (int q = 0; q < 10; q++)
                        {
                            sumrange += DistanceArray[q];
                        }
                        if (Math.Abs(sumrange / 10 - distance) < 10)
                        {
                            StartPage.SystemState.WaterLevelPercent = (int)((StartPage.CurrentDeviceSettings.TankHeigthcm - distance) * 100 / (StartPage.CurrentDeviceSettings.TankHeigthcm - 15));
                        }
                        if (StartPage.SystemState.WaterLevelPercent > 100)
                        {
                            StartPage.SystemState.WaterLevelPercent = 100;
                        }
                        if (StartPage.SystemState.WaterLevelPercent < 0)
                        {
                            StartPage.SystemState.WaterLevelPercent = 0;
                        }
                        if ((StartPage.SystemState.WaterLevelPercent >= 95) && (StartPage.SystemState.IsFillPumpSocketActive))
                        {
                            StartPage.FillPumpPin.Write(GpioPinValue.High);
                            StartPage.AddItemToLogBox("Уровень воды в баке >= 95%, розетка насоса бака отключена.");
                        }
                        for (int q = 0; q < 9; q++)
                        {
                            DistanceArray[q] = DistanceArray[q + 1];
                        }
                        DistanceArray[9] = distance;
                        if (StartPage.CurrentState == StartPage.States.ServiceMode)
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                try
                                {
                                    ServiceModePage.ServiceModePageInstance.waterlevellabel.Text = StartPage.SystemState.WaterLevelPercent.ToString();
                                }
                                catch
                                {

                                }
                            });
                        }
                    }
                    catch
                    {

                    }
                });
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.MEASUREDISTANCELED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.MEASUREDISTANCELED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(1000);
            }
        }

        /// <summary>
        /// Получает данные о параметрах окружающей среды
        /// </summary>
        /// <returns></returns>
        private static async Task GetAmbientVars()
        {
            await Task.Delay(12000);
            StartPage.AddItemToLogBox("Старт опроса датчиков температуры и влажности...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.AMBIENTVARSLED.Fill = mySolidColorBrush;
            });
            while (StartPage.I2cBusDevice == null)
            {
                await Task.Delay(100);
            }
            while (true)
            {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
                await Task.Run(async () =>
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
                {
                    try
                    {
                        byte[] ReceivedData = new byte[32];                       
                        StartPage.I2cBusDevice.Read(ReceivedData);
                        byte[] tmpaddr1 = new byte[8] { ReceivedData[0], ReceivedData[1], ReceivedData[2], ReceivedData[3], ReceivedData[4], ReceivedData[5], ReceivedData[6], ReceivedData[7] };
                        byte[] tmpaddr2 = new byte[8] { ReceivedData[12], ReceivedData[13], ReceivedData[14], ReceivedData[15], ReceivedData[16], ReceivedData[17], ReceivedData[18], ReceivedData[19] };
                        byte[] tmptemp1 = new byte[4] { ReceivedData[8], ReceivedData[9], ReceivedData[10], ReceivedData[11] };
                        byte[] tmptemp2 = new byte[4] { ReceivedData[20], ReceivedData[21], ReceivedData[22], ReceivedData[23] };
                        byte[] tmpambtemp = new byte[4] { ReceivedData[24], ReceivedData[25], ReceivedData[26], ReceivedData[27] };
                        byte[] tmpambhum = new byte[4] { ReceivedData[28], ReceivedData[29], ReceivedData[30], ReceivedData[31] };
                        //reverse arrays if source is not agree with LittleEndian
                        if (!BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(tmptemp1, 0, tmptemp1.Length);
                            Array.Reverse(tmptemp2, 0, tmptemp2.Length);
                            Array.Reverse(tmpambtemp, 0, tmpambtemp.Length);
                            Array.Reverse(tmpambhum, 0, tmpambhum.Length);
                        }
                        if (CompareArrays(tmpaddr1, StartPage.CurrentDeviceSettings.WaterTempSensorAddress))
                        {
                            StartPage.SystemState.WaterTempCelsius = BitConverter.ToSingle(tmptemp1, 0);
                            StartPage.SystemState.InboxTempCelsius = BitConverter.ToSingle(tmptemp2, 0);
                        }
                        if (CompareArrays(tmpaddr2, StartPage.CurrentDeviceSettings.WaterTempSensorAddress))
                        {
                            StartPage.SystemState.WaterTempCelsius = BitConverter.ToSingle(tmptemp2, 0);
                            StartPage.SystemState.InboxTempCelsius = BitConverter.ToSingle(tmptemp1, 0);
                        }
                        StartPage.SystemState.AmbientTempCelsius = BitConverter.ToSingle(tmpambtemp, 0);
                        StartPage.SystemState.AmbientRelativeHumidity = BitConverter.ToSingle(tmpambhum, 0);
                        ///we will make some 'hysteresis' magic to switch heater on and off
                        if ((StartPage.SystemState.AmbientTempCelsius <= GlobalVars.AmbientTempLowerTreshold) && (StartPage.SystemState.IsHeaterOn))
                        {
                            StartPage.HeaterPin.Write(GpioPinValue.Low);
                            StartPage.AddItemToLogBox(string.Format("Внешняя температура ниже предела {0}. Обогрев включен", GlobalVars.AmbientTempLowerTreshold));
                        }
                        if ((StartPage.SystemState.AmbientTempCelsius >= GlobalVars.AmbientTempHigherTreshold) && (StartPage.SystemState.IsHeaterOn))
                        {
                            StartPage.HeaterPin.Write(GpioPinValue.High);
                            StartPage.AddItemToLogBox(string.Format("Внешняя температура выше предела {0}. Обогрев отключен", GlobalVars.AmbientTempHigherTreshold));
                        }
                    }
                    catch
                    {

                    }
                });
                if (StartPage.CurrentState == StartPage.States.ServiceMode)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        ServiceModePage.ServiceModePageInstance.ambienthumlabel.Text = StartPage.SystemState.AmbientRelativeHumidity.ToString("N2");
                        ServiceModePage.ServiceModePageInstance.ambienttemplabel.Text = StartPage.SystemState.AmbientTempCelsius.ToString("N2");
                        ServiceModePage.ServiceModePageInstance.inboxtemplabel.Text = StartPage.SystemState.InboxTempCelsius.ToString("N2");
                        ServiceModePage.ServiceModePageInstance.watertemplabel.Text = StartPage.SystemState.WaterTempCelsius.ToString("N2");
                    });
                }
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.AMBIENTVARSLED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.AMBIENTVARSLED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(5000);
            }
        }
        
        /// <summary>
        /// Сравнивает два массива
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static bool CompareArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) { return false; }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Возвращает количество отработанного машинного времени
        /// </summary>
        /// <returns></returns>
        private static async Task<double> GetHoursCounter()
        {
            string hcfilename = GlobalVars.HardWareID + ".003";
            double tmphcvalue = 0;
            byte[] hcdata = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            StorageFile hcfile = null;
            try
            {
                hcfile = await ApplicationData.Current.LocalFolder.GetFileAsync(hcfilename);
            }
            catch
            {

            }
            if (hcfile == null)
            {
                hcfile = await ApplicationData.Current.LocalFolder.CreateFileAsync(hcfilename);
                //await FileIO.WriteBytesAsync(hcfile, hcdata);
                using (StorageStreamTransaction transaction = await hcfile.OpenTransactedWriteAsync())
                {
                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                    {
                        dataWriter.WriteBytes(hcdata);
                        transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                        await transaction.CommitAsync();
                    }
                }
            }
            try
            {
                IBuffer tmpbuf = await FileIO.ReadBufferAsync(hcfile);
                using (var dataReader = DataReader.FromBuffer(tmpbuf))
                {
                    dataReader.ReadBytes(hcdata);
                    tmphcvalue = BitConverter.ToDouble(hcdata, 0);
                }
            }
            catch
            {

            }
            return Math.Round(tmphcvalue, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Обновляет счетчик машинного времени
        /// </summary>
        /// <returns></returns>
        public static async Task HoursCounter()
        {
            await Task.Delay(5000);
            StartPage.AddItemToLogBox("Старт счетчика отработанного времени...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.HOURSCOUNTERLED.Fill = mySolidColorBrush;
            });
            bool firstloop = true;
            byte[] hcdata = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            while (true)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        Task<double> hcvaluetask = GetHoursCounter();
                        double hcvalue = await hcvaluetask;
                        StartPage.SystemState.TotalHoursWorked = hcvalue;
                        string hctext = hcvalue.ToString("N2").PadLeft(10, '0');
                        if (firstloop)
                        {
                            firstloop = false;
                        }
                        else
                        {
                            hcvalue += 0.05;
                            StartPage.SystemState.TotalHoursWorked = hcvalue;
                            hctext = hcvalue.ToString("N2").PadLeft(10, '0');
                            string hcfilename = GlobalVars.HardWareID + ".003";
                            StorageFile hcfile = await ApplicationData.Current.LocalFolder.GetFileAsync(hcfilename);
                            hcdata = BitConverter.GetBytes(StartPage.SystemState.TotalHoursWorked);
                            using (StorageStreamTransaction transaction = await hcfile.OpenTransactedWriteAsync())
                            {
                                using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                                {
                                    dataWriter.WriteBytes(hcdata);
                                    transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                                    await transaction.CommitAsync();
                                }
                            }
                        }
                        if (StartPage.CurrentState == StartPage.States.ServiceMode)
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                try
                                {
                                    ServiceModePage.ServiceModePageInstance.hourcounterlabel.Text = hctext;
                                    ServiceModePage.ServiceModePageInstance.watercounterlabel.Text = StartPage.SystemState.TotalLitersDIspensed.ToString("N2").PadLeft(10, '0');
                                }
                                catch
                                {

                                }
                            });
                        }
                    }
                    catch
                    {

                    }
                });
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.HOURSCOUNTERLED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.HOURSCOUNTERLED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(178000);
            }
        }

        /// <summary>
        /// Обновляет счетчики монет и купюр
        /// </summary>
        /// <param name="cbcount"></param>
        /// <param name="cbsum"></param>
        /// <param name="babillscount"></param>
        /// <param name="basum"></param>
        /// <param name="cc10"></param>
        /// <param name="cc5"></param>
        /// <param name="cc2"></param>
        /// <param name="cc1"></param>
        /// <returns></returns>
        public static async Task UpdateCashCounter(int cbcount, double cbsum, int babillscount, int basum, int cc10 = 0, int cc5 = 0, int cc2 = 0, int cc1 = 0)
        {
            StartPage.CashCounterAccessFlag.Wait();
            StartPage.SystemState.CC10RURCount = cc10;
            StartPage.SystemState.CC5RURCount = cc5;
            StartPage.SystemState.CC2RURCount = cc2;
            StartPage.SystemState.CC1RURCount = cc1;
            StartPage.SystemState.CCSum = StartPage.SystemState.CC10RURCount * 10 + StartPage.SystemState.CC5RURCount * 5 + StartPage.SystemState.CC2RURCount * 2 + StartPage.SystemState.CC1RURCount;
            byte[] ccdata = new byte[24];
            string wcfilename = GlobalVars.HardWareID + ".005";
            StorageFile wcfile = null;
            try
            {
                wcfile = await ApplicationData.Current.LocalFolder.GetFileAsync(wcfilename);
            }
            catch
            {

            }
            if (wcfile == null)
            {
                wcfile = await ApplicationData.Current.LocalFolder.CreateFileAsync(wcfilename);
                using (StorageStreamTransaction transaction = await wcfile.OpenTransactedWriteAsync())
                {
                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                    {
                        dataWriter.WriteBytes(ccdata);
                        transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                        await transaction.CommitAsync();
                    }
                }
            }
            byte[] tmpcbccount = new byte[4];
            byte[] tmpcbcsum = new byte[8];
            byte[] tmpbabillscount = new byte[4];
            byte[] tmpbasum = new byte[8];
            try
            {
                IBuffer tmpbuf = await FileIO.ReadBufferAsync(wcfile);
                using (var dataReader = DataReader.FromBuffer(tmpbuf))
                {
                    dataReader.ReadBytes(ccdata);
                    Array.Copy(ccdata, 0, tmpcbccount, 0, 4);
                    Array.Copy(ccdata, 4, tmpcbcsum, 0, 8);
                    Array.Copy(ccdata, 12, tmpbabillscount, 0, 4);
                    Array.Copy(ccdata, 16, tmpbasum, 0, 8);
                }
            }
            catch
            {

            }
            tmpcbccount = BitConverter.GetBytes(BitConverter.ToInt32(tmpcbccount, 0) + cbcount);
            tmpcbcsum = BitConverter.GetBytes(BitConverter.ToDouble(tmpcbcsum, 0) + cbsum);
            tmpbabillscount = BitConverter.GetBytes(BitConverter.ToInt32(tmpbabillscount, 0) + babillscount);
            tmpbasum = BitConverter.GetBytes(BitConverter.ToDouble(tmpbasum, 0) + basum);
            StartPage.SystemState.CBCount = BitConverter.ToInt32(tmpcbccount, 0);
            StartPage.SystemState.CBSum = BitConverter.ToDouble(tmpcbcsum, 0);
            StartPage.SystemState.BABillsCount = BitConverter.ToInt32(tmpbabillscount, 0);
            StartPage.SystemState.BASum = BitConverter.ToDouble(tmpbasum, 0);
            StartPage.SystemState.IncassoSum = StartPage.SystemState.CBSum + StartPage.SystemState.BASum;
            byte[] updatedccdata = new byte[24];
            Array.Copy(tmpcbccount, 0, updatedccdata, 0, 4);
            Array.Copy(tmpcbcsum, 0, updatedccdata, 4, 8);
            Array.Copy(tmpbabillscount, 0, updatedccdata, 12, 4);
            Array.Copy(tmpbasum, 0, updatedccdata, 16, 8);
            using (StorageStreamTransaction transaction = await wcfile.OpenTransactedWriteAsync())
            {
                using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                {
                    dataWriter.WriteBytes(updatedccdata);
                    transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                    await transaction.CommitAsync();
                }
            }
            //File.Delete(wcfile.Path);
            StartPage.CashCounterAccessFlag.Release();
        }

        /// <summary>
        /// Обновляет счетчик отгруженной водички
        /// </summary>
        /// <param name="dispensedliters"></param>
        /// <returns></returns>
        public static async void UpdateWaterCounter(double dispensedliters)
        {
            double tmpdisp = Math.Round(dispensedliters, 2, MidpointRounding.AwayFromZero);
            string wcfilename = GlobalVars.HardWareID + ".004";
            StorageFile wcfile = null;
            byte[] wcdata = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            try
            {
                wcfile = await ApplicationData.Current.LocalFolder.GetFileAsync(wcfilename);
            }
            catch
            {

            }
            if (wcfile == null)
            {
                wcfile = await ApplicationData.Current.LocalFolder.CreateFileAsync(wcfilename);
                using (StorageStreamTransaction transaction = await wcfile.OpenTransactedWriteAsync())
                {
                    using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                    {
                        dataWriter.WriteBytes(wcdata);
                        transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                        await transaction.CommitAsync();
                    }
                }

            }
            IBuffer tmpbuf = await FileIO.ReadBufferAsync(wcfile);
            using (var dataReader = DataReader.FromBuffer(tmpbuf))
            {
                dataReader.ReadBytes(wcdata);
            }
            try
            {
                tmpdisp += BitConverter.ToDouble(wcdata, 0);
            }
            catch
            {

            }
            byte[] tmpdispbytes = BitConverter.GetBytes(tmpdisp);
            StartPage.SystemState.TotalLitersDIspensed = tmpdisp;
            //await FileIO.WriteBytesAsync(wcfile, tmpdispbytes);
            using (StorageStreamTransaction transaction = await wcfile.OpenTransactedWriteAsync())
            {
                using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                {
                    dataWriter.WriteBytes(tmpdispbytes);
                    transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                    await transaction.CommitAsync();
                }
            }
        }

        /// <summary>
        /// Выводит данные о состоянии счетчиков денег
        /// </summary>
        /// <returns></returns>
        private static async Task CashCounter()
        {
            StartPage.AddItemToLogBox("Старт отслеживания счетчиков наличных...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.CASHCOUNTERLED.Fill = mySolidColorBrush;
            });
            while (true)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        if (StartPage.CurrentState == StartPage.States.ServiceMode)
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                try
                                {
                                    ServiceModePage.ServiceModePageInstance.cc10rurlabel.Text = StartPage.SystemState.CC10RURCount.ToString().PadLeft(3, '0');
                                    ServiceModePage.ServiceModePageInstance.cc5rurlabel.Text = StartPage.SystemState.CC5RURCount.ToString().PadLeft(3, '0');
                                    ServiceModePage.ServiceModePageInstance.cc2rurlabel.Text = StartPage.SystemState.CC2RURCount.ToString().PadLeft(3, '0');
                                    ServiceModePage.ServiceModePageInstance.cc1rurlabel.Text = StartPage.SystemState.CC1RURCount.ToString().PadLeft(3, '0');
                                    ServiceModePage.ServiceModePageInstance.ccsumlabel.Text = StartPage.SystemState.CCSum.ToString("N2");
                                    ServiceModePage.ServiceModePageInstance.babillslabel.Text = StartPage.SystemState.BABillsCount.ToString().PadLeft(3, '0');
                                    ServiceModePage.ServiceModePageInstance.basumlabel.Text = StartPage.SystemState.BASum.ToString("N2");
                                    ServiceModePage.ServiceModePageInstance.cc10rurfillprogress.Value = StartPage.SystemState.CC10RURCount;
                                    ServiceModePage.ServiceModePageInstance.cc5rurfillprogress.Value = StartPage.SystemState.CC5RURCount;
                                    ServiceModePage.ServiceModePageInstance.cc2rurfillprogress.Value = StartPage.SystemState.CC2RURCount;
                                    ServiceModePage.ServiceModePageInstance.cc1rurfillprogress.Value = StartPage.SystemState.CC1RURCount;
                                    ServiceModePage.ServiceModePageInstance.cc1rurfillprogress.Value = StartPage.SystemState.CC1RURCount;
                                    ServiceModePage.ServiceModePageInstance.babillsprogress.Value = StartPage.SystemState.BABillsCount;
                                    ServiceModePage.ServiceModePageInstance.cbclabel.Text = StartPage.SystemState.CBCount.ToString().PadLeft(4, '0');
                                    ServiceModePage.ServiceModePageInstance.cbsumlabel.Text = StartPage.SystemState.CBSum.ToString("N2");
                                    ServiceModePage.ServiceModePageInstance.incassolabel.Text = (StartPage.SystemState.IncassoSum).ToString("N2");
                                }
                                catch
                                {

                                }
                            });
                        }
                    }
                    catch
                    {

                    }
                });
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.CASHCOUNTERLED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.CASHCOUNTERLED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(10000);
            }
        }

        /// <summary>
        /// Отслеживает состояние силовой нагрузки
        /// </summary>
        /// <returns></returns>
        private static async Task PowerSocketsWatcher()
        {
            await Task.Delay(9000);
            StartPage.AddItemToLogBox("Старт наблюдения за дискретными выходами...");
            StartPage.UpdateProgress(5);
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                SolidColorBrush mySolidColorBrush = new SolidColorBrush
                {
                    Color = Colors.Green
                };
                StartPage.StartPageInstance.POWERSOCKETSWATCHERLED.Fill = mySolidColorBrush;
            });
            while (true)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        StartPage.SystemState.IsHeaterOn = (StartPage.HeaterPin.Read() == GpioPinValue.Low);
                        StartPage.SystemState.IsExternalLightOn = (StartPage.ExternalLightPin.Read() == GpioPinValue.Low);
                        StartPage.SystemState.IsFillPumpSocketActive = (StartPage.FillPumpPin.Read() == GpioPinValue.Low);
                        if (StartPage.CurrentState == StartPage.States.ServiceMode)
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                try
                                {
                                    ServiceModePage.ServiceModePageInstance.heaterswitch.IsOn = (StartPage.SystemState.IsHeaterOn);
                                    ServiceModePage.ServiceModePageInstance.fillpumpswitch.IsOn = (StartPage.SystemState.IsFillPumpSocketActive);
                                    ServiceModePage.ServiceModePageInstance.externallightswitch.IsOn = (StartPage.SystemState.IsExternalLightOn);
                                }
                                catch
                                {

                                }
                            });
                        }
                    }
                    catch
                    {

                    }
                });
                if (StartPage.CurrentState == StartPage.States.Init)
                {
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        SolidColorBrush mySolidColorBrush = new SolidColorBrush
                        {
                            Color = Colors.LightGreen
                        };
                        StartPage.StartPageInstance.POWERSOCKETSWATCHERLED.Fill = mySolidColorBrush;
                        await Task.Delay(100);
                        mySolidColorBrush.Color = Colors.Green;
                        StartPage.StartPageInstance.POWERSOCKETSWATCHERLED.Fill = mySolidColorBrush;
                    });
                }
                await Task.Delay(1000);
            }
        }
    }
}
