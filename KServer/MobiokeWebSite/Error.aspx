<%@ Page Title="Error" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="MobiokeWebSite.Error" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>!</h1>
    </hgroup>

    <article>
        <p>
            Sorry, an error occured. Let's try not to do that again.
        </p>
    </article>

    <%--    <aside>
        <h3>Team Warp Zone</h3>
        <p>
            Mobioke was created by Team Warp Zone at the University of Utah from a Senior Capstone Project. 
        </p>
    </aside>--%>
</asp:Content>
