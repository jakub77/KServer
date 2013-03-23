<%@ Page Title="DJ Registration" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DJRegister.aspx.cs" Inherits="MobiokeWebSite.Account.DJRegister" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %>.</h1>
        <h2>Use the form below to create a new account.</h2>
    </hgroup>

    <p class="message-info">
        If you prefer to sing than DJ, please register <a id="A1" runat="server" href="~/Account/Register2.aspx">HERE!</a><br />
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
                    <asp:Label ID="VenueNameLabel" runat="server" AssociatedControlID="VenueNameTextBox">Name of your Venue</asp:Label>
                </td>
                <td></td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="VenueNameTextBox" runat="server" />
                </td>
                <td>
                    <asp:Label ID="VenueNameErrorLabel" runat="server" AssociatedControlID="VenueNameTextBox" CssClass="field-validation-error"></asp:Label>
                </td>
            </tr>
            <tr>
                <td>
                    <asp:Label ID="VenueAddressLabel" runat="server" AssociatedControlID="VenueAddressTextBox">Address of your Venue</asp:Label>
                </td>
                <td></td>
            </tr>
            <tr>
                <td>
                    <asp:TextBox ID="VenueAddressTextBox" runat="server" />
                </td>
                <td>
                    <asp:Label ID="VenueAddressErrorLabel" runat="server" AssociatedControlID="VenueAddressTextBox" CssClass="field-validation-error"></asp:Label>
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
