using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;
using System.Security.Cryptography;

public partial class User_ConfirmRegistration : System.Web.UI.Page
{
    /// <summary>
    /// При сериализации в xml переопределяем кодировку по умолчанию, чтобы не было проблем с русскими буквами.
    /// </summary>
    private class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding { get { return Encoding.UTF8; } }
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            using (var dbContextTransaction = dc.Database.BeginTransaction())
            {
                try
                {
                    long wvdid = Convert.ToInt64(Request.QueryString["wvdid"]);
                    string prg = Request["prg"];
                    if (prg != null && wvdid != 0)
                    {
                        DateTime dt = DateTime.Now;
                        //убиваем неподтвержденные заявки на регистрацию, которым больше 3 часов. кто не успел - тот опоздал, блять)
                        long regrequestoldestdatetime = Convert.ToInt64(dt.AddHours(-3).ToString("yyyyMMddHHmmss"));
                        dc.WaterDevices.RemoveRange(dc.WaterDevices.Where(x => x.RegistrationRequestDateTime < regrequestoldestdatetime));
                        long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
                        string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
                        WaterDevices tmpwd = dc.WaterDevices.Where(x => x.PendingRegistration && x.PendingRegistrationGUID == prg && x.ID == wvdid && !x.PendingRegConfirmed).First();
                        CryptoHelper ch = new CryptoHelper();
                        SHA512 sha512 = new SHA512Managed();
                        //вычисляем хеш открытого ключа устройства
                        byte[] hash = sha512.ComputeHash(tmpwd.PublicKey);
                        //подписываем хеш открытого ключа устройства и формируем ответ. В ответе указываем текущее распределение лицензий с учетом текущей
                        Uri url = new Uri(Request.Url.Authority);
                        string output = url.GetLeftPart(UriPartial.Authority);
                        WaterDeviceRegistrationResponse authresp = new WaterDeviceRegistrationResponse
                        {
                            RegID = tmpwd.ID,
                            ServerEndPoint = output
                        };
                        authresp.Signature = Convert.ToBase64String(ch.SignHashedData(hash));
                        int activedevicescount = dc.WaterDevices.Where(x => x.AccountID == tmpwd.AccountID && x.Valid && !x.Suspended && !x.PendingRegistration).Count();
                        int pendingdevicescount = dc.WaterDevices.Where(x => x.AccountID == tmpwd.AccountID && x.Valid && !x.Suspended && x.PendingRegistration).Count();
                        Accounts tmpacc = dc.Accounts.Where(x => x.ID == tmpwd.AccountID && x.Valid && !x.Suspended).First();
                        authresp.AuthResponse = "SUCCESS " + (activedevicescount + 1).ToString() + "/" +
                            (pendingdevicescount - 1).ToString() + "/" + tmpacc.DeviceCountLimit.ToString();
                        //Сериализуем объект авторизации в XML документ
                        var xs = new XmlSerializer(authresp.GetType());
                        var xml = new Utf8StringWriter();
                        xs.Serialize(xml, authresp);
                        tmpwd.RegistrationResponse = xml.ToString();
                        tmpwd.PendingRegistration = false;
                        tmpwd.PendingRegConfirmationIP = Request.UserHostAddress;
                        tmpwd.PendingRegConfirmed = true;
                        tmpwd.PendingRegConfirmationDateTime = cdt;
                        tmpwd.PendingRegConfirmationDateTimeStr = cdtstr;
                        tmpwd.RegistrationDateTime = cdt;
                        tmpwd.RegistrationDateTimeStr = cdtstr;
                        dc.SaveChanges();
                        dbContextTransaction.Commit();
                        Logger.AccountLog(Request.UserHostAddress, "Подтверждение регистрации устройства №" + tmpwd.ID + " получено", tmpwd.HardwareID, tmpacc.ID);
                        Logger.SystemLog(Request.UserHostAddress, "Пользователь подтвердил регистрацию устройства №" + tmpwd.ID, tmpacc.UserID, "Server");
                        confirmresult.ForeColor = System.Drawing.Color.Lime;
                        confirmresult.Text = "Подтверждение получено";
                        MailMessage mail = new MailMessage();
                        mail.To.Add(tmpacc.UserID);
                        mail.From = new MailAddress(WWWVars.MailFromAddress, WWWVars.EMailDisplayName, Encoding.UTF8);
                        mail.Subject = "Регистрация устройства завершена";
                        mail.SubjectEncoding = Encoding.UTF8;
                        mail.Body = "Добрый день, уважаемый\\ая сэр\\мадам.<br>" +
                                "<p>В системе успешно зарегистрировано новое устройство: <br>" +
                                "Номер: " + tmpwd.ID.ToString() + "<br>" +
                                "Идентификатор: " + tmpwd.PendingRegistrationGUID + "<br>" +
                                "Для завершения регистрации устройство должно быть включено и подключено в Интернету.<br><p>" +
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
                        Logger.AccountLog("Server", "Отправлено письмо с подтверждением регистрации устройства", tmpacc.UserID, tmpacc.ID);
                        Logger.SystemLog("Server", "Отправлено письмо с подтверждением регистрации устройства", tmpacc.UserID, "Server");
                    }
                }
                catch (Exception ex)
                {
                    confirmresult.ForeColor = System.Drawing.Color.Red;
                    confirmresult.Text = "Ошибка: " + ex.HResult.ToString();
                    Logger.SystemLog(Request.UserHostAddress, "Ошибка при отправке письма с подтверждением регистрации устройства", ex.Message, "Server");
                }
            }
        }
        
    }
}