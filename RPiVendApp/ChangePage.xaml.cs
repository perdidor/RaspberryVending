using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace RPiVendApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>

    public sealed partial class ChangePage : Page
    {
        public static ChangePage ChangePageInstance = null;
        public static SemaphoreSlim ChangeModeSemaphore = new SemaphoreSlim(1, 1);
        public ChangePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ChangePageInstance = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Task.Run( () => { DispenseChange(); });
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// выдаем сдачу пока депозит не станет равен 0
        /// </summary>
        private void DispenseChange()
        {
            ChangeModeSemaphore.Wait();
            if (StartPage.CurrentState != StartPage.States.DispenseChange)
            {
                ChangeModeSemaphore.Release();
                return;
            }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                dispenselabel.Text = Math.Round(StartPage.UserDeposit, 0, MidpointRounding.AwayFromZero).ToString().PadLeft(3,'0');
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            if (StartPage.CurrentDeviceSettings.UseKKT && Math.Round(StartPage.CurrentSaleSession.Quantity, 3) > 0)
            {
                try
                {
                    StartPage.CurrentSaleSession.ReceiptNumber = StartPage.SystemState.KKTReceiptNextNumber;
                    StartPage.CurrentSaleSession.StageNumber = Convert.ToInt16(StartPage.SystemState.KKTStageNumber);
                    StartPage.CurrentSaleSession.TaxSystemInUse = StartPage.CurrentDeviceSettings.TaxSystem;
                    if (Math.Round(StartPage.UserDeposit, 2, MidpointRounding.AwayFromZero) == 0)
                    {
                        CashDesk.PrintReceipt(GlobalVars.ReceiptLinesToPrint, (((int)(Math.Round(StartPage.CurrentSaleSession.UserCash, 2) * 100)) * 1.00 / StartPage.CurrentSaleSession.PRICE_PER_ITEM_MDE),
                                StartPage.CurrentSaleSession.PRICE_PER_ITEM_MDE, (int)(Math.Round(StartPage.CurrentSaleSession.UserCash, 2) * 100));
                    }
                    else
                    {
                        CashDesk.PrintReceipt(GlobalVars.ReceiptLinesToPrint, Math.Round(StartPage.CurrentSaleSession.Quantity, 3), StartPage.CurrentSaleSession.PRICE_PER_ITEM_MDE,
                            (int)(Math.Round(StartPage.CurrentSaleSession.UserCash, 2) * 100));
                    }
                }
                catch
                {

                }
            }
            Task.Delay(15000).Wait();
            try
            {
                CashDesk.GetCurrentStageParameters();
                StartPage.CurrentSaleSession.ActualChangeDispensed = (int)Math.Round(StartPage.UserDeposit, 0, MidpointRounding.AwayFromZero);
                StartPage.CurrentSaleSession.ChangeActualDiff = StartPage.CurrentSaleSession.ActualChangeDispensed - StartPage.UserDeposit;
                StartPage.CurrentSaleSession.CompleteSession();
                ServiceTasks.UpdateWaterCounter(StartPage.CurrentSaleSession.Quantity);
                StartPage.UserDeposit = Math.Round(StartPage.UserDeposit, 0, MidpointRounding.AwayFromZero);
                while ((int)StartPage.UserDeposit > 0)
                {
                    while (MDB.DispenseInProgress || MDB.CheckDispenseResult)
                    {
                        Task.Delay(1000).Wait();
                    }
                    int _change = (int)StartPage.UserDeposit;
                    if (_change > 127)//не более 127 рублей в одной порции сдачи, ограничение монетоприемника
                    {
                        _change = 127;
                    }
                    MDB.Dispensetimeout = DateTime.Now.AddSeconds(10);
                    MDB.PayoutCoins(_change);//выдаем сдачу монетами
                    StartPage.UserDeposit -= _change;
                    Task.Delay(2000).Wait();
                }
                string oosfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".031";
                if (File.Exists(oosfilename))
                {
                    StartPage.CurrentState = StartPage.States.OutOfService;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var frame = Window.Current.Content as Frame;
                        frame.Navigate(typeof(OutOfServicePage));
                    });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    return;
                }
                StartPage.CurrentState = StartPage.States.ReadyToServe;
                MDB.EnableCashDevices();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = Window.Current.Content as Frame;
                    frame.Navigate(typeof(MainPage));
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch
            {

            }
            ChangeModeSemaphore.Release();
        }
    }
}
