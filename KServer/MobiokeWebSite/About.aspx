<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="MobiokeWebSite.About" %>

<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
    <hgroup class="title">
        <h1><%: Title %></h1>
        <h2>Mobioke</h2>
    </hgroup>

    <article>
        <p>        
Mobioke modernizes the karaoke experience to an all digital format and 
helps karaoke singers answer the diﬃcult question, “What should I sing?” To perform a karaoke 
show DJ’s use a custom application that automatically integrates with the singers’ phones. Thick, 
cumbersome and outdated binders of songs are replaced with an interactive and customizable 
Android app. Singers can easily browse and search for songs to sing while seamlessly submitting 
their song requests to the DJ from their phone. Users are empowered with current karaoke show 
information and statistics to tailor the karaoke experience to meet their needs. The entire process 
is supported by web services on Mobioke’s custom server that facilitates all communication 
between DJ and singer. Mobioke provides an unparalleled level of communication and ease to 
karaoke allowing everyone to focus on what they came to do...sing! 
        </p>
    </article>

    <aside>
        <h3>Team Warp Zone</h3>
        <p>
            Created by Rick Arnold, Jakub Szpunar and Hugo Yu, Team Warp Zone set out to create a new seamless way to enjoy karaoke.
        </p>
    </aside>
</asp:Content>