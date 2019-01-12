using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Web;
using System.Web.Security;

public partial class Manager_Summary : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == User.Identity.Name);
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
                if (!IsPostBack)
                {
                    FillSummary();
                    FillUsersChart();
                    FillDevicesChart();
                    FillServerChart();
                    userschartupdatetimer.Enabled = true;
                }
            }
            catch
            {
                Response.Redirect("~/User/User.aspx");
                return;
            }
        }
    }

    private void FillSummary()
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                long lastcdt = Convert.ToInt64(DateTime.Now.AddMinutes(-5).ToString("yyyyMMddHHmmss"));
                long hcdt = Convert.ToInt64(DateTime.Now.AddHours(-1).ToString("yyyyMMddHHmmss"));
                totalaccslink.Text = dc.Accounts.Count().ToString();
                suspendedaccslink.Text = dc.Accounts.Count(x => x.Suspended).ToString();
                awaitingaccslink.Text = dc.Accounts.Count(x => x.Suspended && !x.Valid).ToString();
                totaldevslink.Text = dc.WaterDevices.Count().ToString();
                onlinedevslink.Text = dc.WaterDeviceTelemetry.Where(x => x.DateTime > lastcdt).Select(y => y.ID).Distinct().Count().ToString();
                qphlink.Text = dc.WaterDeviceTelemetry.Count(x => x.DateTime > hcdt).ToString();
                evphlink.Text = dc.SystemLog.Count(x => x.DateTime > hcdt).ToString();
                errphlink.Text = dc.SystemLog.Count(x => x.DateTime > hcdt && x.EventText.Contains("ошибка")).ToString();
            }
            catch (Exception ex)
            {

            }
        }
    }

    private void FillUsersChart()
    {
        DateTime cdt = DateTime.Now.AddDays(1).Date;
        DateTime lastyearstart = cdt.AddYears(-1).AddDays(-1);
        long startlastmonth = Convert.ToInt64(lastyearstart.ToString("yyyyMMddHHmmss"));
        DateTime tmpdt = lastyearstart;
        long cdtl = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
        List<long> day_ranges = new List<long> { startlastmonth };
        while (tmpdt < cdt)
        {
            tmpdt = tmpdt.AddDays(1);
            day_ranges.Add(Convert.ToInt64(tmpdt.ToString("yyyyMMddHHmmss")));
        }
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                var totalaph = dc.Accounts.Where(x => x.RegistrationDateTime > startlastmonth)
                    .GroupBy(y => day_ranges.FirstOrDefault(day_range => day_range >= y.RegistrationDateTime))
                    .Select(g => new { regdt = g.Key, count = g.Count() }).ToList();
                userschart.ChartAreas[0].AxisX.Minimum = lastyearstart.ToOADate();
                userschart.ChartAreas[0].AxisX.Maximum = cdt.AddDays(1).ToOADate();
                for (int i = 0; i < totalaph.Count(); i++)
                {
                    userschart.Series[0].Points.AddXY(DateTime.ParseExact(totalaph[i].regdt.ToString(),"yyyyMMddHHmmss", null).ToOADate(), totalaph[i].count);
                }
            }
            catch
            {

            }
        }
    }

    private void FillDevicesChart()
    {
        DateTime cdt = DateTime.Now.AddDays(1).Date;
        DateTime lastyearstart = cdt.AddYears(-1).AddDays(-1);
        long startlastmonth = Convert.ToInt64(lastyearstart.ToString("yyyyMMddHHmmss"));
        DateTime tmpdt = lastyearstart;
        long cdtl = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
        List<long> day_ranges = new List<long> { startlastmonth };
        while (tmpdt < cdt)
        {
            tmpdt = tmpdt.AddDays(1);
            day_ranges.Add(Convert.ToInt64(tmpdt.ToString("yyyyMMddHHmmss")));
        }
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                var totaldph = dc.WaterDevices.Where(x => x.RegistrationDateTime > startlastmonth)
                    .GroupBy(y => day_ranges.FirstOrDefault(day_range => day_range >= y.RegistrationDateTime))
                    .Select(g => new { regdt = g.Key, count = g.Count() }).ToList();
                devchart.ChartAreas[0].AxisX.Minimum = lastyearstart.ToOADate();
                devchart.ChartAreas[0].AxisX.Maximum = cdt.AddDays(1).ToOADate();
                for (int i = 0; i < totaldph.Count(); i++)
                {
                    devchart.Series[0].Points.AddXY(DateTime.ParseExact(totaldph[i].regdt.ToString(), "yyyyMMddHHmmss", null).ToOADate(), totaldph[i].count);
                }
            }
            catch
            {

            }
        }
    }

    private void FillServerChart()
    {
        DateTime cdt = DateTime.Now;
        cdt = cdt.Date.AddHours(cdt.Hour + 2);
        DateTime dtrangestart = cdt.AddDays(-3);
        long startlastmonth = Convert.ToInt64(dtrangestart.ToString("yyyyMMddHHmmss"));
        DateTime tmpdt = dtrangestart;
        long cdtl = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
        List<long> hour_ranges = new List<long> { startlastmonth };
        while (tmpdt < cdt)
        {
            tmpdt = tmpdt.AddHours(1);
            hour_ranges.Add(Convert.ToInt64(tmpdt.ToString("yyyyMMddHHmmss")));
        }
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                serverchart.ChartAreas["DevTelemetryChartArea"].AxisX.Minimum = dtrangestart.ToOADate();
                serverchart.ChartAreas["DevTelemetryChartArea"].AxisX.Maximum = cdt.ToOADate();
                serverchart.ChartAreas["UserActionsChartArea"].AxisX.Minimum = dtrangestart.ToOADate();
                serverchart.ChartAreas["UserActionsChartArea"].AxisX.Maximum = cdt.ToOADate();
                serverchart.ChartAreas["ServerErrorsChartArea"].AxisX.Minimum = dtrangestart.ToOADate();
                serverchart.ChartAreas["ServerErrorsChartArea"].AxisX.Maximum = cdt.ToOADate();
                var telemetryph = dc.WaterDeviceTelemetry.Where(x => x.DateTime > startlastmonth)
                    .GroupBy(y => hour_ranges.FirstOrDefault(hour_range => hour_range >= y.DateTime))
                    .Select(g => new { regdt = g.Key, count = g.Count() }).ToList();
                for (int i = 0; i < telemetryph.Count(); i++)
                {
                    serverchart.Series["telemetry"].Points.AddXY(DateTime.ParseExact(telemetryph[i].regdt.ToString(), "yyyyMMddHHmmss", null).ToOADate(), telemetryph[i].count);
                }
                var accounteventsph = dc.AccountLog.Where(x => x.DateTime > startlastmonth)
                    .GroupBy(y => hour_ranges.FirstOrDefault(hour_range => hour_range >= y.DateTime))
                    .Select(g => new { regdt = g.Key, count = g.Count() }).ToList();
                for (int i = 0; i < accounteventsph.Count(); i++)
                {
                    serverchart.Series["user"].Points.AddXY(DateTime.ParseExact(accounteventsph[i].regdt.ToString(), "yyyyMMddHHmmss", null).ToOADate(), accounteventsph[i].count);
                }
                var errorsph = dc.SystemLog.Where(x => x.DateTime > startlastmonth && x.EventText.Contains("ошибка"))
                    .GroupBy(y => hour_ranges.FirstOrDefault(hour_range => hour_range >= y.DateTime))
                    .Select(g => new { regdt = g.Key, count = g.Count() }).ToList();
                for (int i = 0; i < errorsph.Count(); i++)
                {
                    serverchart.Series["errors"].Points.AddXY(DateTime.ParseExact(errorsph[i].regdt.ToString(), "yyyyMMddHHmmss", null).ToOADate(), errorsph[i].count);
                }
                //добавляем по одной нулевой точке на каждый график, чтобы они гарантировано отобразились
                if (serverchart.Series["telemetry"].Points.Count == 0) serverchart.Series["telemetry"].Points.AddXY(dtrangestart.AddHours(1).ToOADate(), 0);
                if (serverchart.Series["user"].Points.Count == 0) serverchart.Series["user"].Points.AddXY(dtrangestart.AddHours(1).ToOADate(), 0);
                if (serverchart.Series["errors"].Points.Count == 0) serverchart.Series["errors"].Points.AddXY(dtrangestart.AddHours(1).ToOADate(), 0);
            }
            catch
            {

            }
        }
    }

    protected void userschartupdatetimer_Tick(object sender, EventArgs e)
    {
        userschartupdatetimer.Enabled = false;
        FillSummary();
        FillUsersChart();
        FillDevicesChart();
        FillServerChart();
        userschartupdatetimer.Enabled = true;
    }

    protected void exitbutton_Click(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
                Logger.AccountLog(Request.UserHostAddress, "Выход из системы", "", tmpacc.ID);
                Logger.SystemLog(Request.UserHostAddress, "Выход из системы", tmpacc.UserID, "Server");
            }
            catch
            {

            }
        }
        FormsAuthentication.SignOut();
        Response.Redirect("~/Login.aspx");
    }
}