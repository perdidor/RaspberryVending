using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;
using System.Security.Cryptography;

public partial class Backend_NewWaterDevice : System.Web.UI.Page
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
    protected void Page_Load(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            using (var dbContextTransaction = dc.Database.BeginTransaction())
            {
                Uri url = new Uri(Request.Url.Authority);
                string output = url.GetLeftPart(UriPartial.Authority);
                WaterDeviceRegistrationResponse authresp = new WaterDeviceRegistrationResponse
                {
                    RegID = -1,
                    ServerEndPoint = output
                };
                try
                {
                    DateTime dt = DateTime.Now;
                    long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
                    string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
                    var waterdevices = dc.WaterDevices;
                    //считываем запрос
                    string rawrequest = Request.Form["RegistrationRequest"];
                    byte[] bytes = Convert.FromBase64String(rawrequest);
                    string requestxml = Encoding.UTF8.GetString(bytes);
                    //создаем объект запроса регистрации
                    WaterDeviceRegistrationRequest tmpreq = Deserialize<WaterDeviceRegistrationRequest>(requestxml);
                    //инициализируем криптодвижок для расшифровки
                    CspParameters cspParams = new CspParameters
                    {
                        ProviderType = 1
                    };
                    RSACryptoServiceProvider rsaProvider = new RSACryptoServiceProvider(cspParams);
                    CryptoHelper ch = new CryptoHelper();
                    rsaProvider.ImportCspBlob(ch.PrivateKey);
                    //расшифровываем запрос
                    byte[] plaintextbytes = rsaProvider.Decrypt(tmpreq.AuthorizationString,false);
                    byte[] tmphwidbytes = new byte[] { };
                    byte[] tmpuidbytes = new byte[] { };
                    //делим массив данных на две части, разделитель byte[3] { 254, 11, 254 }
                    for (int i = 0; i < plaintextbytes.Length; i++)
                    {
                        if (plaintextbytes[i] == 254 && plaintextbytes[i + 1] == 11 && plaintextbytes[i + 2] == 254)
                        {
                            Array.Resize(ref tmphwidbytes, i);
                            Array.Copy(plaintextbytes, 0, tmphwidbytes, 0, tmphwidbytes.Length);
                            Array.Resize(ref tmpuidbytes, plaintextbytes.Length - i - 3);
                            Array.Copy(plaintextbytes, i + 3, tmpuidbytes, 0, tmpuidbytes.Length);
                            break;
                        }
                    }
                    //поля запроса: учетная запись и аппаратный идентификатор устройства
                    string userid = Encoding.UTF8.GetString(tmpuidbytes);
                    string hwid = Encoding.UTF8.GetString(tmphwidbytes);
                    //инициализируем криптодвижок для проверки подписи присланных данных
                    rsaProvider = new RSACryptoServiceProvider();
                    rsaProvider.ImportCspBlob(tmpreq.PublicKey);
                    SHA512 sha512 = new SHA512Managed();
                    //вычисляем хеш присланных данных
                    byte[] hash = sha512.ComputeHash(plaintextbytes);
                    int activedevicescount;
                    int pendingdevicescount;
                    Accounts tmpacc;
                    //проверяем подпись
                    bool signcorrect = rsaProvider.VerifyData(plaintextbytes, CryptoConfig.MapNameToOID("SHA512"), tmpreq.AuthSignature);
                    if (signcorrect)
                    {
                        //подпись корректна, ищем акаунт
                        tmpacc = dc.Accounts.First(x => x.UserID == userid && x.Valid && !x.Suspended);
                        activedevicescount = waterdevices.Where(x => x.AccountID == tmpacc.ID && x.Valid && !x.Suspended && !x.PendingRegistration).Count();
                        pendingdevicescount = waterdevices.Where(x => x.AccountID == tmpacc.ID && x.Valid && !x.Suspended && x.PendingRegistration).Count();
                        WaterDevices tmpdev = null;
                        //пробуем найти акаунт с таким же аппаратным идентификатором, чтобы исключить дубликаты
                        try
                        {
                            tmpdev = waterdevices.Where(x => x.HardwareID == hwid/* && x.AccountID == tmpacc.ID && x.Valid && !x.Suspended*/).First();
                            //если устройство нашлось и зарегистрировано, отсылаем содержимое лицензии
                            if (!tmpdev.PendingRegistration)
                            {
                                authresp = Deserialize<WaterDeviceRegistrationResponse>(tmpdev.RegistrationResponse);
                            } else
                            //если устройство нашлось и ожидает регистрации
                            {
                                authresp.Signature = "";
                                authresp.AuthResponse = "OK_PENDING";
                            }
                        }
                        catch
                        {

                        }
                        //если в базе нет устройства
                        if (tmpdev == null)
                        {
                            //есть неиспользованные лицензии, формируем запись нового устройства
                            if (activedevicescount < tmpacc.DeviceCountLimit)
                            {
                                WaterDevices newwaterdevice = new WaterDevices()
                                {
                                    CustomerServiceContactPhone = tmpacc.DefaultContactPhone,
                                    HardwareID = hwid,
                                    AccountID = tmpacc.ID,
                                    LocationAddress = "",
                                    LocationLatitude = 0,
                                    LocationLongtitude = 0,
                                    ProductName = "Вода питьевая",
                                    PendingRegistration = true,
                                    PRICE_PER_ITEM_MDE = 500,
                                    PublicKey = tmpreq.PublicKey,
                                    RegistrationRequest = requestxml,
                                    Suspended = false,
                                    Valid = true,
                                    RegistrationDateTime = 0,
                                    RegistrationRequestDateTime = cdt,
                                    RegistrationDateTimeStr = "",
                                    RegistrationRequestDateTimeStr = cdtstr,
                                    RegistrationResponse = "",
                                    PendingRegistrationGUID = Guid.NewGuid().ToString(),
                                    PendingRegConfirmed = false,
                                    PendingRegConfirmationIP = "",
                                    PendingRegConfirmationDateTime = 0,
                                    PendingRegConfirmationDateTimeStr = ""
                                };
                                //добавляем запись в БД
                                waterdevices.Add(newwaterdevice);
                                dc.SaveChanges();
                                Logger.AccountLog(Request.UserHostAddress, "Принята заявка на регистрацию нового устройства", newwaterdevice.HardwareID, tmpacc.ID);
                                Logger.SystemLog(Request.UserHostAddress, "Регистрация нового устройства", tmpacc.UserID, "Server");
                                MailMessage mail = new MailMessage();
                                mail.To.Add(tmpacc.UserID);
                                mail.From = new MailAddress(WWWVars.MailFromAddress, WWWVars.EMailDisplayName, Encoding.UTF8);
                                mail.Subject = WWWVars.RegDeviceMailSubject;
                                mail.SubjectEncoding = Encoding.UTF8;
                                string confirmurl = output + "/User/ConfirmRegistration.aspx?wvdid=" + newwaterdevice.ID.ToString() + "&prg=" +
                                    newwaterdevice.PendingRegistrationGUID;
                                mail.Body = "Добрый день, уважаемый\\ая сэр\\мадам.<br>" +
                                    "<p>В систему добавлена заявка на регистрацию нового устройства. Для подтверждения пройдите по ссылке ниже:<br>" +
                                    "<a href=\"" + confirmurl + "\">" + confirmurl + "</a><br>" +
                                    "<b><font color=\"red\">Внимание: Ссылка будет активной в течение 3 часов!<br> Если вы не завершите регистрацию до " + DateTime.Now.AddHours(3).ToString("dd.MM.yyyy HH:mm:ss") +
                                    ", процедуру регистрации необходимо будет повторить на устройстве с использованием файла лицензии.</font></b><br></p>" +
                                    "=======================================<br>" +
                                    "письмо сгенерировано автоматически, отвечать на него не нужно. Контакты для обратной связи см. на сайте: <br>" + output;
                                mail.BodyEncoding = Encoding.UTF8;
                                mail.IsBodyHtml = true;
                                mail.Priority = MailPriority.High;
                                SmtpClient client = new SmtpClient
                                {

                                    UseDefaultCredentials = !WWWVars.MailUseSMTPAuth,
                                    Port = WWWVars.SMTPPort,
                                    Host = WWWVars.SMTPHost,
                                    EnableSsl = WWWVars.SMTPUseSSL,
                                };
                                if (!client.UseDefaultCredentials) client.Credentials = new System.Net.NetworkCredential(WWWVars.MailLogin, WWWVars.MailPassword);
                                client.Send(mail);
                                authresp.AuthResponse = "OK_PENDING";
                                Logger.AccountLog("Server", "Отправлено электронное письмо о регистрации нового устройства", tmpacc.UserID, tmpacc.ID);
                                Logger.SystemLog("Server", "Отправлено письмо о регистрации нового устройства", tmpacc.UserID, "Server");
                            }
                            //превышен лимит лицензий, отказ регистрации
                            else
                            {
                                authresp.Signature = "";
                                authresp.AuthResponse = "DENY_OVERQUOTA";
                            }
                        } else
                        //устройство уже есть в БД, ничего не делаем
                        {

                        }
                    } else
                    //подпись некорректна, отказ регистрации
                    {
                        authresp.Signature = "";
                        authresp.AuthResponse = "DENY_SIGNATURE_MISMATCH";
                    }
                    //сохраняем изменения в БД
                    dbContextTransaction.Commit();
                }
                catch (Exception ex)
                {
                    //что-то пошло не так, ошибка на любом этапе, прерываем регистрацию, откатываем изменения в БД
                    if (ex.HResult != -2146233040)
                    {
                        dbContextTransaction.Rollback();
                        authresp.Signature = "";
                        authresp.AuthResponse = "ABORT CODE: " + ex.HResult.ToString();
                        Logger.SystemLog(Request.UserHostAddress, "Ошибка при регистрации устройства: " + ex.Message, "", "Server");
                    }
                }
                finally
                {
                    //Сериализуем объект авторизации в XML документ
                    var xs = new XmlSerializer(authresp.GetType());
                    var xml = new Utf8StringWriter();
                    xs.Serialize(xml, authresp);
                    //переводим в массив байт, кодируем в Base64 для передачи по http
                    byte[] xmlbytes = Encoding.UTF8.GetBytes(xml.ToString());
                    string xmlbytesbase64 = Convert.ToBase64String(xmlbytes);
                    //отсылаем ответ устройству
                    Response.Clear();
                    Response.Write(xmlbytesbase64);
                }
            }
        }
    }
}