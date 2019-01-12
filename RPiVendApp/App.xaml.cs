using System;
using System.IO;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Text;
using System.Xml.Serialization;
using Windows.Storage.Streams;
using Windows.Security.Cryptography.Core;
using System.Runtime.InteropServices.WindowsRuntime;

namespace RPiVendApp
{
    /// <summary>
    /// Обеспечивает зависящее от конкретного приложения поведение, дополняющее класс Application по умолчанию.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Инициализирует одноэлементный объект приложения.  Это первая выполняемая строка разрабатываемого
        /// кода; поэтому она является логическим эквивалентом main() или WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Выполняет обратное преобразование в объект заданного типа из xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        private static T Deserialize<T>(string xml)
        {
            var xs = new XmlSerializer(typeof(T));
            return (T)xs.Deserialize(new StringReader(xml));
        }

        /// <summary>
        /// Вызывается при обычном запуске приложения пользователем.  Будут использоваться другие точки входа,
        /// например, если приложение запускается для открытия конкретного файла.
        /// </summary>
        /// <param name="e">Сведения о запросе и обработке запуска.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Не повторяйте инициализацию приложения, если в окне уже имеется содержимое,
            // только обеспечьте активность окна
            if (rootFrame == null)
            {
                // Создание фрейма, который станет контекстом навигации, и переход к первой странице
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Загрузить состояние из ранее приостановленного приложения
                }

                // Размещение фрейма в текущем окне
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // Если стек навигации не восстанавливается для перехода к первой странице,
                    // настройка новой страницы путем передачи необходимой информации в качестве параметра
                    // параметр
                    string keypairfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".001";
                    if (!File.Exists(keypairfilename))
                    {
                        string RSAProvName = AsymmetricAlgorithmNames.RsaPkcs1;
                        AsymmetricKeyAlgorithmProvider RSAProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(RSAProvName);
                        CryptographicKey ClientKeyPair = RSAProv.CreateKeyPair(2048);
                        byte[] ClientKeyPairBytes = ClientKeyPair.Export().ToArray();
                        File.WriteAllBytes(keypairfilename, ClientKeyPairBytes);
                    }
                    if (IsLicenseValid())
                    {
                        rootFrame.Navigate(typeof(StartPage), e.Arguments);
                    } else
                    {
                        rootFrame.Navigate(typeof(Registration), e.Arguments);
                    }
                }
                // Обеспечение активности текущего окна
                Window.Current.Activate();

            }
        }

        bool IsLicenseValid()
        {
            bool res = false;
            try
            {
                string regfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".002";
                string clientkeypairfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".001";
                string RSAProvName = AsymmetricAlgorithmNames.RsaSignPkcs1Sha512;
                AsymmetricKeyAlgorithmProvider RSAClientProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(RSAProvName);
                GlobalVars.ClientKeyPair = RSAClientProv.ImportKeyPair(File.ReadAllBytes(clientkeypairfilename).AsBuffer());
                string strAlgName = HashAlgorithmNames.Sha512;
                HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
                CryptographicHash objHash = objAlgProv.CreateHash();
                objHash.Append(GlobalVars.ClientKeyPair.ExportPublicKey(CryptographicPublicKeyBlobType.Capi1PublicKey));
                IBuffer ClientPublicKeyHashBuffer = objHash.GetValueAndReset();
                byte[] regbytes = File.ReadAllBytes(regfilename);
                string respxml = Encoding.UTF8.GetString(regbytes);
                WaterDeviceRegistrationResponse regresp = Deserialize<WaterDeviceRegistrationResponse>(respxml);
                byte[] SignatureBytes = Convert.FromBase64String(regresp.Signature);
                IBuffer SignatureBuffer = SignatureBytes.AsBuffer();
                IBuffer ServerPublicKeyBuffer = Convert.FromBase64String(GlobalVars.ServerPublicKey).AsBuffer();
                AsymmetricKeyAlgorithmProvider RSAVerifyProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(RSAProvName);
                CryptographicKey ServerPublicKey = RSAVerifyProv.ImportPublicKey(ServerPublicKeyBuffer, CryptographicPublicKeyBlobType.Capi1PublicKey);
                res = CryptographicEngine.VerifySignatureWithHashInput(ServerPublicKey, ClientPublicKeyHashBuffer, SignatureBuffer);
                if (res)
                {
                    GlobalVars.RegID = regresp.RegID;
                    GlobalVars.SERVER_ENDPOINT = regresp.ServerEndPoint;
                }
            }
            catch
            {

            }
            return res;
        }

        /// <summary>
        /// Вызывается в случае сбоя навигации на определенную страницу
        /// </summary>
        /// <param name="sender">Фрейм, для которого произошел сбой навигации</param>
        /// <param name="e">Сведения о сбое навигации</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Вызывается при приостановке выполнения приложения.  Состояние приложения сохраняется
        /// без учета информации о том, будет ли оно завершено или возобновлено с неизменным
        /// содержимым памяти.
        /// </summary>
        /// <param name="sender">Источник запроса приостановки.</param>
        /// <param name="e">Сведения о запросе приостановки.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Сохранить состояние приложения и остановить все фоновые операции
            deferral.Complete();
        }
    }
}
