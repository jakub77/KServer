<%@ Page Title="Password Reset" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="FinishPasswordReset.aspx.cs" Inherits="MobiokeWebSite.Account.FinishPasswordReset" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <fieldset>
        <legend>Reset Password</legend>
        <table>
            <tr>
                <td>
                    <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="PasswordTextBox">Enter your new password</asp:Label>
                </td>
                <td></td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="PasswordTextBox" runat="server" TextMode="Password" />
                </td>
                <td>
                    <asp:Label ID="PasswordErrorLabel" runat="server" AssociatedControlID="PasswordTextBox" CssClass="field-validation-error"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="PasswordConfirmationLabel" runat="server" AssociatedControlID="PasswordConfirmationTextBox">Confirm password</asp:Label>
                </td>
                <td></td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="PasswordConfirmationTextBox" runat="server" TextMode="Password" />
                </td>
                <td>
                    <asp:Label ID="PasswordConfirmationErrorLabel" runat="server" AssociatedControlID="PasswordConfirmationTextBox" CssClass="field-validation-error"></asp:Label>
                </td>
            </tr>
        </table>
        <table>
            <tr>
                <td>
                    <asp:Button runat="server" ID="Submit" Text="Save Password" OnClick="Submit_Click" />
                </td>
                <td>
                    <asp:Label ID="ResultLabel" runat="server"></asp:Label>
                </td>
            </tr>
        </table>
    </fieldset>

    <br />
    <p>
        You will be sent an email with instructions on how to reset your password.
    </p>

    <p>
        Don't have an account?<a id="A1" runat="server" href="~/Account/Register2.aspx">Register!</a><br />
        Forgotten your<a id="A2" runat="server" href="~/Account/ForgotUsername.aspx">username</a> or <a id="A3" runat="server" href="~/Account/StartPasswordReset.aspx">password</a>?
    </p>
</asp:Content>
