using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Web.Http;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Core;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace RPiVendApp
{
    public class WaterDeviceRegistrationResponse
    {
        public WaterDeviceRegistrationResponse()
        {
            //
            // TODO: добавьте логику конструктора
            //
        }
        public string AuthResponse;
        public long RegID;
        public string ServerEndPoint;
        public string Signature;
    }
    public class WaterDeviceRegistrationRequest
    {
        public WaterDeviceRegistrationRequest()
        {
            //
            // TODO: добавьте логику конструктора
            //
        }

        /// <summary>
        /// Строка авторизации, сгенерированная на клиенте
        /// </summary>
        public byte[] AuthorizationString;
        /// <summary>
        /// подпись авторизации
        /// </summary>
        public byte[] AuthSignature;
        /// <summary>
        /// Открытый ключ клиента
        /// </summary>
        public byte[] PublicKey;
    }


    public class AccLicense
    {
        /// <summary>
        /// e-mail
        /// </summary>
        public string UserID;
        /// <summary>
        /// Дата регистрации в формате bigint
        /// </summary>
        public long RegistrationDateTime;
        /// <summary>
        /// Дата регистрации в строковом выражении
        /// </summary>
        public string RegistrationDateTimeStr;
        /// <summary>
        /// Максимальное количество управляемых объектов
        /// </summary>
        public int DeviceCountLimit;
        /// <summary>
        /// Адрес сервера
        /// </summary>
        public string ServerEndPoint;
        /// <summary>
        /// Цифровая подпись сервера
        /// </summary>
        public byte[] Signature;
        /// <summary>
        /// Проверка лицензии
        /// </summary>
        /// <returns></returns>
        public bool IsLicenseValid()
        {
            bool res = false;
            try
            {
                IBuffer SignatureBuffer = Signature.AsBuffer();
                IBuffer ServerPublicKeyBuffer = Convert.FromBase64String(GlobalVars.ServerPublicKey).AsBuffer();
                AsymmetricKeyAlgorithmProvider RSAVerifyProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaSignPkcs1Sha512);
                CryptographicKey ServerPublicKey = RSAVerifyProv.ImportPublicKey(ServerPublicKeyBuffer, CryptographicPublicKeyBlobType.Capi1PublicKey);
                byte[] tmpuseridbytes = Encoding.UTF8.GetBytes(UserID);
                byte[] tmpregdtbytes = BitConverter.GetBytes(RegistrationDateTime);
                byte[] tmpregdtstrbytes = Encoding.UTF8.GetBytes(RegistrationDateTimeStr);
                byte[] tmpdevcountbytes = BitConverter.GetBytes(DeviceCountLimit);
                byte[] tmpendpointbytes = Encoding.UTF8.GetBytes(ServerEndPoint);
                byte[] tmpbytes = new byte[tmpuseridbytes.Length + tmpregdtbytes.Length + tmpregdtstrbytes.Length + tmpdevcountbytes.Length + tmpendpointbytes.Length];
                Array.Copy(tmpuseridbytes, tmpbytes, tmpuseridbytes.Length);
                Array.Copy(tmpregdtbytes, 0, tmpbytes, tmpuseridbytes.Length, tmpregdtbytes.Length);
                Array.Copy(tmpregdtstrbytes, 0, tmpbytes, tmpuseridbytes.Length + tmpregdtbytes.Length, tmpregdtstrbytes.Length);
                Array.Copy(tmpdevcountbytes, 0, tmpbytes, tmpuseridbytes.Length + tmpregdtbytes.Length + tmpregdtstrbytes.Length, tmpdevcountbytes.Length);
                Array.Copy(tmpendpointbytes, 0, tmpbytes, tmpuseridbytes.Length + tmpregdtbytes.Length + tmpregdtstrbytes.Length + tmpdevcountbytes.Length, tmpendpointbytes.Length);
                string strAlgName = HashAlgorithmNames.Sha512;
                HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
                CryptographicHash objHash = objAlgProv.CreateHash();
                objHash.Append(tmpbytes.AsBuffer());
                IBuffer tmpbyteshash = objHash.GetValueAndReset();
                res = CryptographicEngine.VerifySignatureWithHashInput(ServerPublicKey, tmpbyteshash, SignatureBuffer);
            }
            catch /*(Exception ex)*/
            {

            }
            return res;
        }
    }

    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class Registration : Page
    {
        BitmapImage OK = new BitmapImage(new Uri("ms-appx:///Assets/OK.png"));
        BitmapImage Error = new BitmapImage(new Uri("ms-appx:///Assets/Error.png"));
        BitmapImage loading = new BitmapImage(new Uri("ms-appx:///Assets/loading.gif"));
        BitmapImage Empty = new BitmapImage(new Uri("ms-appx:///Assets/Empty.png"));
        BitmapImage Searching = new BitmapImage(new Uri("ms-appx:///Assets/searching.gif"));
        BitmapImage Waiting = new BitmapImage(new Uri("ms-appx:///Assets/waiting.gif"));
        public Registration()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Task.Run(() => { DoRegister(); });
            base.OnNavigatedTo(e);
        }

        /// <summary>
        /// При сериализации в xml переопределяем кодировку по умолчанию, чтобы не было проблем с русскими буквами.
        /// </summary>
        private class Utf8StringWriter : StringWriter
        {
            public override Encoding Encoding { get { return Encoding.UTF8; } }
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
        /// Указываем файл лицензии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DoRegister()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                pic.Source = Searching;
            });
            var removableDevices = KnownFolders.RemovableDevices;
            var externalDrives = await removableDevices.GetFoldersAsync();
            foreach (var drive in externalDrives)
            {
                try
                {
                    StorageFile licfile = null;
                    licfile = await drive.GetFileAsync("wvlicense.lic");
                    if (licfile != null)
                    {
                        //файл выбран, выводим картинку что все в процессе
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            pic.Source = loading;
                        });
                        //инициализируем криптодвижок для проверки файла лицензии
                        string RSAProvName = AsymmetricAlgorithmNames.RsaPkcs1;
                        StreamReader LicenseFile = File.OpenText(licfile.Path);
                        string LicenseXML = LicenseFile.ReadToEnd();
                        LicenseFile.Dispose();
                        AccLicense tmplic = Deserialize<AccLicense>(LicenseXML);
                        if (tmplic.IsLicenseValid())
                        {
                            //лицензия корректна
                            //выводим пользователю данные лицензии
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                userid.Foreground = new SolidColorBrush(Colors.Green);
                                userid.Text = tmplic.UserID;
                                regdatetime.Foreground = new SolidColorBrush(Colors.Green);
                                regdatetime.Text = tmplic.RegistrationDateTimeStr;
                                devcountlimit.Foreground = new SolidColorBrush(Colors.Green);
                                devcountlimit.Text = tmplic.DeviceCountLimit.ToString();
                                statustext.Text = "запрос регистрации...";
                            });
                            //данные для регистрации переводим в массив байт
                            byte[] tmphwidbytes = Encoding.UTF8.GetBytes(GlobalVars.HardWareID);
                            byte[] tmpuidbytes = Encoding.UTF8.GetBytes(tmplic.UserID);
                            byte[] tmpsplitbytes = new byte[3] { 254, 11, 254 };
                            byte[] tmpauthstrbytes = new byte[tmphwidbytes.Length + tmpuidbytes.Length + 3];
                            Array.Copy(tmphwidbytes, 0, tmpauthstrbytes, 0, tmphwidbytes.Length);
                            Array.Copy(tmpsplitbytes, 0, tmpauthstrbytes, tmphwidbytes.Length, tmpsplitbytes.Length);
                            Array.Copy(tmpuidbytes, 0, tmpauthstrbytes, tmphwidbytes.Length + tmpsplitbytes.Length, tmpuidbytes.Length);
                            //инициализируем криптодвижок для подписи и шифрования запроса
                            AsymmetricKeyAlgorithmProvider RSAEncryptProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(RSAProvName);
                            CryptographicKey ServerPublicKey = RSAEncryptProv.ImportPublicKey(Convert.FromBase64String(GlobalVars.ServerPublicKey).AsBuffer(), CryptographicPublicKeyBlobType.Capi1PublicKey);
                            string strAlgName = HashAlgorithmNames.Sha512;
                            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
                            CryptographicHash objHash = objAlgProv.CreateHash();
                            objHash.Append(tmpauthstrbytes.AsBuffer());
                            IBuffer authstringhash = objHash.GetValueAndReset();
                            byte[] haashbytes = authstringhash.ToArray();
                            //подписываем
                            byte[] tmpsignature = CryptographicEngine.SignHashedData(GlobalVars.ClientKeyPair, authstringhash).ToArray();
                            //шифруем
                            byte[] tmpauthstring = CryptographicEngine.Encrypt(ServerPublicKey, tmpauthstrbytes.AsBuffer(), null).ToArray();
                            //создаем объект запроса
                            WaterDeviceRegistrationRequest tmpreq = new WaterDeviceRegistrationRequest();
                            tmpreq.PublicKey = GlobalVars.ClientKeyPair.ExportPublicKey(CryptographicPublicKeyBlobType.Capi1PublicKey).ToArray();
                            tmpreq.AuthorizationString = tmpauthstring;
                            tmpreq.AuthSignature = tmpsignature;
                            //сериализуем в  XML
                            var xs = new XmlSerializer(tmpreq.GetType());
                            var xml = new Utf8StringWriter();
                            xs.Serialize(xml, tmpreq);
                            //переводим в строку Base64
                            byte[] xmlbytes = Encoding.UTF8.GetBytes(xml.ToString());
                            string xmlbytesbase64 = Convert.ToBase64String(xmlbytes);
                            //формируем запрос
                            Dictionary<string, string> data = new Dictionary<string, string>
                            {
                                { "RegistrationRequest", xmlbytesbase64 }
                            };
                            HttpFormUrlEncodedContent content = new HttpFormUrlEncodedContent(data);
                            while (true)
                            {
                                bool httpsuccess = false;
                                HttpResponseMessage httpResponse = new HttpResponseMessage();
                                //пробуем достучаться до сервера пока не опухнем
                                while (!httpsuccess)
                                {
                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        pic.Source = loading;
                                        statustext.Text = "соединение с сервером...";
                                    });
                                    HttpClient httpClient = new HttpClient();
                                    Uri requestUri = new Uri("https://" + tmplic.ServerEndPoint + GlobalVars.REGISTRATION_PATH);
                                    //отправляем на сервер
                                    httpResponse = await httpClient.PostAsync(requestUri, content);
                                    int delayvalue;
                                    try
                                    {
                                        //все нормально
                                        httpResponse.EnsureSuccessStatusCode();
                                        httpsuccess = true;
                                    }
                                    catch
                                    {
                                        //ошибка, пауза 1 минута, потом новая попытка
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                        {
                                            pic.Source = Waiting;
                                            statustext.Text = "Ошибка соединения, следующая попытка через 01:00";
                                        });
                                        delayvalue = 60;
                                        while (delayvalue > 0)
                                        {
                                            Task.Delay(1000).Wait();
                                            delayvalue--;
                                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                            {
                                                pic.Source = Waiting;
                                                statustext.Text = "Ошибка соединения, следующая попытка через " + (delayvalue / 60).ToString().PadLeft(2, '0') +
                                            ":" + (delayvalue % 60).ToString().PadLeft(2, '0');
                                            });
                                        }
                                    }
                                }
                                //обрабатываем ответ от сервера
                                string resp = httpResponse.Content.ReadAsStringAsync().GetResults();
                                byte[] respbytes = Convert.FromBase64String(resp);
                                string respxml = Encoding.UTF8.GetString(respbytes);
                                //создаем объект ответа
                                WaterDeviceRegistrationResponse regresp = Deserialize<WaterDeviceRegistrationResponse>(respxml);
                                //выводим пользователю сообщение от сервера
                                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                {
                                    regresponse.Text = regresp.AuthResponse;
                                });
                                if (regresp.AuthResponse.StartsWith("SUCCESS"))
                                {
                                    //вычисляем хеш открытого ключа клиента для проверки подписи
                                    strAlgName = HashAlgorithmNames.Sha512;
                                    objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
                                    objHash = objAlgProv.CreateHash();
                                    objHash.Append(tmpreq.PublicKey.AsBuffer());
                                    IBuffer ClientPublicKeyHashBuffer = objHash.GetValueAndReset();
                                    //подпись лицензии
                                    byte[] SignatureBytes = Convert.FromBase64String(regresp.Signature);
                                    IBuffer SignatureBuffer = SignatureBytes.AsBuffer();
                                    AsymmetricKeyAlgorithmProvider RSAVerifyProv = AsymmetricKeyAlgorithmProvider.OpenAlgorithm(AsymmetricAlgorithmNames.RsaSignPkcs1Sha512);
                                    ServerPublicKey = RSAVerifyProv.ImportPublicKey(Convert.FromBase64String(GlobalVars.ServerPublicKey).AsBuffer(), CryptographicPublicKeyBlobType.Capi1PublicKey);
                                    bool res = CryptographicEngine.VerifySignatureWithHashInput(ServerPublicKey, ClientPublicKeyHashBuffer, SignatureBuffer);
                                    if (res)
                                    {
                                        //подпись корректна, сохраняем ее в файл
                                        string signfilename = ApplicationData.Current.LocalFolder.Path + "\\" + GlobalVars.HardWareID + ".002";
                                        File.WriteAllBytes(signfilename, respbytes);
                                        //показываем пользователю, что все ОК
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                        {
                                            pic.Source = OK;
                                            statustext.Text = "OK";
                                        });
                                        return;
                                    }
                                    else
                                    {
                                        //подпись некорректна
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                        {
                                            pic.Source = Error;
                                            statustext.Text = "Регистрация закончилась неудачно";
                                        });
                                        return;
                                    }
                                }
                                if (regresp.AuthResponse == "OK_PENDING")
                                {
                                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                    {
                                        pic.Source = Waiting;
                                        statustext.Text = "Подтверждение, проверка через 05:00";
                                    });
                                    int delayvalue = 300;
                                    while (delayvalue > 0)
                                    {
                                        Task.Delay(1000).Wait();
                                        delayvalue--;
                                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                                        {
                                            statustext.Text = "Подтверждение, проверка через " + (delayvalue / 60).ToString().PadLeft(2, '0') + ":" + (delayvalue % 60).ToString().PadLeft(2, '0');
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            //Лицензия недействительна либо файл поврежден
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                pic.Source = Error;
                                statustext.Text = "Лицензия недействительна";
                            });
                            return;
                        }
                    }
                }
                catch /*(DirectoryNotFoundException ex)*/
                {

                }
                //catch (FileNotFoundException ex)
                //{

                //}
            }
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                pic.Source = Error;
                statustext.Text = "Ошибка: файл лицензии не найден";
            });
        }
    }
}
