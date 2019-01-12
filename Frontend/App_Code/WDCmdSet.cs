using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// сруктура, описывающая команду для управления устройством
/// </summary>
public class WDCmd
{
    public WDCmd(string commandtext)
    {
        Command = commandtext;
    }
    /// <summary>
    /// текст команды
    /// </summary>
    public string Command;
}
/// <summary>
/// Набор команд для управления устройством
/// </summary>
public static class WDCmdSet
{
    public static WDCmd GoToSalesMode = new WDCmd("SalesMode");
    public static WDCmd KKTCloseStage = new WDCmd("KKTCloseStage");
    public static WDCmd KKTRegistrationMode = new WDCmd("KKTRegistrationMode");
    public static WDCmd KKTOpenStage = new WDCmd("KKTOpenStage");
    public static WDCmd KKTCancelReceipt = new WDCmd("KKTCancelReceipt");
    public static WDCmd Shutdown = new WDCmd("Shutdown");
    public static WDCmd Reboot = new WDCmd("Reboot");
    public static WDCmd GoToOOSMode = new WDCmd("OOSMode");
    public static WDCmd GoToServiceMode = new WDCmd("ServiceMode");
    public static WDCmd Incassation = new WDCmd("Incasso");
    public static WDCmd Unregister = new WDCmd("Unregister");
}