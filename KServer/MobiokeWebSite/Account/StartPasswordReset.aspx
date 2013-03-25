<%@ Page Title="Password Reset" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="StartPasswordReset.aspx.cs" Inherits="MobiokeWebSite.Account.StartPasswordReset" %>


<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
    </hgroup>
    <fieldset>
        <legend>Reset Password</legend>
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
                    <asp:CheckBox ID="IsDJCheckBox" runat="server" />
                    <asp:Label ID="Label1" runat="server" AssociatedControlID="IsDJCheckBox" CssClass="checkbox">Is this a DJ account?</asp:Label>
                </td>
                <td></td>
            </tr>
        </table>
        <table>
            <tr>
                <td>
                    <asp:Button runat="server" ID="Submit" Text="Reset my password" OnClick="Submit_Click" />
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


