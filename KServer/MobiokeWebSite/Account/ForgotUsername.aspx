<%@ Page Title="Forgot Username" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ForgotUsername.aspx.cs" Inherits="MobiokeWebSite.Account.ForgotUsername" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <fieldset>
        <legend>Remind Username</legend>
        <table>
            <tr>
                <td>
                    <asp:Label ID="EmailLabel" runat="server" AssociatedControlID="EmailTextBox">Email address</asp:Label>
                </td>
                <td></td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="EmailTextBox" runat="server" TextMode="Email" />
                </td>
                <td>
                    <asp:Label ID="EmailErrorLabel" runat="server" AssociatedControlID="EmailTextBox" CssClass="field-validation-error"></asp:Label>
                </td>
            </tr>
        </table>
        <table>
            <tr>
                <td>
                    <asp:Button runat="server" ID="Submit" Text="Send me my username" OnClick="Submit_Click" />
                </td>
                <td>
                    <asp:Label ID="ResultLabel" runat="server"></asp:Label>
                </td>
            </tr>
        </table>
    </fieldset>

    <p>
        Don't have an account?<a id="A1" runat="server" href="~/Account/Register2.aspx">Register!</a><br />
        Forgotten your<a id="A2" runat="server" href="~/Account/ForgotUsername.aspx">username</a> or <a id="A3" runat="server" href="~/Account/StartPasswordReset.aspx">password</a>?
    </p>
</asp:Content>


