using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace RPiVendApp
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class OutOfServicePage : Page
    {
        public static OutOfServicePage OutOfServicePageInstance = null;
        public OutOfServicePage()
        {
            InitializeComponent();
            OutOfServicePageInstance = this;
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        private void insertcointext_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
