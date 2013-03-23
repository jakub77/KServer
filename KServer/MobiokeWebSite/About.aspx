<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="MobiokeWebSite.About" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
        <h2>Mobioke</h2>
    </hgroup>

    <article>
        <p>        
            Mobioke allows you to seamlessly interact with other singers and DJs to get the most out of your karaoke experience.
        </p>

        <p>        
            Say goodbye to looking through huge books of songs and say hello to searching for songs with the press of a button.
        </p>

        <p>        
            Worried about how long until your turn comes up? Mobioke will tell you!
        </p>
    </article>

    <aside>
        <h3>Team Warp Zone</h3>
        <p>        
            Mobioke was created by Team Warp Zone at the University of Utah from a Senior Capstone Project. 
        </p>
    </aside>
</asp:Content>