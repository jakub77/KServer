<%@ Page Title="Success" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SuccessfulRegistration.aspx.cs" Inherits="MobiokeWebSite.Account.SuccessfulRegistration" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>!</h1>
    </hgroup>

    <article>
        <p>
            Now that you are registered, get a hold of one of our applications and start the Karaoke!
        </p>

        <p>
            To read more about Mobioke see out <a id="A1" runat="server" href="~/About.aspx">About</a> page.
        </p>
    </article>

<%--    <aside>
        <h3>Team Warp Zone</h3>
        <p>
            Mobioke was created by Team Warp Zone at the University of Utah from a Senior Capstone Project. 
        </p>
    </aside>--%>
</asp:Content>
