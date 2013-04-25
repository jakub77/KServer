<%@ Page Title="Login, register, and join a venue" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="MobiokeWebSite.MobileHelp.Login" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
        <p>
            This section describes registration, logging in, and joining a karaoke venue.
        </p>
            <asp:Table runat="server">
                <asp:TableRow>
                    <asp:TableCell>
                        <a runat= "server" href="~/mhi/launch.jpg"><img src="../mhi/launch.jpg" width="256" height="427" /></a>
                    </asp:TableCell>
                    <asp:TableCell>
                        To launch the Mobioke mobile app, find the Mobioke icon on your mobile desktop,
                        or find the Mobioke icon in your apps list.
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        <a id="A9" runat="server" href="~/mhi/login_screen.jpg"><img src="../mhi/login_screen.jpg" width="256" height="427" /></a>
                    </asp:TableCell>
                    <asp:TableCell>
                        After launching the app, you will be prompted to log in. If you have an account,
                        you simply enter your username and password into the provided fields. If you wish
                        for the app to remember your username and password, check the "Remember me" checkbox.
                        Once you have entered your information, press the "Login" button to log in.
                        <br /><br />
                        If you do not already have a Mobioke mobile account, press the "New User" button to
                        be taken to a registration page. If you have a Mobioke mobile account but forgot your
                        username or password, long press the MOBIOKE icon to be taken to<a runat="server" href="~/Account/Manage2.aspx">our website</a>
                        which features account management features such as password resets.
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        <a id="A10" runat="server" href="~/mhi/register.jpg">
                            <img src="../mhi/register.jpg" width="256" height="427" /></a>
                    </asp:TableCell>
                    <asp:TableCell>
                        If you did not need to register, skip this step. To register, simply fill in the
                        form with your information. Note that your username will be visible to other Mobioke users
                        and your email address will be kept private and only used to allow you to reset your password
                        or recover your username. Once done, press "Sign Up" and login as described before.
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <asp:Table runat="server">
                <asp:TableRow>
                    <asp:TableCell>
                        <a id="A11" runat="server" href="~/mhi/scan_qr.jpg">
                            <img src="../mhi/scan_qr.jpg" width="427" height="256" /></a>
                    </asp:TableCell>
                </asp:TableRow>
                <asp:TableRow>
                    <asp:TableCell>
                        Once you are logged in, the app will request you to scan a QR code belonging to the DJ
                        venue that you wish to sing at. If you do not have <a id="A8" runat="server" href="https://play.google.com/store/apps/details?id=com.google.zxing.client.android">Barcode Scanner</a>
                        installed, the app will direct you to install it. A QR code helps the app figure out what venue you are in
                        and pair you with the DJ at that venue. If you are not at a venue, you will be unable to get
                        past this step. Simply point the camera at the QR code and the app will take care of the rest.
                        You have now joined a venue, let's start singing!
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
    </article>

    <aside>
        <h3>Directory</h3>
        <ul>
            <li><a id="A1" runat="server" href="MobileHelp.aspx">Mobile overview</a></li>
            <li><a id="A2" runat="server" href="Login.aspx">Login, registration, and joining a venue</a></li>
            <li><a id="A3" runat="server" href="Library.aspx">The library</a></li>
            <li><a id="A4" runat="server" href="Queue.aspx">The queue</a></li>
            <li><a id="A5" runat="server" href="Playlist.aspx">Playlists</a></li>
            <li><a id="A6" runat="server" href="Profile.aspx">My Profile</a></li>
            <li><a id="A7" runat="server" href="History.aspx">Song History</a></li>
        </ul>
    </aside>
</asp:Content>
