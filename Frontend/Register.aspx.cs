using System;
using System.Security.Cryptography;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web.UI;

public partial class Register : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        regdonebox.Visible = false;
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                if (dc.Accounts.Count() == 0)
                {
                    ScriptManager.RegisterClientScriptBlock((sender as Control), this.GetType(), "alert", "alert('Сервер не настроен!')", true);
                    regbox.Visible = false;
                }
            }
            catch
            {

            }
        }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        DateTime dt = DateTime.Now;
        long cdt = Convert.ToInt64(dt.ToString("yyyyMMddHHmmss"));
        string cdtstr = dt.ToString("dd.MM.yyyy HH:mm:ss");
        int a = 0;
        cp2.ValidateCaptcha(captchatext.Text);
        captchatext.Text = "";
        if (!cp2.UserValidated)
        {
            a += 1;
        }
        if (!IsEmailSyntaxValid(email.Text))
        {
            a += 2;
        }
        if ((!System.Text.RegularExpressions.Regex.IsMatch(phone.Text, @"^\d+$")) | (phone.Text.Length < 10) | (phone.Text.Length > 10))
        {
            a += 4;
        }
        if (userpass1.Text != userpass2.Text || userpass1.Text.Length < 8 || userpass2.Text.Length < 8)
        {
            a += 8;
        }
        if (a == 0)
        {
            try
            {
                using (VendingModelContainer dc = new VendingModelContainer())
                {
                    Accounts tmpacc = null;
                    try
                    {
                        tmpacc = dc.Accounts.First(x => x.UserID == email.Text);
                    }
                    catch
                    {

                    }
                    if (tmpacc == null)
                    {
                        if (dc.Accounts.Count() == 0)
                        {
                            a += 128;
                            throw new Exception();
                        }
                        tmpacc = new Accounts
                        {
                            RegistrationDateTime = cdt,
                            RegistrationDateTimeStr = cdtstr,
                            DefaultContactPhone = string.Concat("+7", phone.Text),
                            DeviceCountLimit = 0,
                            LicenseContent = "",
                            PaidTillDateTime = 0,
                            PaidTillDateTimeStr = "",
                            Suspended = true,
                            Valid = false,
                            TOTPSecret = "",
                            UserID = email.Text
                        };
                        SHA512 shaM = new SHA512Managed();
                        byte[] HashedPsssword = shaM.ComputeHash(Encoding.UTF8.GetBytes(userpass1.Text));
                        tmpacc.PasswordHash = Convert.ToBase64String(HashedPsssword);
                        dc.Accounts.Add(tmpacc);
                        dc.SaveChanges();
                        Logger.AccountLog(Request.UserHostAddress, "Заявка на регистрацию принята", "Требуется активация администратором сервера", tmpacc.ID);
                        Logger.SystemLog(Request.UserHostAddress, "Регистрация нового акаунта", tmpacc.UserID, "Server");
                        try
                        {
                            //Uri url = new Uri(WWWVars.ServerEndPoint,);
                            //string output = url.GetLeftPart(UriPartial.Authority);
                            var url = new UriBuilder(WWWVars.ServerEndPoint);
                            var output = url.Uri;
                            MailMessage mail = new MailMessage();
                            mail.To.Add(tmpacc.UserID);
                            mail.From = new MailAddress(WWWVars.MailFromAddress, WWWVars.EMailDisplayName, Encoding.UTF8);
                            mail.Subject = WWWVars.RegAccountMailSubject;
                            mail.SubjectEncoding = Encoding.UTF8;
                            mail.Body = "Добрый день, уважаемый\\ая сэр\\мадам.<br>" +
                                "<p>В систему добавлена заявка на регистрацию нового акаунта. Ваш адрес электронной почты указан в качестве контактного.<br>" +
                                "В течение 3 рабочих дней администрация ресурса свяжется с вами для активации вашего личного кабинета.</p>" +
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
                            Logger.AccountLog("Server", "Отправлено электронное письмо", tmpacc.UserID, tmpacc.ID);
                            Logger.SystemLog("Server", "Отправлено письмо с подтверждением регистрации акаунта", tmpacc.UserID, "Server");
                        }
                        catch
                        {
                            a += 64;
                        }
                    } else
                    {
                        a += 16;
                    }
                }
            }
            catch
            {
                a += 32;
            }
        }
        if (a == 0)
        {
            regbox.Visible = false;
            regdonebox.Visible = true;
        }
        else
        {
            loginmsg.Text = "Ошибка " + a.ToString();
        }
    }

    private bool IsEmailSyntaxValid(string emailToValidate)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(emailToValidate,
            @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
    }

}