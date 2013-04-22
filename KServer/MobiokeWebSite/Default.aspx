<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MobiokeWebSite._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1>Mobioke</h1>
                <h2>the truly seamless and electronic karaoke system.</h2>
            </hgroup>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">

    <h3>Our Process</h3>
    <ol class="round">
        <li class="one">
            <h5>DJs</h5>
            DJ's use a custom client to run the show.
        </li>
        <li class="two">
            <h5>Singers</h5>
            Singers run a mobile app form their android-powered phones.
        </li>
        <li class="three">
            <h5>Sing together</h5>
            DJ's start karaoke sessions and singers easily join in the fun, searching for and singing their favorite hits.
        </li>
    </ol>
</asp:Content>
