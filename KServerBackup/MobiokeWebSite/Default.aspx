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
            Contact us about receiving a test copy of our Mobioke DJ software to start up Karaoke sessions wherever you go!
        </li>
        <li class="two">
            <h5>Singers</h5>
            Fire up your android phones and contact us for a test copy of our Mobioke singer software to start singing at your favorite karaoke spots.
        </li>
        <li class="three">
            <h5>Sing together</h5>
            DJ's create karaoke sessions. With the click of a button singers can join in the fun and start searching for and singing their favorite hits.
        </li>
    </ol>
</asp:Content>
