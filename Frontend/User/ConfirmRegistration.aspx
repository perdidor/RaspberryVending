<%@ Page Title="Подтверждение регистрации нового устройства" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="ConfirmRegistration.aspx.cs" Inherits="User_ConfirmRegistration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <div id="main_column">
                <div class="section_w590">
                    <h3>Результат подтверждения:</h3>
                    <asp:label runat="server" ID="confirmresult" Font-Size="20pt"></asp:label>
                   
                </div>
            </div> <!-- end of main column -->
            
            <!-- end of side column -->
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
</asp:Content>

