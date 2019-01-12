<%@ Page Title="Регистрация" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Register.aspx.cs" Inherits="Register" %>
<%--<%@ Page RegisterAssembly="MSCaptcha" Namespace="MSCaptcha" TagPrefix="cc1" %>--%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <div id="main_column">
                <asp:ScriptManager runat="server"></asp:ScriptManager>
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <div class="section_w590" runat="server" id="regbox">
                            <h3>Учетные данные для регистрации:</h3>
                            <p>адрес e-mail:</p>
                            <asp:TextBox ID="email" runat="server" TextMode="Email"></asp:TextBox>
                            <p>номер телефона для связи (10 цифр, начиная с кода):</p>
                            <asp:TextBox ID="phone" runat="server"></asp:TextBox>
                            <p>пароль:</p>
                            <asp:TextBox ID="userpass1" runat="server" TextMode="Password"></asp:TextBox>
                            <p>повтор пароля:</p>
                            <asp:TextBox ID="userpass2" runat="server" TextMode="Password"></asp:TextBox>
                            <p>защита от спама:</p>
                            <cc1:CaptchaControl ID="cp2" runat="server" Height="50px" 
                                Width="190px" CaptchaLength="6" 
                                CaptchaChars="0123456789" BackColor="White" 
                                EnableViewState="False" ArithmeticFunction="Random" 
                                CaptchaMaxTimeout="0" 
                                CustomValidatorErrorMessage="Введенный текст не совпадает с изображением!" 
                                ViewStateMode="Disabled" CaptchaMinTimeout="0" 
                            />
                            <asp:Label ID="Label1" runat="server" Text="введите цифры с картинки "></asp:Label><br>
                            <asp:TextBox ID="captchatext" runat="server" TextMode="Number"></asp:TextBox>
                            <p>
                                <asp:Label ID="loginmsg" runat="server" Font-Bold="True" ForeColor="#FF3300"></asp:Label>
                            </p>
                            <p><asp:Button ID="regbutton" runat="server" Text="Регистрация" Font-Bold="True" Font-Size="Medium" Width="190px" OnClick="Button1_Click" OnClientClick="this.value='Вход...'; this.disabled=true;" UseSubmitBehavior="false"/></p>
                        </div>
                        <div class="section_w590" runat="server" id="regdonebox">
                          <h3>Заявка на регистрацию принята</h3>
                            <p>На указанный адрес выслано письмо с описанием дальнейших действий.</p>    
                            <div class="cleaner_h20"></div>
                        </div>
                    </ContentTemplate>
                </asp:UpdatePanel>
                <div class="cleaner"></div>
                <%--<p><a href="/Register" target="_parent">Регистрация</a></p>--%>
           </div>         <!-- end of main column -->
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>

