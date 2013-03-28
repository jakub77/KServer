<%@ Page Title="Logged out" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Logout.aspx.cs" Inherits="MobiokeWebSite.Account.Logout" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>

    <p class="message-info">
        You are now logged out, please click<a id="A1" runat="server" href="~/">here</a>if your browser does not automatically redirect you in 5 seconds.
    </p>

</asp:Content>

