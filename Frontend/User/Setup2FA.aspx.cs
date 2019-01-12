using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using TwoFactorAuthNet;

public partial class Setup2FA : System.Web.UI.Page
{
    TwoFactorAuth tfa = null;
    protected void Page_Load(object sender, EventArgs e)
    {
        Accounts useracc = null;
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            var useraccs = dc.Set<Accounts>();
            useracc = useraccs.First(x => x.UserID == User.Identity.Name);
        }
        if (IsPostBack) return;
        if (useracc.TOTPSecret == "")
        {
            tfa = new TwoFactorAuth(WWWVars.SiteName);
            string secret = tfa.CreateSecret(160);
            totps.ImageUrl = tfa.GetQrCodeImageAsDataUri(User.Identity.Name, secret, 200);
            Session["totps"] = secret;
            twofasetupcompletebox.Visible = false;
        } else
        {
            twofasetupcompletebox.Visible = true;
            twofasetupbox.Visible = false;
        }
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        tfa = new TwoFactorAuth(WWWVars.SiteName);
        string totpsecret = "";
        Accounts useracc = null;
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            var useraccs = dc.Set<Accounts>();
            useracc = useraccs.First(x => x.UserID == User.Identity.Name);
        }
        totpsecret = (string)Session["totps"];
        if (totpsecret != "" && (string)Request.Form["totp"] != null)
        {
            if (tfa.VerifyCode(totpsecret, (string)Request.Form["totp"]))
            {
                using (VendingModelContainer dc = new VendingModelContainer())
                {
                    try
                    {
                        var useraccs = dc.Set<Accounts>();
                        Accounts useracc2 = useraccs.First(x => x.UserID == User.Identity.Name);
                        useracc2.TOTPSecret = (string)Session["totps"];
                        Session["totps"] = "";
                        dc.SaveChanges();
                        twofasetupbox.Visible = false;
                        twofasetupcompletebox.Visible = true;
                        Logger.AccountLog(Request.UserHostAddress, "Завершена настройка двухфакторной авторизации", "", useracc2.ID);
                        Logger.SystemLog(Request.UserHostAddress, "Пользователь настроил двухфакторную авторизацию", useracc2.UserID, "Server");
                    }
                    catch (Exception ex)
                    {
                        totpmsg.Text = "Произошла ошибка, попробуйте еще раз!";
                        Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                    }
                }
            }
            else 
            {
                totpmsg.Text = "Неверный одноразовый пароль!";
            }
        }
    }

    protected void Button2_Click(object sender, EventArgs e)
    {
        
    }
}