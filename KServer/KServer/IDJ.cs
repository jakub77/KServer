// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// This is the declaration of what methods the DJ can call on the server.
// Each operation contract is commented in DJ.svc.cs. This file also contains
// all data contracts that are used throughout the server. 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace KServer
{
    [ServiceContract()]
    public interface IDJ
    {
        // Log in /etc
        [OperationContract]
        Response DJSignUp(string userName, string password, Venue venue, string email);
        [OperationContract]
        LogInResponse DJSignIn(string userName, string password);
        [OperationContract]
        Response DJSignOut(long DJKey);
        [OperationContract]
        Response DJCreateSession(long DJKey);
        [OperationContract]
        Response DJStopSession(long DJKey);
        [OperationContract]
        Response DJGetQRNumber(long DJKey);
        [OperationContract]
        Response DJGenerateNewQRNumber(long DJKey);

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
        Response DJMoveSongRequest(SongRequest sr, int newIndex, long DJKey);
        [OperationContract]
        Response DJRemoveUser(int userID, long DJKey);
        [OperationContract]
        Response DJMoveUser(int userID, int index, long DJKey);
        [OperationContract]
        Response DJGetQueue(out List<queueSinger> queue, long DJKey);
        [OperationContract]
        Response DJPopQueue(SongRequest sr, long DJKey);
        [OperationContract]
        Response DJNewUserWaitTime(long DJKey);
        [OperationContract]
        Response DJTestQueueFill(long DJKey);
        [OperationContract]
        Response DJGetMostPopularSongs(long DJKey, bool limitToVenue, int start, int count, out List<Song> songs, out List<int> counts);
        [OperationContract]
        Response DJBanUser(User userToBan, long DJKey);
        [OperationContract]
        Response DJUnbanUser(User userToUnban, long DJKey);
        [OperationContract]
        Response DJGetBannedUsers(long DJKey, out List<User> users);
        [OperationContract]
        Response DJAddAchievement(Achievement achievement, long DJKey);
        [OperationContract]
        Response DJDeleteAchievement(int achievementID, long DJKey);
        [OperationContract]
        Response DJViewAchievements(long DJKey, out List<Achievement> achievements);
    }

    [DataContract]
    public class AchievementSelect
    {
        // DateTime.min /max if you don't care
        [DataMember]
        public DateTime startDate { get; set; }
        [DataMember]
        public DateTime endDate { get; set; }
        
        // Count = 2 etc. newest = 3, gets the third newest person to meet clause.
        [DataMember]
        public SelectKeyword selectKeyword { get; set; }
        [DataMember]
        public string selectValue { get; set; }

        // songtitle=umbrella etc
        [DataMember]
        public ClauseKeyword clauseKeyword { get; set; }
        [DataMember]
        public string clauseValue { get; set; }    
    }
    [DataContract]
    public enum SelectKeyword
    {
        [EnumMember]
        CountEqual,
        [EnumMember]
        CountNotEqual,
        [EnumMember]
        CountGreaterThan,
        [EnumMember]
        CountLessThan,
        [EnumMember]
        Max,
        [EnumMember]
        Min,
        [EnumMember]
        Newest,
        [EnumMember]
        Oldest
    }
    [DataContract]
    public enum ClauseKeyword
    {
        [EnumMember]
        Artist,
        [EnumMember]
        Title,
        [EnumMember]
        SongID
    }
    [DataContract]
    public class Achievement
    {
        // ID of achievement. return ID from achievement create method on server.
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string description { get; set; }
        [DataMember]
        public byte[] imageArray { get; set; }
        // All statmeents in selectList are anded together if true, otherwise, all ored togethere
        [DataMember]
        public bool statementsAnd { get; set; }
        [DataMember]
        public List<AchievementSelect> selectList { get; set; }
    }

    // Describes a song history object.
    [DataContract]
    public class SongHistory
    {
        [DataMember]
        public Song song { get; set; }
        [DataMember]
        public Venue venue { get; set; }
        [DataMember]
        public DateTime date { get; set; }
    }

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

    // Describe a playlist.
    [DataContract]
    public class Playlist
    {
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public Venue venue { get; set; }
        [DataMember]
        public List<Song> songs { get; set; }
        [DataMember]
        public DateTime dateCreated { get; set; }
    }

    // Describe a song
    [DataContract]
    public class Song
    {
        public Song()
        {
            rating = -1;
        }
        [DataMember]
        public string title { get; set; }
        [DataMember]
        public string artist { get; set; }
        [DataMember]
        public int ID { get; set; }
        [DataMember]
        public string pathOnDisk {get; set; }
        [DataMember]
        public int duration { get; set; }
        // -1 = not rated
        // 0 - 5 = star rating
        [DataMember]
        public int rating { get; set; }
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

    // Describe a user in the queue.
    [DataContract]
    public class queueSinger
    {
        [DataMember]
        public User user { get; set; }
        [DataMember]
        public List<Song> songs { get; set; }
    }

    // Desribe a response passed between functions.
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

    // Describe a reponse only used for logins.
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

    // Describe a user's credentials.
    [DataContract]
    public class Credentials
    {
        [DataMember]
        public string userName { get; set; }
        [DataMember]
        public string password { get; set; }
    }
}
