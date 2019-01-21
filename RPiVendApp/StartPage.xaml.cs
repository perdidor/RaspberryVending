using System;
using Windows.Storage.Streams;
using Windows.Devices.Gpio;
using System.Collections.Generic;
using System.IO;
using Windows.UI;
using Windows.UI.Xaml;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using System.Threading;
using Windows.Web.Http;
using Windows.Storage;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;




namespace RPiVendApp
{

    public class WaterDeviceIncasso
    {
        public long ID;
        public long WaterDeviceID;
        public long IncassoDatetime;
        public string IncassoDatetimeStr;
        public int IncassoCashboxCoins;
        public double IncassoCashboxSum;
        public int IncassoBABills;
        public double IncassoBASum;
        public long CommandID;
    }
    /// <summary>
    /// При сериализации в xml переопределяем кодировку по умолчанию, чтобы не было проблем с русскими буквами.
    /// </summary>
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }

    /// <summary>
    /// Окно по умолчанию, загружается и показывается первым при старте.
    /// </summary>
    public sealed partial class StartPage : Page
    {
        /// <summary>
        /// Текущий сеанс продажи
        /// </summary>
        public static WaterSales CurrentSaleSession;

        public void AddWaterSales(WaterSales SaleInstance)
        {
            
        }
        /// <summary>
        /// Статусы машины состояний
        /// </summary>
        public enum States
        {
            /// <summary>
            /// Инициализация. Светодиод состояния быстро моргает.
            /// </summary>
            Init,                   //0
            /// <summary>
            /// Основной режим. Светодиод состояния горит зеленым, раз в две секунды моргает.
            /// </summary>
            ReadyToServe,           //1
            /// <summary>
            /// Состояние активного сеанса продажи при положительном остатке денег клиента. Светодиод состояния горит зеленым, раз в две секунды моргает.
            /// </summary>
            ReadyToDispenseWater,   //2
            /// <summary>
            /// Выдача сдачи. Светодиод состояния горит зеленым, раз в две секунды моргает.
            /// </summary>
            DispenseChange,         //3
            /// <summary>
            /// Не обслуживает. Светодиод состояния моргает трижды с паузой на 2 сек
            /// </summary>
            OutOfService,           //4
            /// <summary>
            /// Служебный режим. Светодиод состояния моргает дважды с паузой на 2 сек
            /// </summary>
            ServiceMode             //5
        }

        /// <summary>
        /// Выполняет обратное преобразование в объект заданного типа из xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static T Deserialize<T>(string xml)
        {
            var xs = new XmlSerializer(typeof(T));
            return (T)xs.Deserialize(new StringReader(xml));
        }

        public static GpioController gpio = null;
        public static I2cDevice I2cBusDevice = null;
        public static GpioPin TriggerPin = null;
        public static GpioPin EchoPin = null;
        public static GpioPin WaterFlowSensorPin = null;
        public static GpioPin StartLEDPin = null;
        public static GpioPin StopLEDPin = null;
        public static GpioPin EndLEDPin = null;
        public static GpioPin StartButtonPin = null;
        public static GpioPin StopButtonPin = null;
        public static GpioPin EndButtonPin = null;
        public static GpioPin PumpPin = null;
        public static GpioPin HeaterPin = null;
        public static GpioPin FillPumpPin = null;
        public static GpioPin WaterValvePin = null;
        public static GpioPin MDBPin = null;
        public static GpioPin HeartBeatPin = null;
        public static GpioPin ExternalLightPin = null;
        //private static GpioPin ReservedPin1 = null;

        //private static bool powerup = false;
        public static bool ccinit = false;
        public static bool bainit = false;
        public static bool bareset = false;
        public static bool ccreset = false;

        public static StartPage StartPageInstance = null;

        public static long ClientID = 0;

        public static CancellationTokenSource GlobalCancellationTokenSource;

        public static int WaterFlowPulseCounter = 0;

        public static WaterDeviceTelemetry SystemState;

        public static States CurrentState = States.Init;

        /// <summary>
        /// внутренняя переменная для хранения остатка денег покупателя
        /// </summary>
        private static double _sessiondeposit = 0.00;
        private static SemaphoreSlim UserDepositSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Текущий остаток денег пользователя
        /// </summary>
        public static double UserDeposit
        {
            get
            {
                return Math.Round(_sessiondeposit, 2);
            }
            set
            {
                UserDepositSemaphore.Wait();
                double a = value - _sessiondeposit;
                _sessiondeposit = Math.Round(value, 2, MidpointRounding.AwayFromZero);
                if (_sessiondeposit < 0)
                {
                    _sessiondeposit = 0;
                }
                switch (CurrentState)
                {
                    case States.DispenseChange:
                    {
    #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                ChangePage.ChangePageInstance.dispenselabel.Text = ((int)_sessiondeposit).ToString().PadLeft(3, '0');
                            }
                            catch
                            {

                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            break;
                    }
                    //Uncomment following {} will reduce GUI respondability during water dispensing
                    //if ((CurrentState == States.ReadyToDispenseWater) || (CurrentState == States.ReadyToServe))
                    //{
                    //    if ((_sessiondeposit >= GlobalVars.MaxUserDepositTreshold))
                    //    {
                    //        MDB.DisableAcceptBills();
                    //        MDB.DisableAcceptCoins();
                    //    }
                    //    else
                    //    {
                    //        MDB.EnableAcceptBills();
                    //        MDB.EnableAcceptCoins();
                    //    }
                    //}
                    case States.ReadyToDispenseWater:
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                if (CurrentState == States.ReadyToDispenseWater)
                                {
                                    if (a > 0)
                                    {
                                        CurrentSaleSession.UserCash += a;
                                    }
                                    ReadyToStartPage.ReadyToStartPageInstance.depositlabel.Text = Math.Round(_sessiondeposit, 2).ToString("N2").PadLeft(6, '0');
                                    ReadyToStartPage.ReadyToStartPageInstance.waterleftlabel.Text = (Math.Round(_sessiondeposit, 2) / (CurrentDeviceSettings.PRICE_PER_ITEM_MDE * 0.01)).ToString("N2").PadLeft(5, '0');
                                }
                            }
                            catch
                            {

                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        break;
                    }
                    case States.ServiceMode:
                    {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            try
                            {
                                if (CurrentState == States.ReadyToDispenseWater)
                                {
                                    if (a > 0)
                                    {
                                        CurrentSaleSession.UserCash += a;
                                    }
                                    ReadyToStartPage.ReadyToStartPageInstance.depositlabel.Text = Math.Round(_sessiondeposit, 2).ToString("N2").PadLeft(6, '0');
                                    ReadyToStartPage.ReadyToStartPageInstance.waterleftlabel.Text = (Math.Round(_sessiondeposit, 2) / (CurrentDeviceSettings.PRICE_PER_ITEM_MDE * 0.01)).ToString("N2").PadLeft(5, '0');
                                }
                            }
                            catch
                            {

                            }
                        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        break;
                    }
                    case States.ReadyToServe:
                    {
                            if (_sessiondeposit > 0)
                            {
                                CurrentSaleSession = new WaterSales();
                                CurrentSaleSession.UserCash = Math.Round(_sessiondeposit, 2);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    try
                                    {
                                        CurrentState = States.ReadyToDispenseWater;
                                        AddItemToLogBox("Сеанс продажи начат по инициативе покупателя: внесены наличные средства");
                                        StartLEDPin.Write(GpioPinValue.Low);
                                        EndLEDPin.Write(GpioPinValue.Low);
                                        var frame = Window.Current.Content as Frame;
                                        frame.Navigate(typeof(ReadyToStartPage));
                                        while (ReadyToStartPage.ReadyToStartPageInstance == null) Task.Delay(1000).Wait();
                                        ReadyToStartPage.ReadyToStartPageInstance.depositlabel.Text = _sessiondeposit.ToString("N2").PadLeft(6, '0');
                                        ReadyToStartPage.ReadyToStartPageInstance.waterleftlabel.Text = (_sessiondeposit / (CurrentDeviceSettings.PRICE_PER_ITEM_MDE * 0.01)).ToString("N2").PadLeft(5, '0');
                                    }
                                    catch
                                    {

                                    }
                                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            }
                        break;
                    }
                }
                UserDepositSemaphore.Release();
            }
        }



        /// <summary>
        /// Программа стартовала, первое открытие формы
        /// </summary>
        public StartPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            GlobalCancellationTokenSource = new CancellationTokenSource();
            StartPageInstance = this;
            SystemState = new WaterDeviceTelemetry();
            SystemState.ProgramVersion = typeof(App).GetTypeInfo().Assembly.GetName().Version.ToString(4);
            SystemState.WaterDeviceID = GlobalVars.RegID;
            InitGPIO();
            Task.Run(HeartBeatLEDTask, GlobalCancellationTokenSource.Token);
            if (ReadStartupSettings())
            {
                Task.Run(() => { RunAfterInit(); });
            }
            else
            {
                AddItemToLogBox("Не настроены индивидуальные параметры устройства.");
                AddItemToLogBox("Необходимо закончить настройку устройства в панели управения на сайте. Устройство продолжит работу автоматически после получения настроек с сервера.");
                StartIfNewCurrentReceiptSettings = true;
            }
            Task.Run(GetReceiptSettings, GlobalCancellationTokenSource.Token);
        }

        /// <summary>
        /// определяет, стартовать ли после получения настроек
        /// </summary>
        private bool StartIfNewCurrentReceiptSettings = false;

        /// <summary>
        /// Второй этап запуска - стартуем асинхронные задачи
        /// </summary>
        /// <returns></returns>
        private void RunAfterInit()
        {
            if (CurrentDeviceSettings.UseKKT) CashDeskHelper.CashDesk_Init();
            MDB.ConnectMDBSerialPort(CashDeskDeviceID);
            I2c_Init();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServiceTasks.UpdateWaterCounter(0);
            ServiceTasks.UpdateCashCounter(0, 0, 0, 0);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            AddItemToLogBox("Управляющее устройство успешно инициализировано, продолжаем...");
            int initdelaymillisec = 10000;//подождем немного, винде нужно время, чтобы выделить память, отсвопиться своим гавном
                                           //на SD-карту и успокоиться.
            while (initdelaymillisec > 0)
            {
                AddItemToLogBox("Ожидание запуска задач " + (initdelaymillisec / 1000).ToString() + "с...");
                int count = 0;
                while (count < 100)
                {
                    Task.Delay(100).Wait();
                    count++;
                }
                initdelaymillisec -= 10000;
            }
            //Task.Run(CCC, GlobalCancellationTokenSource.Token);
            ServiceTasks.StartAll();
            AddItemToLogBox("Задачи запущены");
            Task.Run(MDBHelper.Init_MDB);
        }


        /// <summary>
        /// Набор настроек для взаимодействия с ККТ. Обновляются по команде оператора вручную.
        /// </summary>
        public class DeviceSettings
        {
            public double Latitude = 0;
            public double Longitude = 0;
            /// <summary>
            /// Высота бака для воды (высота установки датчика уровня)
            /// </summary>
            public int TankHeigthcm = 0;

            public string ProductName = "Вода питьевая";
            /// <summary>
            /// Номер телефона для общения с восторженными покупателями. Будем печатать на чеках.
            /// </summary>
            public string CustomerServiceContactPhone = "";
            /// <summary>
            /// Цена за литр (цена единицы товара в МДЕ - минимальная денежная единица в РФ = 1 копейка)
            /// </summary>
            public int PRICE_PER_ITEM_MDE = 0;
            /// <summary>
            /// используемая система налогообложения (см. документацию к ККТ)
            /// </summary>
            public int TaxSystem = 0;
            public long SettingsVersion = 0;
            public bool UseKKT = false;
            public byte[] WaterTempSensorAddress = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
        }

        public static DeviceSettings CurrentDeviceSettings;

        private bool ReadStartupSettings()
        {
            bool res = true;
            try
            {
                string ssfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".012";
                CurrentDeviceSettings = (Deserialize<DeviceSettings>(File.ReadAllText(ssfilename)));
            }
            catch
            {
                res = false;
                CurrentDeviceSettings = new DeviceSettings();
            }
            return res;
        }

        /// <summary>
        /// Получает индивидуальные настройки устройства
        /// </summary>
        private async Task GetReceiptSettings()
        {
            AddItemToLogBox("Старт получения индивидуальных настроек...");
            while (true)
            {
                    try
                    {
                        while (CurrentState == States.DispenseChange || CurrentState == States.ReadyToDispenseWater)
                        {
                            await Task.Delay(10000);
                        }
                        ClientRequest tmpreq = RequestEncoder.EncodeRequestData(GlobalVars.RegID);
                        Dictionary<string, string> data = new Dictionary<string, string>
                        {
                            { "Request", tmpreq.Request },
                            { "Signature", tmpreq.Signature },
                            { "AData", tmpreq.AData },
                            { "BData", tmpreq.BData }
                        };
                        HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(data);
                        HttpClient httpClient = NewWebClient();
                        Uri requestUri = new Uri(GlobalVars.SERVER_ENDPOINT + GlobalVars.GET_DEVICE_SETTINGS_PATH);
                        HttpResponseMessage httpResponse = new HttpResponseMessage();
                        httpResponse = await httpClient.PostAsync(requestUri, content);
                        httpResponse.EnsureSuccessStatusCode();
                        string tmpres = await httpResponse.Content.ReadAsStringAsync();
                        DeviceSettings tmp = (Deserialize<DeviceSettings>(tmpres));
                        if (tmp.SettingsVersion != CurrentDeviceSettings.SettingsVersion)
                        {
                            CurrentDeviceSettings = tmp;
                            var xs = new XmlSerializer(tmp.GetType());
                            var xml = new Utf8StringWriter();
                            xs.Serialize(xml, tmp);
                            string ssfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".012";
                            File.WriteAllText(ssfilename, xml.ToString());
                            AddItemToLogBox("Новые настройки получены");
                            if (StartIfNewCurrentReceiptSettings)
                            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                Task.Run(() => { RunAfterInit(); });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                StartIfNewCurrentReceiptSettings = false;
                            }
                        }
                    }
                    catch
                    {

                    }
                await Task.Delay(60000);
            }
        }

        /// <summary>
        /// Инициализирует логические входы и выходы общего назначения
        /// </summary>
        private void InitGPIO()
        {
            UpdateStartLEDs(GPIOLED, Colors.Yellow);
            try
            {
                gpio = GpioController.GetDefault();

                TriggerPin = gpio.OpenPin(GlobalVars.TriggerPin);
                EchoPin = gpio.OpenPin(GlobalVars.EchoPin);
                EchoPin.SetDriveMode(GpioPinDriveMode.InputPullDown);
                TriggerPin.SetDriveMode(GpioPinDriveMode.Output);
                TriggerPin.Write(GpioPinValue.Low);

                WaterFlowSensorPin = gpio.OpenPin(GlobalVars.WaterFlowSensorPin);
                WaterFlowSensorPin.SetDriveMode(GpioPinDriveMode.InputPullUp);
                WaterFlowSensorPin.ValueChanged += WaterFlowCounter.WaterFlowSensorPin_ValueChanged;

                StartLEDPin = gpio.OpenPin(GlobalVars.StartLEDPin);
                StopLEDPin = gpio.OpenPin(GlobalVars.StopLEDPin);
                EndLEDPin = gpio.OpenPin(GlobalVars.EndLEDPin);
                StartLEDPin.SetDriveMode(GpioPinDriveMode.Output);
                StopLEDPin.SetDriveMode(GpioPinDriveMode.Output);
                EndLEDPin.SetDriveMode(GpioPinDriveMode.Output);
                StartLEDPin.Write(GpioPinValue.High);
                StopLEDPin.Write(GpioPinValue.High);
                EndLEDPin.Write(GpioPinValue.High);

                StartButtonPin = gpio.OpenPin(GlobalVars.StartButtonPin);
                StopButtonPin = gpio.OpenPin(GlobalVars.StopButtonPin);
                EndButtonPin = gpio.OpenPin(GlobalVars.EndButtonPin);
                StartButtonPin.SetDriveMode(GpioPinDriveMode.InputPullDown);
                StopButtonPin.SetDriveMode(GpioPinDriveMode.InputPullDown);
                EndButtonPin.SetDriveMode(GpioPinDriveMode.InputPullDown);
                StartButtonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                StopButtonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                EndButtonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
                StartButtonPin.ValueChanged += StartButtonPin_ValueChanged;
                StopButtonPin.ValueChanged += StopButtonPin_ValueChanged;
                EndButtonPin.ValueChanged += EndButtonPin_ValueChanged;

                PumpPin = gpio.OpenPin(GlobalVars.PumpPin);
                HeaterPin = gpio.OpenPin(GlobalVars.HeaterPin);
                FillPumpPin = gpio.OpenPin(GlobalVars.FillPumpPin);
                WaterValvePin = gpio.OpenPin(GlobalVars.WaterValvePin);
                MDBPin = gpio.OpenPin(GlobalVars.MDBPin);
                HeartBeatPin = gpio.OpenPin(GlobalVars.HeartBeatPin);
                ExternalLightPin = gpio.OpenPin(GlobalVars.ExternalLightPin);
                //ReservedPin1 = gpio.OpenPin(GlobalVars.ReservedPin1);
                //ReservedPin2 = gpio.OpenPin(GlobalVars.ReservedPin2);
                PumpPin.SetDriveMode(GpioPinDriveMode.Output);
                HeaterPin.SetDriveMode(GpioPinDriveMode.Output);
                FillPumpPin.SetDriveMode(GpioPinDriveMode.Output);
                WaterValvePin.SetDriveMode(GpioPinDriveMode.Output);
                MDBPin.SetDriveMode(GpioPinDriveMode.Output);
                //ReservedPin1.SetDriveMode(GpioPinDriveMode.Output);
                //ReservedPin2.SetDriveMode(GpioPinDriveMode.Output);
                ExternalLightPin.SetDriveMode(GpioPinDriveMode.Output);
                HeartBeatPin.SetDriveMode(GpioPinDriveMode.Output);
                HeartBeatPin.Write(GpioPinValue.High);
                UpdateStartLEDs(GPIOLED, Colors.Green);
                AddItemToLogBox("Настройка контроллера GPIO завершена");
                UpdateProgress(10);
            }
            catch (Exception ex)
            {
                UpdateStartLEDs(GPIOLED, Colors.Red);
                AddItemToLogBox("Ошибка контроллера GPIO: " + ex.Message);
                gpio = null;
            }
        }

        /// <summary>
        /// Нажата кнопка Сдача
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void EndButtonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                try
                {
                    switch (CurrentState)
                    {
                        case States.ReadyToDispenseWater:
                            {
                                CurrentState = States.DispenseChange;
                                MDB.DisableCashDevices();
                                WaterValvePin.Write(GpioPinValue.High);
                                PumpPin.Write(GpioPinValue.High);
                                StartLEDPin.Write(GpioPinValue.High);
                                StopLEDPin.Write(GpioPinValue.High);
                                EndLEDPin.Write(GpioPinValue.High);
                                ReadyToStartPage.ChangeWaterValveStatus(false);
                                AddItemToLogBox("Сеанс продажи закончен по инициативе покупателя (нажата кнопка \"Конец\"), остаток " + UserDeposit.ToString("N2") + ", выдаем сдачу в размере " + Math.Round(UserDeposit, MidpointRounding.AwayFromZero).ToString("N2"));
                                AddItemToLogBox("Выдача сдачи");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    var frame2 = Window.Current.Content as Frame;
                                    frame2.Navigate(typeof(ChangePage));
                                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                while (ChangePage.ChangePageInstance == null) Task.Delay(100).Wait();
                                break;
                            }
                        case States.ServiceMode:
                            {
                                WaterValvePin.Write(GpioPinValue.High);
                                PumpPin.Write(GpioPinValue.High);
                                StartLEDPin.Write(GpioPinValue.High);
                                StopLEDPin.Write(GpioPinValue.High);
                                EndLEDPin.Write(GpioPinValue.High);
                                ReadyToStartPage.ChangeWaterValveStatus(false);
                                string oosfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".031";
                                if (File.Exists(oosfilename))
                                {
                                    CurrentState = States.OutOfService;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        var frame = Window.Current.Content as Frame;
                                        frame.Navigate(typeof(OutOfServicePage));
                                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                    AddItemToLogBox("Временно не обслуживает");
                                    return;
                                }
                                CurrentState = States.ReadyToServe;
                                MDB.EnableCashDevices();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    var frame = Window.Current.Content as Frame;
                                    frame.Navigate(typeof(MainPage));
                                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                                while (MainPage.MainPageInstance == null) Task.Delay(100).Wait();
                                AddItemToLogBox("Выход из служебного режима по инициативе оператора (нажата кнопка \"Конец\")");
                                break;
                            }
                        default:
                            {
                                AddItemToLogBox("Кнопка \"Конец\" нажата вне контекста продажи или служебного режима, действие не требуется.");
                                break;
                            }
                    }
                }
                catch
                {

                }
                finally
                {

                }
            }
        }

        /// <summary>
        /// Нажата кнопка Стоп
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StopButtonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                try
                {
                    switch (CurrentState)
                    {
                        case States.ReadyToDispenseWater:
                            {
                                WaterValvePin.Write(GpioPinValue.High);
                                PumpPin.Write(GpioPinValue.High);
                                StopLEDPin.Write(GpioPinValue.High);
                                StartLEDPin.Write(GpioPinValue.Low);
                                ReadyToStartPage.ChangeWaterValveStatus(false);
                                AddItemToLogBox("Кнопка \"Стоп\" нажата в контексте продажи, кран закрыт, насос выключен");
                                break;
                            }
                        case States.ServiceMode:
                            {
                                WaterValvePin.Write(GpioPinValue.High);
                                PumpPin.Write(GpioPinValue.High);
                                StopLEDPin.Write(GpioPinValue.High);
                                StartLEDPin.Write(GpioPinValue.Low);
                                ReadyToStartPage.ChangeWaterValveStatus(false);
                                AddItemToLogBox("Кнопка \"Стоп\" нажата в контексте служебного режима, кран закрыт, насос выключен");
                                break;
                            }
                        default:
                            {
                                AddItemToLogBox("Кнопка \"Стоп\" нажата вне контекста продажи или служебного режима, действий не требуется");
                                break;
                            }
                    }
                }
                catch
                {

                }
                finally
                {

                }
            }
        }

        /// <summary>
        /// нажата кнопка Старт
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StartButtonPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            Mutex GPIOButtonsAccessMutex = new Mutex(true, "GPIOButtonsAccessMutex", out bool mutexcreated);
            if (!mutexcreated)
            {
                try
                {
                    GPIOButtonsAccessMutex.WaitOne();
                }
                catch
                {

                }
            }
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                try
                {
                    switch (CurrentState)
                    {
                        case States.ReadyToDispenseWater:
                            {
                                WaterValvePin.Write(GpioPinValue.Low);
                                PumpPin.Write(GpioPinValue.Low);
                                StartLEDPin.Write(GpioPinValue.High);
                                StopLEDPin.Write(GpioPinValue.Low);
                                ReadyToStartPage.ChangeWaterValveStatus(true);
                                AddItemToLogBox("Кнопка \"Старт\" нажата в контексте продажи, кран открыт, насос включен");
                                break;
                            }
                        case States.ServiceMode:
                            {
                                WaterValvePin.Write(GpioPinValue.Low);
                                PumpPin.Write(GpioPinValue.Low);
                                StartLEDPin.Write(GpioPinValue.High);
                                StopLEDPin.Write(GpioPinValue.Low);
                                ReadyToStartPage.ChangeWaterValveStatus(true);
                                AddItemToLogBox("Кнопка \"Старт\" нажата в контексте служебного режима, кран открыт, насос включен");
                                break;
                            }
                        default:
                            {
                                AddItemToLogBox("Кнопка \"Старт\" нажата вне контекста продажи или служебного режима, действий не требуется");
                                break;
                            }
                    }
                }
                catch
                {

                }
                finally
                {

                }
            }
            GPIOButtonsAccessMutex.ReleaseMutex();
            GPIOButtonsAccessMutex.Dispose();
        }

        /// <summary>
        /// Инициализируем шину I2C
        /// </summary>
        private async void I2c_Init()
        {
            try
            {
                UpdateStartLEDs(I2CLED, Colors.Yellow);
                AddItemToLogBox("Поиск контроллера I2C...");
                string AQS = I2cDevice.GetDeviceSelector("I2C1");
                DeviceInformationCollection DIS = await DeviceInformation.FindAllAsync(AQS);
                var Settings = new I2cConnectionSettings(GlobalVars.I2cSlaveAddress);
                I2cBusDevice = await I2cDevice.FromIdAsync(DIS[0].Id, Settings);
                AddItemToLogBox("Найден контроллер шины I2C: " + I2cBusDevice.DeviceId);
                UpdateProgress(10);
                UpdateStartLEDs(I2CLED, Colors.Green);
            }
            catch
            {
                UpdateStartLEDs(I2CLED, Colors.Red);
            }
        }

        public static string CashDeskDeviceID = "";

        /// <summary>
        /// Добавляет строку в лог. Если длина строки больше предела символов, она выводится по частям.
        /// </summary>
        /// <param name="logstr"></param>
        public static void AddItemToLogBox(string logstr)
        {
            if (CurrentState == States.Init)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Mutex m = new Mutex(true, "AddLogItemsMutex", out bool mutexWasCreated);
                    if (!mutexWasCreated)
                    {
                        try
                        {
                            m.WaitOne();
                        }
                        catch (AbandonedMutexException)
                        {

                        }
                    }
                    try
                    {
                    string sstr = "";
                    int startpos = 0;
                    int LengthLimit = 75;
                    if (logstr.Length <= LengthLimit) sstr = logstr.Substring(startpos, logstr.Length); else sstr = logstr.Substring(startpos, LengthLimit);
                    startpos += sstr.Length;
                    LengthLimit = 93;
                    string LogString = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + " * " + sstr;
                    while (startpos < logstr.Length || sstr.Length > 0)
                    {
                        StartPageInstance.LogBox.Items.Add(LogString);
                        if (logstr.Length - startpos <= LengthLimit) sstr = logstr.Substring(startpos); else sstr = logstr.Substring(startpos, LengthLimit);
                        startpos += sstr.Length;
                        LogString = string.Concat(">>> ", sstr);
                        Task.Delay(10).Wait();
                    }
                    StartPageInstance.LogBox.ScrollIntoView(StartPageInstance.LogBox.Items[StartPageInstance.LogBox.Items.Count - 1]);
                    }
                    catch
                    {

                    }
                    finally
                    {
                        m.ReleaseMutex();
                        m.Dispose();
                    }
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void But1_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ServiceModePage));
        }

        /// <summary>
        /// управляет светодиодом индикации состояния
        /// </summary>
        /// <returns></returns>
        private async Task HeartBeatLEDTask()
        {
            AddItemToLogBox("Старт управления светодиодом состояния...");
            while (true)
            {
                switch (CurrentState)
                {
                    case States.Init:
                        {
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(100);
                            break;
                        }
                    case States.ReadyToServe:
                        {
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(2000);
                            break;
                        }
                    case States.DispenseChange:
                        {
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(2000);
                            break;
                        }
                    case States.ReadyToDispenseWater:
                        {
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(2000);
                            break;
                        }
                    case States.ServiceMode:
                        {
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(2000);
                            break;
                        }
                    case States.OutOfService:
                        {
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.Low);
                            await Task.Delay(100);
                            HeartBeatPin.Write(GpioPinValue.High);
                            await Task.Delay(2000);
                            break;
                        }
                    default:
                        {
                            await Task.Delay(10);
                            break;
                        }
                }
            }
        }

        

        public HttpClient NewWebClient()
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "RPiVendApp/" + SystemState.ProgramVersion);
            return httpClient;
        }

        /// <summary>
        /// Инкассация - изымаем купюры и монеты из кэшбокса, обнуляем счетчики
        /// </summary>
        public async void MakeIncassation()
        {
            try
            {
                CashCounterAccessFlag.Wait();
                WaterDeviceIncasso tmpincasso = new WaterDeviceIncasso
                {
                    CommandID = 0,
                    ID = 0,
                    IncassoBABills = SystemState.BABillsCount,
                    IncassoBASum = SystemState.BASum,
                    IncassoCashboxCoins = SystemState.CBCount,
                    IncassoCashboxSum = SystemState.CBSum,
                    IncassoDatetime = 0,
                    IncassoDatetimeStr = "",
                    WaterDeviceID = GlobalVars.RegID
                };
                ClientRequest tmpreq = RequestEncoder.EncodeRequestData(tmpincasso);
                Dictionary<string, string> data = new Dictionary<string, string>
                    {
                        { "Request", tmpreq.Request },
                        { "Signature", tmpreq.Signature },
                        { "AData", tmpreq.AData },
                        { "BData", tmpreq.BData }
                    };
                HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(data);
                HttpClient httpClient = NewWebClient();
                Uri requestUri = new Uri(GlobalVars.SERVER_ENDPOINT + GlobalVars.INCASSO_REPORT_PATH);
                HttpResponseMessage httpResponse = new HttpResponseMessage();
                httpResponse = await httpClient.PostAsync(requestUri, content);
                httpResponse.EnsureSuccessStatusCode();
                string tmpres = await httpResponse.Content.ReadAsStringAsync();
                if (Convert.ToDouble(tmpres) == SystemState.BASum + SystemState.CBSum)
                {
                    byte[] ccdata = new byte[24];
                    string wcfilename = GlobalVars.HardWareID + ".005";
                    byte[] tmpcbccount = BitConverter.GetBytes(0);
                    byte[] tmpcbcsum = BitConverter.GetBytes(0.00);
                    byte[] tmpbabillscount = BitConverter.GetBytes(0);
                    byte[] tmpbasum = BitConverter.GetBytes(0.00);
                    SystemState.IncassoSum = 0;
                    SystemState.CBCount = 0;
                    SystemState.CBSum = 0;
                    SystemState.BABillsCount = 0;
                    SystemState.BASum = 0;
                    byte[] updatedccdata = new byte[24];
                    Array.Copy(tmpcbccount, 0, updatedccdata, 0, 4);
                    Array.Copy(tmpcbcsum, 0, updatedccdata, 4, 8);
                    Array.Copy(tmpbabillscount, 0, updatedccdata, 12, 4);
                    Array.Copy(tmpbasum, 0, updatedccdata, 16, 8);
                    StorageFile wcfile = null;
                    wcfile = await ApplicationData.Current.LocalFolder.GetFileAsync(wcfilename);
                    using (StorageStreamTransaction transaction = await wcfile.OpenTransactedWriteAsync())
                    {
                        using (DataWriter dataWriter = new DataWriter(transaction.Stream))
                        {
                            dataWriter.WriteBytes(updatedccdata);
                            transaction.Stream.Size = await dataWriter.StoreAsync(); // reset stream size to override the file
                            await transaction.CommitAsync();
                        }
                    }
                }
            }
            catch
            {

            }
            CashCounterAccessFlag.Release();
        }

        public static SemaphoreSlim CashCounterAccessFlag = new SemaphoreSlim(1,1);

        /// <summary>
        /// Обновляет прогресс при старте
        /// </summary>
        /// <param name="ProgressValue"></param>
        public static void UpdateProgress(int ProgressValue)
        {
            if (CurrentState == States.Init)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    StartPageInstance.startprogress.Value += ProgressValue;
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        /// <summary>
        /// Закрашивает выбранный индикатор указанным цветом
        /// </summary>
        /// <param name="LED"></param>
        /// <param name="BrushColor"></param>
        public static void UpdateStartLEDs(Windows.UI.Xaml.Shapes.Ellipse LED, Color BrushColor)
        {
            if (CurrentState == States.Init)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    SolidColorBrush mySolidColorBrush = new SolidColorBrush
                    {
                        Color = BrushColor
                    };
                    LED.Fill = mySolidColorBrush;
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CashDesk.RequestDeviceType();
        }
    }
}
