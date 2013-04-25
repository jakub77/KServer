<%@ Page Title="Profile" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="MobiokeWebSite.MobileHelp.Profile" %>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
    </hgroup>

    <article>
        <p>
            This section describes your profile page. Your profile page tells you which venue you are at, allows you
            to sign out, and allows you to view your achievements.
        </p>
        <asp:Table ID="Table1" runat="server">
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A13" runat="server" href="~/mhi/profile.jpg">
                        <img src="../mhi/profile.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    Here we see the main profile page. The Mobioke icon can be long pressed to
                    go to our website which allows for account management. Towards the bottom of the 
                    screen, we see our name and which venue we are signed in to. Second from the bottom, 
                    we have the "Sign out" button which allows us to sign out. This will remove us from
                    the venue and clear all of our song requests. At the very bottom of the page we have
                    the "Achievements" button which allows us to see our achievements as described below.
                </asp:TableCell>
            </asp:TableRow>
            <asp:TableRow>
                <asp:TableCell>
                    <a id="A8" runat="server" href="~/mhi/achievements.jpg">
                        <img src="../mhi/achievements.jpg" width="256" height="427" /></a>
                </asp:TableCell>
                <asp:TableCell>
                    Here we see the achievements we have earned. Achievements are defined by the DJ
                    of each venue. In this image we have earned 3 achievements. At the top we can switch between
                    viewing earned and unearned achievements. Note that DJs can create hidden achievements that 
                    you cannot see in the "Not Achieved" section.
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
