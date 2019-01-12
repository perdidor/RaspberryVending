<%@ Page Title="Личный кабинет" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="User.aspx.cs" Inherits="User_User" %>
<%@ MasterType VirtualPath="~/MasterPage.master"%>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <asp:UpdatePanel ID="accmenupanel" runat="server">
                <ContentTemplate>
                    <asp:Menu ID="AccountMenu" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="15pt" Orientation="Horizontal" ForeColor="#0066FF" OnMenuItemClick="AccountMenu_MenuItemClick">
                    <Items>
                        <asp:MenuItem Selected="True" Text="Лицензия" Value="licence"></asp:MenuItem>
                        <%--<asp:MenuItem Text="Сводка" Value="summary"></asp:MenuItem>--%>
                        <asp:MenuItem Text="Список устройств" Value="mydevlist"></asp:MenuItem>
                        <asp:MenuItem Text="Выход из системы" Value="exit"></asp:MenuItem>
                    </Items>
                    <StaticHoverStyle ForeColor="#99CCFF" />
                     <StaticSelectedStyle ForeColor="White" />
                        <StaticMenuItemStyle HorizontalPadding="20px" />
                    </asp:Menu>
                 </ContentTemplate>
            </asp:UpdatePanel>
            <asp:UpdatePanel ID="accountpanel" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional">
                    <ContentTemplate>
                        <asp:Panel ID="licpanel" runat="server">
                            <div id="main_column">
                                <div class="section_w590">
                                    <br>
                                    <h3>Сведения об учетной записи</h3>
                                    <p>
                                        e-mail:  
                                        <asp:Label ID="useridlabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>
                                    <p>
                                        услуга активна до:  
                                        <asp:Label ID="paidtilllabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>
                                    <p>
                                        зарегистрировано устройств:  
                                        <asp:Label ID="licensedlabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>
                                    <p>
                                        Устройств онлайн (активность за последние 10 минут):  
                                        <asp:Label ID="onlinedevcountlabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>
                                    <p>
                                        максимальное количество:  
                                        <asp:Label ID="maxregslabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>
                                    <p>
                                        контактный номер телефона:  
                                        <asp:Label ID="phonenumberlabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>
                                    <p>
                                        двухфакторная авторизация:  
                                        <asp:Label ID="twofaenabledlabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                        <asp:LinkButton ID="setup2falink" runat="server" PostBackUrl="~/User/Setup2FA.aspx" Visible="False">Включить</asp:LinkButton>
                                    </p>
                                    <p>
                                        дата регистрации:  
                                        <asp:Label ID="regdatetimelabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>
                                    <p>
                                        активен:  
                                        <asp:Label ID="accactivelabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>

                                    <p>
                                        Сумма наличности (всего / инкассация):  
                                        <asp:Label ID="totalcashlabel" runat="server" ForeColor="Lime" Font-Size="14pt"></asp:Label>
                                    </p>

                                    <p>
                                        &nbsp;</p>
                                    <p>
                                        &nbsp;</p>
                            </div>
                            </div> <!-- end of main column -->
            
                            <div id="side_column">
                                <div class="side_column_box">
                                    <br>
                                    <h3>Лицензия</h3>
                                     <div class="news_section">
                                         <asp:Image ID="LicenseImage" runat="server" class="licenceimage"/><br />
                                         <asp:Button ID="licdownloadbutton" runat="server" OnClick="licdownloadbutton_Click" Text="Скачать" Font-Bold="True" Font-Names="Arial" Font-Size="12pt" Width="191px" />
                                         <p>Файл лицензии необходим для регистрации нового устройства в системе.  <a href="#">Далее...</a></p>
                                    </div>
                                </div>
                            </div>
                            <!-- end of side column -->
                        </asp:Panel>
                        <asp:Panel ID="devlistpanel" runat="server" Visible="False" EnableViewState="False">
                            <br>
                            <asp:Table ID="devlist" runat="server" BorderStyle="Solid" GridLines="Both" Font-Size="11pt" Width="900px" Font-Bold="False" Font-Names="Arial">
                                <asp:TableRow BackColor="Black" ForeColor="#FFFF99">
                                    <asp:TableCell Width="100px" HorizontalAlign="Center">Номер</asp:TableCell>
                                    <asp:TableCell Width="390px" HorizontalAlign="Center">Адрес установки</asp:TableCell>
                                    <asp:TableCell Width="60px" HorizontalAlign="Center">Связь</asp:TableCell>
                                    <asp:TableCell Width="70px" HorizontalAlign="Center">Состояние</asp:TableCell>
                                    <asp:TableCell Width="120px" HorizontalAlign="Center">На инкассацию, ₽</asp:TableCell>
                                    <asp:TableCell Width="120px" HorizontalAlign="Center">Всего, ₽</asp:TableCell>
                                </asp:TableRow>
                            </asp:Table>
                        </asp:Panel>
              </ContentTemplate>
                    <Triggers>
                        <asp:PostBackTrigger ControlID="licdownloadbutton"/>
                        <asp:AsyncPostBackTrigger ControlID="AccountMenu" EventName="MenuItemClick" />
                    </Triggers>
        </asp:UpdatePanel>
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>

