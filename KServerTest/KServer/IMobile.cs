using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMobile" in both code and config file together.
    [ServiceContract]
    public interface IMobile
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/test/?string={s}")]
        Response test(string s);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSignUp/?username={username}&password={password}")]
        Response MobileSignUp(string username, string password);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSignIn/?username={username}&password={password}")]
        LogInResponse MobileSignIn(string username, string password);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSignOut/?userKey={userKey}")]
        Response MobileSignOut(long userKey);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSongSearch/?title={title}&artist={artist}&venueID={venueID}")]
        List<Song> MobileSongSearch(string title, string artist, int venueID);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSongBrowse/?firstLetter={firstLetter}&isArtist={isArtist}&start={start}&count={count}&venueID={venueID}")]
        List<Song> MobileSongBrowse(char firstLetter, bool isArtist, int start, int count, int venueID);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "/MobileSongRequest/?songID={songID}&userKey={userKey}")]
        Response MobileSongRequest(int songID, long userKey);


    }


    /*
     *         [OperationContract]
        // Song management
        [OperationContract]
        Response DJAddSongs(List<Song> songs, long DJKey);
        [OperationContract]
        Response DJRemoveSongs(List<Song> songs, long DJKey);
        [OperationContract]
        Response DJListSongs(out List<Song> songs, long DJKey);

        // Queue management
        [OperationContract]
        Response DJAddQueue(SongRequest sr, int queueIndex, int sessionID, long DJKey);
        [OperationContract]
        Response DJRemoveSongRequest(SongRequest sr, int sessionID, long DJKey);
        [OperationContract]
        Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey);
        [OperationContract]
        Response DJRemoveUser(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey);
        [OperationContract]
        Response DJMoveUser(int userID, int sessionID, long DJKey);
     */
    

}
