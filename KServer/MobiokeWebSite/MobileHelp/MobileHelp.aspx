<%@ Page Title="Mobile Documentation" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MobileHelp.aspx.cs" Inherits="MobiokeWebSite.MobileHelp.MobileHelp" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
        <p class="message-info">
            If you are looking for DJ documentation, see our Help menu in the DJ Client. 
            If you do not have an account for the DJ Client, register for one <a runat="server" href="~/Account/DJRegister.aspx">HERE.</a>
        </p>
        <br />
        <p>
            This documentation targets the Mobioke mobile client. This documentation also assumes you
            have the Mobioke mobile client installed on your phone. If you do not have the Mobioke mobile
            client installed on your phone please<a runat="server" href="~/Contact.aspx">contact us</a>
            for more information. To navigate between documentation sections, simply click on one of the 
            links on the right hand side to see the documentation for that section.
        </p>
        <p>
            The Mobioke mobile client requires an Android 2.2 (Froyo) or newer smartphone with a working
            camera built-in to the smartphone.
            The client uses the<a runat="server" href ="https://play.google.com/store/apps/details?id=com.google.zxing.client.android">Barcode Scanner</a>app
            to scan QR codes. If your phone does not have this program installed, you will
            be prompted and directed to the play store to install it after logging in.
        </p>
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
