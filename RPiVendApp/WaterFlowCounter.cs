using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using MDBLib;


namespace RPiVendApp
{
    /// <summary>
    /// Счетчик воды
    /// </summary>
    public static class WaterFlowCounter
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Обработка импульса с датчика потока
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void WaterFlowSensorPin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (args.Edge == GpioPinEdge.RisingEdge)
            {
                WaterCounterTicked();
            }
        }

        private static SemaphoreSlim watercountertickerSemaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Прибавляет 50мл к счетчику отгруженной водички
        /// </summary>
        /// <returns></returns>
        public static void WaterCounterTicked()
        {
            if (StartPage.CurrentState == StartPage.States.ReadyToDispenseWater || StartPage.CurrentState == StartPage.States.ServiceMode)
            {
                watercountertickerSemaphore.Wait();
                if (!(StartPage.CurrentState == StartPage.States.ReadyToDispenseWater || StartPage.CurrentState == StartPage.States.ServiceMode))
                {
                    watercountertickerSemaphore.Release();
                    return;
                }
                StartPage.WaterFlowPulseCounter++;
                if (StartPage.WaterFlowPulseCounter >= 17)//50ml dispensed
                {
                    StartPage.WaterFlowPulseCounter = 0;
                    if (StartPage.CurrentState == StartPage.States.ReadyToDispenseWater)
                    {
                        StartPage.UserDeposit -= Math.Round(StartPage.CurrentSaleSession.PRICE_PER_ITEM_MDE * 0.0005, 2);
                        StartPage.CurrentSaleSession.Quantity += 0.05;
                        if (Math.Round(StartPage.CurrentSaleSession.UserCash, 2) <= Math.Round(StartPage.CurrentSaleSession.Quantity * StartPage.CurrentSaleSession.PRICE_PER_ITEM_MDE * 0.01, 2))
                        {
                            StartPage.CurrentState = StartPage.States.DispenseChange;
                            StartPage.WaterValvePin.Write(GpioPinValue.High);
                            StartPage.PumpPin.Write(GpioPinValue.High);
                            StartPage.StartLEDPin.Write(GpioPinValue.High);
                            StartPage.StopLEDPin.Write(GpioPinValue.High);
                            StartPage.EndLEDPin.Write(GpioPinValue.High);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            MDB.DisableCashDevicesAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                var frame = Window.Current.Content as Frame;
                                frame.Navigate(typeof(ChangePage));
                            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                        }
                    }
                }
                watercountertickerSemaphore.Release();
            }
        }
    }

}