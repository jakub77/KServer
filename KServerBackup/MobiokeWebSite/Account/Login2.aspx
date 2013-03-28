<%@ Page Title="Log in" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login2.aspx.cs" Inherits="MobiokeWebSite.Account.Login2" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
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
                        <asp:CheckBox ID="IsDJCheckBox" runat="server"/>
                        <asp:Label runat="server" AssociatedControlID="IsDJCheckBox" CssClass="checkbox">Are you a DJ?</asp:Label>
                    </td>
                    <td></td>
                </tr>
            </table>
            <table>
                <tr>
                    <td>
                        <asp:Button runat="server" ID="LoginSubmit" OnClick="LoginSubmit_Click" Text="Log in" />
                    </td>
                    <td>
                        <asp:Label ID="ResultLabel" runat="server"></asp:Label>
                    </td>
                </tr>
            </table>
        </fieldset>

        <p>
            Don't have an account?<a runat="server" href="~/Account/Register2.aspx">Register!</a><br />
            Forgotten your<a runat="server" href="~/Account/ForgotUsername.aspx">username</a> or <a id="A1" runat="server" href="~/Account/StartPasswordReset.aspx">password</a>?
        </p>
</asp:Content>


