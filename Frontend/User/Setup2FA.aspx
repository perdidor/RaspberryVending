<%@ Page Title="Настройка повышенной безопасности учетной записи" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Setup2FA.aspx.cs" Inherits="Setup2FA" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
        <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <div id="main_column">
                <asp:ScriptManager runat="server"></asp:ScriptManager>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <div class="section_w590" runat="server" id="twofasetupbox">
                            <h3>Настройка двухфакторной авторизации</h3>
                            <p>В целях предотвращения несанкционированного доступа к данным в нашей системе используются одноразовые пароли.</p>
                            <p><p2>Для работы с данной технологией вам необходимо:</p2></p>
                            <p><p2>1. Скачать и установить на ваше мобильное устройство одну из программ из списка ниже</p2></p>
                            <ul class="list_01">
                            <li>Mictosoft Authenticator (рекомендуется)</li><p>на момент публикации доступны версии для <a href="http://go.microsoft.com/fwlink/?Linkid=825071" target="_blank">Windows Phone</a>, <a href="http://go.microsoft.com/fwlink/?Linkid=825072" target="_blank">Android</a> и <a href="http://go.microsoft.com/fwlink/?Linkid=825073" target="_blank">iOS</a></p>
                            <li><p>Google Authenticator</li><p>на момент публикации доступны версии для <a href="https://play.google.com/store/apps/details?id=com.google.android.apps.authenticator2&hl=ru" target="_blank">Android</a> и <a href="https://itunes.apple.com/ru/app/google-authenticator/id388497605?mt=8" target="_blank">iOS</a></p>
                            </ul>

                            <p><p2>2. После установки программы добавьте в нее учетную запись путем сканирования данного кода с экрана:<p2></p>
                            <asp:Image ID="totps" runat="server" AlternateText="SharedSecretQRCode" Height="200px" ImageAlign="Middle" Width="200px" />
                            <p><p2>3. Введите 6-значный одноразовый код из программы и нажмите кнопку "Подтвердить"<p2></p>
                            <p><input type="text" value="6-значный код с устройства" name="totp" maxlength="6" class="inputfield" onfocus="clearText(this)" onblur="clearText(this)" /></p>
                            <asp:Label ID="totpmsg" runat="server" Font-Bold="True" ForeColor="#FF3300"></asp:Label>
                            <p><asp:Button ID="Button1" runat="server" Text="Подтвердить" Font-Bold="True" Font-Size="Medium" OnClick="Button1_Click"/>
                            <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="Logout" />
                            </p>
                        </div>
                        <div class="section_w590" runat="server" id="twofasetupcompletebox">
                            <h3>Настройка одноразовых кодов завершена</h3>
                        </div>
                        </ContentTemplate>
                </asp:UpdatePanel>
                <%--<p><a href="/Register" target="_parent">Регистрация</a></p>--%>
           </div>         <!-- end of main column -->
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>

