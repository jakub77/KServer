<%@ Page Title="Error" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="MobiokeWebSite.Error" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>!</h1>
    </hgroup>

    <article>
        <p>
            Sorry, an error occured. Let's try not to do that again.
        </p>
        <p>
            <asp:Label ID ="ErrorLabel" runat="server">If you are interested, the specifics of the error are: </asp:Label>
        </p>
    </article>

</asp:Content>
