using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        // DJ STUFF
        // Log in etc
        [OperationContract]
        LogInResponse DJSignUp(string userName, string password);
        [OperationContract]
        LogInResponse DJSignIn(string userName, string password);
        [OperationContract]
        Response DJSignOut(long DJKey);

        [OperationContract]
        Session DJCreateSession(long DJKey);

        // Song management
        [OperationContract]
        Response DJAddSongs(List<Song> songs, long DJKey);
        [OperationContract]
        Response DJRemoveSongs(List<Song> songs, long DJKey);

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

        // CLIENT STUFF
        // Log in etc
        [OperationContract]
        LogInResponse ClientSignUp(string userName, string password);
        [OperationContract]
        LogInResponse ClientSignIn(string userName, string password);
        [OperationContract]
        Response ClientSignOut(long userKey);

        // Song lists
        [OperationContract]
        List<Song> Search(string title, string artist, int venueID);
        [OperationContract]
        List<Song> Browse(char firstLetter, bool isArtist, int start, int count, int venueID);

        // Song Requests
        [OperationContract]
        Response ClientSongRequest(SongRequest sr, long userKey);
        [OperationContract]
        Response ClientRemoveSongRequest(SongRequest sr, long userKey);
        [OperationContract]
        Response ClientChangeSongRequest(int prevSongID, SongRequest sr, long userKey);







    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.

    // Describe individual venues.
    [DataContract]
    public class Venue
    {
        [DataMember]
        public int venueID { get; set; }
        [DataMember]
        public string venueName { get; set; }
        [DataMember]
        public string venueAddress { get; set; }
    }

    // Describe individual users.
    [DataContract]
    public class User
    {
        [DataMember]
        public int userID { get; set; }
        [DataMember]
        public string userName { get; set; }
    }

    // Describe a song
    [DataContract]
    public class Song
    {
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string artist { get; set; }
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string pathOnDisk {get; set; }
    }

    // Describe a session
    [DataContract]
    public class Session
    {
        [DataMember]
        public int venueID { get; set; }
        [DataMember]
        public int sessionID { get; set; }
    }

    // Song Request
    [DataContract]
    public class SongRequest
    {
        [DataMember]
        public User user { get; set; }
        [DataMember]
        public int songID { get; set; }
    }

    [DataContract]
    public class queueSinger
    {
        [DataMember]
        public User user { get; set; }
        [DataMember]
        public List<Song> songs { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember]
        public bool error { get; set; }
        [DataMember]
        public string errorMessage { get; set; }
        [DataMember]
        public int result { get; set; }
    }

    [DataContract]
    public class LogInResponse
    {
        [DataMember]
        public bool error { get; set; }
        [DataMember]
        public string errorMessage { get; set; }
        [DataMember]
        public long userKey { get; set; }
        [DataMember]
        public User user { get; set; }
    }
    

    [DataContract]
    public class Session
    {
        [DataMember]
        public int sessionID { get; set; }
    }

    [DataContract]
    public class Credentials
    {
        [DataMember]
        public string userName { get; set; }
        [DataMember]
        public string password { get; set; }
    }
}
