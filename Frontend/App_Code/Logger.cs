using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Сводное описание для Logger
/// </summary>
public static class Logger
{
    static Logger()
    {
        //
        // TODO: добавьте логику конструктора
        //
    }

    public static void AccountLog(string ip, string eventtext, string description, long acctid)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                DateTime cdt = DateTime.Now;
                string cdtstr = cdt.ToString("dd.MM.yyyy HH:mm:ss");
                long cdtl = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
                AccountLog tmplog = new AccountLog() {  AccountID = acctid, DateTime = cdtl, DateTimeStr = cdtstr, IPAddress = ip, EventText = eventtext, Description = description};
                dc.AccountLog.Add(tmplog);
                dc.SaveChanges();
            }
            catch
            {

            }
        }
    }

    public static void SystemLog(string ip, string eventtext, string description, string userid)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                DateTime cdt = DateTime.Now;
                string cdtstr = cdt.ToString("dd.MM.yyyy HH:mm:ss");
                long cdtl = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
                SystemLog tmplog = new SystemLog() { UserID = userid, DateTime = cdtl, DateTimeStr = cdtstr, IPAddress = ip, EventText = eventtext, Description = description };
                dc.SystemLog.Add(tmplog);
                dc.SaveChanges();
            }
            catch
            {

            }
        }
    }
}