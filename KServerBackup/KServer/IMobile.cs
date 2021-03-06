﻿// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// This is the declaration of what methods the Mobile user can call on the server.
// Each operation contract is commented in Mobile.svc.cs. All of these calls are
// meant to be called restfully via http get requests.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace KServer
{
    [ServiceContract]
    public interface IMobile
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/test/?string={s}")]
        Response test(string s);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/GetDateTime/")]
        DateTime GetDateTime();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/TestPushNotification/?DeviceId={deviceID}")]
        string TestPushNotification(string deviceID);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/TestPushToMobile/?message={message}&userKey={userKey}")]
        Response TestPushToMobile(long userKey, string message);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSignUp/?username={username}&password={password}&email={email}")]
        Response MobileSignUp(string username, string password, string email);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSignIn/?username={username}&password={password}&deviceID={deviceID}")]
        LogInResponse MobileSignIn(string username, string password, string deviceID);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSignOut/?userKey={userKey}")]
        Response MobileSignOut(long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSongSearch/?title={title}&artist={artist}&start={start}&count={count}&venueID={venueID}&userKey={userKey}")]
        List<Song> MobileSongSearch(string title, string artist, int start, int count, int venueID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSongBrowse/?firstLetter={firstLetter}&isArtist={isArtist}&start={start}&count={count}&venueID={venueID}&userKey={userKey}")]
        List<Song> MobileSongBrowse(string firstLetter, bool isArtist, int start, int count, int venueID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSongRequest/?songID={songID}&userKey={userKey}")]
        Response MobileSongRequest(int songID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileChangeSongRequest/?oldSongID={oldSongID}&newSongID={newSongID}&userKey={userKey}")]
        Response MobileChangeSongRequest(int oldSongID, int newSongID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileRemoveSongRequest/?songID={songID}&userKey={userKey}")]
        Response MobileRemoveSongRequest(int songID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileMoveSongRequestToTop/?songID={songID}&userKey={userKey}")]
        Response MobileMoveSongRequestToTop(int songID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileViewQueue/?userKey={userKey}")]
        List<queueSinger> MobileViewQueue(long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileGetWaitTime/?userKey={userKey}")]
        Response MobileGetWaitTime(long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileJoinVenue/?QR={QR}&userKey={userKey}")]
        Response MobileJoinVenue(string QR, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileViewSongHistory/?start={start}&count={count}&userKey={userKey}")]
        List<SongHistory> MobileViewSongHistory(int start, int count, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileCreatePlaylist/?name={name}&venueID={venueID}&userKey={userKey}")]
        Response MobileCreatePlaylist(string name, int venueID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileDeletePlaylist/?playListID={playListID}&userKey={userKey}")]
        Response MobileDeletePlaylist(int playListID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileAddSongToPlaylist/?songID={songID}&playListID={playListID}&userKey={userKey}")]
        Response MobileAddSongToPlaylist(int songID, int playListID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileRemoveSongFromPlaylist/?songID={songID}&playListID={playListID}&userKey={userKey}")]
        Response MobileRemoveSongFromPlaylist(int songID, int playListID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileGetPlayLists/?venueID={venueID}&userKey={userKey}")]
        List<Playlist> MobileGetPlayLists(int venueID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileRateSong/?songID={songID}&rating={rating}&venueID={venueID}&userKey={userKey}")]
        Response MobileRateSong(int songID, int rating, int venueID, long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileViewSongRating/?songID={songID}&venueID={venueID}&userKey={userKey}")]
        Response MobileViewSongRating(int songID, int venueID, long userKey);
    }
}
