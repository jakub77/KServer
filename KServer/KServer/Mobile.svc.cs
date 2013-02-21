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
        public Response MobileSignUp(string username, string password)
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
                        return (Response) CommonMethods.LogError(listResponse.message, Environment.StackTrace, listResponse, 0);
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

                // Try to see if the username already exists. If it does, inform the client.
                r = db.MobileValidateUsername(username);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                if (r.message.Trim() != string.Empty)
                {
                    r.error = true;
                    r.message = "That username already exists.";
                    return r;
                }

                // Information seems valid, sign up client and return successfulness.
                r = db.MobileSignUp(username, password);
                if(r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public LogInResponse MobileSignIn(string username, string password)
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
                    return (LogInResponse)CommonMethods.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);

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
                    return (LogInResponse)CommonMethods.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);
                }

                // Make sure the client is not logged in. RIGHT NOW: JUST DON'T CHECK ANYTHING USEFUL TO ALLOW FOR LOGINS TO OCCUR WHEN LOGGED IN!
                r = MobileCheckStatus(MobileID, "!4", db);
                if (r.error)
                    return new LogInResponse(r);

                // Information seems valid, attempt to sign in.
                r = db.MobileSetStatus(MobileID, 1);
                if (r.error)
                    return (LogInResponse)CommonMethods.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);

                // Attempt to change the MobileID into a userKey
                long userKey;
                r = MobileIDToKey(MobileID, out userKey);
                if (r.error)
                    return (LogInResponse)CommonMethods.LogError(r.message, Environment.StackTrace, new LogInResponse(r), 0);

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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(MobileID, "!0", db);
                if (r.error)
                    return r;

                // A sign out seems to be valid.
                r = db.MobileSetStatus(MobileID, 0);
                if(r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Remove the key from the DB.
                r = db.MobileSetKey(MobileID, null);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Clear the venue from the DB
                r = db.MobileSetVenue(MobileID, null);
                if(r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }
        public List<Song> MobileSongSearch(string title, string artist, int venueID)
        {
            int venueStatus;
            List<Song> songs;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (List<Song>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                    return (List<Song>)CommonMethods.LogError("MobileSongSeach venueID parse fail (bad venueID given?)", Environment.StackTrace, null, 0);

                // Complete the search.
                r = db.MobileSearchSongs(out songs, title.Trim(), artist.Trim(), venueID);
                if (r.error)
                    return (List<Song>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                return songs;
            }
        }
        public List<Song> MobileSongBrowse(string firstLetter, bool isArtist, int start, int count, int venueID)
        {
            int venueStatus;
            List<Song> s;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Check to make sure the venue exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (List<Song>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                    return (List<Song>)CommonMethods.LogError("MobileSongBrose venueID parse fail (bad venueID given?)", Environment.StackTrace, null, 0);

                r = db.MobileBrowseSongs(out s, firstLetter, isArtist, start, count, venueID);
                if (r.error)
                    return (List<Song>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                return s;
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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r ;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song";
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                r = CommonMethods.DBToMinimalList(requests, out queue);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                        CommonMethods.MinimalListToDB(queue, out newRequests);
                        r = db.SetSongRequests(venueID, newRequests);
                        if(r.error)
                            return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                CommonMethods.MinimalListToDB(queue, out newRequests);
                r = db.SetSongRequests(venueID, newRequests);
                if(r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(venueID, newSongID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find new song";
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                r = CommonMethods.DBToMinimalList(requests, out queue);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                            CommonMethods.MinimalListToDB(queue, out newRequests);
                            r = db.SetSongRequests(venueID, newRequests);
                            if(r.error)
                                return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                r = CommonMethods.DBToMinimalList(requests, out queue);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                                CommonMethods.MinimalListToDB(queue, out newRequests);
                                r = db.SetSongRequests(venueID, newRequests);
                                if(r.error)
                                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                    return (List<queueSinger>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (List<queueSinger>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return (List<queueSinger>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return queue;

                r = db.GetSongRequests(venueID);
                if (r.error)
                    return (List<queueSinger>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    return queue;
                }

                r = DBToNearlyFullList(raw, out queue, venueID, db);
                if (r.error)
                    return (List<queueSinger>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);
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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venue of this qr string.
                r = db.GetVenueIDByQR(QR.Trim());
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                r = db.GetVenueName(venueID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
        private Response DBToNearlyFullList(string raw, out List<queueSinger> queue, int DJID, DatabaseConnectivity db)
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
                    Song s = new Song();
                    s.ID = int.Parse(parts[j]);
                    r = db.SongInformation(DJID, s.ID);
                    if (r.error)
                        return r;
                    if (r.message.Trim().Length == 0)
                    {
                        r.error = true;
                        r.message = "DB Song lookup exception in DJGETQUEUE!";
                        return r;
                    }
                    string[] songParts = r.message.Split(',');
                    s.title = songParts[0];
                    s.artist = songParts[1];
                    s.pathOnDisk = string.Empty;
                    qs.songs.Add(s);

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
            Response r = new Response();
            r.error = true;
            return r;
        }

        public List<SongHistory> MobileViewSongHistory(int start, int count, long userKey)
        {
            return new List<SongHistory>();
        }

        public Response MobileCreatePlaylist(string name, int venueID, long userKey)
        {
            int mobileID = -1;
            int venueStatus;
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.error = true;
                    r.message = "Could not validate venue";
                    if (r.error)
                        return r;
                }

                r = db.MobileCreatePlaylist(name, venueID, mobileID, DateTime.Now);
                if(r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                r = db.MobileDeletePlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venue information from the playlist in DB.
                r = db.MobileGetVenueFromPlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out venueID))
                {
                    r.error = true;
                    r.message = "Could not figure out Venue from DB";
                    return r;
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song in venue's library.";
                    return r;
                }

                // Get the current songs in the playlist.
                r = db.MobileGetSongsFromPlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                string songsString = r.message.Trim();
                string newSongsString = string.Empty;

                // If there were no songs, simply add this one song and return.
                if(songsString.Length == 0)
                {
                    newSongsString = songID.ToString();
                    r = db.MobileSetPlaylistSongs(playListID, mobileID, newSongsString);
                    if(r.error)
                        return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the current songs in the playlist.
                r = db.MobileGetSongsFromPlaylist(playListID, mobileID);
                if (r.error)
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);

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
                    return (Response)CommonMethods.LogError(r.message, Environment.StackTrace, r, 0);
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
                    return (List<Playlist>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return (List<Playlist>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return (List<Playlist>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                    return (List<Playlist>)CommonMethods.LogError("MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace, null, 0);

                r = db.MobileGetPlaylists(venueID, mobileID);
                if (r.error)
                    return (List<Playlist>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);

                // Case of empty playlists.
                if (r.message.Trim().Length == 0)
                    return playLists;

                try
                {
                    string[] playlistLines = r.message.Trim().Split('\n');
                    foreach (string playlistLine in playlistLines)
                    {
                        string[] playlistParts = playlistLine.Split(',');
                        Playlist p = new Playlist();

                        int id;
                        if (!int.TryParse(playlistParts[0], out id))
                            return (List<Playlist>)CommonMethods.LogError("MobileGetPlaylists songID parse fail.", Environment.StackTrace, null, 0);
                        p.ID = id;

                        p.name = playlistParts[1];
                        p.dateCreated = Convert.ToDateTime(playlistParts[3]);

                        int vid;
                        if (!int.TryParse(playlistParts[4], out vid))
                            return (List<Playlist>)CommonMethods.LogError("MobileGetPlaylists venueID parse fail from playlist.", Environment.StackTrace, null, 0);
                        p.venueID = vid;

                        string[] songs = playlistParts[2].Trim().Split('~');
                        p.songs = new List<Song>();
                        foreach (string s in songs)
                        {
                            if (s.Trim().Length == 0)
                                continue;
                            Song song = new Song();
                            song.ID = int.Parse(s);
                            r = db.SongInformation(venueID, song.ID);
                            if (r.error)
                                return (List<Playlist>)CommonMethods.LogError(r.message, Environment.StackTrace, null, 0);
                            if (r.message.Trim().Length < 4)
                                return (List<Playlist>)CommonMethods.LogError("MobiileGetPlaylists Failure to parse Song information: '" + r.message.Trim() + "'", Environment.StackTrace, null, 0);
                            string[] songParts = r.message.Split(',');
                            song.title = songParts[0];
                            song.artist = songParts[1];
                            p.songs.Add(song);
                        }
                        playLists.Add(p);
                    }
                    return playLists;
                }
                catch (Exception e)
                {
                    return (List<Playlist>)CommonMethods.LogError(e.Message, e.StackTrace, null, 0);
                }
            }
        }

        public Response MobileRateSong(int songID, int venueID, long userKey)
        {
            Response r = new Response();
            r.error = true;
            return r;
        }

        public Response MobileViewSongRating(int songID, int venueID, long userKey)
        {
            Response r = new Response();
            r.error = true;
            return r;
        }
    }
}
