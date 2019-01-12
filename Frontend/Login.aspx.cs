using System;
using System.Linq;
using System.Text;
using System.Web.Security;
using TwoFactorAuthNet;
using System.Security.Cryptography;

public partial class Login : System.Web.UI.Page
{
    TwoFactorAuth tfa = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.IsAuthenticated)
        {
            Response.Redirect("~/User/User.aspx",true);
        }
        totpbox.Visible = false;
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        byte[] HashedPsssword = new byte[] { };
        string passhash = "";
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                var useraccs = dc.Set<Accounts>();
                string userid = Request.Form["userid"];
                string userpass = Request.Form["userpass"];
                SHA512 shaM = new SHA512Managed();
                HashedPsssword = shaM.ComputeHash(Encoding.UTF8.GetBytes(Request.Form["userpass"]));
                passhash = Convert.ToBase64String(HashedPsssword);
                cp3.ValidateCaptcha(captchatext.Text);
                captchatext.Text = "";
                if (!cp3.UserValidated)
                {
                    loginmsg.Text = "Введено некорректное значение числа с картинки";
                    Logger.AccountLog(Request.UserHostAddress, "Введено некорректное значение числа с картинки", "", -1);
                    Logger.SystemLog(Request.UserHostAddress, "Ошибка: неверное значение числа с картинки", userid, "Server");
                    return;
                }
                Accounts useracc = useraccs.First(x => x.UserID == userid && x.PasswordHash == passhash && x.Valid && !x.Suspended);
                Logger.AccountLog(Request.UserHostAddress, "Введены верные учетные данные", "", useracc.ID);
                Logger.SystemLog(Request.UserHostAddress, "Введены верные учетные данные", useracc.UserID, "Server");
                if (useracc.TOTPSecret != "")
                {
                    Session["userid"] = userid;
                    loginbox.Visible = false;
                    totpbox.Visible = true;
                    totp.Focus();
                } else
                {
                    FormsAuthentication.RedirectFromLoginPage(useracc.UserID, false);
                }
            }
            catch
            {
                loginmsg.Text = "Учетная запись не найдена или неактивна, либо введены неверные данные";
                Logger.SystemLog(Request.UserHostAddress, "Ошибка: неверные учетные данные", (string)Session["userid"], "Server");
            }
        }
    }



    protected void Unnamed1_Click(object sender, EventArgs e)
    {
        tfa = new TwoFactorAuth(WWWVars.SiteName);
        Accounts useracc = null;
        string userid = (string)Session["userid"];
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            var useraccs = dc.Set<Accounts>();
            useracc = useraccs.First(x => x.UserID == userid);
        }
        if (useracc.TOTPSecret != "" && totp.Text != null)
        {
            if (tfa.VerifyCode(useracc.TOTPSecret, totp.Text))
            {
                Session["userid"] = "";
                FormsAuthentication.RedirectFromLoginPage(useracc.UserID, false);
                Logger.AccountLog(Request.UserHostAddress, "Доступ предоставлен", "Введен правильный одноразовый код", useracc.ID);
                Logger.SystemLog(Request.UserHostAddress, "Доступ в систему предоставлен", useracc.UserID, "Server");
            }
            else
            {
                totpmsg.Text = "Неверный одноразовый пароль";
                loginbox.Visible = false;
                totpbox.Visible = true;
                Logger.AccountLog(Request.UserHostAddress, "Доступ запрещен", "Введен неправильный одноразовый код", useracc.ID);
                Logger.SystemLog(Request.UserHostAddress, "Ошибка: неверный одноразовый код", useracc.UserID, "Server");
            }
        }
    }
}
