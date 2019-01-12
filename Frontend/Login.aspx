<%@ Page Title="Вход в систему" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Login.aspx.cs" Inherits="Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <div id="main_column">
                <asp:ScriptManager runat="server"></asp:ScriptManager>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <div class="section_w590" runat="server" id="loginbox">
                        <h3>Введите ваши учетные данные:</h3>
                        <p>адрес e-mail:</p>
                        <p><input type="text" <%--value="email" --%>name="userid" size="15" class="inputfield" <%--onfocus="clearText(this)" onblur="clearText(this)" --%>/></p>
                        <p>пароль:</p>
                        <p><input type="password" name="userpass" size="15" class="inputfield" <%--onfocus="clearText(this)" onblur="clearText(this)" --%>/></p>
                        <p>защита от спама:</p>
                            <cc1:CaptchaControl ID="cp3" runat="server" Height="50px" 
                                Width="190px" CaptchaLength="6" 
                                CaptchaChars="0123456789" BackColor="White" 
                                EnableViewState="False" ArithmeticFunction="Random" 
                                CaptchaMaxTimeout="0" 
                                CustomValidatorErrorMessage="Введенный текст не совпадает с изображением!" 
                                ViewStateMode="Disabled" CaptchaMinTimeout="0" 
                            />
                        <asp:Label ID="Label1" runat="server" Text="введите цифры с картинки "></asp:Label><br>
                        <asp:TextBox ID="captchatext" runat="server" TextMode="Number"></asp:TextBox><br>
                        <asp:Label ID="loginmsg" runat="server" Font-Bold="True" ForeColor="#FF3300"></asp:Label>
                        <p><asp:Button ID="loginbutton" runat="server" Text="Вход" Font-Bold="True" Font-Size="Medium" Width="100px" OnClick="Button1_Click" OnClientClick="this.value='Вход...'; this.disabled=true;" UseSubmitBehavior="false"/></p>
                        <p><asp:HyperLink ID="reglink" runat="server" NavigateUrl="~/Register.aspx" EnableViewState="False">Регистрация</asp:HyperLink></p>
                        </div>
                        <div class="section_w590" runat="server" id="totpbox">
                          <h3>Введите одноразовый пароль</h3>
                            <p>В целях защиты от несанкционированного доступа система запрашивает одноразовый пароль, который можно получить только с помощью ранее настроенного мобильного устройства.</p>    
                            <div class="cleaner_h20"></div>
                            <div>
                                <img class="image_wrapper twofa" src="images/2fa_app.jpg" alt="image" />
                                <p><asp:TextBox runat="server" ID="totp" Font-Bold="True" MaxLength="6" Width="153px"></asp:TextBox></p>
                                <asp:Label ID="totpmsg" runat="server" Font-Bold="True" ForeColor="#FF3300"></asp:Label>
                                <p><asp:Button runat="server" Text="Отправить" Font-Bold="True" ID="totpbutton" OnClick="Unnamed1_Click" OnClientClick="this.value='Подтверждение...'; this.disabled=true;" UseSubmitBehavior="false"></asp:Button></p>
                            </div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="cleaner"></div>
           </div>         <!-- end of main column -->
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>

