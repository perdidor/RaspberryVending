using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class User_ChangeDeviceAddress : System.Web.UI.Page
{
    long wvdid;
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            long tmpwvdid = Convert.ToInt64(Request.QueryString["wvdid"]);
            using (VendingModelContainer dc = new VendingModelContainer())
            {
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
                WaterDevices tmpdev = dc.WaterDevices.First(x => x.ID == tmpwvdid && x.AccountID == tmpacc.ID);
                wvdid = tmpdev.ID;
                wvdidtext.Value = wvdid.ToString();
                if (!Page.IsPostBack)
                {
                    latitude.Value = tmpdev.LocationLatitude.ToString("N6").Replace(',','.');
                    longtitude.Value = tmpdev.LocationLongtitude.ToString("N6").Replace(',', '.');
                    addressbox.Text = tmpdev.LocationAddress;
                }
            }
            Title += wvdid.ToString();
            if (!ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                
            }
        }
        catch (Exception ex)
        {
            Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, "Server");
        }
    }

    protected void devaddressavebutton_Click(object sender, EventArgs e)
    {
        try
        {
            using (VendingModelContainer dc = new VendingModelContainer())
            {
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
                WaterDevices tmpdev = dc.WaterDevices.First(x => x.ID == wvdid && x.AccountID == tmpacc.ID);
                string oldaddress = tmpdev.LocationAddress;
                tmpdev.LocationAddress = addressbox.Text;
                tmpdev.LocationLatitude = Math.Round(Convert.ToDecimal(latitude.Value.Replace('.',',')),6);
                tmpdev.LocationLongtitude = Math.Round(Convert.ToDecimal(longtitude.Value.Replace('.', ',')), 6);
                tmpdev.SettingsVersion++;
                dc.SaveChanges();
                Response.Clear();
                StringBuilder sb = new StringBuilder();
                sb.Append("<html>");
                sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
                sb.AppendFormat("<form name='form' action='{0}' method='post'>", "/User/WaterDeviceInfo.aspx?wvdid=" + wvdid.ToString());
                sb.AppendFormat("<input type='hidden' name='successheadertext' value='{0}'>", "Команда выполнена");
                sb.AppendFormat("<input type='hidden' name='successmsgtext' value='{0}'>", "Адрес устройства успешно изменен");
                // Other params go here
                sb.Append("</form>");
                sb.Append("</body>");
                sb.Append("</html>");
                Response.Write(sb.ToString());
                Response.End();
                Logger.AccountLog(Request.UserHostAddress, "Изменен адрес устройства №" + wvdid.ToString(), "Старый адрес: " + oldaddress + Environment.NewLine + "Новый адрес: " + tmpdev.LocationAddress, tmpacc.ID);
                Logger.SystemLog(Request.UserHostAddress, "Изменен адрес устройства №" + wvdid.ToString(), "", "Server");
                //Response.Redirect("/User/WaterDeviceInfo.aspx?wvdid=" + wvdid.ToString());
            }
        }
        catch (Exception ex)
        {
            Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, "Server");
        }
    }

    protected void addresscancelbutton_Click(object sender, EventArgs e)
    {
        Response.Redirect("/User/WaterDeviceInfo.aspx?wvdid=" + wvdid.ToString());
    }
}