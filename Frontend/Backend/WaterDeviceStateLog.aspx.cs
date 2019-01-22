using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Security.Cryptography;

public partial class Backend_WaterDeviceStateLog : System.Web.UI.Page
{
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
    protected void Page_Load(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            using (var dbContextTransaction = dc.Database.BeginTransaction())
            {
                try
                {
                    DateTime dt = DateTime.Now;
                    long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
                    string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
                    var waterdevices = dc.WaterDevices;
                    //считываем запрос
                    string encryptedrequest = Request.Form["Request"];
                    byte[] encryptedrequestbytes = Convert.FromBase64String(encryptedrequest);
                    string signature = Request.Form["Signature"];
                    byte[] signaturebytes = Convert.FromBase64String(signature);
                    string encryptedaeskey = Request.Form["AData"];
                    byte[] encryptedaeskeybytes = Convert.FromBase64String(encryptedaeskey);
                    string encryptediv = Request.Form["BData"];
                    byte[] encryptedivbytes = Convert.FromBase64String(encryptediv);
                    //инициализируем криптодвижок для расшифровки
                    CspParameters cspParams = new CspParameters
                    {
                        ProviderType = 1
                    };
                    RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
                    CryptoHelper ch = new CryptoHelper();
                    rsaProvider.ImportCspBlob(ch.PrivateKey);
                    //расшифровываем симметричный ключ и вектор инициализации
                    byte[] AESKeyBytes = rsaProvider.Decrypt(encryptedaeskeybytes, false);
                    byte[] AESIVBytes = rsaProvider.Decrypt(encryptedivbytes, false);
                    AesCryptoServiceProvider AESProv = new AesCryptoServiceProvider
                    {
                        Mode = CipherMode.CBC,
                        Padding = PaddingMode.PKCS7,
                        KeySize = 128,
                        Key = AESKeyBytes,
                        IV = AESIVBytes
                    };
                    //расшифровываем запрос
                    string plaintext = "";
                    MemoryStream memoryStream = null;
                    try
                    {
                        memoryStream = new MemoryStream(encryptedrequestbytes);
                        using (CryptoStream cryptoStream = new CryptoStream(memoryStream, AESProv.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            plaintext = new StreamReader(cryptoStream, Encoding.UTF8).ReadToEnd();
                        }
                    }
                    finally
                    {
                        if (memoryStream != null) memoryStream.Dispose();
                    }
                    var devicestable = dc.WaterDevices;
                    WaterDeviceTelemetry logentry = Deserialize<WaterDeviceTelemetry>(plaintext);
                    //инициализируем криптодвижок для проверки подписи присланных данных
                    rsaProvider = new RSACryptoServiceProvider();
                    rsaProvider.ImportCspBlob(devicestable.First(x => x.ID == logentry.WaterDeviceID).PublicKey);
                    bool signcorrect = rsaProvider.VerifyData(Encoding.UTF8.GetBytes(plaintext), CryptoConfig.MapNameToOID("SHA512"), signaturebytes);
                    if (signcorrect)
                    {
                        logentry.DateTime = cdt;
                        logentry.DateTimeStr = cdtstr;
                        var telemetrytable = dc.WaterDeviceTelemetry;
                        var statetable = dc.WaterDeviceState;
                        try
                        {
                            WaterDeviceState tmpstate = statetable.Where(x => x.WaterDeviceID == logentry.WaterDeviceID).First();
                            dc.Entry(tmpstate).CurrentValues.SetValues(logentry);
                        }
                        catch
                        {
                            var tmpstate = (WaterDeviceState)logentry;
                            statetable.Add(tmpstate);
                        }
                        telemetrytable.Add(logentry);
                        dc.SaveChanges();
                    }
                    //сохраняем изменения в БД
                    dbContextTransaction.Commit();
                }
                catch /*(Exception ex)*/
                {
                    //что-то пошло не так, ошибка на любом этапе, откатываем изменения в БД
                    dbContextTransaction.Rollback();
                }
                finally
                {

                }
            }
        }
    }
}
