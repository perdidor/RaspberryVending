using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace RPiVendApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class ReadyToStartPage : Page
    {
        public static ReadyToStartPage ReadyToStartPageInstance = null;
        public ReadyToStartPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;
            ReadyToStartPageInstance = this;
        }

        public static void ChangeWaterValveStatus(bool open)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    if (open) ReadyToStartPageInstance.valvestatus.Text = "Кран открыт"; else ReadyToStartPageInstance.valvestatus.Text = "Кран закрыт";
                }
                catch
                {

                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}
