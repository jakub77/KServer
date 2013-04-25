<%@ Page Title="Queue" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Queue.aspx.cs" Inherits="MobiokeWebSite.MobileHelp.Queue" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
        <p>
            This section describes the singer queue feature of the app. The queue allows you to see the order for
            singers and which songs they are singing.
        </p>
        <asp:Table ID="Table1" runat="server">
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A13" runat="server" href="~/mhi/queue.jpg">
                        <img src="../mhi/queue.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    In the queue page we see all the users who are in line to sing a song. We see the username
                    of the user and the song they wish to sing. In the top right hand corner of the screen, we also
                    see the estimated wait time until our turn. In the image it is 17 minutes. The queue is
                    automatically refreshed each time it changes, but you can hit "Refresh" to manually refresh it.
                    If you click on your song, you will be offered the ability to change the song request.
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A8" runat="server" href="~/mhi/edit_song_queue.jpg">
                        <img src="../mhi/edit_song_queue.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    If you elected to change a song request, you will be shown a screen similar to this one. By clicking
                    on any of the songs in that screen, you are able to remove the song from your requests, or set it to
                    be the next song you wish to sing.
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
