<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Register2.aspx.cs" Inherits="MobiokeWebSite.Account.Register2" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
        <h2>Use the form below to create a new account.</h2>
    </hgroup>

    <p class="message-info">
        Are you a DJ? If so register<a id="A1" runat="server" href="~/Account/DJRegister.aspx">HERE!</a><br />
        <b><asp:Label ID="RegistrationLockedMessage" runat="server" /></b>
    </p>

    <fieldset>
        <legend>Registration Form</legend>
        <table>
            <tr>
                <td>
                    <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserNameTextBox">User name</asp:Label>
                </td>
                <td></td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="UserNameTextBox" runat="server" />
                </td>
                <td>
                    <asp:Label ID="UserNameErrorLabel" runat="server" AssociatedControlID="UserNameTextBox" CssClass="field-validation-error"></asp:Label>
                </td>
            </tr>
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
            <tr>
                <td>
                    <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="PasswordTextBox">Password</asp:Label>
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
                    <asp:Button runat="server" ID="RegisterSubmit" OnClick="RegisterSubmit_Click" Text="Register" />
                </td>
                <td>
                    <asp:Label ID="ResultLabel" runat="server"></asp:Label>
                </td>
            </tr>
        </table>
    </fieldset>
</asp:Content>
