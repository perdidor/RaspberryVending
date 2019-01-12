<%@ Page Title="Активация лицензий" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Activation.aspx.cs" Inherits="Manager_Activation" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <asp:ScriptManager ID="scm" runat="server"></asp:ScriptManager>
                    <asp:Panel ID="waitingpanel" runat="server">
                        <h3>Акаунты, ожидающие активации</h3>
                        <asp:Repeater ID="waitingrepeater" runat="server" DataSourceID="LinqDataSource1" OnItemDataBound="waitingrepeater_ItemDataBound" OnItemCommand="waitingrepeater_ItemCommand" OnItemCreated="waitingrepeater_ItemCreated">
                            <FooterTemplate>  
                                 <asp:Panel ID="footerpanel" runat="server" BorderStyle="Double">
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="nodatalabel" runat="server" Text="Нет данных для отображения" Visible="False"/></p>
                                    <br>
                                </asp:Panel>
                            </FooterTemplate>  
                            <HeaderTemplate>  
                                <!-- child controls -->  
                            </HeaderTemplate>  
                            <ItemTemplate>  
                                <asp:Panel ID="itempanel" runat="server" BorderStyle="Double" BackColor="#006666">
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label1" runat="server" Text="e-mail: "></asp:Label>&nbsp&nbsp<asp:Label ID="email" runat="server" Text=<%# Eval("UserID") %>></asp:Label></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label2" runat="server" Text="номер телефона: "></asp:Label>&nbsp&nbsp<asp:Label ID="phone" runat="server" Text=<%# Eval("DefaultContactPhone") %>>></asp:Label></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label4" runat="server" Text="Дата регистрации: "></asp:Label>&nbsp&nbsp<asp:Label ID="regdt" runat="server" Text=<%# Eval("RegistrationDateTimeStr") %>>></asp:Label></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:CheckBox ID="activecb" runat="server" Text="Активен" Checked=<%# Eval("Valid") %>/>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label6" runat="server" Text="Лимит устройств: "></asp:Label><asp:TextBox ID="devlimit" runat="server" TextMode="Number" Text=<%# Eval("DeviceCountLimit") %>>></asp:TextBox></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:CheckBox ID="suspendedcb" runat="server" Text="Приостановлен" Checked=<%# Eval("Suspended") %>/></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Button ID="savebutton" runat="server" Text="Активировать лицензию" CommandName="Activate" />&nbsp&nbsp&nbsp&nbsp<asp:Button ID="delbutton" runat="server" Text="Удалить акаунт" CommandName="DeleteAcc" OnClientClick="if (!confirm('Заявка на регистрацию будет удалена. Продолжить?')) return false;"/></p>
                                    <br>
                                </asp:Panel>
                            </ItemTemplate>
                            <AlternatingItemTemplate>
                                <asp:Panel ID="altitempanel" runat="server" BorderStyle="Double" BackColor="#333399">
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label1" runat="server" Text="e-mail: "></asp:Label>&nbsp&nbsp<asp:Label ID="email" runat="server" Text=<%# Eval("UserID") %>></asp:Label></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label2" runat="server" Text="номер телефона: "></asp:Label>&nbsp&nbsp<asp:Label ID="phone" runat="server" Text=<%# Eval("DefaultContactPhone") %>>></asp:Label></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label4" runat="server" Text="Дата регистрации: "></asp:Label>&nbsp&nbsp<asp:Label ID="regdt" runat="server" Text=<%# Eval("RegistrationDateTimeStr") %>>></asp:Label></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:CheckBox ID="activecb" runat="server" Text="Активен" Checked=<%# Eval("Valid") %>/>&nbsp&nbsp&nbsp&nbsp<asp:Label ID="Label6" runat="server" Text="Лимит устройств: "></asp:Label><asp:TextBox ID="devlimit" runat="server" TextMode="Number" Text=<%# Eval("DeviceCountLimit") %>>></asp:TextBox></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:CheckBox ID="suspendedcb" runat="server" Text="Приостановлен" Checked=<%# Eval("Suspended") %>/></p>
                                    <br>
                                    <p>&nbsp&nbsp&nbsp&nbsp<asp:Button ID="savebutton" runat="server" Text="Активировать лицензию" CommandName="Activate" />&nbsp&nbsp&nbsp&nbsp<asp:Button ID="delbutton" runat="server" Text="Удалить акаунт" CommandName="DeleteAcc" OnClientClick="if (!confirm('Заявка на регистрацию будет удалена. Продолжить?')) return false;"/></p>
                                    <br>
                                </asp:Panel>
                            </AlternatingItemTemplate>
                            <SeparatorTemplate>  
                                <!-- child controls -->  
                            </SeparatorTemplate>
                        </asp:Repeater>
                        <asp:LinqDataSource ID="LinqDataSource1" runat="server" ContextTypeName="VendingModelContainer" EntityTypeName="" OrderBy="RegistrationDateTime desc" TableName="Accounts" Where="LicenseContent.length < 10 && UserID != @admemail">
                            
                         </asp:LinqDataSource>
                    </asp:Panel>
            <%--</asp:UpdatePanel>--%>
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>
