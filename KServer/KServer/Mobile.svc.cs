using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Mobile" in code, svc and config file together.
    //[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, ConcurrencyMode = ConcurrencyMode.Multiple, SessionMode = SessionMode.)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, AddressFilterMode = AddressFilterMode.Any)]
    public class Mobile : IMobile
    {
        public Response test(string s)
        {
            Response r = new Response();
            for (int i = s.Length - 1; i >= 0; i--)
                r.message += s[i];
            r.result = s.Length;
            return r;
        }
        public DateTime GetDateTime()
        {
            return DateTime.Now;
        }
        public string TestPushNotification(string deviceID)
        {
            Response r = Common.PushAndroidNotification(deviceID, "Hello there, did you know the time is " + DateTime.Now.ToShortTimeString() + "?");
            Common.LogError("Google Server Reply for push notification", r.message, null, 2);
            return "Googles servers told me: " + r.message;
        }

        public Response TestPushToMobile(long userKey, string message)
        {
            int mobileID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                return Common.PushMessageToMobile(mobileID, "You sent me: \"" + message + "\"");
            }

        }

        public Response MobileSignUp(string username, string password, string email)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Escape to allow the MobileTestClient to list all Mobile information
                // WILL BE REMOVED FOR RELEASE!
                if (username.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    Response listResponse = db.MobileListMembers();
                    if (listResponse.error)
                        return (Response) Common.LogError(listResponse.message, Environment.StackTrace, listResponse, 0);
                    return listResponse;
                }

                // Validate that username and password are not blank.
                if (username.Length == 0 || password.Length == 0)
                {
                    r.error = true;
                    r.message = "Username or password is blank.";
                    return r;
                }

                // Validate that username and password are not too long.
                if (username.Length > 20 || password.Length > 20)
                {
                    r.error = true;
                    r.message = "Username or password is longer than 20 characters.";
                    return r;
                }

                // Validate the email address.
                try
                {
                    var address = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    r.error = true;
                    r.message = "Email address is not valid";
                    return r;
                }

                // Try to see if the username already exists. If it does, inform the client.
                r = db.MobileValidateUsername(username);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (r.message.Trim() != string.Empty)
                {
                    r.error = true;
                    r.message = "That username already exists.";
                    return r;
                }

                // Information seems valid, sign up client and return successfulness.
                r = db.MobileSignUp(username, password, email);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public LogInResponse MobileSignIn(string username, string password, string deviceID)
        {
            int MobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // See if the username/password combination is valid.
                // If it is valid, the userkey will be stored in r.message.
                // If it is not valid, r.message will be empty.
                r = db.MobileValidateUsernamePassword(username, password);
                if (r.error)
                    return (LogInResponse)Common.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);

                // If the username/password couldn't be found, inform user.
                if (r.message.Trim() == string.Empty)
                {
                    r.error = true;
                    r.message = "Username/Password is incorrect.";
                    return new LogInResponse(r);
                }

                // Get the client ID stored in r.message.
                if (!int.TryParse(r.message.Trim(), out MobileID))
                {
                    r.error = true;
                    r.message = "Exception in MobileSignIn: Unable to parse MobileID from DB!";
                    return (LogInResponse)Common.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);
                }

                // Make sure the client is not logged in. RIGHT NOW: JUST DON'T CHECK ANYTHING USEFUL TO ALLOW FOR LOGINS TO OCCUR WHEN LOGGED IN!
                r = MobileCheckStatus(MobileID, "!4", db);
                if (r.error)
                    return new LogInResponse(r);

                r = db.MobileSignIn(MobileID, deviceID);
                if (r.error)
                    return (LogInResponse)Common.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);

                // Attempt to change the MobileID into a userKey
                long userKey;
                r = MobileIDToKey(MobileID, out userKey);
                if (r.error)
                    return (LogInResponse)Common.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);

                // If there was no error, create a loginResponse with the successful information.
                LogInResponse lr = new LogInResponse();
                lr.result = r.result;
                lr.userKey = userKey;
                User u = new User();
                u.userName = username;
                u.userID = MobileID;
                return lr;
            }
        }
        public Response MobileSignOut(long userKey)
        {
            int MobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out MobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(MobileID, "!0", db);
                if (r.error)
                    return r;

                // A sign out seems to be valid.
                r = db.MobileSetStatus(MobileID, 0);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Remove the key from the DB.
                r = db.MobileSetKey(MobileID, null);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Clear the venue from the DB
                r = db.MobileSetVenue(MobileID, null);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public List<Song> MobileSongSearch(string title, string artist, int start, int count, int venueID, long userKey)
        {
            int venueStatus;
            int mobileID = -1;
            List<Song> songs = new List<Song>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                    return (List<Song>)Common.LogError("MobileSongSeach venueID parse fail (bad venueID given?)", Environment.StackTrace, null, 0);

                // Complete the search.
                r = db.MobileSearchSongs(title.Trim(), artist.Trim(), venueID, start, count);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                if (r.message.Trim() == string.Empty)
                    return songs;

                string[] songLines = r.message.Trim().Split('\n');
                foreach (string songLine in songLines)
                {
                    string[] songParts = Common.splitByDel(songLine);
                    Song song = new Song();
                    int id;
                    if (!int.TryParse(songParts[0], out id))
                        return (List<Song>)Common.LogError("Exception in MobileListSongsSQL: could not parse song id", Environment.StackTrace, null, 0);
                    song.ID = id;
                    song.title = songParts[1];
                    song.artist = songParts[2];
                    song.duration = int.Parse(songParts[3]);
                    Common.LoadSongRating(ref song, mobileID, db);
                    song.pathOnDisk = "";
                    songs.Add(song);
                }
                return songs;
            }
        }
        public List<Song> MobileSongBrowse(string firstLetter, bool isArtist, int start, int count, int venueID, long userKey)
        {
            int venueStatus;
            int mobileID = -1;
            List<Song> songs = new List<Song>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Check to make sure the venue exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                    return (List<Song>)Common.LogError("MobileSongBrose venueID parse fail (bad venueID given?)", Environment.StackTrace, null, 0);

                r = db.MobileBrowseSongs(firstLetter, isArtist, start, count, venueID);
                if (r.error)
                    return (List<Song>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                if (r.message.Trim() == string.Empty)
                    return songs;

                int count2 = 0;
                string[] songLines = r.message.Trim().Split('\n');
                foreach (string songLine in songLines)
                {
                    string[] songParts = Common.splitByDel(songLine);
                    Song song = new Song();
                    int id;
                    if (!int.TryParse(songParts[0], out id))
                        return (List<Song>)Common.LogError("Exception in MobileListSongsSQL: could not parse song id", Environment.StackTrace, null, 0);
                    song.ID = id;
                    song.title = songParts[1];
                    song.artist = songParts[2];
                    song.duration = int.Parse(songParts[3]);
                    Common.LoadSongRating(ref song, mobileID, db);
                    song.pathOnDisk = "";
                    songs.Add(song);
                    count2++;
                }
                return songs;
            }
        }
        public Response MobileSongRequest(int songID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song";
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                string requests = r.message;
                string newRequests = string.Empty;

                if (requests.Trim().Length == 0)
                {
                    requests = mobileID.ToString() + "~" + songID.ToString();
                    r = db.SetSongRequests(venueID, requests);
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Search to see if the user is already in this list of singers.
                for (int i = 0; i < queue.Count; i++)
                {
                    // We found the userID already in here.
                    if (queue[i].user.userID == mobileID)
                    {
                        // Loop through the songs to see if the user is already singing this song.
                        for (int j = 0; j < queue[i].songs.Count; j++)
                        {
                            if (queue[i].songs[j].ID == songID)
                            {
                                r.error = true;
                                r.message = "You are already in queue to sing that song";
                                return r;
                            }

                        }
                        // They dont' already have the song in the list, add them to the list
                        Song s = new Song();
                        s.ID = songID;
                        queue[i].songs.Add(s);
                        Common.MinimalListToDB(queue, out newRequests);
                        r = db.SetSongRequests(venueID, newRequests);
                        if(r.error)
                            return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                        return r;
                    }
                }

                queueSinger qs = new queueSinger();
                qs.songs = new List<Song>();

                User u = new User();
                u.userID = mobileID;
                qs.user = u;

                Song song = new Song();
                song.ID = songID;
                qs.songs.Add(song);

                queue.Add(qs);
                Common.MinimalListToDB(queue, out newRequests);
                r = db.SetSongRequests(venueID, newRequests);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public Response MobileChangeSongRequest(int oldSongID, int newSongID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            bool songChangeMade = false;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(venueID, newSongID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find new song";
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                string requests = r.message;
                string newRequests = string.Empty;

                // If there are no song requests.
                if (requests.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "There are no song requests.";
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Search to see if the user is already in this list of singers.
                for (int i = 0; i < queue.Count; i++)
                {
                    // We found the userID already in here.
                    if (queue[i].user.userID == mobileID)
                    {
                        // Loop through the songs to find the old song.
                        for (int j = 0; j < queue[i].songs.Count; j++)
                        {
                            // If we find the new song, we don't want to allow duplicates
                            if (queue[i].songs[j].ID == newSongID)
                            {
                                r.error = true;
                                r.message = "You are already singing the new song";
                                return r;
                            }
                            // If we found the old song.
                            if (queue[i].songs[j].ID == oldSongID)
                            {
                                queue[i].songs[j].ID = newSongID;
                                songChangeMade = true;
                            }

                        }

                        if (songChangeMade)
                        {
                            Common.MinimalListToDB(queue, out newRequests);
                            r = db.SetSongRequests(venueID, newRequests);
                            if(r.error)
                                return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                            return r;
                        }
                        // If we couldn't find the old song, inform user.
                        r.error = true;
                        r.message = "Could not find the old song";
                        return r;
                    }
                }

                // If we couldn't find the user.
                r.error = true;
                r.message = "You have no song reqeusts.";
                return r;
            }
        }
        public Response MobileMoveSongRequestToTop(int songID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song in venue library.";
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                string requests = r.message;
                string newRequests = string.Empty;

                // If there are no song requests.
                if (requests.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "There are no song requests.";
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Search to see if the user is already in this list of singers.
                for (int i = 0; i < queue.Count; i++)
                {
                    // We found the userID already in here.
                    if (queue[i].user.userID == mobileID)
                    {
                        // Loop through the songs to find the song.
                        for (int j = 0; j < queue[i].songs.Count; j++)
                        {
                            // Remove this song, and add them to the front of the queue.
                            if (queue[i].songs[j].ID == songID)
                            {
                                Song s = queue[i].songs[j];
                                queue[i].songs.RemoveAt(j);
                                queue[i].songs.Insert(0, s);
                                r = Common.MinimalListToDB(queue, out newRequests);
                                if(r.error)
                                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                                r = db.SetSongRequests(venueID, newRequests);
                                if (r.error)
                                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                                return r;
                            }
                        }  
                    }
                }

                // If we couldn't find the user.
                r.error = true;
                r.message = "Could not find that song in your queue.";
                return r;
            }
        }
        public Response MobileRemoveSongRequest(int songID, long userKey)
        {
            int venueID = -1;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileIDx
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                string requests = r.message;
                string newRequests = string.Empty;

                if (requests.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "There are no song requests to remove";
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Search to see if the user is already in this list of singers.
                for (int i = 0; i < queue.Count; i++)
                {
                    // If the user is found.
                    if (queue[i].user.userID == mobileID)
                    {
                        // Loop through the songs to find the current song.
                        for (int j = 0; j < queue[i].songs.Count; j++)
                        {
                            if (queue[i].songs[j].ID == songID)
                            {
                                queue[i].songs.RemoveAt(j);
                                if (queue[i].songs.Count == 0)
                                    queue.RemoveAt(i);
                                Common.MinimalListToDB(queue, out newRequests);
                                r = db.SetSongRequests(venueID, newRequests);
                                if(r.error)
                                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                                return r;
                            }

                        }
                        // If we can't find the current song.
                        r.error = true;
                        r.message = "Could not find the song to remove";
                        return r;
                    }
                }

                r.error = true;
                r.message = "Could not find you in the queue";
                return r;
            }
        }
        public List<queueSinger> MobileViewQueue(long userKey)
        {
            int venueID = -1;
            int mobileID = -1;
            List<queueSinger> queue = new List<queueSinger>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (List<queueSinger>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (List<queueSinger>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (List<queueSinger>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return queue;

                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (List<queueSinger>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    return queue;
                }

                r = DBToNearlyFullList(raw, out queue, venueID, mobileID, db);
                if (r.error)
                    return (List<queueSinger>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                return queue;
            }
        }
        public Response MobileJoinVenue(string QR, long userKey)
        {
            int mobileID = -1;
            int venueID = -1;
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venue of this qr string.
                r = db.GetVenueIDByQR(QR.Trim());
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Parse the venueID.
                if (!int.TryParse(r.message.Trim(), out venueID))
                {
                    r.error = true;
                    r.message = "Could not match QR code to a venue";
                    return r;
                }

                // Set the venue of the client
                r = db.MobileSetVenue(mobileID, venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                r = db.GetVenueName(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                r.result = venueID;
            }
            return r;
        }

        private Response VenueCheckStatus(int venueID, string desiredStatus, DatabaseConnectivity db)
        {
            Response r;
            int DJStatus, desired;
            bool notStatus = false;
            // Get the status of the DJ.
            r = db.DJGetStatus(venueID);
            if (r.error)
                return r;

            // Attempt to parse that status of the DJ.
            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in VenueCheckStatus: Unable to parse status from DB!";
                return r;
            }

            if (desiredStatus[0] == '!')
            {
                notStatus = true;
                desiredStatus = desiredStatus.Substring(1);
            }

            if (!int.TryParse(desiredStatus, out desired))
            {
                r.error = true;
                r.message = "Exception in VenueCheckStatus: Cannot parse desired Status";
                return r;
            }

            if (!notStatus)
            {
                if (DJStatus != desired)
                {
                    r.error = true;
                    if (desired == 0)
                        r.message = "Venue is online.";
                    else if (desired == 1)
                        r.message = "Venue is not online.";
                    else
                        r.message = "Venue does not have a session running";
                    return r;
                }
            }
            else if (DJStatus == desired)
            {
                r.error = true;
                if (desired == 0)
                    r.message = "Venue is not online.";
                else if (desired == 1)
                    r.message = "Venue is online.";
                else
                    r.message = "Venue has a session running.";
                return r;
            }

            r.result = DJStatus;
            return r;
        }
        private Response DBToNearlyFullList(string raw, out List<queueSinger> queue, int DJID, int mobileID, DatabaseConnectivity db)
        {
            queue = new List<queueSinger>();
            Response r = new Response();
            int count = 0;

            string[] clientRequests = raw.Split('`');
            for (int i = 0; i < clientRequests.Length; i++)
            {
                string[] parts = clientRequests[i].Split('~');
                if (parts.Length == 0)
                {
                    r.error = true;
                    r.message = "Error in DBtoList 1";
                    return r;
                }

                queueSinger qs = new queueSinger();
                qs.songs = new List<Song>();
                User u = new User();
                u.userID = int.Parse(parts[0]);

                if (u.userID < 0)
                    r = db.DJGetTempUserName(u.userID, DJID);
                else
                    r = db.MobileIDtoUsername(u.userID);

                if (r.error)
                    return r;
                if (r.message.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "DB Username lookup exception in DJGetQueue!";
                    return r;
                }

                u.userName = r.message.Trim();
                qs.user = u;

                for (int j = 1; j < parts.Length; j++)
                {
                    Song song;
                    r = Common.GetSongInformation(int.Parse(parts[j]), DJID, mobileID, out song, db);
                    if (r.error)
                        return r;
                    qs.songs.Add(song);

                }
                queue.Add(qs);
                count++;
            }
            return r;
        }
        private Response MobileCheckStatus(int mobileID, string desiredStatus, DatabaseConnectivity db)
                {
                    Response r;
                    int MobileStatus, desired;
                    bool notStatus = false;
                    // Get the status of the DJ.
                    r = db.MobileGetStatus(mobileID);
                    if (r.error)
                        return r;

                    // Attempt to parse that status of the DJ.
                    if (!int.TryParse(r.message.Trim(), out MobileStatus))
                    {
                        r.error = true;
                        r.message = "Exception in MobileCheckStatus: Unable to parse status from DB!";
                        return r;
                    }

                    if (desiredStatus[0] == '!')
                    {
                        notStatus = true;
                        desiredStatus = desiredStatus.Substring(1);
                    }

                    if (!int.TryParse(desiredStatus, out desired))
                    {
                        r.error = true;
                        r.message = "Exception in MobileCheckStatus: Cannot parse desired Status";
                        return r;
                    }

                    if (!notStatus)
                    {
                        if (MobileStatus != desired)
                        {
                            r.error = true;
                            if (desired == 0)
                                r.message = "You are not signed out.";
                            else if (desired == 1)
                                r.message = "You are not signed in.";
                            else
                                r.message = "You are in the wrong state, do you have a venue selected?";
                            return r;
                        }
                    }
                    else if (MobileStatus == desired)
                    {
                        r.error = true;
                        if (desired == 0)
                            r.message = "You are signed out and cannot do that.";
                        else if (desired == 1)
                            r.message = "You are signed in and cannot do that.";
                        else
                            r.message = "You are in the wrong state, do you have a venue selected?";
                        return r;
                    }

                    r.result = MobileStatus;
                    return r;
                }
        private Response MobileKeyToID(long MobileKey, out int MobileID)
        {
            MobileID = -1;
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                r = db.MobileGetIDFromKey(MobileKey);
                if (r.error)
                    return r;
                if (r.message.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "MobileKey is not valid.";
                    return r;
                }
                if (!int.TryParse(r.message.Trim(), out MobileID))
                {
                    r.error = true;
                    r.message = "Exception in MobileKeyToID: MobileKey Parse Fail";
                    return r;
                }
                return r;
            }
        }
        private Response MobileIDToKey(int MobileID, out long MobileKey)
        {
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] res = sha.ComputeHash(BitConverter.GetBytes(MobileID));
            MobileKey = BitConverter.ToInt64(res, 0);

            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                r = db.MobileSetKey(MobileID, MobileKey);
                if (r.error)
                    return r;
            }
            return r;
        }
        private Response MobileGetVenue(int mobileID, DatabaseConnectivity db)
        {
            int venueID = -1;
            Response r = new Response();
            r = db.MobileGetVenue(mobileID);
            if (r.error)
                return r;

            if (!int.TryParse(r.message.Trim(), out venueID))
            {
                r.error = true;
                r.message = "Could not parse venueID from DB";
                return r;
            }

            r.result = venueID;
            return r;
        }

        public Response MobileGetWaitTime(long userKey)
        {
            int venueID = -1;
            int mobileID = -1;
            List<queueSinger> queue = new List<queueSinger>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();
                
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    r.error = false;
                    r.message = "Empty Queue";
                    r.result = 0;
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                r = DBToNearlyFullList(raw, out queue, venueID, mobileID, db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                int time = 0;
                for (int i = 0; i < queue.Count; i++)
                {
                    if (queue[i].user.userID == mobileID)
                        break;
                    else
                        time += queue[i].songs[0].duration + Common.TIME_BETWEEN_REQUESTS;
                }

                r.error = false;
                r.message = time.ToString().Trim();
                r.result = time;
                return r;
            }
        }
        public Response MobileCreatePlaylist(string name, int venueID, long userKey)
        {
            Response r = new Response();
            if (name.Length < 1 || name.Length > 20)
            {
                r.error = true;
                r.message = "Name must be between 1 and 20 characters.";
                return r;
            }

            int mobileID = -1;
            int venueStatus;
            
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.error = true;
                    r.message = "Could not validate venue";
                    if (r.error)
                        return r;
                }

                r = db.MobileCreatePlaylist(name, venueID, mobileID, DateTime.Now);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public Response MobileDeletePlaylist(int playListID, long userKey)
        {
            int mobileID = -1;
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                r = db.MobileDeletePlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                if (r.result == 0)
                {
                    r.error = true;
                    r.message = "No playlist matched that criteria";
                    return r;
                }
                return r;
            }
        }
        public Response MobileAddSongToPlaylist(int songID, int playListID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venue information from the playlist in DB.
                r = db.MobileGetVenueFromPlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out venueID))
                {
                    r.error = true;
                    r.message = "Could not figure out Venue from DB";
                    return r;
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song in venue's library.";
                    return r;
                }

                // Get the current songs in the playlist.
                r = db.MobileGetSongsFromPlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                string songsString = r.message.Trim();
                string newSongsString = string.Empty;

                // If there were no songs, simply add this one song and return.
                if(songsString.Length == 0)
                {
                    newSongsString = songID.ToString();
                    r = db.MobileSetPlaylistSongs(playListID, mobileID, newSongsString);
                    if(r.error)
                        return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                    return r;
                }

                // Check to see if the song already exists.
                string[] splitSongs = songsString.Split('~');
                foreach (string s in splitSongs)
                {
                    if (s.Equals(songID.ToString()))
                    {
                        r.error = true;
                        r.message = "You already have that song in this playlist";
                        return r;
                    }
                }

                // If there are songs, append this song to the list.
                newSongsString = songsString + "~" + songID.ToString();
                r = db.MobileSetPlaylistSongs(playListID, mobileID, newSongsString);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public Response MobileRemoveSongFromPlaylist(int songID, int playListID, long userKey)
        {
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the current songs in the playlist.
                r = db.MobileGetSongsFromPlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                string songsString = r.message.Trim();
                string newSongsString = string.Empty;

                // Find the index of the song to remove.
                string[] splitSongs = songsString.Split('~');

                int songRemoveIndex = -1;
                for (int i = 0; i < splitSongs.Length; i++)
                    if (songID.ToString().CompareTo(splitSongs[i].Trim()) == 0)
                        songRemoveIndex = i;

                // If we didn't find a song to remove, return the error.
                if (songRemoveIndex == -1)
                {
                    r.error = true;
                    r.message = "Could not find the song in the playlist " + songsString;
                    return r;
                }

                for (int i = 0; i < splitSongs.Length; i++)
                    if (i != songRemoveIndex)
                        newSongsString += splitSongs[i] + "~";

                if (splitSongs.Length > 1)
                    newSongsString = newSongsString.Substring(0, newSongsString.Length - 1);

                r = db.MobileSetPlaylistSongs(playListID, mobileID, newSongsString);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public List<Playlist> MobileGetPlayLists(int venueID, long userKey)
        {
            int mobileID = -1;
            int venueStatus;
            List<Playlist> playLists = new List<Playlist>();
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (List<Playlist>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (List<Playlist>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Validate venueID if the any venueID not given.
                if (venueID != -1)
                {
                    // Make sure the venueID exists.
                    r = db.DJGetStatus(venueID);
                    if (r.error)
                        return (List<Playlist>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                    if (!int.TryParse(r.message.Trim(), out venueStatus))
                        return (List<Playlist>)Common.LogError("MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace, null, 0);
                }

                r = db.MobileGetPlaylists(venueID, mobileID);
                if (r.error)
                    return (List<Playlist>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Case of empty playlists.
                if (r.message.Trim().Length == 0)
                    return playLists;

                try
                {
                    string[] playlistLines = r.message.Trim().Split('\n');
                    foreach (string playlistLine in playlistLines)
                    {
                        string[] playlistParts = Common.splitByDel(playlistLine);
                        Playlist p = new Playlist();

                        int id;
                        if (!int.TryParse(playlistParts[0], out id))
                            return (List<Playlist>)Common.LogError("MobileGetPlaylists songID parse fail.", Environment.StackTrace, null, 0);
                        p.ID = id;

                        p.name = playlistParts[1];
                        p.dateCreated = Convert.ToDateTime(playlistParts[3]);

                        int vid;
                        if (!int.TryParse(playlistParts[4], out vid))
                            return (List<Playlist>)Common.LogError("MobileGetPlaylists venueID parse fail from playlist.", Environment.StackTrace, null, 0);
                        p.venue = new Venue();
                        p.venue.venueID = vid;

                        r = db.GetVenueName(vid);
                        if (r.error)
                            return (List<Playlist>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                        p.venue.venueName = r.message.Trim();

                        r = db.GetVenueAddress(vid);
                        if(r.error)
                            return (List<Playlist>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                        p.venue.venueAddress = r.message.Trim();

                        string[] songs = playlistParts[2].Trim().Split('~');
                        p.songs = new List<Song>();
                        foreach (string s in songs)
                        {
                            if (s.Trim().Length == 0)
                                continue;
                            Song song = new Song();
                            r = Common.GetSongInformation(int.Parse(s), vid, mobileID, out song, db);
                            if(r.error)
                                return (List<Playlist>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                            p.songs.Add(song);
                        }
                        playLists.Add(p);
                    }
                    return playLists;
                }
                catch (Exception e)
                {
                    return (List<Playlist>)Common.LogError(e.Message, e.StackTrace, null, 0);
                }
            }
        }

        
        
        public List<SongHistory> MobileViewSongHistory(int start, int count, long userKey)
        {
            int mobileID = -1;
            List<SongHistory> songHistory = new List<SongHistory>();
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (List<SongHistory>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (List<SongHistory>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                r = db.MobileGetSongHistory(mobileID, start, count);
                if (r.error)
                    return (List<SongHistory>)Common.LogError(r.message, Environment.StackTrace, null, 0);

                // Case of empty playlists.
                if (r.message.Trim().Length == 0)
                    return songHistory;

                //"ID", "VenueID", "MobileID", "SongID", "DateSung"
                try
                {
                    string[] songHistoryLines = r.message.Trim().Split('\n');
                    foreach (string songHistoryLine in songHistoryLines)
                    {
                        string[] songHistoryParts = Common.splitByDel(songHistoryLine);
                        SongHistory sh = new SongHistory();

                        int vid;
                        if (!int.TryParse(songHistoryParts[1], out vid))
                            return (List<SongHistory>)Common.LogError("MobileGetSongHistory venueID parse fail from MobileSongHistory.", Environment.StackTrace, null, 0);
                        sh.venue = new Venue();
                        sh.venue.venueID = vid;

                        r = db.GetVenueName(vid);
                        if (r.error)
                            return (List<SongHistory>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                        sh.venue.venueName = r.message.Trim();

                        r = db.GetVenueAddress(vid);
                        if (r.error)
                            return (List<SongHistory>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                        sh.venue.venueAddress = r.message.Trim();

                        int songID;
                        if(!int.TryParse(songHistoryParts[3], out songID))
                            return (List<SongHistory>)Common.LogError("MobileGetSongHistory songID parse fail from MobileSongHistory.", Environment.StackTrace, null, 0);

                        Song song;
                        r = Common.GetSongInformation(songID, vid, mobileID,out song, db, false);
                        if(r.error)
                            return (List<SongHistory>)Common.LogError(r.message, Environment.StackTrace, null, 0);
                        sh.song = song;
                        sh.date = Convert.ToDateTime(songHistoryParts[4]);
                        songHistory.Add(sh); 
                    }
                    return songHistory;
                }
                catch (Exception e)
                {
                    return (List<SongHistory>)Common.LogError(e.Message, e.StackTrace, null, 0);
                }
            }
        }





        // Anytime songs are requested, put a set the rating field if applicable.
        public Response MobileRateSong(int songID, int rating, int venueID, long userKey)
        {
            int mobileID = -1;
            int venueStatus;
            int songExists;
            List<Playlist> playLists = new List<Playlist>();
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                if (rating < -1 || rating > 5)
                {
                    r.error = true;
                    r.message = "Rating must be between -1 and 5 (inclusive).";
                    return r;
                }

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.error = true;
                    r.message= "MobileGetPlayLists venueID parse fail (Bad venueID given?)";
                    return r;
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song";
                    return r;
                }

                // Set the song rating.
                r = db.MobileSetSongRating(mobileID, songID, rating);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public Response MobileViewSongRating(int songID, int venueID, long userKey)
        {
            int mobileID = -1;
            int venueStatus;
            int songExists;
            int rating;
            List<Playlist> playLists = new List<Playlist>();
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.error = true;
                    r.message = "MobileGetPlayLists venueID parse fail (Bad venueID given?)";
                    return r;
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song";
                    return r;
                }

                // Set the song rating.
                r = db.MobileGetSongRating(mobileID, songID);
                if(r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                if (r.message.Trim().Length == 0)
                {
                    r.message = "-1";
                    r.result = -1;
                    return r;
                }

                if (!int.TryParse(r.message.Trim(), out rating))
                {
                    r.error = true;
                    r.message = "Could not parse rating";
                    return r;
                }

                r.message = rating.ToString();
                r.result = rating;
                return r;
            }
        }

        // GCM google push notifications.
    }
}