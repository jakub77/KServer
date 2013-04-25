<%@ Page Title="Library" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Library.aspx.cs" Inherits="MobiokeWebSite.MobileHelp.Library" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
        <p>
            This section describes the library tab found in the Mobioke mobile app. The library tab allows you to
            find songs, send song requests, and rate songs. The library tab allows you to find songs by browsing by
            title, searching by title or artist, browsing most popular songs, and browsing songs suggested to you
            by our Mobioke system.
        </p>
        <asp:Table ID="Table1" runat="server">
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A10" runat="server" href="~/mhi/browse.jpg">
                        <img src="../mhi/browse.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    The first way to find songs is to browse. Click the "Browse" button in the lower left hand corner; 
                    you are now viewing songs based on title sorted in alphabetical order. In this image, songs are being
                    viewed from the letter A. To view more songs by the letter A, drag your finger up while touching the song
                    list to see more songs. If you get to the bottom of the list, press "Load More Songs" to get the next set
                    of matchin songs. If you wish to see songs from a different letter, simply select a different
                    letter from the drop down box at the top of the screen. 
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A8" runat="server" href="~/mhi/serach.jpg">
                        <img src="../mhi/serach.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    The second way to find songs is to search for them. To search for a song, click the "Search" button at
                    the bottom of the screen. Next, type the song title or artist you wish to search for in the textbox at 
                    the top of the screen. Before you hit "Search" make sure you have "By Artist" or "By Title" properly
                    selected. After hitting search, you will see your results. To scroll through them, drag your finger up while
                    touching the list of songs. If you reach the bottom, press "Load More Songs" to see more songs.
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A9" runat="server" href="~/mhi/top_songs.jpg">
                        <img src="../mhi/top_songs.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    The third way to find songs is to see what songs others have been singing. After clicking the "Top Songs" button
                    at the bottom of the screen, you will see the songs that were sung most often at your venue.
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A11" runat="server" href="~/mhi/suggestions.jpg">
                        <img src="../mhi/suggestions.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    The fourth way to find songs is to see what songs the Mobioke system suggests you sing. After clicking the "Suggestions"
                    button at the lower right hand corner of the screen, you will see what songs Mobioke things you would like to sing.
                    These suggestions are based primarily on your past history of song choices.
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A12" runat="server" href="~/mhi/song.jpg">
                        <img src="../mhi/song.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    If you click on a song from any of the previous four methods, you will see a screen similar to this one.
                    This screen allows you to send a song request to the queue, save a song request to a playlist, search  for
                    the lyrics to this song, or find the video of this song on YouTube.
                    If you click on "Send to the Queue," your song will be received by the DJ and you will be in line to sing!
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A13" runat="server" href="~/mhi/song_rated.jpg">
                        <img src="../mhi/song_rated.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    The song screen also allows you to rate a song for personal reference. Your song ratings will only be seen by you.
                    To rate a song, simply drag your finger over the number of stars you wish to rate the song. Your song ratings are
                    persistent over logging in and out.
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
