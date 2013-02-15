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
                r = db.DJSetStatus(DJID, 1);
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
                r = db.DJSetStatus(DJID, 0);
                if (r.error)
                    return r;

                // Remove the key from the DB.
                r = db.DJSetKey(DJID, null);
                return r;
            }
        }
        /// <summary>
        /// Completed
        /// </summary>
        /// <param name="DJKey"></param>
        /// <returns></returns>
        public Response DJGenerateNewQRNumber(long DJKey)
        {
            Response r;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                int DJID;
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;
                String s = DJID.ToString() + DateTime.Now.ToString();
                System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                byte[] res = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(s));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < res.Length; i++)
                    sb.Append(res[i].ToString("x2"));
                String hex = sb.ToString().Substring(0, 8);
                r = db.DJSetQR(hex, DJID);
                if (r.error)
                    return r;
                return r;
            }
        }
        /// <summary>
        ///  Complete
        /// </summary>
        /// <param name="DJKey"></param>
        /// <returns></returns>
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
                r = CommonMethods.DBToMinimalList(raw, out  queue);
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
                r = CommonMethods.MinimalListToDB(queue, out raw);
                if (r.error)
                    return r;
                return db.SetSongRequests(DJID, raw);
            }
        }
        /// <summary>
        /// Done
        /// </summary>
        /// <param name="queue"></param>
        /// <param name="DJKey"></param>
        /// <returns></returns>
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

                r = CommonMethods.DBToFullList(raw, out queue, DJID, db);
                if (r.error)
                    return r;
                r.result = count;
                return r;
            }
        }
        /// <summary>
        /// Done
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="queueIndex"></param>
        /// <param name="DJKey"></param>
        /// <returns></returns>
        public Response DJAddQueue(SongRequest sr, int queueIndex, long DJKey)
        {
            int DJID = -1;
            int songID = -1;
            int clientID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(DJID, sr.songID);
                if (r.error)
                    return r;

                // Make sure a songID was sent back.
                if (!int.TryParse(r.message.Trim(), out songID))
                {
                    r.error = true;
                    r.message = "Could not find song";
                    return r;
                }

                // If no userID is passed in.
                if (sr.user.userID == 0 || sr.user.userID == -1)
                {
                    r = db.MobileValidateUsername(sr.user.userName);
                    if (r.error)
                        return r;
                    if (!int.TryParse(r.message.Trim(), out clientID))
                    {
                        r.error = true;
                        r.message = "CLient name could not be validated.";
                        return r;
                    }
                }
                // If a userID is passed in.
                else
                {
                    r = db.MobileValidateID(sr.user.userID);
                    if (r.error)
                        return r;
                    // See if an ID was returned.
                    if (r.message.Trim() == String.Empty)
                    {
                        string s = r.message.Trim();
                        r.error = true;
                        r.message = "Client ID could not be validated.";
                        return r;
                    }
                    clientID = sr.user.userID;
                }

                // Get the current song Requests
                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string requests = r.message;
                string newRequests = string.Empty;

                // If there were no requests, simply add the single request.
                if (requests.Trim().Length == 0)
                {
                    newRequests = sr.user.userID.ToString() + "~" + sr.songID.ToString();
                    r = db.SetSongRequests(DJID, newRequests);
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
                    if (queue[i].user.userID == clientID)
                    {
                        // Loop through the songs to see if the user is already singing this song.
                        for (int j = 0; j < queue[i].songs.Count; j++)
                        {
                            if (queue[i].songs[j].ID == sr.songID)
                            {
                                r.error = true;
                                r.message = "User is already singing that song";
                                return r;
                            }

                        }
                        // They dont' already have the song in the list, add them to the list
                        Song s = new Song();
                        s.ID = sr.songID;
                        queue[i].songs.Add(s);
                        CommonMethods.MinimalListToDB(queue, out newRequests);
                        return db.SetSongRequests(DJID, newRequests);
                    }
                }

                // Now they are not in the queue, add them at queueIndex. 
                queueSinger qs = new queueSinger();
                qs.songs = new List<Song>();

                qs.user = sr.user;
                qs.user.userID = clientID;

                Song song = new Song();
                song.ID = sr.songID;
                qs.songs.Add(song);

                if (queueIndex < 0)
                    queueIndex = 0;
                if (queueIndex > queue.Count)
                    queueIndex = queue.Count;
                queue.Insert(queueIndex, qs);
                CommonMethods.MinimalListToDB(queue, out newRequests);
                return db.SetSongRequests(DJID, newRequests);
            }
        }
        /// <summary>
        /// Done
        /// </summary>
        /// <param name="sr"></param>
        /// <param name="DJKey"></param>
        /// <returns></returns>
        public Response DJRemoveSongRequest(SongRequest sr, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Get the current song Requests
                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string requests = r.message;
                string newRequests = string.Empty;

                // If there were no requests, simply add the single request.
                if (requests.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "The queue is empty";
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
                    if (queue[i].user.userID == sr.user.userID)
                    {
                        // Loop through the songs to see if the user is already singing this song.
                        for (int j = 0; j < queue[i].songs.Count; j++)
                        {
                            if (queue[i].songs[j].ID == sr.songID)
                            {
                                queue[i].songs.RemoveAt(j);
                                if (queue[i].songs.Count == 0)
                                    queue.RemoveAt(i);
                                CommonMethods.MinimalListToDB(queue, out newRequests);
                                return db.SetSongRequests(DJID, newRequests);
                            }

                        }
                        // If we can't find the current song.
                        r.error = true;
                        r.message = "Could not find the song to remove";
                        return r;
                    }
                }

                r.error = true;
                r.message = "Could not find client in the queue.";
                return r;
            }
        }
        /// <summary>
        /// Done
        /// </summary>
        /// <param name="newSR"></param>
        /// <param name="oldSR"></param>
        /// <param name="DJKey"></param>
        /// <returns></returns>
        public Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, long DJKey)
        {
            int DJID = -1;
            int songID = -1;
            bool songChangeMade = false;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                if (newSR.user.userID != oldSR.user.userID)
                {
                    r.error = true;
                    r.message = "User must be the same between song requets";
                    return r;
                }

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(DJID, newSR.songID);
                if (r.error)
                    return r;

                // Make sure a songID was sent back.
                if (!int.TryParse(r.message.Trim(), out songID))
                {
                    r.error = true;
                    r.message = "Could not find new song.";
                    return r;
                }

                // Make sure the mobile user is valid.
                    r = db.MobileValidateID(oldSR.user.userID);
                    if (r.error)
                        return r;
                    // See if an ID was returned.
                    if (r.message.Trim() == String.Empty)
                    {
                        string s = r.message.Trim();
                        r.error = true;
                        r.message = "Client ID could not be validated.";
                        return r;
                    }

                // Get the current song Requests
                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string requests = r.message;
                string newRequests = string.Empty;

                // If there were no requests, simply add the single request.
                if (requests.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "There are no song requests";
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
                    if (queue[i].user.userID == oldSR.user.userID)
                    {
                        // Loop through the songs to see if the user is already singing this song.
                        for (int j = 0; j < queue[i].songs.Count; j++)
                        {
                            if (queue[i].songs[j].ID == newSR.songID)
                            {
                                r.error = true;
                                r.message = "User is already singing that song";
                                return r;
                            }
                            if (queue[i].songs[j].ID == oldSR.songID)
                            {
                                queue[i].songs[j].ID = newSR.songID;
                                songChangeMade = true;
                            }

                        }

                        
                        if (songChangeMade)
                        {
                            CommonMethods.MinimalListToDB(queue, out newRequests);
                            return db.SetSongRequests(DJID, newRequests);
                        }

                        // We didn't find the old song.
                        r.error = true;
                        r.message = "Could not find the old song.";
                        return r;
                    }
                }

                // We didn't find the user.
                r.error = true;
                r.message = "Could not find the user.";
                return r;
            }
        }
        /// <summary>
        /// Done
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="DJKey"></param>
        /// <returns></returns>
        public Response DJRemoveUser(int userID, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Get the current song Requests
                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string requests = r.message;
                string newRequests = string.Empty;

                // If there were no requests, simply add the single request.
                if (requests.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "The queue is empty";
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
                    if (queue[i].user.userID == userID)
                    {
                        queue.RemoveAt(i);
                        CommonMethods.MinimalListToDB(queue, out newRequests);
                        return db.SetSongRequests(DJID, newRequests);
                    }
                }

                r.error = true;
                r.message = "Could not find client in the queue.";
                return r;
            }
        }
        /// <summary>
        /// Done
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="index"></param>
        /// <param name="DJKey"></param>
        /// <returns></returns>
        public Response DJMoveUser(int userID, int index, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                Response r = new Response();

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJCheckStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Get the current song Requests
                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string requests = r.message;
                string newRequests = string.Empty;

                // If there were no requests, simply add the single request.
                if (requests.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "The queue is empty";
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
                    if (queue[i].user.userID == userID)
                    {
                        if (index < 0)
                            index = 0;
                        if (index >= queue.Count)
                            index = queue.Count - 1;
                        queueSinger tmp = queue[i];
                        queue.RemoveAt(i);
                        queue.Insert(index, tmp);
                        CommonMethods.MinimalListToDB(queue, out newRequests);
                        return db.SetSongRequests(DJID, newRequests);
                    }
                }

                r.error = true;
                r.message = "Could not find client in the queue.";
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
        public Response DJNewUserWaitTime(long DJKey)
        {
            Response r = new Response();
            return r;
        }


        /// <summary>
        /// Done
        /// </summary>
        /// <param name="DJID"></param>
        /// <param name="desiredStatus"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private Response DJCheckStatus(int DJID, string desiredStatus, DatabaseConnectivity db)
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
