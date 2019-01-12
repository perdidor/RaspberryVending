<%@ Page Title="Администрирование" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Summary.aspx.cs" Inherits="Manager_Summary" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
        <asp:ScriptManager ID="scm" runat="server"></asp:ScriptManager>
            <asp:Button ID="exitbutton" runat="server" Text="Выход из системы" class="submitbutton" Width="131px" OnClick="exitbutton_Click" Font-Bold="True" Height="38px" />
            <asp:Panel ID="totalspanel" runat="server" Height="2280px">
                <h3>Пользователи</h3>
                
                    <asp:Panel ID="Panel1" runat="server" Height="350px">
                        <asp:UpdatePanel ID="userschartupdatepanel" runat="server">
                            <ContentTemplate>
                                <asp:Label ID="Label1" runat="server" Text="Всего: "></asp:Label>
                                <asp:HyperLink ID="totalaccslink" runat="server" Font-Size="15pt" ForeColor="#6699FF">0000</asp:HyperLink>
                                &nbsp;&nbsp;&nbsp;
                                <asp:Label ID="Label2" runat="server" Text="Неактивных: "></asp:Label>
                                <asp:HyperLink ID="suspendedaccslink" runat="server" Font-Size="15pt" ForeColor="#FF9900">0000</asp:HyperLink>
                                &nbsp;&nbsp;&nbsp;
                                <asp:Label ID="Label3" runat="server" Text="Ожидающие активации: "></asp:Label>
                                <asp:HyperLink ID="awaitingaccslink" runat="server" Font-Size="15pt" ForeColor="#FF3300" NavigateUrl="~/Manager/Activation.aspx">0000</asp:HyperLink>
                                <br/>
                                <br/>
                                <asp:Timer ID="userschartupdatetimer" runat="server" Enabled="False" OnTick="userschartupdatetimer_Tick"></asp:Timer>
                                <asp:Chart ID="userschart" runat="server" Height="304px" Width="880px">
                                    <Series>
                                        <asp:Series Name="total" Color="DodgerBlue" CustomProperties="PixelPointWidth=2" LegendText="Регистраций в день" YValueType="Int32"></asp:Series>
                                    </Series>
                                    <ChartAreas>
                                        <asp:ChartArea Name="ChartArea1">
                                            <AxisY IntervalType="Number">
                                            </AxisY>
                                            <AxisX IntervalType="Days" Title="Дата">
                                            </AxisX>
                                        </asp:ChartArea>
                                    </ChartAreas>
                                    <Titles>
                                        <asp:Title Name="Title1" Text="Регистрация новых пользователей (1/сутки)">
                                        </asp:Title>
                                    </Titles>
                                </asp:Chart>
                           </ContentTemplate>
                        </asp:UpdatePanel>
                    </asp:Panel>
                    <br/>
                    <h3>Устройства</h3>
                        <asp:Panel ID="Panel2" runat="server" Height="350px">
                            <asp:UpdatePanel ID="devschartupdatepanel" runat="server">
                                <ContentTemplate>
                                    <asp:Label ID="Label4" runat="server" Text="Всего: "></asp:Label>
                                    <asp:HyperLink ID="totaldevslink" runat="server" Font-Size="15pt" ForeColor="#6699FF">0000</asp:HyperLink>
                                    &nbsp;&nbsp;&nbsp;
                                    <asp:Label ID="Label5" runat="server" Text="Онлайн (активны последние 5 минут): "></asp:Label>
                                    <asp:HyperLink ID="onlinedevslink" runat="server" Font-Size="15pt" ForeColor="#99CC00">0000</asp:HyperLink>
                                    <br/>
                                    <br/>
                                    <asp:Chart ID="devchart" runat="server" Height="304px" Width="880px">
                                        <Series>
                                            <asp:Series Color="DodgerBlue" CustomProperties="PixelPointWidth=2" LegendText="Регистраций в день" Name="total" YValueType="Int32">
                                            </asp:Series>
                                        </Series>
                                        <ChartAreas>
                                            <asp:ChartArea Name="ChartArea1">
                                                <AxisY IntervalType="Number">
                                                </AxisY>
                                                <AxisX IntervalType="Days" Title="Дата">
                                                </AxisX>
                                            </asp:ChartArea>
                                        </ChartAreas>
                                        <Titles>
                                            <asp:Title Name="Title1" Text="Регистрация новых устройств (1/сутки)">
                                            </asp:Title>
                                        </Titles>
                                    </asp:Chart>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </asp:Panel>
                        <br/>
                        <br/>
                    <h3>Состояние сервера</h3>
                        <asp:Panel ID="Panel3" runat="server" Height="1300px">
                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                <ContentTemplate>
                                    <asp:Label ID="Label6" runat="server" Text="За последние 60 минут запросов от устройств: "></asp:Label>
                                    <asp:HyperLink ID="qphlink" runat="server" Font-Size="15pt" ForeColor="#6699FF">0000</asp:HyperLink>
                                    &nbsp;&nbsp;&nbsp;
                                    <asp:Label ID="Label7" runat="server" Text="Событий: "></asp:Label>
                                    <asp:HyperLink ID="evphlink" runat="server" Font-Size="15pt" ForeColor="#99CC00">0000</asp:HyperLink>
                                    &nbsp;&nbsp;&nbsp;
                                    <asp:Label ID="Label8" runat="server" Text="Ошибок: "></asp:Label>
                                    <asp:HyperLink ID="errphlink" runat="server" Font-Size="15pt" ForeColor="Red">0000</asp:HyperLink>
                                    <br/>
                                    <br/>
                                    <asp:Chart ID="serverchart" runat="server" Height="1300px" Width="880px">
                                        <Series>
                                            <asp:Series BorderWidth="3" ChartArea="DevTelemetryChartArea" Color="DodgerBlue" CustomProperties="PixelPointWidth=6, DrawingStyle=Cylinder" LegendText="Телеметрия (1/час)" Name="telemetry" XValueType="Date" YValueType="Int32">
                                            </asp:Series>
                                            <asp:Series BorderWidth="3" ChartArea="UserActionsChartArea" Color="DarkGoldenrod" CustomProperties="PixelPointWidth=6, DrawingStyle=Cylinder" LegendText="Действия пользователя (1/час)" Name="user" XValueType="Date" YValueType="Int32">
                                            </asp:Series>
                                            <asp:Series BorderWidth="3" ChartArea="ServerErrorsChartArea" Color="Red" CustomProperties="PixelPointWidth=6, DrawingStyle=Cylinder" LegendText="Ошибки (1/час)" Name="errors" XValueType="Date" YValueType="Int32">
                                            </asp:Series>
                                        </Series>
                                        <ChartAreas>
                                            <asp:ChartArea Name="DevTelemetryChartArea">
                                                <AxisY IntervalType="Number">
                                                </AxisY>
                                                <AxisX Interval="4" IntervalType="Hours">
                                                    <LabelStyle Format="dd.MM.yyyy HH:mm:ss" />
                                                </AxisX>
                                                <Area3DStyle Rotation="0" WallWidth="0" />
                                            </asp:ChartArea>
                                            <asp:ChartArea Name="UserActionsChartArea">
                                                <AxisY IntervalType="Number">
                                                </AxisY>
                                                <AxisX Interval="4" IntervalType="Hours">
                                                    <LabelStyle Format="dd.MM.yyyy HH:mm:ss" />
                                                </AxisX>
                                                <Area3DStyle Rotation="0" WallWidth="0" />
                                            </asp:ChartArea>
                                            <asp:ChartArea Name="ServerErrorsChartArea">
                                                <AxisY IntervalType="Number">
                                                </AxisY>
                                                <AxisX Interval="4" IntervalType="Hours">
                                                    <LabelStyle Format="dd.MM.yyyy HH:mm:ss" />
                                                </AxisX>
                                                <Area3DStyle Rotation="0" WallWidth="0" />
                                            </asp:ChartArea>
                                        </ChartAreas>
                                        <Titles>
                                            <asp:Title DockedToChartArea="DevTelemetryChartArea" IsDockedInsideChartArea="False" Name="Title1" Text="Телеметрия с устройств (1/час)">
                                            </asp:Title>
                                            <asp:Title DockedToChartArea="UserActionsChartArea" IsDockedInsideChartArea="False" Name="Title2" Text="Действия пользователя (1/час)">
                                            </asp:Title>
                                            <asp:Title DockedToChartArea="ServerErrorsChartArea" IsDockedInsideChartArea="False" Name="Title3" Text="Ошибки (1/час)">
                                            </asp:Title>
                                        </Titles>
                                    </asp:Chart>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </asp:Panel>
                        <br/>
                </asp:Panel>
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>


