using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Web.UI.HtmlControls;

public partial class MasterPage : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.IsAuthenticated)
        {
            loginlogout.InnerText = HttpContext.Current.User.Identity.Name;
            loginlink.Attributes["class"] = "home_menulauthorized";
            //AddMainMenuItem("Обзор", "/User/User.aspx", "AccountOverviewMenuItem");
            //AccountOverviewMenuItem.Visible = true;
            //DeviceListMenuItem.Visible = true;
         }
        else
        {
            //AccountOverviewMenuItem.Visible = false;
            //DeviceListMenuItem.Visible = false;
        }
        SetCurrentMenuItem();
    }

    public void RemoveMainMenuItem(string ID)
    {
        try
        {

        }
        catch
        {

        }
    }

    /// <summary>
    /// Recursive FindControl method, to search a control and all child
    /// controls for a control with the specified ID.
    /// </summary>
    /// <returns>Control if found or null</returns>
    private Control FindControlRecursive(Control root, string id)
    {
        if (id == string.Empty)
            return null;

        if (root.ClientID == id)
            return root;

        foreach (Control c in root.Controls)
        {
            Control t = FindControlRecursive(c, id);
            if (t != null)
            {
                return t;
            }
        }
        return null;
    }

    public void SetCurrentMenuItem()
    {
        try
        {
            string s = Page.GetType().ToString();
            switch (s)
            {
                case "ASP.user_user_aspx":
                    {
                        //AccountOverviewMenuItem.Attributes.Add("class", "current");
                        break;
                    }
                case "ASP.user_devicelist_aspx":
                    {
                        //DeviceListMenuItem.Attributes.Add("class", "current");
                        break;
                    }
                case "ASP.default_aspx":
                    {
                        //HomeMenuItem.Attributes.Add("class", "current");
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, "Server");
        }
    }

}
