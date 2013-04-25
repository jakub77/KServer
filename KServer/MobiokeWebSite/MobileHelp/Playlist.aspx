<%@ Page Title="Playlists" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Playlist.aspx.cs" Inherits="MobiokeWebSite.MobileHelp.Playlist" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
        <p>
            This section describes how to create and manage playlists. Playlists allow
            you to easily access the songs you wish to.
        </p>
        <asp:Table ID="Table1" runat="server">
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A13" runat="server" href="~/mhi/playlist_list.jpg">
                        <img src="../mhi/playlist_list.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    Here we see the list of playlists we have. In this case it is a single playlist
                    called "favorite." We can create a new play list by pressing "Create a Play List"
                    and typing in a name for the playlist. The "Refresh" button refreshes our playlists
                    from the Mobioke server. Under the playlist name is the date the playlist was created.
                    To view or edit a playlist, click on a playlist name.
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A8" runat="server" href="~/mhi/playlist.jpg">
                        <img src="../mhi/playlist.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    After clicking a playlist, we can see the songs in the playlist. To delete any song,
                    long press the song and confirm you wish to delete it. If you click on any song, you will
                    be sent to the song menu described in the library help documentation. From this window you
                    can send a song request and do more! Finally, to add a song to a playlist, find the song
                    in the library, click on the song, press "Save to Play List" and click on the playlist name
                    you wish to add to.
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
