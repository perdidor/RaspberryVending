using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace RPiVendApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ServiceModePage : Page
    {
        public static ServiceModePage ServiceModePageInstance = null;
        public ServiceModePage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ServiceModePageInstance = this;
            UpdateCounters();
        }

        private async void UpdateCounters()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    ServiceModePageInstance.hourcounterlabel.Text = StartPage.SystemState.TotalHoursWorked.ToString("N2").PadLeft(10, '0');
                    ServiceModePageInstance.watercounterlabel.Text = StartPage.SystemState.TotalLitersDIspensed.ToString("N2").PadLeft(10, '0');
                }
                catch
                {

                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StartPage));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            ServiceTasks.UpdateCashCounter(1,0.5,1,100,2,1,3,1);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
