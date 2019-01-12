<%@ Page Title="" Language="C#" MasterPageFile="~/MasterPage.master" AutoEventWireup="true" CodeFile="Support.aspx.cs" Inherits="Support_Support" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
    <asp:ScriptManager runat="server"></asp:ScriptManager>
    <div id="templatemo_content_wrapper">
        <div id="templatemo_content">
            <asp:UpdatePanel ID="supportmenupanel" runat="server">
                <ContentTemplate>
                    <asp:Menu ID="SupportMenu" runat="server" Font-Bold="True" Font-Names="Arial" Font-Size="15pt" Orientation="Horizontal" ForeColor="#0066FF">
                    <Items>
                        <asp:MenuItem Selected="True" Text="Документация" Value="supportdocs"></asp:MenuItem>
                        <asp:MenuItem Text="FAQ" Value="supporfaq"></asp:MenuItem>
                        <%--<asp:MenuItem Text="Выход из системы" Value="exit"></asp:MenuItem>--%>
                    </Items>
                    <StaticHoverStyle ForeColor="#99CCFF" />
                     <StaticSelectedStyle ForeColor="White" />
                        <StaticMenuItemStyle HorizontalPadding="20px" />
                    </asp:Menu>
                 </ContentTemplate>
            </asp:UpdatePanel>
            <asp:UpdatePanel ID="supportpanel" runat="server" ChildrenAsTriggers="False" UpdateMode="Conditional">
               <ContentTemplate>
                        
              </ContentTemplate>
        </asp:UpdatePanel>
        <div class="cleaner"></div>
        </div> <!-- end of content -->
    
    	<div class="cleaner"></div>
    </div> <!-- end of templatmeo_content_wrapper -->
    </asp:Content>