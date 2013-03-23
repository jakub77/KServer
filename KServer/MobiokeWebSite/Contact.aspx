<%@ Page Title="Contact" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Contact.aspx.cs" Inherits="MobiokeWebSite.Contact" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
        <h2>Mobioke</h2>
    </hgroup>
    <section class="contact">
        <header>
            <h3>Email:</h3>
        </header>
        <p>
            <span><a href="mailto:Mobioke@live.org">Mobioke@live.org</a></span>
        </p>
    </section>
</asp:Content>