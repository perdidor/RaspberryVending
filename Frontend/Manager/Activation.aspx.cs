using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Manager_Activation : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == User.Identity.Name && x.Valid && !x.Suspended);
                if (tmpacc.UserID != WWWVars.AdminEmail)
                {
                    Response.Redirect("~/User/User.aspx");
                    return;
                }
                if (tmpacc.TOTPSecret == "")
                {
                    Response.Redirect("~/User/Setup2FA.aspx");
                    return;
                }
                LinqDataSource1.WhereParameters.Add(new Parameter("admemail", TypeCode.String, WWWVars.AdminEmail));
            }
            catch (Exception ex)
            {
                Response.Redirect("~/User/User.aspx");
                Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, "Server");
                return;
            }
        }
    }


    protected void waitingrepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        //Accounts row = (Accounts)e.Item.DataItem;
        //if (row != null)
        //{
        //    e.Item.FindControl("activecb").ID = "activecb" + row.ID.ToString();
        //    e.Item.FindControl("devlimit").ID = "devlimit" + row.ID.ToString();
        //    e.Item.FindControl("suspendedcb").ID = "suspendedcb" + row.ID.ToString();
        //    e.Item.FindControl("savebutton").ID = "savebutton" + row.ID.ToString();
        //}
        if (waitingrepeater.Items.Count < 1)
        {
            if (e.Item.ItemType == ListItemType.Footer)
            {
                Label lblFooter = (Label)e.Item.FindControl("nodatalabel");
                lblFooter.Visible = true;
            }
        } else
        {
            
        }
    }

    protected void waitingrepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        switch (e.CommandName)
        {
            case "Activate":
                {
                    using (VendingModelContainer dc = new VendingModelContainer())
                    {
                        try
                        {
                            string tmpuserid = ((Label)(e.Item.FindControl("email"))).Text;
                            Accounts tmpacc = dc.Accounts.First(x => x.UserID == tmpuserid);
                            tmpacc.Suspended = ((CheckBox)(e.Item.FindControl("suspendedcb"))).Checked;
                            tmpacc.Valid = ((CheckBox)(e.Item.FindControl("activecb"))).Checked;
                            tmpacc.DeviceCountLimit = Convert.ToInt32(((TextBox)(e.Item.FindControl("devlimit"))).Text);
                            dc.SaveChanges();
                            if (tmpacc.DeviceCountLimit > 0)
                            {
                                AccLicense tmplic = new AccLicense(tmpacc.ID);
                            }
                            Logger.AccountLog(Request.UserHostAddress, "Акаунт активирован", "", tmpacc.ID);
                            Logger.SystemLog(Request.UserHostAddress, "Активация нового акаунта", tmpacc.UserID, User.Identity.Name);
                        }
                        catch (Exception ex)
                        {
                            Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, "Server");
                        }
                    }
                    break;
                }
            case "DeleteAcc":
                {
                    using (VendingModelContainer dc = new VendingModelContainer())
                    {
                        try
                        {
                            string tmpuserid = ((Label)(e.Item.FindControl("email"))).Text;
                            Accounts tmpacc = dc.Accounts.First(x => x.UserID == tmpuserid);
                            dc.Accounts.Remove(tmpacc);
                            dc.SaveChanges();
                            Logger.AccountLog(Request.UserHostAddress, "Акаунт удален", "", tmpacc.ID);
                            Logger.SystemLog(Request.UserHostAddress, "Удаление акаунта", tmpacc.UserID, User.Identity.Name);
                        }
                        catch (Exception ex)
                        {
                            Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, "Server");
                        }
                    }
                    break;
                }
        }
        waitingrepeater.DataBind();
    }

    protected void waitingrepeater_ItemCreated(object sender, RepeaterItemEventArgs e)
    {

    }
}