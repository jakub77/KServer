<%@ Page Title="Account settings" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Manage2.aspx.cs" Inherits="MobiokeWebSite.Account.Manage2" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
        <asp:Label ID="AccountNameType" runat="server" Font-Bold="True"></asp:Label>
    </hgroup>

    <section id="loginForm">
        <h2>Change your password.</h2>
        <fieldset>
            <legend>Password Change</legend>
            <table>
                <tr>
                    <td>
                        <asp:Label ID="OldPasswordLabel" runat="server" AssociatedControlID="OldPasswordTextBox">Old password</asp:Label>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="OldPasswordTextBox" runat="server" TextMode="Password" />
                    </td>
                    <td>
                        <asp:Label ID="OldPasswordErrorLabel" runat="server" AssociatedControlID="OldPasswordTextBox" CssClass="field-validation-error"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="NewPasswordLabel" runat="server" AssociatedControlID="NewPasswordTextBox">New password</asp:Label>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="NewPasswordTextBox" runat="server" TextMode="Password" />
                    </td>
                    <td>
                        <asp:Label ID="NewPasswordErrorLabel" runat="server" AssociatedControlID="NewPasswordTextBox" CssClass="field-validation-error"></asp:Label>
                    </td>
                </tr>
                <tr>
                    <td>
                        <asp:Label ID="ConfirmNewPasswordLabel" runat="server" AssociatedControlID="ConfirmNewPasswordTextBox">Confirm new password</asp:Label>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="ConfirmNewPasswordTextBox" runat="server" TextMode="Password" />
                    </td>
                    <td>
                        <asp:Label ID="ConfirmNewPasswordErrorLabel" runat="server" AssociatedControlID="ConfirmNewPasswordTextBox" CssClass="field-validation-error"></asp:Label>
                    </td>
                </tr>
            </table>
            <table>
                <tr>
                    <td>
                        <asp:Button runat="server" ID="PasswordSubmit" Text="Update password" OnClick="PasswordSubmit_Click" />
                    </td>
                    <td>
                        <asp:Label ID="PasswordResultLabel" runat="server"></asp:Label>
                    </td>
                </tr>
            </table>
        </fieldset>
    </section>

    <section id="socialLoginForm">
        <h2>Change your email.</h2>
        <fieldset>
            <legend>Password Change</legend>
            <table>
                <tr>
                    <td>
                        <asp:Label ID="NewEmailLabel" runat="server" AssociatedControlID="NewEmailTextBox">New Email</asp:Label>
                    </td>
                    <td></td>
                </tr>
                <tr>
                    <td>
                        <asp:TextBox ID="NewEmailTextBox" runat="server" TextMode="Email"  />
                    </td>
                    <td>
                        <asp:Label ID="NewEmailErrorLabel" runat="server" AssociatedControlID="NewEmailTextBox" CssClass="field-validation-error"></asp:Label>
                    </td>
                </tr>
            </table>
            <table>
                <tr>
                    <td>
                        <asp:Button runat="server" ID="EmailSubmit" Text="Update email" OnClick="EmailSubmit_Click" />
                    </td>
                    <td>
                        <asp:Label ID="EmailResultLabel" runat="server"></asp:Label>
                    </td>
                </tr>
            </table>
        </fieldset>
    </section>
</asp:Content>
