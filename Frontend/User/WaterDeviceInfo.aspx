<%@ Page Title="Устройство № " Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="WaterDeviceInfo.aspx.cs" Inherits="User_WaterDeviceInfo" %>

<%@ Register assembly="System.Web.DataVisualization, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" namespace="System.Web.UI.DataVisualization.Charting" tagprefix="asp" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="templatemo_content_wrapper">
    	<div id="templatemo_content">
        <asp:ScriptManager ID="ScriptManager1" runat="server">
                </asp:ScriptManager>
            <div class="devicemenu">
                <asp:UpdatePanel ID="menuupdatepanel" runat="server">
                    <ContentTemplate>
                        <asp:Menu ID="DeviceMenu" runat="server" Font-Size="13pt" ForeColor="#0066FF" StaticDisplayLevels="3" OnMenuItemClick="DeviceMenu_MenuItemClick" Height="1000px">
                            <DynamicHoverStyle ForeColor="#99CCFF" />
                            <DynamicSelectedStyle ForeColor="White" />
                            <Items>
                                <asp:MenuItem Text="Продажи" Value="sales" Selected="True">
                                </asp:MenuItem>
                                <asp:MenuItem Text="Информация" Value="devinfo">
                                </asp:MenuItem>
                                <asp:MenuItem Text="Наличность" Value="cash">
                                </asp:MenuItem>
                                <asp:MenuItem Text="Периферия" Value="devcomponents"></asp:MenuItem>
                                <asp:MenuItem Text="Настройки" Value="devsettings"></asp:MenuItem>
                                <asp:MenuItem Text="Управление" Value="devcontrol"></asp:MenuItem>
                            </Items>
                            <StaticHoverStyle ForeColor="#99CCFF" />
                            <StaticSelectedStyle ForeColor="White" />
                        </asp:Menu>
                     </ContentTemplate>
                </asp:UpdatePanel>
            </div>
            <div id="devmgmtcontent">
                <asp:UpdatePanel ID="devactionpanel" runat="server" OnLoad="devactionpanel_Load" ChildrenAsTriggers="False" UpdateMode="Conditional">
                    <ContentTemplate>
                        <div ID="timeoutwarnpanel" runat="server" Visible="False" EnableViewState="False">
                            <asp:Panel runat="server" Width="750px" BackColor="#CC9900" Height="39px">
                                <asp:Label ID="Label3" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="14pt" ForeColor="Black" Text="Информация устарела!"></asp:Label><br>
                                <asp:Label ID="Label1" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="13pt" ForeColor="Black" Text="Нет связи с устройством с "></asp:Label>
                                <asp:Label ID="lastseendatetimelabel" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="13pt" ForeColor="Black" Text="08.08.2018 22:22:31"></asp:Label>
                            </asp:Panel>
                        </div>
                        <div ID="pendingcmdwarnpanel" runat="server" Visible="False" EnableViewState="False">
                            <br>
                            <asp:Panel runat="server" Width="750px" BackColor="#CC9900" Height="70px">
                                <asp:Label ID="Label2" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="14pt" ForeColor="Black" Text="Ожидающая команда: "></asp:Label><br>
                                <asp:Label ID="pendingcommand" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="13pt" ForeColor="Black"></asp:Label><br>
                                <asp:Button ID="cancelpendingcmdbutton" runat="server" Text="Отменить" Font-Bold="True" Font-Names="Arial" Font-Size="12pt" OnClientClick="this.value='Отмена...'; this.disabled=true;" UseSubmitBehavior="false" OnClick="cancelpendingcmdbutton_Click" Width="178px" />
                                <br><br>
                            </asp:Panel>
                        </div>
                        <div ID="devwarningpanel" runat="server" Visible="False" EnableViewState="False">
                            <br>
                            <asp:Panel runat="server" ID="warnpanel" Width="750px" BackColor="#CC9900">
                                <%--<asp:Label ID="Label4" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="14pt" ForeColor="Black" Text="Обнаружены проблемы: "></asp:Label><br/>--%>
                                <%--<asp:Label ID="Label5" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="13pt" ForeColor="Black" Text=""></asp:Label><br>--%>
                                <br><br>
                            </asp:Panel>
                        </div>
                        <div ID="successpanel" runat="server" Visible="False" EnableViewState="False">
                            <br>
                            <asp:Panel runat="server" Width="750px" BackColor="#009933" Height="39px">
                                <asp:Label ID="successheader" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="14pt" ForeColor="Black" Text=""></asp:Label><br>
                                <asp:Label ID="successmsg" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="13pt" ForeColor="Black" Text=""></asp:Label>
                            </asp:Panel>
                        </div>
                        <br>
                        <asp:Panel ID="salesinfopanel" runat="server" EnableViewState="False">
                            <asp:Table ID="salesheadertable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Последние 100 продаж в обратном порядке" Font-Size="12pt" Font-Names="Arial">
                                <asp:TableRow HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Width="130px">Дата и время</asp:TableCell>
                                    <asp:TableCell runat="server" Width="100px">Литры</asp:TableCell>
                                    <asp:TableCell runat="server" Width="100px">Сумма</asp:TableCell>
                                    <asp:TableCell runat="server" Width="100px">Принято</asp:TableCell>
                                    <asp:TableCell runat="server" Width="100px">Сдача расчет</asp:TableCell>
                                    <asp:TableCell runat="server" Width="100px">Выдано</asp:TableCell>
                                    <asp:TableCell runat="server" Width="120px">смена \ чек</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                            <div class="scrolling-table-container">
                                <asp:Table ID="salestable" runat="server" BorderWidth="1px" GridLines="Both" Font-Size="10pt" Font-Names="Arial">
                                </asp:Table>
                            </div>
                            <br><br>
                            <asp:Chart ID="saleslast24hchart" runat="server" Width="750px" BackColor="Silver">
                                <series>
                                    <asp:Series Name="Series1" Color="Lime" CustomProperties="PixelPointWidth=5, DrawingStyle=Cylinder">
                                    </asp:Series>
                                </series>
                                <chartareas>
                                    <asp:ChartArea Name="ChartArea1" BackColor="Silver">
                                        <AxisY Title="Сумма" IsLabelAutoFit="False" TitleFont="Microsoft Sans Serif, 10pt, style=Bold">
                                            <LabelStyle Font="Microsoft Sans Serif, 10pt, style=Bold" />
                                        </AxisY>
                                        <AxisX Interval="2" IntervalType="Hours" Title="Время" IsLabelAutoFit="False" TitleFont="Microsoft Sans Serif, 10pt, style=Bold">
                                            <LabelStyle Format="HH:mm" Interval="2" IntervalType="Hours" Font="Microsoft Sans Serif, 8pt, style=Bold" />
                                        </AxisX>
                                    </asp:ChartArea>
                                </chartareas>
                                <Titles>
                                    <asp:Title DockedToChartArea="ChartArea1" IsDockedInsideChartArea="False" Name="Title1" Text="Продажи за последние сутки" Font="Microsoft Sans Serif, 10pt, style=Bold">
                                    </asp:Title>
                                </Titles>
                            </asp:Chart>
                            <asp:Chart ID="saleslastmonthchartdaily" runat="server" Width="750px" BackColor="Silver">
                                <series>
                                    <asp:Series Name="Series1" Color="Lime" CustomProperties="PixelPointWidth=2, DrawingStyle=Cylinder">
                                    </asp:Series>
                                </series>
                                <chartareas>
                                    <asp:ChartArea Name="ChartArea1" BackColor="Silver">
                                        <AxisY Title="Сумма" IsLabelAutoFit="False" TitleFont="Microsoft Sans Serif, 10pt, style=Bold">
                                            <LabelStyle Font="Microsoft Sans Serif, 10pt, style=Bold" />
                                        </AxisY>
                                        <AxisX Interval="2" IntervalType="Days" Title="Дата" IsLabelAutoFit="False" TitleFont="Microsoft Sans Serif, 10pt, style=Bold">
                                            <LabelStyle Format="dd.MM" Interval="2" IntervalType="Days" Font="Microsoft Sans Serif, 8pt, style=Bold" />
                                        </AxisX>
                                    </asp:ChartArea>
                                </chartareas>
                                <Titles>
                                    <asp:Title DockedToChartArea="ChartArea1" IsDockedInsideChartArea="False" Name="Title1" Text="Продажи за последний месяц" Font="Microsoft Sans Serif, 10pt, style=Bold">
                                    </asp:Title>
                                </Titles>
                            </asp:Chart>
                            <asp:Chart ID="saleslastyearweekly" runat="server" Width="750px" BackColor="Silver">
                                <series>
                                    <asp:Series Name="Series1" Color="Lime" CustomProperties="PixelPointWidth=1, DrawingStyle=Cylinder">
                                    </asp:Series>
                                </series>
                                <chartareas>
                                    <asp:ChartArea Name="ChartArea1" BackColor="Silver">
                                        <AxisY Title="Сумма" IsLabelAutoFit="False" TitleFont="Microsoft Sans Serif, 10pt, style=Bold">
                                            <LabelStyle Font="Microsoft Sans Serif, 10pt, style=Bold" />
                                        </AxisY>
                                        <AxisX Interval="2" IntervalType="Months" Title="Месяц, год" IsLabelAutoFit="False" TitleFont="Microsoft Sans Serif, 10pt, style=Bold">
                                            <LabelStyle Format="MMM yy" Interval="1" IntervalType="Months" Font="Microsoft Sans Serif, 8pt, style=Bold" />
                                        </AxisX>
                                    </asp:ChartArea>
                                </chartareas>
                                <Titles>
                                    <asp:Title DockedToChartArea="ChartArea1" IsDockedInsideChartArea="False" Name="Title1" Text="Продажи за последний год" Font="Microsoft Sans Serif, 10pt, style=Bold">
                                    </asp:Title>
                                </Titles>
                            </asp:Chart>
                        </asp:Panel>
                        <asp:Panel ID="deviceinfopanel" runat="server" Visible="False" EnableViewState="False">
                            <asp:Table ID="devinfotable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Список текущих параметров устройства" Font-Size="11pt" Font-Names="Arial">
                                <asp:TableRow ID="devinfoheadersrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Width="180px">Параметр</asp:TableCell>
                                    <asp:TableCell runat="server" Width="150px">Значение</asp:TableCell>
                                    <asp:TableCell runat="server" Width="420px">Примечание\пояснение</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="regdtrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Дата регистрации</asp:TableCell>
                                    <asp:TableCell ID="regdtcell" runat="server" Height="60px" Width="150px">01.01.2018 12:22:41</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="lastseenrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Виден в сети</asp:TableCell>
                                    <asp:TableCell ID="lastseendatetimecell" runat="server" Height="60px" Width="150px">01.01.2018 12:33:22</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Дата и время последнего сеанса связи.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="devsuspendedrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Неактивно</asp:TableCell>
                                    <asp:TableCell ID="devsuspendedcell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Функция не реализована.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="vmcmoderow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="120" Width="180px">Текущий режим</asp:TableCell>
                                    <asp:TableCell ID="vmcmodecell" runat="server" Height="60px" Width="150px">0</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">0 = инициализация<br>1 = режим продаж<br>2 = активный сеанс продажи<br>3 = выдача сдачи<br>4 = не обслуживает<br>5 = служебный режим</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="devaddressrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Адрес установки</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                        <asp:HyperLink ID="addresslink" runat="server" Text="N\A" NavigateUrl="#" Target="_parent">N\A</asp:HyperLink></asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Нажмите на адрес для просмотра или изменения.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="devproductrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Название товара</asp:TableCell>
                                    <asp:TableCell ID="devproductnamecell" runat="server" Height="60px" Width="150px">Вода питьевая</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Печатается на чеке</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="devcontactphonerow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Контактный номер телефона</asp:TableCell>
                                    <asp:TableCell ID="devcontactphonecell" runat="server" Height="60px" Width="150px">+79990123456</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Контакт для поддержки покупателей. Печатается на чеке, показывается на экране.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="taxsystemtyperow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Тип налога</asp:TableCell>
                                    <asp:TableCell ID="taxsystemtypecell" runat="server" Height="60px" Width="150px">00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Тип налога (см. документацию к ККТ)</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="devpricerow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Цена за литр</asp:TableCell>
                                    <asp:TableCell ID="devpricecell" runat="server" Height="60px" Width="150px">500</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Цена за единицу товара в МДЕ (в РФ - копейка).</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="softversionrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Версия</asp:TableCell>
                                    <asp:TableCell ID="softversioncell" runat="server" Height="60px" Width="150px">0.0.0.0</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Версия управляющей программы.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="totalhoursrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Пробег: часы</asp:TableCell>
                                    <asp:TableCell ID="totalhourscell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Счетчик отработанного времени. Обнуляется при замене главного модуля.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="wcrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Пробег: литры</asp:TableCell>
                                    <asp:TableCell ID="wccell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Счетчик проданной воды. Обнуляется при замене главного модуля.</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                        </asp:Panel>
                        <asp:Panel ID="cashinfopanel" runat="server" Visible="False" EnableViewState="False">
                            <asp:Table ID="totaltable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Текущие итоги" Font-Size="11pt" Font-Names="Arial">
                                <asp:TableRow ID="totalheadersrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Width="180px">Параметр</asp:TableCell>
                                    <asp:TableCell runat="server" Width="150px">Значение</asp:TableCell>
                                    <asp:TableCell runat="server" Width="420px">Примечание\пояснение</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="totalrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Всего наличности в устройстве</asp:TableCell>
                                    <asp:TableCell ID="devtotalsum" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">итого</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="incassorow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Сумма наличных для инкассация</asp:TableCell>
                                    <asp:TableCell ID="incassocell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">купюроприемник + кэшбокс</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="ccsumrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Монетоприемник (все трубки)</asp:TableCell>
                                    <asp:TableCell ID="ccsumcell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">сумма номиналов монет в трубках монетоприемника</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="cbsumrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Кэшбокс количество\сумма</asp:TableCell>
                                    <asp:TableCell ID="cbsumcell" runat="server" Height="60px" Width="150px">0\0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">количество и сумма номиналов монет в кэшбоксе</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="basumrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Купюроприемник количество\сумма</asp:TableCell>
                                    <asp:TableCell ID="basumcell" runat="server" Height="60px" Width="150px">0\0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">количество и сумма номиналов купюр в стекере</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                            <asp:Table ID="ссinfotable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Состояние монетоприемника" Font-Size="11pt" Font-Names="Arial">
                                <asp:TableRow ID="ccinfoheadersrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Width="180px">Параметр</asp:TableCell>
                                    <asp:TableCell runat="server" Width="150px">Значение</asp:TableCell>
                                    <asp:TableCell runat="server" Width="420px">Примечание\пояснение</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="cc10row" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">10₽ монет</asp:TableCell>
                                    <asp:TableCell ID="cc10cell" runat="server" Height="60px" Width="150px">0</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">трубка монетоприемника</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="cc5row" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">5₽ монет</asp:TableCell>
                                    <asp:TableCell ID="cc5cell" runat="server" Height="60px" Width="150px">0</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">трубка монетоприемника</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="cc2row" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">2₽ монет</asp:TableCell>
                                    <asp:TableCell ID="cc2cell" runat="server" Height="60px" Width="150px">0</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">трубка монетоприемника</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="cc1row" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Height="60px" Width="180px">1₽ монет</asp:TableCell>
                                    <asp:TableCell ID="cc1cell" runat="server" Height="60px" Width="150px">0</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">трубка монетоприемника</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                            <br>
                            <asp:Table ID="lastincassoheaderstable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Последние 100 инкассаций в обратном порядке" Font-Size="12pt" Font-Names="Arial">
                                <asp:TableRow HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Width="200px">Дата и время</asp:TableCell>
                                    <asp:TableCell runat="server" Width="275px">Монет из кэшбокса (кол-во\сумма)</asp:TableCell>
                                    <asp:TableCell runat="server" Width="275px">Купюр (кол-во\сумма)</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                            <div class="scrolling-table-container" runat="server" id="last100incassodiv" Visible="False">
                                <asp:Table ID="last100incassotable" runat="server" BorderWidth="1px" GridLines="Both" Font-Size="10pt" Font-Names="Arial">
                                </asp:Table>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="devcomponentspanel" runat="server" Visible="False" EnableViewState="False">
                            <asp:Table ID="devcomponentstable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Состояние контролируемых внешних устройств, показания датчиков" Font-Size="11pt" Font-Names="Arial">
                                <asp:TableRow ID="devcomponentsheadersrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Width="180px">Параметр</asp:TableCell>
                                    <asp:TableCell runat="server" Width="150px">Значение</asp:TableCell>
                                    <asp:TableCell runat="server" Width="420px">Примечание\пояснение</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="watertemprow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Температура воды, 'C</asp:TableCell>
                                    <asp:TableCell ID="watertempcell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="inboxtemprow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Температура внутри управляющего устройства, 'C</asp:TableCell>
                                    <asp:TableCell ID="inboxtempcell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="ambienttemprow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Температура воздуха, 'C</asp:TableCell>
                                    <asp:TableCell ID="ambienttempcell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="ambienthumrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Относительная влажность воздуха, %</asp:TableCell>
                                    <asp:TableCell ID="ambienthumcell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="waterlevelrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Уровень воды, %</asp:TableCell>
                                    <asp:TableCell ID="waterlevelcell" runat="server" Height="60px" Width="150px">0.00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="mdbinitsteprow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="120px" Width="180px">Состояние устройств приема наличных</asp:TableCell>
                                    <asp:TableCell ID="mdbinitstepcell" runat="server" Height="120px" Width="150px">0</asp:TableCell>
                                    <asp:TableCell runat="server" Height="120px" Width="420px">В норме = 5, значения менее 5 - инициализация не завершена. Значения более 5 - ошибка. Если устройства приема наличных не выходят на рабочий режим в течение 5 минут, скорее всего одно из них зависло и потребуется перезагрузка системы для сброса их состояния.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="heateronrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Обогреватель включен</asp:TableCell>
                                    <asp:TableCell ID="heateroncell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="xtltonrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Внешнее освещение включено</asp:TableCell>
                                    <asp:TableCell ID="xtltoncell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="fillpumponrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Розетка наполнения включена</asp:TableCell>
                                    <asp:TableCell ID="fillpumponcell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                            <asp:Table ID="kktinfotable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Состояние ККТ" Font-Size="11pt" Font-Names="Arial">
                                <asp:TableRow ID="kktmfgnumberrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Заводской номер ККТ</asp:TableCell>
                                    <asp:TableCell ID="kktmfgnumbercell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktmoderow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Режим</asp:TableCell>
                                    <asp:TableCell ID="kktmodecell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">01 - режим регистрации (режим продаж); х3 - режим отчетов; 0 - режим выбора (активен после включения питания ККТ)</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktstageopenedrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Смена открыта</asp:TableCell>
                                    <asp:TableCell ID="kktstageopenedcell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktstageover24hrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Смена > 24ч</asp:TableCell>
                                    <asp:TableCell ID="kktstageover24hcell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">max длительность смены без снятия отчета - 24ч</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktstagenumberrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Номер смены</asp:TableCell>
                                    <asp:TableCell ID="kktstagenumbercell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Если смена закрыта - номер последней закрытой смены</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktsreceiptopenedrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Чек открыт</asp:TableCell>
                                    <asp:TableCell ID="kktsreceiptopenedcell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Имеет смысл только в режиме регистрации</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktstagecloseddatetimerow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Последний отчет</asp:TableCell>
                                    <asp:TableCell ID="kktstagecloseddatetimecell" runat="server" Height="60px" Width="150px">00.00.0000 00:00:00</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Смена открыта - дата и время окончания текущей смены.</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktnextreceiptnumberrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Номер чека</asp:TableCell>
                                    <asp:TableCell ID="kktnextreceiptnumbercell" runat="server" Height="60px" Width="150px">Нет</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Номер следующего кассового чека</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                            <asp:Table ID="kktprinterinfotable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Состояние чекового принтера" Font-Size="11pt" Font-Names="Arial">
                                <asp:TableRow ID="kktprinterconnectedrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Принтер присоединен</asp:TableCell>
                                    <asp:TableCell ID="kktprinterconnectedcell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktprinterpaperemptyrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Бумага кончилась</asp:TableCell>
                                    <asp:TableCell ID="kktprinterpaperemptycell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktprinterpaperendingrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Бумага кончается</asp:TableCell>
                                    <asp:TableCell ID="kktprinterpaperendingcell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktprintererrorrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Критическая ошибка</asp:TableCell>
                                    <asp:TableCell ID="kktprintererrorcell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Требуется вмешательство оператора</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktprintercuttererrorrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Ошибка отрезчика</asp:TableCell>
                                    <asp:TableCell ID="kktprintercuttererrorcell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktprinteroverheatrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Перегрев</asp:TableCell>
                                    <asp:TableCell ID="kktprinteroverheatcell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Перегрев печатающей головки</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="kktprinterpaperjammedrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Замятие бумаги</asp:TableCell>
                                    <asp:TableCell ID="kktprinterpaperjammedcell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                        </asp:Panel>
                        <asp:Panel ID="devicesettingspanel" runat="server" Visible="False" EnableViewState="False">
                            <asp:Table ID="devicesettingstable" runat="server" BorderWidth="2px" GridLines="Both" Caption="Список текущих индивидуальных настроек" Font-Size="11pt" Font-Names="Arial">
                                <asp:TableRow ID="TableRow1" runat="server" HorizontalAlign="Center" VerticalAlign="Middle">
                                    <asp:TableCell runat="server" Width="180px">Параметр</asp:TableCell>
                                    <asp:TableCell runat="server" Width="150px">Значение</asp:TableCell>
                                    <asp:TableCell runat="server" Width="420px">Примечание\пояснение</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="rcptprodnamerow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Название товара</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                        <asp:TextBox ID="rcptprodnamecell" runat="server">N\A</asp:TextBox></asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="phonerow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Номер телефона для поддержки покупателей</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                            <asp:TextBox ID="phonecell" runat="server">N\A</asp:TextBox></asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="pricerow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Цена за литр в МДЕ</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                        <asp:TextBox ID="pricecell" runat="server">N\A</asp:TextBox></asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="taxsystemrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Тип налога</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                        <asp:TextBox ID="taxsystemcell" runat="server">N\A</asp:TextBox></asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="tankheigthrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Высота бака для воды (высота установки датчика уровня)</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                        <asp:TextBox ID="tankheigthcell" runat="server">N\A</asp:TextBox></asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="watersensoraddressrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Адрес 1w датчика температуры воды DS18B20</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                        <asp:TextBox ID="watersensoraddresscell" runat="server">N\A</asp:TextBox></asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">8 HEX байт без пробелов 0123456789ABCDEF</asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="usekktrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Использовать ККМ</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="150px">
                                        <asp:DropDownList ID="usekktcell" runat="server">
                                            <asp:ListItem Value="1">ДА</asp:ListItem>
                                            <asp:ListItem Value="0">НЕТ</asp:ListItem>
                                        </asp:DropDownList>
                                        </asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px"></asp:TableCell>
                                </asp:TableRow>
                                <asp:TableRow ID="devicesettingsversionrow" runat="server" HorizontalAlign="Center" VerticalAlign="Middle" >
                                    <asp:TableCell runat="server" Height="60px" Width="180px">Версия настроек</asp:TableCell>
                                    <asp:TableCell ID="devicesettingsversioncell" runat="server" Height="60px" Width="150px">N\A</asp:TableCell>
                                    <asp:TableCell runat="server" Height="60px" Width="420px">Увеличивается на 1 при каждом обновлении настроек</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                            <asp:Button ID="updatercptsettingsbutton" runat="server" Text="Обновить настройки"  OnClientClick="this.value='Подтверждение...'; this.disabled=true;" UseSubmitBehavior="false" OnClick="updatercptsettingsbutton_Click" Font-Bold="True" Font-Names="Arial" Font-Size="12pt"/>
                            <br><br>
                        </asp:Panel>
                        <asp:Panel ID="vmcpanel" runat="server" Visible="False" EnableViewState="False">
                            <div class="section_w750" runat="server" id="changedevmodebox">
                                <h3>Действия с управляющим устройством</h3>
                                <img class="picfloatleftnoborder" src="/images/settings_64.png" alt="image" />
                                <p>Выбрать команду из списка: <asp:DropDownList ID="devmodecb" runat="server" Font-Names="Arial" Font-Size="12pt" OnSelectedIndexChanged="devmodecb_SelectedIndexChanged" AutoPostBack="True" ViewStateMode="Enabled">
                                    <asp:ListItem Value="dashes">--------------------------------</asp:ListItem>
                                    <asp:ListItem Value="incasso">Инкассация</asp:ListItem>
                                    <asp:ListItem Value="salesmode">Режим продажи (по умолчанию)</asp:ListItem>
                                    <asp:ListItem Value="oosmode">Не обслуживает</asp:ListItem>
                                    <asp:ListItem Value="servicemode">Служебный режим (заполнение монетоприемника)</asp:ListItem>
                                    <asp:ListItem Value="shutdown">Выключение</asp:ListItem>
                                    <asp:ListItem Value="reboot">Перезагрузка</asp:ListItem>
                                    <asp:ListItem Value="KKTCloseStage">ККТ: закрыть смену</asp:ListItem>
                                    <asp:ListItem Value="KKTRegistrationMode">ККТ: режим регистрации</asp:ListItem>
                                    <asp:ListItem Value="KKTOpenStage">ККТ: открыть смену</asp:ListItem>
                                    <asp:ListItem Value="KKTCancelReceipt">ККТ: отмена текущего открытого чека</asp:ListItem>
                                    <asp:ListItem Value="Unregister">Удаление (отмена регистрации)</asp:ListItem>
                                    </asp:DropDownList>
                                <br><br>
                                <asp:Label ID="changedevmodemsg" runat="server" Font-Bold="True" ForeColor="#FF3300" EnableViewState="False"></asp:Label>
                                <br><br>
                                <asp:Button ID="changedevmodebutton" runat="server" Text="Послать команду на устройство" Font-Bold="True" Font-Names="Arial" Font-Size="12pt" Enabled="False" OnClick="changedevmodebutton_Click" />
                                <br><br>
                                </p>
                            <br><br>
                            </div>
                            <div class="section_w750" runat="server" id="changedevmodetotpbox" Visible="False" EnableViewState="False">
                                <h3>Введите одноразовый пароль</h3>
                                <img class="picfloatrightnoborder" src="/images/2fa_app.jpg" alt="image" />
                                <p>Для подтверждения действия система запрашивает одноразовый пароль, который можно получить только с помощью ранее настроенного мобильного устройства.</p>   
                                <p><asp:TextBox runat="server" ID="changedevmodetotp" Font-Bold="True" MaxLength="6" Width="153px"></asp:TextBox></p>
                                <asp:Label ID="changedevmodetotpmsg" runat="server" Font-Bold="True" ForeColor="#FF3300"></asp:Label>
                                <p><asp:Button runat="server" Text="Подтвердить" Font-Bold="True" ID="changedevmodetotpbutton" OnClientClick="this.value='Подтверждение...'; this.disabled=true;" UseSubmitBehavior="false" OnClick="changedevmodetotpbutton_Click"></asp:Button></p>
                                <br><br>
                            </div>
                        </asp:Panel>
                    </ContentTemplate>
                    <Triggers>
                        <asp:AsyncPostBackTrigger ControlID="updatercptsettingsbutton" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="DeviceMenu" EventName="MenuItemClick" />
                        <asp:AsyncPostBackTrigger ControlID="devmodecb" EventName="SelectedIndexChanged" />
                        <asp:AsyncPostBackTrigger ControlID="changedevmodetotpbutton" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="changedevmodebutton" EventName="Click" />
                        <asp:AsyncPostBackTrigger ControlID="cancelpendingcmdbutton" EventName="Click" />
                    </Triggers>
                </asp:UpdatePanel>
                </div>

        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>

