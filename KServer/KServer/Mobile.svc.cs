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
                        return listResponse;
                    if (r.error)
                        return r;
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
                if (username.Length > 10 || password.Length > 10)
                {
                    r.error = true;
                    r.message = "Username or password is longer than 10 characters.";
                    return r;
                }

                // Try to see if the username already exists. If it does, inform the client.
                r = db.MobileValidateUsername(username);
                if (r.error)
                    return r;
                if (r.message.Trim() != string.Empty)
                {
                    r.error = true;
                    r.message = "That username already exists.";
                    return r;
                }

                // Information seems valid, sign up client and return successfulness.
                r = db.MobileSignUp(username, password);
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
                    return new LogInResponse(r);

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
                    return new LogInResponse(r);
                }

                // Make sure the client is not logged in. RIGHT NOW: JUST DON'T CHECK ANYTHING USEFUL TO ALLOW FOR LOGINS TO OCCUR WHEN LOGGED IN!
                r = MobileCheckStatus(MobileID, "!4", db);
                if (r.error)
                    return new LogInResponse(r);

                // Information seems valid, attempt to sign in.
                r = db.MobileSetStatus(MobileID, 1);
                if (r.error)
                    return new LogInResponse(r);

                // Attempt to change the MobileID into a userKey
                long userKey;
                r = MobileIDToKey(MobileID, out userKey);
                if (r.error)
                    return new LogInResponse(r);

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
                    return r;

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(MobileID, "!0", db);
                if (r.error)
                    return r;

                // A sign out seems to be valid.
                r = db.MobileSetStatus(MobileID, 0);
                if(r.error)
                    return r;

                // Remove the key from the DB.
                r = db.MobileSetKey(MobileID, null);
                if (r.error)
                    return r;

                // Clear the venue from the DB
                r = db.MobileSetVenue(MobileID, null);
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
                    return null;
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                    return null;

                // Complete the search.
                r = db.MobileSearchSongs(out songs, title.Trim(), artist.Trim(), venueID);
                if (r.error)
                    return null;

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
                    return null;
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                    return null;

                r = db.MobileBrowseSongs(out s, firstLetter, isArtist, start, count, venueID);
                if (r.error)
                    return null;

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
                    return r;

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return r;
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return r;
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find song";
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return r;

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
                    return r;

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
                        return db.SetSongRequests(venueID, newRequests);
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
                return db.SetSongRequests(venueID, newRequests);
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
                    return r;

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return r;
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(venueID, newSongID);
                if (r.error)
                    return r;
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.error = true;
                    r.message = "Could not find new song";
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return r;

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
                    return r;

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
                            return db.SetSongRequests(venueID, newRequests);
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
                    return r;

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return r;
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return r;

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return r;

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
                    return r;

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
                                return db.SetSongRequests(venueID, newRequests);
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
                    return null;

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return null;

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return null;
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db);
                if (r.error)
                    return queue;

                r = db.GetSongRequests(venueID);
                if (r.error)
                    return null;

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    return queue;
                }

                r = DBToNearlyFullList(raw, out queue, venueID, db);
                if (r.error)
                    return null;
                return queue;
            }
        }
       

        public Response VenueCheckStatus(int venueID, string desiredStatus, DatabaseConnectivity db)
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
        public static Response MobileCheckStatus(int mobileID, string desiredStatus, DatabaseConnectivity db)
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
                    return r;

                // Make sure the client isn't already logged out.
                r = MobileCheckStatus(mobileID, "!0", db);
                if (r.error)
                    return r;

                // Get the venue of this qr string.
                r = db.GetVenueIDByQR(QR.Trim());
                if (r.error)
                    return r;

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
                    return r;

                r = db.GetVenueName(venueID);
                if (r.error)
                    return r;
                r.result = venueID;                
            }
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
            Response r = new Response();
            r.error = true;
            return r;
        }

        public Response MobileDeletePlaylist(int playListID, long userKey)
        {
            Response r = new Response();
            r.error = true;
            return r;
        }

        public Response MobileAddSongToPlaylist(int songID, int playListID, long userKey)
        {
            Response r = new Response();
            r.error = true;
            return r;
        }

        public Response MobileRemoveSongFromPlaylist(int songID, int playListID, long userKey)
        {
            Response r = new Response();
            r.error = true;
            return r;
        }

        public List<Playlist> MobileGetPlayLists(int venueID, long userKey)
        {

            return new List<Playlist>();
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
