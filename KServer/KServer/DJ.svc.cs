using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Security.Permissions;
using System.Security.Cryptography;

namespace KServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]

    public class Service1 : IDJ
    {
        /// <summary>
        /// Sign a DJ up for the service.
        /// If an error occurs, the Response will have error set to true, and the error message will be in message.
        /// </summary>
        /// <param name="username">The requested username.</param>
        /// <param name="password">The requested password.</param>
        /// <returns>The success of the operation.</returns>
        public Response DJSignUp(string username, string password, Venue venue, string email)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();
                // Escape to allow the DJTestClient to list all DJ information
                // WILL BE REMOVED FOR RELEASE!
                if (username.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    Response listResponse = db.DJListMembers();
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
                r = db.DJValidateUsername(username);
                if (r.error)
                    return r;
                if (r.message.Trim() != string.Empty)
                {
                    r.error = true;
                    r.message = "That username already exists.";
                    return r;
                }

                // Information seems valid, sign up DJ and return successfulness.
                r = db.DJSignUp(username, password);
                if (r.error)
                    return r;

                return r;
            }
        }

        /// <summary>
        /// Attempts to sign in the DJ using the given credentials.
        /// If an error occurs, the LogInResponse will have the error field as true, and the error will be in message.
        /// </summary>
        /// <param name="username">Username to sign in with.</param>
        /// <param name="password">Password to sign in with.</param>
        /// <returns>LogInReponse returns the outcome. The UserKey sent back is used for all communicaiton in further methods.</returns>
        public LogInResponse DJSignIn(string username, string password)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // See if the username/password combination is valid.
                // If it is valid, the DJID will be stored in r.message.
                // If it is not valid, r.message will be empty.
                r = db.DJValidateUsernamePassword(username, password);
                if (r.error)
                    return new LogInResponse(r);

                // If the username/password couldn't be found, inform user.
                if (r.message.Trim() == string.Empty)
                {
                    r.error = true;
                    r.message = "Username/Password is incorrect.";
                    return new LogInResponse(r);
                }

                // Get the DJID stored in r.message.
                if (!int.TryParse(r.message.Trim(), out DJID))
                {
                    r.error = true;
                    r.message = "Exception in DJSignIn: Unable to parse DJID from DB!";
                    return new LogInResponse(r);
                }

                // Make sure the DJ is not logged in. RIGHT NOW: JUST DON'T CHECK ANYTHING USEFUL TO ALLOW FOR LOGINS TO OCCUR WHEN LOGGED IN!
                r = DJCheckStatus(DJID, "!4", db);
                if (r.error)
                    return new LogInResponse(r);

                // Information seems valid, attempt to sign in.
                r = db.DJSignIn(DJID);
                if (r.error)
                    return new LogInResponse(r);

                // Attempt to change the DJID into a userKey
                long userKey;
                r = DJIDToKey(DJID, out userKey);
                if (r.error)
                    return new LogInResponse(r);

                // If there was no error, create a loginResponse with the successful information.
                LogInResponse lr = new LogInResponse();
                lr.result = r.result;
                lr.userKey = userKey;
                User u = new User();
                u.userName = username;
                u.userID = DJID;
                return lr;
            }
        }

        /// <summary>
        /// Attempt to sign out the DJ belonging to the DJKey.
        /// </summary>
        /// <param name="DJKey">The Unique DJKey that describes the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJSignOut(long DJKey)
        {
            int DJID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // A sign out seems to be valid.
                r = db.DJSignOut(DJID);
                if (r.error)
                    return r;

                // Remove the key from the DB.
                r = db.DJSetKey(DJID, null);
                return r;
            }
        }

        public Response DJCreateSession(long DJKey) 
        {
            Response r = new Response();
            r.error = true;
            r.message = "DJCreateSession is not yet implemented";
            return r; 
        }

        /// <summary>
        /// Add the given songs to the list of songs for the given DJ.
        /// Response.result contains the number of songs successfully added.
        /// </summary>
        /// <param name="songs">Songs to add.</param>
        /// <param name="DJKey">Unique DJKey that describes the DJ.</param>
        /// <returns>The outcome of the operaiton.</returns>
        public Response DJAddSongs(List<Song> songs, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Adding songs seems to be valid, add the list of songs to the DJ.
                r = db.DJAddSongsIgnoringDuplicates(songs, DJID);
                return r;
            }
        }

        /// <summary>
        /// Remove given songs from list of songs belonging to the given DJ.
        /// Response.result contains the number of songs successfully removed.
        /// </summary>
        /// <param name="songs">The list of songs to Remove.</param>
        /// <param name="DJKey">The Unique DJKey describing the DJ</param>
        /// <returns></returns>
        public Response DJRemoveSongs(List<Song> songs, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Information seems to be valid, remove songs.
                r = db.DJRemoveSongs(songs, DJID);
                return r;
            }
        }

        /// <summary>
        /// Get all the songs that belong to the given DJ.
        /// Response.result contains the number of songs.
        /// </summary>
        /// <param name="songs">OUT parameter that holds the list of songs.</param>
        /// <param name="DJKey">Unique Key that describes the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJListSongs(out List<Song> songs, long DJKey)
        {
            songs = null;
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Information seems valid, list the songs.
                r = db.DJListSongs(out songs, DJID);
                return r;
            }
        }

        public Response DJPopQueue(SongRequest sr, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    r.error = true;
                    r.message = "Empty Queue";
                    return r;
                }

                List<queueSinger> queue;
                r = DBToMinimalList(raw, out  queue);
                if (r.error)
                    return r;

                if (queue[0].user.userID != sr.user.userID || queue[0].songs[0].ID != sr.songID)
                {
                    r.error = true;
                    r.message = "Song Request to Pop did not match first song Request, is your queue out of date?";
                    r.message += "\nDBUID: " + queue[0].user.userID + ", GUID: " + sr.user.userID;
                    r.message += "\nDBSID: " + queue[0].songs[0].ID + ", GSID: " + sr.songID;
                    return r;
                }

                queue[0].songs.RemoveAt(0);
                if (queue[0].songs.Count == 0)
                    queue.RemoveAt(0);
                else
                {
                    queueSinger temp = queue[0];
                    queue.RemoveAt(0);
                    queue.Add(temp);
                }

                raw = string.Empty;
                r = MinimalListToDB(queue, out raw);
                if (r.error)
                    return r;
                return db.SetSongRequests(DJID, raw);
            }
        }


        // Queue management
        public Response DJGetQueue(out List<queueSinger> queue, long DJKey)
        {
            queue = new List<queueSinger>();
            int DJID = -1, count = 0;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    r.error = false;
                    r.message = "Empty Queue";
                    return r;
                }

                r = DBToFullList(raw, out queue, DJID, db);
                if (r.error)
                    return r;
                r.result = count;
                return r;
            }
        }

        private Response MinimalListToDB(List<queueSinger> queue, out string raw)
        {
            raw = string.Empty;
            foreach (queueSinger qs in queue)
            {
                raw += qs.user.userID.ToString();
                foreach (Song s in qs.songs)
                {
                    raw += "~" + s.ID;
                }
                raw += "`";
            }
            if (raw.Length > 0)
                raw = raw.Substring(0, raw.Length - 1);
            return new Response();
        }

        private Response DBToMinimalList(string raw, out List<queueSinger> queue)
        {
            int count = 0;
            Response r = new Response();

            queue = new List<queueSinger>();
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
                qs.user = u;

                for (int j = 1; j < parts.Length; j++)
                {
                    Song s = new Song();
                    s.ID = int.Parse(parts[j]);
                    qs.songs.Add(s);

                }
                queue.Add(qs);
                count++;
            }
            return r;
        }

        public Response DBToFullList(string raw, out List<queueSinger> queue, int DJID, DatabaseConnectivity db)
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
                        s.pathOnDisk = songParts[2];
                        qs.songs.Add(s);

                    }
                    queue.Add(qs);
                    count++;
                }
                return r;
        }

        public Response DJGenerateNewQRNumber(long DJKey)
        {
            Response r;
            using(DatabaseConnectivity db = new DatabaseConnectivity())
            {
                int DJID;
                r = DJKeyToID(DJKey, out DJID);
                if(r.error)
                    return r;
                String s = DJID.ToString() + DateTime.Now.ToString();
                System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                byte[] res = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(s));
                StringBuilder sb = new StringBuilder();
                for(int i = 0; i < res.Length; i++)
                    sb.Append(res[i].ToString("x2"));
                String hex = sb.ToString().Substring(0, 8);
                r = db.DJSetQR(hex, DJID);
                if (r.error)
                    return r;
                return r;
            }
        }

        public Response DJGetQRNumber(long DJKey)
        {
            Response r;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                int DJID;
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;
                r = db.DJGetQR(DJID);
                if (r.error)
                    return r;

                if (r.message.Trim().Length == 0)
                {
                    r = DJGenerateNewQRNumber(DJKey);
                    if (r.error)
                        return r;
                    return DJGetQRNumber(DJKey);
                }
                return r;
            }
        }
        public Response DJNewUserWaitTime(long DJKey)
        {
            Response r = new Response();
            return r;
        }

        public Response DJAddQueue(SongRequest sr, int queueIndex, long DJKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "DJAddQueue is not yet implemented";
            return r;
        }
        public Response DJRemoveSongRequest(SongRequest sr, long DJKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "DJRemoveSongRequest is not yet implemented";
            return r;
        }
        public Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, long DJKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "DJChangeSongRequest is not yet implemented";
            return r;
        }
        public Response DJRemoveUser(int userID, long DJKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "DJRemoveUser is not yet implemented";
            return r;
        }
        public Response DJMoveUser(SongRequest newSR, SongRequest oldSR, long DJKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "DJMoveUser is not yet implemented";
            return r;
        }

        public Response DJCheckStatus(int DJID, string desiredStatus, DatabaseConnectivity db)
        {
            Response r;
            int DJStatus, desired;
            bool notStatus = false;
            // Get the status of the DJ.
            r = db.DJGetStatus(DJID);
            if (r.error)
                return r;

            // Attempt to parse that status of the DJ.
            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in DJCheckStatus: Unable to parse status from DB!";
                return r;
            }

            if (desiredStatus[0] == '!')
            {
                notStatus = true;
                desiredStatus = desiredStatus.Substring(1);
            }

            if(!int.TryParse(desiredStatus, out desired))
            {
                r.error = true;
                r.message = "Exception in DJCheckStatus: Cannot parse desired Status";
                return r;
            }

            if (!notStatus)
            {
                if (DJStatus != desired)
                {
                    r.error = true;
                    if (desired == 0)
                        r.message = "You are not signed out.";
                    else if (desired == 1)
                        r.message = "You are not signed in.";
                    else
                        r.message = "You are in the wrong state, possibly not created a session?";
                    return r;
                }
            }
            else if (DJStatus == desired)
            {
                r.error = true;
                if (desired == 0)
                    r.message = "You are signed out and cannot do that.";
                else if (desired == 1)
                    r.message = "You are signed in and cannot do that.";
                else
                    r.message = "You are in the wrong state, do you have a session running?";
                return r;
            }

            r.result = DJStatus;
            return r;
        }

        /// <summary>
        /// Convert a DJKey to a DJID.
        /// </summary>
        /// <param name="DJKey">The DJKey.</param>
        /// <param name="DJID">OUT parameter for the DJID.</param>
        /// <returns>The outcome of the operation.</returns>
        private Response DJKeyToID(long DJKey, out int DJID)
        {
            DJID = -1;
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                r = db.DJGetIDFromKey(DJKey);
                if (r.error)
                    return r;
                if (r.message.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "DJKey is not valid";
                    return r;
                }
                if (!int.TryParse(r.message.Trim(), out DJID))
                {
                    r.error = true;
                    r.message = "Exception in DJKeyToID: DJID Parse Fail";
                    return r;
                }
                return r;
            }
        }

        /// <summary>
        /// Convert a DJID to a DJKey.
        /// </summary>
        /// <param name="DJID">The DJID</param>
        /// <param name="DJKey">OUT parameter for the DJKey</param>
        /// <returns></returns>
        private Response DJIDToKey(int DJID, out long DJKey)
        {
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            byte[] res = sha.ComputeHash(BitConverter.GetBytes(DJID));
            DJKey = BitConverter.ToInt64(res, 0);

            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                r = db.DJSetKey(DJID, DJKey);
                if (r.error)
                    return r;
            }
            return r;
        }

    }
}
