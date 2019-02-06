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
            try
            {
                DateTime dt = DateTime.Now;
                CryptoHelper ch = new CryptoHelper();
                long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
                string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
                var waterdevices = dc.WaterDevices;
                //считываем запрос
                string encryptedrequest = Request.Form["Request"];
                string signature = Request.Form["Signature"];
                string encryptedaeskey = Request.Form["AData"];
                string encryptediv = Request.Form["BData"];
                var devicestable = dc.WaterDevices;
                WaterDeviceTelemetry logentry = null;
                bool signcorrect = ch.DecryptVerifyDeviceLog(encryptedrequest, signature, encryptedaeskey, encryptediv, out logentry);
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
            }
            catch /*(Exception ex)*/
            {

            }
            finally
            {

            }
        }
    }
}