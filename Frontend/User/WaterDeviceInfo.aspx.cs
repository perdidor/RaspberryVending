using System;
using TwoFactorAuthNet;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.Web.UI.WebControls;

public partial class User_WaterDeviceInfo : System.Web.UI.Page
{
    long wvdid;
    MenuItem globalmenuitemhandler;
    string successheadertext = "";
    string successmsgtext = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            globalmenuitemhandler = DeviceMenu.SelectedItem;
            long tmpwvdid = Convert.ToInt64(Request.QueryString["wvdid"]);
            using (VendingModelContainer dc = new VendingModelContainer())
            {
                Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name && x.Valid && !x.Suspended);
                WaterDevices tmpdev = dc.WaterDevices.First(x => x.ID == tmpwvdid && x.AccountID == tmpacc.ID && x.Valid);
                wvdid = tmpdev.ID;
            }
            Title += wvdid.ToString();
            if (!ScriptManager.GetCurrent(Page).IsInAsyncPostBack)
            {
                if (Request.Form.Count == 2)
                {
                    successheadertext = Request.Form["successheadertext"];
                    successmsgtext = Request.Form["successmsgtext"];
                }
                FillTablesWithData();
            }
        }
        catch (Exception ex)
        {
            Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
        }
    }

    protected void DeviceMenu_MenuItemClick(object sender, MenuEventArgs e)
    {
        globalmenuitemhandler = e.Item;
        FillTablesWithData();
    }

    protected void devactionpanel_Load(object sender, EventArgs e)
    {
        //if (!ScriptManager.GetCurrent(Page).IsInAsyncPostBack) ;
        
    }

    private void FillTablesWithData()
    {
        List<string> warnlist = new List<string>() { };
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            using (var dbContextTransaction = dc.Database.BeginTransaction())
            {
                try
                {
                    try
                    {
                        if (globalmenuitemhandler.Value != "sales") salesinfopanel.Visible = false;
                        if (globalmenuitemhandler.Value != "devcontrol") vmcpanel.Visible = false;
                        switch (globalmenuitemhandler.Value)
                        {
                            case "devinfo":
                                {
                                    deviceinfopanel.Visible = true;
                                    break;
                                }
                            case "cash":
                                {
                                    cashinfopanel.Visible = true;
                                    break;
                                }
                            case "devcontrol":
                                {
                                    vmcpanel.Visible = true;
                                    break;
                                }
                            case "devsettings":
                                {
                                    devicesettingspanel.Visible = true;
                                    break;
                                }
                            case "devcomponents":
                                {
                                    devcomponentspanel.Visible = true;
                                    break;
                                }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                    }
                    //если пришла информация об успешном выполнении операции, показываем сообщение что все заебись
                    if (successheadertext.Length > 5 && successmsgtext.Length > 5)
                    {
                        successheader.Text = successheadertext;
                        successmsg.Text = successmsgtext;
                        successpanel.Visible = true;
                    }
                    //инициализируем переменные и считываем объекты из БД
                    Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name && x.Valid && !x.Suspended);
                    if (tmpacc.TOTPSecret == "")
                    {
                        warnlist.Add("Не настроена двухфакторная авторизация. Управление устройством невозможно.");
                    }
                    WaterDevices tmpdev = dc.WaterDevices.First(x => x.ID == wvdid && x.AccountID == tmpacc.ID && x.Valid);
                    WaterDeviceTelemetry tmptele = dc.WaterDeviceTelemetry.Where(x => x.WaterDeviceID == wvdid).OrderByDescending(y => y.DateTime).First();
                    //если связи с устройством нет, показываем предупреждение
                    DateTime lastlogdt = DateTime.ParseExact(tmptele.DateTime.ToString(), "yyyyMMddHHmmss",
                                            System.Globalization.CultureInfo.InvariantCulture);
                    lastseendatetimelabel.Text = tmptele.DateTimeStr;
                    if (lastlogdt.AddMinutes(5) < DateTime.Now)
                    {
                        timeoutwarnpanel.Visible = true;
                    }
                    else
                    {

                    }
                    DateTime cdt = DateTime.Now;
                    long cdtlong = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
                    string cdtstr = cdt.ToString("dd.MM.yyyy HH:mm:ss");
                    if (salesinfopanel.Visible)
                    {
                        //заполняем таблицу продаж
                        salestable.Rows.Clear();
                        var watersales = dc.WaterSales.Where(x => x.WaterDeviceID == tmpdev.ID).OrderByDescending(y => y.ID).Take(100);
                        foreach (var item in watersales)
                        {
                            TableRow newtablerow = new TableRow
                            {
                                HorizontalAlign = HorizontalAlign.Center,
                                VerticalAlign = VerticalAlign.Middle
                            };
                            TableCell datetimecell = new TableCell
                            {
                                Width = 125,
                                Text = item.EndDateTimeString
                            };
                            TableCell qtycell = new TableCell
                            {
                                Width = 99,
                                Text = item.Quantity.ToString("N2")
                            };
                            TableCell totalcell = new TableCell
                            {
                                Width = 99,
                                Text = (item.Quantity * item.PRICE_PER_ITEM_MDE / 100).ToString("N2")
                            };
                            TableCell usercashcell = new TableCell
                            {
                                Width = 99,
                                Text = item.UserCash.ToString("N2")
                            };
                            TableCell changecell = new TableCell
                            {
                                Width = 99,
                                Text = (item.ActualChangeDispensed - item.ChangeActualDiff).ToString("N2")
                            };
                            TableCell actchangecell = new TableCell
                            {
                                Width = 99,
                                Text = item.ActualChangeDispensed.ToString("N2")
                            };
                            TableCell stageandreceiptcell = new TableCell
                            {
                                Width = 120,
                                Text = item.StageNumber.ToString() + " \\ " + item.ReceiptNumber.ToString()
                            };
                            newtablerow.Cells.AddRange(new TableCell[] { datetimecell, qtycell, totalcell, usercashcell, changecell, actchangecell, stageandreceiptcell });
                            salestable.Rows.Add(newtablerow);
                        }
                        //Заполняем графики данными
                        DateTime last24hstart = cdt.AddDays(-1);
                        DateTime lastmonthstart = cdt.AddMonths(-1).Date;
                        DateTime lastyearstart = cdt.AddYears(-1).Date;
                        long start24h = Convert.ToInt64(last24hstart.ToString("yyyyMMddHHmmss"));
                        long startmonth = Convert.ToInt64(lastmonthstart.ToString("yyyyMMddHHmmss"));
                        long startyear = Convert.ToInt64(lastyearstart.ToString("yyyyMMddHHmmss"));
                        saleslast24hchart.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(last24hstart.ToOADate());
                        saleslast24hchart.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(cdt.ToOADate());
                        saleslastmonthchartdaily.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(lastmonthstart.ToOADate());
                        saleslastmonthchartdaily.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(cdt.AddDays(1).Date.ToOADate());
                        saleslastyearweekly.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(lastyearstart.ToOADate());
                        saleslastyearweekly.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(cdt.AddDays(1).Date.ToOADate());
                        saleslast24hchart.Series[0].Points.Clear();
                        saleslastmonthchartdaily.Series[0].Points.Clear();
                        saleslastyearweekly.Series[0].Points.Clear();
                        var last24hwatersales = dc.WaterSales.Where(x => x.WaterDeviceID == tmpdev.ID && x.EndDateTime > start24h).OrderBy(y => y.ID);
                        foreach (var item in last24hwatersales)
                        {
                            DateTime saledt = DateTime.ParseExact(item.EndDateTime.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                            double total = ((double)(item.Quantity) * item.PRICE_PER_ITEM_MDE / 100 - (double)(item.ChangeActualDiff));
                            saleslast24hchart.Series[0].Points.AddXY(saledt, total);
                        }
                        var lastmonthwatersales = dc.WaterSales.Where(x => x.WaterDeviceID == tmpdev.ID && x.EndDateTime > startmonth).OrderBy(y => y.ID);
                        foreach (var item in lastmonthwatersales)
                        {
                            DateTime saledt = DateTime.ParseExact(item.EndDateTime.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                            double total = ((double)(item.Quantity) * item.PRICE_PER_ITEM_MDE / 100 - (double)(item.ChangeActualDiff));
                            saleslastmonthchartdaily.Series[0].Points.AddXY(saledt, total);
                        }
                        var lastyearwatersales = dc.WaterSales.Where(x => x.WaterDeviceID == tmpdev.ID && x.EndDateTime > startyear).OrderBy(y => y.ID);
                        foreach (var item in lastyearwatersales)
                        {
                            DateTime saledt = DateTime.ParseExact(item.EndDateTime.ToString(), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
                            double total = ((double)(item.Quantity) * item.PRICE_PER_ITEM_MDE / 100 - (double)(item.ChangeActualDiff));
                            saleslastyearweekly.Series[0].Points.AddXY(saledt, total);
                        }
                    }
                    //если есть неисполненные команды устройству, отсылка новых команд невозможна, кнопки надо заблокировать
                    bool pendingcommands = (dc.WaterDeviceCommands.Where(x => x.WaterDeviceID == tmpdev.ID && x.RequestedDatetime == 0).Count() > 0);
                    devmodecb.Enabled = (!pendingcommands && !changedevmodetotpbox.Visible && tmpacc.TOTPSecret != "");
                    changedevmodebutton.Enabled = (devmodecb.Enabled && devmodecb.SelectedIndex > 0);
                    if (pendingcommands)
                    {
                        changedevmodemsg.Text = "Отмените предыдущую команду или дождитесь ее выполнения";
                        pendingcommand.Text = dc.WaterDeviceCommands.Where(x => x.WaterDeviceID == tmpdev.ID && x.RequestedDatetime == 0).First().Command;
                    }
                    pendingcmdwarnpanel.Visible = pendingcommands;
                    ccsumcell.Text = tmptele.CCSum.ToString("N2");
                    cbsumcell.Text = tmptele.CBCount.ToString() + "\\" + tmptele.CBSum.ToString("N2");
                    basumcell.Text = tmptele.BABillsCount.ToString() + "\\" + tmptele.BASum.ToString("N2");
                    if (tmpdev.LocationAddress == "" || tmpdev.LocationLatitude == 0 || tmpdev.LocationLongtitude == 0)
                    {
                        warnlist.Add("Не указан адрес установки");
                    }
                    if (!tmptele.KKTPrinterConnected)
                    {
                        warnlist.Add("Принтер чеков не найден");
                    }
                    if (tmptele.KKTPrinterPaperEmpty)
                    {
                        warnlist.Add("Принтер чеков: кончилась бумага");
                    }
                    if (tmptele.KKTPrinterNonRecoverableError)
                    {
                        warnlist.Add("Принтер чеков: критическая ошибка");
                    }
                    if (tmptele.KKTPrinterCutterError)
                    {
                        warnlist.Add("Принтер чеков: ошибка отрезчика");
                    }
                    if (tmptele.KKTPrinterOverHeated)
                    {
                        warnlist.Add("Принтер чеков: перегрев");
                    }
                    if (tmptele.KKTPrinterPaperJammed)
                    {
                        warnlist.Add("Принтер чеков: замятие бумаги");
                    }
                    if (tmptele.KKTPrinterPaperEnding)
                    {
                        warnlist.Add("Принтер чеков: бумага кончается");
                    }
                    if (tmptele.KKTStageOver24h)
                    {
                        warnlist.Add("Длительность смены превысила 24 часа");
                    }
                    if (tmptele.VMCMode == "4" || tmptele.VMCMode == "5")
                    {
                        warnlist.Add("Режим продаж неактивен");
                    }
                    if (tmptele.IncassoSum > 3000)
                    {
                        warnlist.Add("Рекомендуется проведение инкасации (сумма больше 3000)");
                    }
                    if (tmptele.BABillsCount > 300)
                    {
                        warnlist.Add("Рекомендуется проведение инкасации (купюроприемник близок к заполнению)");
                    }
                    if (tmptele.CC10RURCount < 20 || tmptele.CC5RURCount < 20 || tmptele.CC2RURCount < 20 || tmptele.CC1RURCount < 20)
                    {
                        warnlist.Add("Монетоприемник недостаточно заполнен");
                    }
                    if (tmptele.WaterTempCelsius < 4)
                    {
                        warnlist.Add("Критически низкая температура воды");
                    }
                    inboxtempcell.Text = tmptele.InboxTempCelsius.ToString("N2");
                    if (tmptele.InboxTempCelsius > 50)
                    {
                        warnlist.Add("Критически высокая температура устройства, возможны сбои в работе");
                    }
                    ambienttempcell.Text = tmptele.AmbientTempCelsius.ToString("N2");
                    ambienthumcell.Text = tmptele.AmbientRelativeHumidity.ToString("N2");
                    if (tmptele.AmbientRelativeHumidity > 70)
                    {
                        warnlist.Add("Критически высокая влажность внутри торгового автомата");
                    }
                    waterlevelcell.Text = tmptele.WaterLevelPercent.ToString();
                    if (tmptele.WaterLevelPercent < 20)
                    {
                        warnlist.Add("Низкий уровень воды");
                    }
                    if (tmptele.MDBInitStep != 5)
                    {
                        warnlist.Add("Устройства приема наличных не готовы");
                    }
                    if (deviceinfopanel.Visible)
                    {
                        //заполняем таблицу информации об устройстве
                        regdtcell.Text = tmpdev.RegistrationDateTimeStr;
                        addresslink.Text = tmpdev.LocationAddress;
                        addresslink.NavigateUrl = "/User/ChangeDeviceAddress.aspx?wvdid=" + wvdid.ToString();
                        devproductnamecell.Text = tmpdev.ProductName;
                        devcontactphonecell.Text = tmpdev.CustomerServiceContactPhone;
                        taxsystemtypecell.Text = tmpdev.TaxSystemType.ToString();
                        devpricecell.Text = tmpdev.PRICE_PER_ITEM_MDE.ToString();
                        totalhourscell.Text = tmptele.TotalHoursWorked.ToString("N2");
                        wccell.Text = tmptele.TotalLitersDIspensed.ToString("N2");
                        lastseendatetimecell.Text = tmptele.DateTimeStr;
                        vmcmodecell.Text = tmptele.VMCMode;
                        softversioncell.Text = tmptele.ProgramVersion;
                    }
                    if (cashinfopanel.Visible)
                    {
                        //заполняем таблицу текущих итогов
                        devtotalsum.Text = (tmptele.CCSum + tmptele.IncassoSum).ToString("N2");
                        incassocell.Text = tmptele.IncassoSum.ToString("N2");
                        //заполняем таблицу состояния монетоприемника
                        cc10cell.Text = tmptele.CC10RURCount.ToString();
                        cc5cell.Text = tmptele.CC5RURCount.ToString();
                        cc2cell.Text = tmptele.CC2RURCount.ToString();
                        cc1cell.Text = tmptele.CC1RURCount.ToString();
                        //заполняем таблицу инкассаций
                        last100incassotable.Rows.Clear();
                        var waterincassos = dc.WaterDeviceIncasso.Where(x => x.WaterDeviceID == tmpdev.ID).OrderByDescending(y => y.ID).Take(100);
                        foreach (var item in waterincassos)
                        {
                            TableRow newtablerow = new TableRow
                            {
                                HorizontalAlign = HorizontalAlign.Center,
                                VerticalAlign = VerticalAlign.Middle
                            };
                            TableCell datetimecell = new TableCell
                            {
                                Width = 200,
                                Text = item.IncassoDatetimeStr
                            };
                            TableCell cashboxcoins = new TableCell
                            {
                                Width = 275,
                                Text = item.IncassoCashboxCoins.ToString() + "\\" + item.IncassoCashboxSum.ToString("N2")
                            };
                            TableCell babillscell = new TableCell
                            {
                                Width = 275,
                                Text = item.IncassoBABills.ToString() + "\\" + item.IncassoBASum.ToString("N2")
                            };
                            newtablerow.Cells.AddRange(new TableCell[] { datetimecell, cashboxcoins, babillscell });
                            last100incassotable.Rows.Add(newtablerow);
                        }
                        //если таблица не пустая, показываем ее
                        last100incassodiv.Visible = (last100incassotable.Rows.Count > 0);
                    }
                    if (devicesettingspanel.Visible)
                    {
                        //заполняем таблицу индивидуальных настроек
                        rcptprodnamecell.Text = tmpdev.ProductName;
                        phonecell.Text = tmpdev.CustomerServiceContactPhone;
                        pricecell.Text = tmpdev.PRICE_PER_ITEM_MDE.ToString();
                        taxsystemcell.Text = tmpdev.TaxSystemType.ToString();
                        tankheigthcell.Text = tmpdev.WaterTankHeigthcm.ToString();
                        devicesettingsversioncell.Text = tmpdev.SettingsVersion.ToString();
                    }
                    if (devcomponentspanel.Visible)
                    {
                        //заполняем таблицу периферийных устройств
                        watertempcell.Text = tmptele.WaterTempCelsius.ToString("N2");
                        string heaterstatestring = "Нет";
                        string xtltstatestring = "Нет";
                        string fillpumpstatestring = "Нет";
                        if (tmptele.IsHeaterOn) heaterstatestring = "Да";
                        if (tmptele.IsExternalLightOn) xtltstatestring = "Да";
                        if (tmptele.IsFillPumpSocketActive) fillpumpstatestring = "Да";
                        heateroncell.Text = heaterstatestring;
                        xtltoncell.Text = xtltstatestring;
                        fillpumponcell.Text = fillpumpstatestring;
                        kktmfgnumbercell.Text = tmptele.KKTMfgNumber;
                        kktmodecell.Text = BitConverter.ToString(new byte[] { (byte)tmptele.KKTCurrentMode });
                        string kktstageopenedstr = "Нет";
                        string kktstageover24hstr = "Нет";
                        string kktprnconnectedstr = "Нет";
                        string kktprnpaperemptystr = "Нет";
                        string kktprnctirerrorstr = "Нет";
                        string kktprncuttererrorstr = "Нет";
                        string kktprnoverheatstr = "Нет";
                        string kktprnpaperjammedstr = "Нет";
                        string kktprnpaperendingstr = "Нет";
                        string kktreceiptopenedstr = "Нет";
                        if (tmptele.KKTPrinterConnected) kktprnconnectedstr = "Да";
                        if (tmptele.KKTPrinterPaperEmpty) kktprnpaperemptystr = "Да";
                        if (tmptele.KKTPrinterNonRecoverableError) kktprnctirerrorstr = "Да";
                        if (tmptele.KKTPrinterCutterError) kktprncuttererrorstr = "Да";
                        if (tmptele.KKTPrinterOverHeated) kktprnoverheatstr = "Да";
                        if (tmptele.KKTPrinterPaperJammed) kktprnpaperjammedstr = "Да";
                        if (tmptele.KKTPrinterPaperEnding) kktprnpaperendingstr = "Да";
                        if (tmptele.KKTStageOpened) kktstageopenedstr = "Да";
                        if (tmptele.KKTStageOver24h) kktstageover24hstr = "Да";
                        if (tmptele.KKTReceiptOpened) kktreceiptopenedstr = "Да";
                        kktstageopenedcell.Text = kktstageopenedstr;
                        kktstageover24hcell.Text = kktstageover24hstr;
                        kktprinterconnectedcell.Text = kktprnconnectedstr;
                        kktprinterpaperemptycell.Text = kktprnpaperemptystr;
                        kktprinterpaperendingcell.Text = kktprnpaperendingstr;
                        kktprintercuttererrorcell.Text = kktprncuttererrorstr;
                        kktprinteroverheatcell.Text = kktprnoverheatstr;
                        kktprinterpaperjammedcell.Text = kktprnpaperjammedstr;
                        kktprintererrorcell.Text = kktprnctirerrorstr;
                        kktstagenumbercell.Text = tmptele.KKTStageNumber;
                        kktsreceiptopenedcell.Text = kktreceiptopenedstr;
                        kktstagecloseddatetimecell.Text = tmptele.LastStageClosedDateTimeStr;
                        kktnextreceiptnumbercell.Text = tmptele.KKTReceiptNextNumber.ToString();
                        mdbinitstepcell.Text = tmptele.MDBInitStep.ToString();
                    }
                }
                catch (Exception ex)
                {
                    Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                }
                finally
                {
                    warnpanel.Controls.Clear();
                    if (warnlist.Count > 0)
                    {
                        Label caplabel = new Label();
                        caplabel.Text = "Предупреждения: ";
                        caplabel.Font.Bold = true;
                        caplabel.Font.Name = "Arial";
                        caplabel.Font.Size = 14;
                        caplabel.ForeColor = System.Drawing.Color.Black;
                        warnpanel.Controls.Add(caplabel);
                        warnpanel.Controls.Add(new LiteralControl("<br>"));
                        foreach (var item in warnlist)
                        {
                            Label newlabel = new Label();
                            newlabel.Text = item;
                            newlabel.Font.Bold = true;
                            newlabel.Font.Name = "Arial";
                            newlabel.Font.Size = 13;
                            newlabel.ForeColor = System.Drawing.Color.Black;
                            warnpanel.Controls.Add(newlabel);
                            warnpanel.Controls.Add(new LiteralControl("<br>"));
                        }
                        devwarningpanel.Visible = true;
                    }
                    if (globalmenuitemhandler != null)
                    {

                    }
                }
            }
        }
    }

    protected void updatercptsettingsbutton_Click(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            using (var dbContextTransaction = dc.Database.BeginTransaction())
            {
                try
                {
                    Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name && x.Valid && !x.Suspended);
                    WaterDevices tmpdev = dc.WaterDevices.First(x => x.ID == wvdid && x.AccountID == tmpacc.ID && x.Valid);
                    tmpdev.ProductName = rcptprodnamecell.Text;
                    tmpdev.CustomerServiceContactPhone = phonecell.Text;
                    tmpdev.PRICE_PER_ITEM_MDE = Convert.ToInt32(pricecell.Text);
                    tmpdev.SettingsVersion++;
                    dc.SaveChanges();
                    dbContextTransaction.Commit();
                    ScriptManager.RegisterClientScriptBlock((sender as Control), this.GetType(), "alert", "alert('Настройки печати чеков успешно обновлены, дождитесь обновления настроек на устройстве.')", true);
                    rcptprodnamecell.Text = tmpdev.ProductName;
                    phonecell.Text = tmpdev.CustomerServiceContactPhone;
                    pricecell.Text = tmpdev.PRICE_PER_ITEM_MDE.ToString();
                    taxsystemcell.Text = tmpdev.TaxSystemType.ToString();
                    devicesettingsversioncell.Text = tmpdev.SettingsVersion.ToString();
                    Logger.AccountLog(Request.UserHostAddress, "Обновлены настройки устройства №" + tmpdev.ID, "", tmpacc.ID);
                    Logger.SystemLog(Request.UserHostAddress, "Обновлены настройки устройства №" + tmpdev.ID, "", "Server");
                }
                catch (Exception ex)
                {
                    dbContextTransaction.Rollback();
                    Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                }
            }
        }
        FillTablesWithData();
        //devactionpanel.Update();
    }

    //protected void incasstotpbutton_Click(object sender, EventArgs e)
    //{
    //    TwoFactorAuth tfa = new TwoFactorAuth("La Gioia Water Vending");
    //    Accounts useracc = null;
    //    using (VendingModelContainer dc = new VendingModelContainer())
    //    {
    //        useracc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name);
    //        if (useracc.TOTPSecret != "" && incassototp.Text != "")
    //        {
    //            if (tfa.VerifyCode(useracc.TOTPSecret, incassototp.Text))
    //            {
    //                DateTime cdt = DateTime.Now;
    //                long cdtlong = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
    //                string cdtstr = cdt.ToString("dd.MM.yyyy HH:mm:ss");
    //                //формируем команду инкассации
    //                WaterDeviceCommands tmpincassocmd = new WaterDeviceCommands()
    //                {
    //                    AckDatetime = 0,
    //                    AckDatetimeStr = "",
    //                    Command = WDCmdSet.Incassation.Command,
    //                    FormedDatetime = cdtlong,
    //                    FormedDatetimeStr = cdtstr,
    //                    WaterDeviceID = wvdid,
    //                    RequestedDatetime = 0,
    //                    RequestedDatetimeStr = "",
    //                    Result = ""
    //                };
    //                dc.WaterDeviceCommands.Add(tmpincassocmd);
    //                dc.SaveChanges();
    //                incassototpbox.Visible = false;
    //                makeincassobutton.Visible = true;
    //                ScriptManager.RegisterClientScriptBlock((sender as Control), this.GetType(), "alert", "alert('Команда обнуления счетчиков наличности сформирована. Дождитесь выхода устройства из рабочего режима, произведите изъятие наличных из купюроприемника и кэшбокса, затем верните устройство в рабочий режим с помощью этой панели управления.')", true);
    //            }
    //            else
    //            {
    //                incasstotpmsg.Text = "Неверный одноразовый пароль";
    //                incassototpbox.Visible = true;
    //                makeincassobutton.Visible = false;
    //            }
    //        }
    //    }
    //    FillTablesWithData();
    //}

    protected void devmodecb_SelectedIndexChanged(object sender, EventArgs e)
    {
        //changedevmodemsg.Text = devmodecb.SelectedValue.ToString();
        //devmodecb.SelectedIndexChanged += (EventHandler)devmodecb_SelectedIndexChanged;
        if (devmodecb.SelectedIndex != 0 && devmodecb.Enabled)
        {
            changedevmodebutton.Enabled = true;
            changedevmodemsg.Text = "Новая команда: " + devmodecb.SelectedItem.Text;
            devmodecb.Enabled = false;
            devicesettingspanel.Visible = false;
        }
        FillTablesWithData();
    }

    protected void changedevmodebutton_Click(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            try
            {
                Accounts useracc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name && x.Valid && !x.Suspended);
                if (useracc.TOTPSecret != "")
                {
                    changedevmodemsg.Text = "Новая команда: \"" + devmodecb.SelectedItem.Text + "\"";
                    changedevmodebutton.Visible = false;
                    changedevmodetotpbox.Visible = true;
                    changedevmodetotp.Focus();
                }
                else
                {
                    changedevmodemsg.Text = "Учетная запись не настроена на двухфакторную авторизацию";
                    ScriptManager.RegisterClientScriptBlock((sender as Control), this.GetType(), "alert", "alert('Учетная запись не настроена на двухфакторную авторизацию.')", true);
                }
            }
            catch (Exception ex)
            {
                Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                changedevmodemsg.Text = "Ошибка";
            }
        }
        FillTablesWithData();
    }

    protected void changedevmodetotpbutton_Click(object sender, EventArgs e)
    {
        TwoFactorAuth tfa = new TwoFactorAuth(WWWVars.SiteName);
        Accounts useracc = null;
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            useracc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name && x.Valid && !x.Suspended);
            if (useracc.TOTPSecret != "" && changedevmodetotp.Text != "")
            {
                if (tfa.VerifyCode(useracc.TOTPSecret, changedevmodetotp.Text))
                {
                    DateTime cdt = DateTime.Now;
                    long cdtlong = Convert.ToInt64(cdt.ToString("yyyyMMddHHmmss"));
                    string cdtstr = cdt.ToString("dd.MM.yyyy HH:mm:ss");
                    WDCmd tmpwdcmd = null;
                    string cmddescr = "";
                    switch (devmodecb.SelectedValue)
                    {
                        case "salesmode":
                            {
                                tmpwdcmd = WDCmdSet.GoToSalesMode;
                                cmddescr = "Переход в РЕЖИМ ПРОДАЖ";
                                break;
                            }
                        case "oosmode":
                            {
                                tmpwdcmd = WDCmdSet.GoToOOSMode;
                                cmddescr = "Переход в режим НЕ ОБСЛУЖИВАЕТ";
                                break;
                            }
                        case "servicemode":
                            {
                                tmpwdcmd = WDCmdSet.GoToServiceMode;
                                cmddescr = "Переход в СЛУЖЕБНЫЙ РЕЖИМ";
                                break;
                            }
                        case "shutdown":
                            {
                                tmpwdcmd = WDCmdSet.Shutdown;
                                cmddescr = "ВЫКЛЮЧЕНИЕ";
                                break;
                            }
                        case "reboot":
                            {
                                tmpwdcmd = WDCmdSet.Reboot;
                                cmddescr = "ПЕРЕЗАГРУЗКА";
                                break;
                            }
                        case "incasso":
                            {
                                tmpwdcmd = WDCmdSet.Incassation;
                                cmddescr = "ИНКАССАЦИЯ";
                                break;
                            }
                        case "KKTCloseStage":
                            {
                                tmpwdcmd = WDCmdSet.KKTCloseStage;
                                cmddescr = "ЗАКРЫТИЕ СМЕНЫ";
                                break;
                            }
                        case "KKTRegistrationMode":
                            {
                                tmpwdcmd = WDCmdSet.KKTRegistrationMode;
                                cmddescr = "ККТ: РЕЖИМ РЕГИСТРАЦИИ";
                                break;
                            }
                        case "KKTOpenStage":
                            {
                                tmpwdcmd = WDCmdSet.KKTOpenStage;
                                cmddescr = "ККТ: ОТКРЫТЬ СМЕНУ";
                                break;
                            }
                        case "KKTCancelReceipt":
                            {
                                tmpwdcmd = WDCmdSet.KKTCancelReceipt;
                                cmddescr = "ККТ: ОТМЕНА ТЕКУЩЕГО ЧЕКА";
                                break;
                            }
                        case "Unregister":
                            {
                                tmpwdcmd = WDCmdSet.Unregister;
                                cmddescr = "Удаление устройства (отмена регистрации)";
                                break;
                            }
                    }
                    //формируем команду
                    WaterDeviceCommands tmpcmd = new WaterDeviceCommands()
                    {
                        AckDatetime = 0,
                        AckDatetimeStr = "",
                        Command = tmpwdcmd.Command,
                        FormedDatetime = cdtlong,
                        FormedDatetimeStr = cdtstr,
                        WaterDeviceID = wvdid,
                        RequestedDatetime = 0,
                        RequestedDatetimeStr = "",
                        Result = ""
                    };
                    dc.WaterDeviceCommands.Add(tmpcmd);
                    dc.SaveChanges();
                    Logger.AccountLog(Request.UserHostAddress, "Новая команда устройству №" + tmpcmd.WaterDeviceID, tmpcmd.Command, tmpcmd.WaterDeviceID);
                    Logger.SystemLog(Request.UserHostAddress, "Новая команда устройству №" + tmpcmd.WaterDeviceID, tmpcmd.Command, "Server");
                    changedevmodetotpbox.Visible = false;
                    changedevmodebutton.Visible = true;
                    changedevmodemsg.Text = "";
                    //devmodecb.SelectedIndex = 0;
                    devmodecb.Enabled = true;
                    ScriptManager.RegisterClientScriptBlock((sender as Control), this.GetType(), "alert", "alert('Команда \"" + cmddescr + "\" сформирована. Дождитесь ее выполнения на устройстве.')", true);
                }
                else
                {
                    changedevmodetotpmsg.Text = "Неверный одноразовый пароль";
                    changedevmodebutton.Visible = false;
                    changedevmodetotpbox.Visible = true;
                }
            }
        }
        FillTablesWithData();
    }

    protected void cancelpendingcmdbutton_Click(object sender, EventArgs e)
    {
        using (VendingModelContainer dc = new VendingModelContainer())
        {
            using (var dbContextTransaction = dc.Database.BeginTransaction())
            {
                try
                {
                    Accounts tmpacc = dc.Accounts.First(x => x.UserID == HttpContext.Current.User.Identity.Name && x.Valid && !x.Suspended);
                    WaterDevices tmpdev = dc.WaterDevices.First(x => x.ID == wvdid && x.Valid && x.AccountID == tmpacc.ID);
                    WaterDeviceCommands cmdtocancel = dc.WaterDeviceCommands.Where(x => x.WaterDeviceID == tmpdev.ID && x.RequestedDatetime == 0).First();
                    dc.WaterDeviceCommands.Remove(cmdtocancel);
                    dc.SaveChanges();
                    dbContextTransaction.Commit();
                    ScriptManager.RegisterClientScriptBlock((sender as Control), this.GetType(), "alert", "alert('Команда успешно отменена.')", true);
                    Logger.AccountLog(Request.UserHostAddress, "отмена команды устройству №" + cmdtocancel.WaterDeviceID, cmdtocancel.Command, cmdtocancel.WaterDeviceID);
                    Logger.SystemLog(Request.UserHostAddress, "отмена команды устройству №" + cmdtocancel.WaterDeviceID, cmdtocancel.Command, "Server");
                }
                catch (Exception ex)
                {
                    Logger.SystemLog(Request.UserHostAddress, "Ошибка: " + ex.Message, ex.InnerException?.Message, User.Identity.Name);
                }
            }
        }
        FillTablesWithData();
    }
}