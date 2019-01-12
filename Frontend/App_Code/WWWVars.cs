using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Сводное описание для WWWVars
/// </summary>
public static class WWWVars
{
    /// <summary>
    /// конструктор класса
    /// </summary>
    static WWWVars()
    {
        Refresh();
    }


    public static void Refresh()
    {
        try
        {
            using (VendingModelContainer dc = new VendingModelContainer())
            {
                WebSettings tmpws = dc.WebSettings.First();
                AdminEmail = tmpws.AdminEmail;
                SiteName = tmpws.SiteName;
                ServerEndPoint = tmpws.ServerEndPoint;
                MailFromAddress = tmpws.MailFromAddress;
                EMailDisplayName = tmpws.EMailDisplayName;
                EMailDisplayName = tmpws.EMailDisplayName;
                RegDeviceMailSubject = tmpws.RegDeviceMailSubject;
                RegAccountMailSubject = tmpws.RegAccountMailSubject;
                MailUseSMTPAuth = tmpws.MailUseSMTPAuth;
                MailLogin = tmpws.MailLogin;
                MailPassword = tmpws.MailPassword;
                SMTPHost = tmpws.SMTPHost;
                SMTPPort = tmpws.SMTPPort;
                SMTPUseSSL = tmpws.SMTPUseSSL;
                BingMapsAPIKey = tmpws.BingMapsAPIKey;
            }
        }
        catch
        {

        }
    }
    /// <summary>
    /// адрес администратора
    /// </summary>
    public static string AdminEmail;
    /// <summary>
    /// Название сайта - выводится в заголовке браузера
    /// </summary>
    public static string SiteName;
    /// <summary>
    /// IP или доменное имя сервера
    /// </summary>
    public static string ServerEndPoint;
    /// <summary>
    /// Адрес исходящей почты для письма о регистрации нового устройства
    /// </summary>
    public static string MailFromAddress;
    /// <summary>
    /// Отображаемое имя для отправителя писем
    /// </summary>
    public static string EMailDisplayName;
    /// <summary>
    /// тема письма о регистрации нового устройства
    /// </summary>
    public static string RegDeviceMailSubject;
    /// <summary>
    /// тема письма о регистрации нового акаунта
    /// </summary>
    public static string RegAccountMailSubject;
    /// <summary>
    /// флаг использования учетных данных при отправке почты
    /// </summary>
    public static bool MailUseSMTPAuth;
    /// <summary>
    /// имя пользователя при отправке письма о регистрации нового устройства
    /// </summary>
    public static string MailLogin;
    /// <summary>
    /// пароль при отправке письма о регистрации нового устройства
    /// </summary>
    public static string MailPassword;
    /// <summary>
    /// SMTP server host
    /// </summary>
    public static string SMTPHost;
    /// <summary>
    /// SMTP server port
    /// </summary>
    public static int SMTPPort;
    /// <summary>
    /// SMTP server security
    /// </summary>
    public static bool SMTPUseSSL;
    public static string BingMapsAPIKey = "ArjzrGspjJHXBJLasyo-Gpsln_ezAyEwE1NHzcFDITvJ2LPedZaeWFc8TI12vtuK";
}