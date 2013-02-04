using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace KServer
{
    //public interface IService1Callback
    //{
    //    [OperationContract(IsOneWay=true)]
    //    void DJQueueChanged(List<queueSinger> queue);
    //}
    // SessionMode = SessionMode.Allowed,
    //  [ServiceContract(CallbackContract = typeof(IService1Callback))]
    [ServiceContract()]
    public interface IDJ
    {
        // DJ STUFF
        // Log in etc
        [OperationContract]
        Response DJSignUp(string userName, string password);
        [OperationContract]
        LogInResponse DJSignIn(string userName, string password);
        [OperationContract]
        Response DJSignOut(long DJKey);
        [OperationContract]
        Response DJCreateSession(long DJKey);

        // Song management
        [OperationContract]
        Response DJAddSongs(List<Song> songs, long DJKey);
        [OperationContract]
        Response DJRemoveSongs(List<Song> songs, long DJKey);
        [OperationContract]
        Response DJListSongs(out List<Song> songs, long DJKey);

        // Queue management
        [OperationContract]
        Response DJAddQueue(SongRequest sr, int queueIndex, long DJKey);
        [OperationContract]
        Response DJRemoveSongRequest(SongRequest sr, long DJKey);
        [OperationContract]
        Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, long DJKey);
        [OperationContract]
        Response DJRemoveUser(int userID, long DJKey);
        [OperationContract]
        Response DJMoveUser(SongRequest newSR, SongRequest oldSR, long DJKey);
        [OperationContract]
        Response DJGetQueue(out List<queueSinger> queue, long DJKey);
        [OperationContract]
        Response DJPopQueue(SongRequest sr, long DJKey);
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
        public Response()
        {
            error = false;
            message = string.Empty;
            result = -1;
        }
        [DataMember]
        public bool error { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public int result { get; set; }
    }

    [DataContract]
    public class LogInResponse
    {
        public LogInResponse()
        {
            error = false;
            message = string.Empty;
            userKey = -1;
            user = null;
            result = -1;
        }

        public LogInResponse(Response r)
        {
            error = r.error;
            message = r.message;
            userKey = -1;
            user = null;
            result = r.result;
        }

        [DataMember]
        public bool error { get; set; }
        [DataMember]
        public string message { get; set; }
        [DataMember]
        public long userKey { get; set; }
        [DataMember]
        public User user { get; set; }
        [DataMember]
        public int result { get; set; }
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
