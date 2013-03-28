<%@ Page Title="Administration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Administration.aspx.cs" Inherits="MobiokeWebSite.Administration" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
            <p>Admin Username</p>
            <p><asp:TextBox ID="adminUsername" runat="server"/></p>
            <p>Admin Pass</p>
            <p><asp:TextBox ID="adminPassword" runat="server" TextMode="Password" /></p>
            <p><asp:Button ID ="enableRegistration" runat="server" OnClick="enableRegistration_Click" Text="Enable Registration"/></p>
            <p><asp:Button ID ="disableRegistration" runat="server" OnClick="disableRegistration_Click" Text="Disable Registration" /></p>
            <p><asp:Label ID ="resultLabel" runat="server" /></p>
    </article>

    <%--    <aside>
        <h3>Team Warp Zone</h3>
        <p>
            Mobioke was created by Team Warp Zone at the University of Utah from a Senior Capstone Project. 
        </p>
    </aside>--%>
</asp:Content>

