using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.Security;

public partial class User_User : System.Web.UI.Page
{
    MenuItem globalmenuitemhandler;
    protected void Page_Load(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                globalmenuitemhandler = AccountMenu.SelectedItem;
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
                double totalcash =0, totalincasso = 0;
                int onlinedevcount = 0;
                try
                {
                    long cdt = Convert.ToInt64(DateTime.Now.AddMinutes(-10).ToString("yyyyMMddHHmmss"));
                    var userdevices = dc.WaterDevices.Where(x => x.AccountID == tmpacc.ID).Select(y => y.ID).ToList();
                    totalincasso = (double)dc.WaterDeviceState.Where(x => userdevices.Contains(x.ID)).Sum(y => y.IncassoSum);
                    totalcash = (double)dc.WaterDeviceState.Where(x => userdevices.Contains(x.ID)).Sum(y => (y.IncassoSum + y.CCSum));
                    onlinedevcount = dc.WaterDeviceState.Count(x => x.DateTime > cdt);
                }
                catch
                {

                }
                if (tmpacc.UserID == WWWVars.AdminEmail)
                {
                    Response.Redirect("/Manager/Summary.aspx");
                }
                if (tmpacc.LicenseContent != "")
                {
                    licdownloadbutton.Enabled = true;
                    LicenseImage.ImageUrl = "/images/license.png";
                }
                else
                {
                    licdownloadbutton.Text = "Лицензия отсутствует";
                    licdownloadbutton.Enabled = false;
                    LicenseImage.ImageUrl = "/images/licensegrayscale.png";
                }
                useridlabel.Text = tmpacc.UserID;
                paidtilllabel.Text = tmpacc.PaidTillDateTimeStr;
                maxregslabel.Text = tmpacc.DeviceCountLimit.ToString();
                phonenumberlabel.Text = tmpacc.DefaultContactPhone;
                string twofaenabled = "";
                setup2falink.Visible = (tmpacc.TOTPSecret == "");
                if (tmpacc.TOTPSecret != "") twofaenabled = "Да"; else twofaenabled = "Нет";
                twofaenabledlabel.Text = twofaenabled;
                regdatetimelabel.Text = tmpacc.RegistrationDateTimeStr;
                string accenabled = "";
                if (!tmpacc.Suspended) accenabled = "Да"; else accenabled = "Нет";
                accactivelabel.Text = accenabled;
                onlinedevcountlabel.Text = onlinedevcount.ToString();
                totalcashlabel.Text = String.Format("{0} / {1}", totalcash.ToString("N2"), totalincasso.ToString("N2"));
                licensedlabel.Text = dc.WaterDevices.Where(x => x.AccountID == tmpacc.ID).Count().ToString();
            }
            catch (Exception ex)
            {
                Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
            }
        }
    }

    protected override void OnPreInit(EventArgs e)
    {
        
        base.OnPreInit(e);
    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        AccLicense tmpal = new AccLicense(2);
    }

    protected void licdownloadbutton_Click(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
                Response.Clear();
                Response.AddHeader("content-disposition", "attachment; filename=wvlicense.lic");
                Response.AddHeader("content-type", "text/plain");
                Response.Write(tmpacc.LicenseContent);
                Response.End();
            }
            catch (Exception ex)
            {
                Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
            }
        }
    }

    protected void accexitbutton_Click(object sender, EventArgs e)
    {
        FormsAuthentication.SignOut();
        Response.Redirect("~/Default.aspx");
    }

    protected void AccountMenu_MenuItemClick(object sender, MenuEventArgs e)
    {
        globalmenuitemhandler = e.Item;
        if (globalmenuitemhandler.Value != "licence") licpanel.Visible = false;
        switch (globalmenuitemhandler.Value)
        {
            case "licence":
                {
                    licpanel.Visible = true;
                    break;
                }
            case "mydevlist":
                {
                    using (VendingModelContainer dc = new VendingModelContainer())
                    {
                        Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
                        List<WaterDevices> tmpdevlist = dc.WaterDevices.Where(x => x.AccountID == tmpacc.ID).ToList();
                        for (int i = 0; i < tmpdevlist.Count; i++)
                        {
                            try
                            {
                                WaterDevices tmpdev = tmpdevlist[i];
                                WaterDeviceTelemetry tmpdevtelemetry = dc.WaterDeviceTelemetry.Where(x => x.WaterDeviceID == tmpdev.ID).OrderByDescending(y => y.DateTime).First();
                                TableRow devtablerow = new TableRow();
                                if (i % 2 == 1) devtablerow.BackColor = System.Drawing.Color.FromArgb(0x53, 0x00, 0xA6);
                                else
                                    devtablerow.BackColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x99);
                                devtablerow.Height = 60;
                                TableCell devidcell = new TableCell();
                                devidcell.Width = 100;
                                devidcell.Height = 60;
                                devidcell.HorizontalAlign = HorizontalAlign.Center;
                                devidcell.VerticalAlign = VerticalAlign.Middle;
                                devidcell.Text = "<a href=\"/User/WaterDeviceInfo.aspx?wvdid=" + tmpdev.ID.ToString() + "\">" + tmpdev.ID.ToString() + "</a>";
                                TableCell adresscell = new TableCell();
                                adresscell.Width = 390;
                                adresscell.Height = 60;
                                adresscell.HorizontalAlign = HorizontalAlign.Center;
                                adresscell.VerticalAlign = VerticalAlign.Middle;
                                adresscell.Text = tmpdev.LocationAddress;
                                TableCell onlinecell = new TableCell();
                                onlinecell.Width = 60;
                                onlinecell.Height = 60;
                                onlinecell.HorizontalAlign = HorizontalAlign.Center;
                                onlinecell.VerticalAlign = VerticalAlign.Middle;
                                DateTime lastlogdt = DateTime.ParseExact(tmpdevtelemetry.DateTime.ToString(), "yyyyMMddHHmmss",
                                               System.Globalization.CultureInfo.InvariantCulture);
                                if (lastlogdt.AddMinutes(5) < DateTime.Now)
                                {
                                    onlinecell.Text = string.Format("<img src=\"/images/disconnected-64.png\" />");
                                }
                                else
                                {
                                    onlinecell.Text = string.Format("<img src='/images/connected-64.png' />");
                                }
                                TableCell statuscell = new TableCell();
                                statuscell.Width = 70;
                                statuscell.Height = 60;
                                statuscell.HorizontalAlign = HorizontalAlign.Center;
                                statuscell.VerticalAlign = VerticalAlign.Middle;
                                if (tmpdevtelemetry.KKTPrinterNonRecoverableError || tmpdevtelemetry.KKTPrinterPaperEnding || tmpdevtelemetry.KKTPrinterPaperJammed ||
                                    tmpdevtelemetry.KKTPrinterPaperEmpty || tmpdevtelemetry.KKTPrinterCutterError || tmpdevtelemetry.KKTStageOver24h || tmpdevtelemetry.WaterLevelPercent < 20 ||
                                    tmpdevtelemetry.BABillsCount > 300 || tmpdevtelemetry.WaterTempCelsius < 4 || tmpdevtelemetry.CC10RURCount < 20 || tmpdevtelemetry.CC5RURCount < 20 ||
                                    tmpdevtelemetry.CC2RURCount < 20 || tmpdevtelemetry.CC1RURCount < 20 || tmpdevtelemetry.VMCMode != "1" || tmpdevtelemetry.MDBInitStep != 5 || tmpdevtelemetry.KKTCurrentMode != 1)
                                {
                                    statuscell.Text = string.Format("<img src=\"/images/warning.png\" />");
                                }
                                else
                                {
                                    statuscell.Text = string.Format("<img src='/images/OK.png' />");
                                }
                                TableCell incassosumcell = new TableCell();
                                incassosumcell.Width = 120;
                                incassosumcell.Height = 60;
                                incassosumcell.HorizontalAlign = HorizontalAlign.Center;
                                incassosumcell.VerticalAlign = VerticalAlign.Middle;
                                incassosumcell.Text = tmpdevtelemetry.IncassoSum.ToString();
                                TableCell totalsumcell = new TableCell();
                                totalsumcell.Width = 120;
                                totalsumcell.Height = 60;
                                totalsumcell.HorizontalAlign = HorizontalAlign.Center;
                                totalsumcell.VerticalAlign = VerticalAlign.Middle;
                                totalsumcell.Text = (tmpdevtelemetry.IncassoSum + tmpdevtelemetry.CCSum).ToString();
                                devtablerow.Cells.AddRange(new TableCell[] { devidcell, adresscell, onlinecell, statuscell, incassosumcell, totalsumcell });
                                devlist.Rows.Add(devtablerow);
                            }
                            catch (Exception ex)
                            {
                                Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                            }
                        }
                    }
                    devlistpanel.Visible = true;
                    break;
                }
            case "exit":
                {
                    //vmcpanel.Visible = true;
                    FormsAuthentication.SignOut();
                    using (VendingModelContainer dc = new VendingModelContainer())
                    {
                        try
                        {
                            Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
                            Logger.AccountLog(Request.UserHostAddress, "Выход из системы", "", tmpacc.ID);
                            Logger.SystemLog(Request.UserHostAddress, "Выход из системы", tmpacc.UserID, "Server");
                        }
                        catch (Exception ex)
                        {
                            Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                        }
                    }
                    Response.Redirect("~/Login.aspx");
                    break;
                }
        }
    }
}