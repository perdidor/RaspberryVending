using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

/// <summary>
/// Сводное описание для AccLicense
/// </summary>
public class AccLicense
{
    public AccLicense()
    {

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
    /// Генерирует новую лицензию для учетной записи пользователя с указанным идентификатором
    /// </summary>
    /// <param name="AccID"></param>
    public AccLicense(long AccID)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            //using (var dbContextTransaction = dc.Database.BeginTransaction())
            //{
            try
            {
                Accounts tmpacc = dc.Accounts.First(x => x.ID == AccID);
                UserID = tmpacc.UserID;
                RegistrationDateTime = tmpacc.RegistrationDateTime;
                RegistrationDateTimeStr = tmpacc.RegistrationDateTimeStr;
                DeviceCountLimit = tmpacc.DeviceCountLimit;
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
                CryptoHelper ch = new CryptoHelper();
                Signature = ch.SignByteArray(tmpbytes);
                var xs = new XmlSerializer(GetType());
                var xml = new Utf8StringWriter();
                xs.Serialize(xml, this);
                tmpacc.LicenseContent = xml.ToString();
                dc.SaveChanges();
            }
            catch /*(Exception ex)*/
            {

            }
            //}
        }
    }

    /// <summary>
    /// Проверка лицензии
    /// </summary>
    /// <returns></returns>
    public bool IsLicenseValid()
    {
        CryptoHelper ch = new CryptoHelper();
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
        return ch.CheckSignature(tmpbytes, Signature);
    }

    /// <summary>
    /// Создает объект лицензии для ее проверки
    /// </summary>
    /// <param name="Content"></param>
    public AccLicense(string Content)
    {
        try
        {
            AccLicense tmpacclicense = Deserialize<List<AccLicense>>(Content)[0];
            UserID = tmpacclicense.UserID;
            RegistrationDateTime = tmpacclicense.RegistrationDateTime;
            RegistrationDateTimeStr = tmpacclicense.RegistrationDateTimeStr;
            DeviceCountLimit = tmpacclicense.DeviceCountLimit;
            Signature = tmpacclicense.Signature;
            ServerEndPoint = tmpacclicense.ServerEndPoint;
        }
        catch
        {

        }
    }

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
    public string ServerEndPoint = WWWVars.ServerEndPoint;
    /// <summary>
    /// Цифровая подпись сервера
    /// </summary>
    public byte[] Signature;
}