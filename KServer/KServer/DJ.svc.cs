// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// DJ contains all the methods that the DJ client can call. This is how the
// DJ client interfaces with the server and mobile users. This implements the
// interface defined in IDJ.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Security.Permissions;
using System.Security.Cryptography;
using System.Diagnostics;
using System.IO;
using System.Data.SqlClient;

// Notes:
// improve error message.
// make database browse ignore non alphanumberic characters until it finds alphanumeric.

namespace KServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]

    public class Service1 : IDJ
    {
        #region SignInOutEtc

        /// <summary>
        /// Registers a DJ for the Mobioke service.
        /// If an error occurs, the response will describe the error.
        /// </summary>
        /// <param name="username">The username to use. Must not be in use by the service already</param>
        /// <param name="password">The password to use.</param>
        /// <param name="venue">Object that describes the DJ's venue.</param>
        /// <param name="email">The email address of the DJ</param>
        /// <returns>A Response object indicating the result of the operation.</returns>
        public Response DJSignUp(string username, string password, Venue venue, string email)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

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
                if (username.Length > 20 || password.Length > 20)
                {
                    r.error = true;
                    r.message = "Username or password is longer than 20 characters.";
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

                if (venue == null)
                {
                    r.error = true;
                    r.message = "Venue information must be passed in.";
                    return r;
                }

                if (venue.venueName == null || venue.venueName.Length == 0)
                {
                    r.error = true;
                    r.message = "Venue name must be set";
                    return r;
                }

                if (venue.venueName.Length > 20)
                {
                    r.error = true;
                    r.message = "Venue name is longer than 20 characters.";
                    return r;
                }

                if (venue.venueAddress.Length > 100)
                {
                    r.error = true;
                    r.message = "Venue address is longer than 100 characters";
                    return r;
                }

                if (venue.venueAddress == null || venue.venueAddress.Length == 0)
                {
                    r.error = true;
                    r.message = "Venue address must be set";
                    return r;
                }

                // Information seems valid, create a salt and hash the password.
                string salt = Common.CreateSalt(16);
                string hashSaltPassword = Common.CreatePasswordHash(password, salt);

                // Sign up the user.
                r = db.DJSignUp(username, hashSaltPassword, email, venue.venueName, venue.venueAddress, salt);
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
        /// 
        public LogInResponse DJSignIn(string username, string password)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return new LogInResponse(r);

                // Get the salt from the database and salt/hash the password.
                string salt;
                r = db.DJGetSalt(username, out salt);
                if (r.error)
                    return new LogInResponse(r);
                string saltHashPassword = Common.CreatePasswordHash(password, salt);

                // See if the username/password combination is valid.
                // If it is valid, the DJID will be stored in r.message.
                // If it is not valid, r.message will be empty.
                r = db.DJValidateUsernamePassword(username, saltHashPassword);
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
                r = DJValidateStatus(DJID, "!4", db);
                if (r.error)
                    return new LogInResponse(r);

                // Information seems valid, attempt to sign in.
                r = db.DJSetStatus(DJID, 1);
                if (r.error)
                    return new LogInResponse(r);

                // Attempt to change the DJID into a userKey
                long userKey;
                r = DJGenerateKey(DJID, out userKey, db);
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
        /// Attempt to sign out the DJ. 
        /// </summary>
        /// <param name="DJKey">The DJKey of the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJSignOut(long DJKey)
        {
            int DJID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJValidateStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // A sign out seems to be valid.
                r = db.DJSetStatus(DJID, 0);
                if (r.error)
                    return r;

                // Remove the key from the DB.
                r = db.DJSetKey(DJID, null);
                if (r.error)
                    return r;

                // Close out the song requests for this DJ.
                r = db.DJCloseSongRequests(DJID);
                if (r.error)
                    return r;

                r = db.DJRemoveAllTempUsers(DJID);
                if (r.error)
                    return r;
                return r;
            }
        }

        /// <summary>
        /// Discard the current QR code for the DJ and generate a new QR code.
        /// </summary>
        /// <param name="DJKey">The DJKey of the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJGenerateNewQRNumber(long DJKey)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                int DJID;
                r = DJKeyToID(DJKey, out DJID, db);
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
        /// Get the QR Code belonging the the DJ. Returned in Response.message.
        /// </summary>
        /// <param name="DJKey">The DJKey of the DJ.</param>
        /// <returns></returns>
        public Response DJGetQRNumber(long DJKey)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                int DJID;
                r = DJKeyToID(DJKey, out DJID, db);
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
        /// Starts a DJ session up. Mobile users can now make song requests, The DJ can now control the queue.
        /// </summary>
        /// <param name="DJKey">The DJ's assigned key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJCreateSession(long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJValidateStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Set the status of the DJ to accepting songs.
                r = db.DJSetStatus(DJID, 2);
                if (r.error)
                    return r;
                // Create a new field for song requests.
                r = db.DJOpenSongRequests(DJID);
                return r;
            }
        }

        /// <summary>
        /// Close a DJ's session. The DJ must have a session running for this to work.
        /// </summary>
        /// <param name="DJKey">The DJKey assigned to the DJ.</param>
        /// <returns>The outcome of the operation.b</returns>
        public Response DJStopSession(long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ has a session running.
                r = DJValidateStatus(DJID, "2", db);
                if (r.error)
                    return r;

                // Set the status of the DJ to logged in.
                r = db.DJSetStatus(DJID, 1);
                if (r.error)
                    return r;

                // Delete the song request field.
                r = db.DJCloseSongRequests(DJID);
                return r;
            }
        }
        #endregion

        #region SongControl

        /// <summary>
        /// Add songs to a DJ's library. If a song with a matching artist and title exists,
        /// the song is updated to the newly supplied duration and path on disk, otherwise
        /// a new song is added to the DJ's library.
        /// </summary>
        /// <param name="songs">Songs to add.</param>
        /// <param name="DJKey">Unique DJKey that describes the DJ.</param>
        /// <returns>The outcome of the operaiton.</returns>
        public Response DJAddSongs(List<Song> songs, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJValidateStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                // Adding songs seems to be valid, add the list of songs to the DJ.
                r = db.DJAddSongsUpdatingDuplicates(songs, DJID);
                return r;
            }
        }

        /// <summary>
        /// Remove given songs from list of songs belonging to the given DJ.
        /// DJ must be logged in, without a session running to successfully call this method.
        /// </summary>
        /// <param name="songs">The list of songs to Remove.</param>
        /// <param name="DJKey">The Unique DJKey describing the DJ</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJRemoveSongs(List<Song> songs, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ is logged in, without a session running.
                r = DJValidateStatus(DJID, "1", db);
                if (r.error)
                    return r;

                // Information seems to be valid, remove songs.
                r = db.DJRemoveSongs(songs, DJID);
                return r;
            }
        }

        /// <summary>
        /// Get all the songs that belong to the given DJ.
        /// </summary>
        /// <param name="songs">OUT parameter that holds the list of songs.</param>
        /// <param name="DJKey">Unique Key that describes the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJListSongs(out List<Song> songs, long DJKey)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            songs = new List<Song>();
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out
                r = DJValidateStatus(DJID, "!0", db);
                if (r.error)
                    return r;

                r = db.DJListSongs(DJID, out songs);
                sw.Stop();
                r.result = (int)sw.ElapsedMilliseconds;
                return r;
            }
        }
        #endregion

        #region QueueControl

        /// <summary>
        /// Pop the top song off the queue and updates the queue.
        /// </summary>
        /// <param name="sr">A Song request that represents the top song off the queue. Must match what the server believes is the top of the queue. Used to make sure the queues are in sync.</param>
        /// <param name="DJKey">The DJKey given to the DJ.</param>
        /// <returns>The outcome of the operaton.</returns>
        public Response DJPopQueue(SongRequest sr, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                r = DJValidateStatus(DJID, "2", db);
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
                r = Common.DBToMinimalList(raw, out  queue);
                if (r.error)
                    return r;

                if (queue[0].user.userID != sr.user.userID || queue[0].songs[0].ID != sr.songID)
                {
                    r.error = true;
                    r.message = "Song Request to Pop did not match first song Request, is your queue out of date?";
                    return r;
                }

                int nextUserID = queue[0].user.userID;
                if (nextUserID > 0)
                {
                    r = Common.PushMessageToMobile(nextUserID, "turn", db);
                }

                queue[0].songs.RemoveAt(0);
                if (queue[0].songs.Count == 0)
                {
                    if (queue[0].user.userID < 0)
                    {
                        r = db.DJRemoveTempUser(queue[0].user.userID, DJID);
                        queue.RemoveAt(0);
                        if (r.error)
                            return r;
                    }
                    else
                        queue.RemoveAt(0);

                }
                else
                {
                    queueSinger temp = queue[0];
                    queue.RemoveAt(0);
                    queue.Add(temp);
                }

                
                if (queue.Count > 0 && nextUserID > 0)
                {
                    nextUserID = queue[0].user.userID;
                    if(nextUserID > 0)
                        r = Common.PushMessageToMobile(nextUserID, "next", db);
                }

                raw = string.Empty;
                r = Common.MinimalListToDB(queue, out raw);
                if (r.error)
                    return r;
                r = db.SetSongRequests(DJID, raw);
                if (r.error)
                    return r;

                r = db.MobileAddSongHistory(sr.user.userID, DJID, sr.songID, DateTime.Now);
                if (r.error)
                    Common.LogError(r.message, Environment.StackTrace, r, 1);

                Common.PushMessageToUsersOfDJ(DJID, "queue", db);

                r = RunAchievements(DJID, db);
                if (r.error)
                    return r;


                return r;
            }
        }

        /// <summary>
        /// Get the DJ's queue from the server.
        /// </summary>
        /// <param name="queue">The out parameter that represents the queue.</param>
        /// <param name="DJKey">The DJKey assigned to the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJGetQueue(out List<queueSinger> queue, long DJKey)
        {
            queue = new List<queueSinger>();
            int DJID = -1, count = 0;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ is not logged out.
                r = DJValidateStatus(DJID, "2", db);
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

                r = Common.DBToFullList(raw, out queue, DJID, db);
                if (r.error)
                    return r;
                r.result = count;
                return r;
            }
        }

        /// <summary>
        /// Add a song request to the queue. Automatically figures out of the user is already in the queue or not.
        /// If the song request userID is > 0, matches based on registered user id.
        /// If the song request userID is 0, matches based in registered user name.
        /// If the song request uesrID is less than 0, matches based on temporary user name.
        /// Automaticlaly creates the temporary user if needed.
        /// </summary>
        /// <param name="sr">The song request to add.</param>
        /// <param name="queueIndex">The position to add the user in, if they don't already have song requests in the queue.</param>
        /// <param name="DJKey">The DJ's assigned key.</param>
        /// <returns>The outcome of the operation. If the operation is sucessful, the client ID number is returned in result and message.</returns>
        public Response DJAddQueue(SongRequest sr, int queueIndex, long DJKey)
        {
            int DJID = -1;
            int songID = -1;
            int clientID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
                if (r.error)
                    return r;

                // Check to see if song exists.
                r = db.SongExists(DJID, sr.songID);
                if (r.error)
                    return r;

                // Make sure the songExists method returned a song.
                if (!int.TryParse(r.message.Trim(), out songID))
                {
                    r.error = true;
                    r.message = "Could not find song";
                    return r;
                }

                if (sr.user.userID < -1)
                    sr.user.userID = -1;

                // when userID == -1, we are dealing with creating an anonmymous user.
                if (sr.user.userID == -1)
                {
                    // See if this username exists.
                    r = db.DJValidateTempUserName(sr.user.userName, DJID);
                    if (r.error)
                        return r;
                    // In this case, the username does not exist.
                    if (r.message.Trim().Length == 0)
                    {
                        // Add the tempUser.
                        r = db.DJAddTempUser(sr.user.userName, DJID);
                        if (r.error)
                            return r;
                        // Get the tempUser's ID from the DB.
                        r = db.DJValidateTempUserName(sr.user.userName, DJID);
                        if (r.error)
                            return r;
                        // Parse the ID.
                        if (!int.TryParse(r.message.Trim(), out clientID))
                        {
                            r.error = true;
                            r.message = "Unable to get the clientID of the new user.";
                            return r;
                        }
                    }
                    // In this case, the username already exists.
                    else
                    {
                        // Get the tempUser's ID from the DB.
                        r = db.DJValidateTempUserName(sr.user.userName, DJID);
                        if (r.error)
                            return r;
                        // Parse the ID.
                        if (!int.TryParse(r.message.Trim(), out clientID))
                        {
                            r.error = true;
                            r.message = "Unable to get the clientID of the temp user.";
                            return r;
                        }
                    }
                }
                // When userID == 0, we look the user up by username instead of userID.
                else if (sr.user.userID == 0)
                {
                    r = db.MobileValidateUsername(sr.user.userName);
                    if (r.error)
                        return r;
                    if (!int.TryParse(r.message.Trim(), out clientID))
                    {
                        r.error = true;
                        r.message = "Client name could not be validated.";
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
                    newRequests = clientID.ToString() + "~" + sr.songID.ToString();
                    r = Common.PushMessageToMobile(sr.user.userID, "queue", db);
                    r = db.SetSongRequests(DJID, newRequests);
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
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
                        Common.MinimalListToDB(queue, out newRequests);
                        r = db.SetSongRequests(DJID, newRequests);
                        if (r.error)
                            return r;

                        Common.PushMessageToUsersOfDJ(DJID, "queue", db);

                        r.message = clientID.ToString();
                        r.result = clientID;
                        return r;
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
                Common.MinimalListToDB(queue, out newRequests);
                r = db.SetSongRequests(DJID, newRequests);
                if (r.error)
                    return r;

                Common.PushMessageToUsersOfDJ(DJID, "queue", db);

                r.message = clientID.ToString();
                r.result = clientID;
                return r;
            }
        }

        /// <summary>
        /// Remove a song request from the queue. If the user has multiple song requests, only removes the specified one.
        /// </summary>
        /// <param name="sr">The song request to remove.</param>
        /// <param name="DJKey">The DJ's assigned key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJRemoveSongRequest(SongRequest sr, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
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
                r = Common.DBToMinimalList(requests, out queue);
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
                                {
                                    queue.RemoveAt(i);
                                    if (sr.user.userID < 0)
                                    {
                                        r = db.DJRemoveTempUser(sr.user.userID, DJID);
                                        if (r.error)
                                            return r;
                                    }
                                }
                                Common.MinimalListToDB(queue, out newRequests);
                                r = db.SetSongRequests(DJID, newRequests);
                                if (r.error)
                                    return r;
                                Common.PushMessageToUsersOfDJ(DJID, "queue", db);
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
                r.message = "Could not find client in the queue.";
                return r;
            }
        }

        /// <summary>
        /// Changes a user's song request.
        /// </summary>
        /// <param name="newSR">The new song request to user.</param>
        /// <param name="oldSR">The old song request to replace.</param>
        /// <param name="DJKey">The DJ's assigned key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, long DJKey)
        {
            int DJID = -1;
            int songID = -1;
            bool songChangeMade = false;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                if (newSR.user.userID != oldSR.user.userID)
                {
                    r.error = true;
                    r.message = "User must be the same between song requets";
                    return r;
                }

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
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
                r = Common.DBToMinimalList(requests, out queue);
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
                            Common.MinimalListToDB(queue, out newRequests);
                            r = db.SetSongRequests(DJID, newRequests);
                            if (r.error)
                                return r;
                            Common.PushMessageToUsersOfDJ(DJID, "queue", db);
                            return r;
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
        /// Move a song in a user's song requets new a new position in his/her song requests.
        /// Does not move singers, moves a singer's songs.
        /// </summary>
        /// <param name="sr">The song request to move</param>
        /// <param name="newIndex">The new index to insert the song into.</param>
        /// <param name="DJKey">The unique key that describes the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJMoveSongRequest(SongRequest sr, int newIndex, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
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
                r = Common.DBToMinimalList(requests, out queue);
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
                            // If we find the song.
                            if (queue[i].songs[j].ID == sr.songID)
                            {
                                // Make sure the indexes are in range.
                                if (newIndex < 0)
                                    newIndex = 0;
                                if (newIndex > queue[i].songs.Count - 1)
                                    newIndex = queue[i].songs.Count - 1;

                                // Get the song out, and insert it into the new index.
                                Song temp = queue[i].songs[j];
                                queue[i].songs.RemoveAt(j);
                                queue[i].songs.Insert(newIndex, temp);
                                Common.MinimalListToDB(queue, out newRequests);
                                r = db.SetSongRequests(DJID, newRequests);
                                if (r.error)
                                    return r;
                                Common.PushMessageToUsersOfDJ(DJID, "queue", db);
                                return r;
                            }

                        }
                        // If we can't find the current song.
                        r.error = true;
                        r.message = "Could not find the song to move.";
                        return r;
                    }
                }

                r.error = true;
                r.message = "Could not find client in the queue.";
                return r;
            }
        }

        /// <summary>
        /// Remove a user from the queue. All of the user's song requests are removed.
        /// </summary>
        /// <param name="userID">The userID of the user</param>
        /// <param name="DJKey">The DJ's assigned key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJRemoveUser(int userID, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
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
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return r;

                // Search to see if the user is already in this list of singers.
                for (int i = 0; i < queue.Count; i++)
                {
                    // We found the userID already in here.
                    if (queue[i].user.userID == userID)
                    {
                        queue.RemoveAt(i);

                        if (userID < 0)
                        {
                            r = db.DJRemoveTempUser(userID, DJID);
                            if (r.error)
                                return r;
                        }

                        Common.MinimalListToDB(queue, out newRequests);
                        r = db.SetSongRequests(DJID, newRequests);
                        if (r.error)
                            return r;
                        Common.PushMessageToUsersOfDJ(DJID, "queue", db);
                        return r;
                    }
                }

                r.error = true;
                r.message = "Could not find client in the queue.";
                return r;
            }
        }

        /// <summary>
        /// Move the user to a new position in the queue (Zero based).
        /// </summary>
        /// <param name="userID">The user's ID.</param>
        /// <param name="index">The new index of the user.</param>
        /// <param name="DJKey">The DJ's assigned key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJMoveUser(int userID, int index, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
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
                r = Common.DBToMinimalList(requests, out queue);
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
                        Common.MinimalListToDB(queue, out newRequests);
                        r = db.SetSongRequests(DJID, newRequests);
                        if (r.error)
                            return r;
                        Common.PushMessageToUsersOfDJ(DJID, "queue", db);
                        return r;
                    }
                }

                r.error = true;
                r.message = "Could not find client in the queue.";
                return r;
            }
        }

        /// <summary>
        /// Create a test Queue for the Rick account. Does not work with any other account.
        /// </summary>
        /// <param name="DJKey">The DJ Key assigned to the Rick account.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJTestQueueFill(long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                if (DJID != 5 && DJID != 4)
                {
                    r.error = true;
                    r.message = "You are not the rick or jakub account.";
                    return r;
                }

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
                if (r.error)
                    return r;

                // Get the current song Requests
                r = db.GetSongRequests(DJID);
                if (r.error)
                    return r;

                string newRequests = string.Empty;
                if (DJID == 5)
                    newRequests = "1~32066~31846`3~23565~23504`2003~37516~36965`2~41440~41193";
                else if (DJID == 4)
                    newRequests = "1~55474~56758`2~59321~42050`3~43357~47751";

                return db.SetSongRequests(DJID, newRequests);
            }
        }

        #endregion

        #region BanUsers
        public Response DJBanUser(User userToBan, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;
                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;
                r = db.MobileValidateID(userToBan.userID);
                if (r.error)
                    return r;
                if (r.message.Length == 0)
                {
                    r.message = "Could not find that userID";
                    r.error = true;
                    return r;
                }
                r = db.DJBanUser(DJID, userToBan.userID);
                if (r.error)
                    return r;

                if (r.result != 1)
                {
                    r.message = "DB failed to ban user?";
                    r.error = true;
                    return r;
                }

                r = db.DJRemoveUserFromVenueIfAtVenue(DJID, userToBan.userID);
                if (r.error)
                    return r;

                DJRemoveUser(userToBan.userID, DJKey);

                return r;
            }
        }

        public Response DJUnbanUser(User userToUnban, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;
                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;
                r = db.MobileValidateID(userToUnban.userID);
                if (r.error)
                    return r;
                if (r.message.Length == 0)
                {
                    r.message = "Could not find that userID";
                    r.error = true;
                    return r;
                }

                bool isBanned;
                r = db.MobileIsBanned(DJID, userToUnban.userID, out isBanned);
                if (r.error)
                    return r;
                if (!isBanned)
                {
                    r.error = true;
                    r.message = "User is not banned!";
                    return r;
                }

                r = db.DJUnbanUser(DJID, userToUnban.userID);
                if (r.error)
                    return r;

                if (r.result != 1)
                {
                    r.message = "DB failed to unban user?";
                    r.error = true;
                    return r;
                }
                return r;
            }
        }

        public Response DJGetBannedUsers(long DJKey, out List<User> users)
        {
            int DJID = -1;
            users = new List<User>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;
                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                r = db.DJGetBannedUsers(DJID, out users);
                if (r.error)
                    return r;

                for (int i = 0; i < users.Count; i++)
                {
                    r = db.MobileIDtoUsername(users[i].userID);
                    if (r.error)
                        return r;
                    users[i].userName = r.message.Trim();
                }

                return r;
            }
        }

        #endregion

        #region GetInformation
        /// <summary>
        /// Get the approximate wait time for a new user if they were to join the queue. If no error occures, result stored in result and message.
        /// </summary>
        /// <param name="DJKey">The DJ key assigned to the DJ.</param>
        /// <returns>The outcome of the opeartion.</returns>
        public Response DJNewUserWaitTime(long DJKey)
        {
            int DJID = -1;
            List<queueSinger> queue = new List<queueSinger>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                // Make sure the DJ isn't logged out.
                r = DJValidateStatus(DJID, "2", db);
                if (r.error)
                    return r;

                r = db.GetSongRequests(DJID);
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
                r = Common.DBToFullList(raw, out queue, DJID, db);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                int time = 0;
                foreach (queueSinger qs in queue)
                    time += qs.songs[0].duration + Common.TIME_BETWEEN_REQUESTS;

                r.error = false;
                r.message = time.ToString().Trim();
                r.result = time;
                return r;
            }
        }
        /// <summary>
        /// Gets the most or the least popular songs at a venue.
        /// </summary>
        /// <param name="DJKey">The unique DJ key.</param>
        /// <param name="limitToVenue">If true, results are only from this DJ's venue, otherwise, results are from all venues.</param>
        /// <param name="start">Results start at the given index.</param>
        /// <param name="count">Sets the number of results.</param>
        /// <param name="songs">Out list of songs.</param>
        /// <param name="counts">Out list of counts that contains how many times that song shows up. Indexes match song indexes. ie counts[2] is the number of times songs[2] was sung.</param>
        /// <returns></returns>
        public Response DJGetMostPopularSongs(long DJKey, bool limitToVenue, int start, int count, out List<Song> songs, out List<int> counts)
        {
            int DJID = -1;
            songs = new List<Song>();
            counts = new List<int>();
            List<Song> songIDs;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                int chosenVenue = -1;
                if(limitToVenue)
                    chosenVenue = DJID;
                r = db.GetMostPopularSongs(chosenVenue, start, count, out songIDs, out counts);
                if (r.error)
                    return r;

                foreach(Song s in songIDs)
                {
                    Song fullSong;
                    Common.GetSongInformation(s.ID, DJID, -1, out fullSong, db, true);
                    songs.Add(fullSong);
                }
                return r;
            }
        }

        #endregion

        #region Achievements

        public Response DJAddAchievement(Achievement achievement, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                foreach(AchievementSelect a in achievement.selectList)
                {
                    if (a.startDate.Year < 1754)
                    {
                        a.startDate = new DateTime(1900, 1, 1);
                    }
                }


                r = db.DJAddAchievement(DJID, achievement);
                if (r.error)
                    return r;
                return r;
            }
        }

        public Response DJModifyAchievement(Achievement achievement, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                foreach (AchievementSelect a in achievement.selectList)
                {
                    if (a.startDate.Year < 1754)
                    {
                        a.startDate = new DateTime(1900, 1, 1);
                    }
                }

                r = db.DJModifyAchievement(DJID, achievement);
                if (r.error)
                    return r;

                r = RunAchievements(DJID, db);
                if (r.error)
                    return r;

                return r;
            }
        }

        public Response DJDeleteAchievement(int achievementID, long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                r = db.DJDeleteAchievement(DJID, achievementID);
                if (r.error)
                    return r;

                if (r.result < 1 && achievementID != -1)
                {
                    r.error = true;
                    r.message = "The achievement didn't exist? nothing deleted";
                    return r;
                }
                return r;
            }
        }

        public Response DJViewAchievements(long DJKey, out List<Achievement> achievements)
        {
            int DJID = -1;
            achievements = new List<Achievement>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                r = db.DJViewAchievements(DJID, out achievements);
                if (r.error)
                    return r;
                return r;
            }
        }

        public Response DJEvaluateAchievements(long DJKey)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                r = RunAchievements(DJID, db);
                if (r.error)
                    return r;
                return r;
            }

        }

        public Response ViewAchievementSql(long DJKey, int achievementID)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                List<Achievement> achievements;
                r = db.DJViewAchievements(DJID, out achievements);
                if (r.error)
                    return r;

                foreach (Achievement a in achievements)
                {
                    if (a.ID == achievementID)
                    {
                        string sql;
                        List<SqlCommand> sqlCommands;
                        r = AchievementParser.CreateAchievementSQL(a, DJID, out sql, out sqlCommands);
                        if (r.error)
                            return r;
                        r.message = sql;
                        r.error = false;
                        return r;
                    }
                }
                r.error = true;
                r.message = "Achievement not found";
                return r;
            }
        }

        #endregion

        public Response InsertFauxSongHistory(long DJKey, List<string> bands, int numberPerBand, int mobileID)
        {
            int DJID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID, db);
                if (r.error)
                    return r;

                string extraMessage = string.Empty;
                foreach (string band in bands)
                {
                    List<int> songIDs = new List<int>();
                    r = db.GetRandomSongsForArtist(band, DJID, numberPerBand, out songIDs);
                    if (r.error)
                        extraMessage += "Error looking for : " + band + "\n";

                    for (int i = 0; i < songIDs.Count; i++)
                    {
                        r = db.MobileAddSongHistory(mobileID, DJID, songIDs[i], DateTime.Now);
                        if (r.error)
                            return r;
                    }
                    r.message = extraMessage;
                }
                return r;
            }
        }


        #region PrivateMethods

        private Response RunAchievements(int DJID, DatabaseConnectivity db)
        {
            Response r;
            List<Achievement> achievements;
            r = db.DJViewAchievements(DJID, out achievements);
            if (r.error)
                return r;


            foreach (Achievement a in achievements)
            {
                r = EvaluateAchievement(DJID, a, db);
                if (r.error)
                    return r;
            }

            return r;
        }
        private Response CombineLists(List<List<int>> users, out List<int> results, bool andLists)
        {
            Response r = new Response();
            results = new List<int>();
            try
            {
                if (users.Count < 1)
                {
                    r.error = true;
                    r.message = "Exception in AndLists, list size is < 1.";
                    return r;
                }

                //// DEBUG
                //foreach (List<int> u in users)
                //{
                //    r.message += "Statement: ";
                //    foreach (int i in u)
                //    {
                //        r.message += i + " ";
                //    }
                //    r.message += "\n";
                //}
                //// END DEBUG


                results = users[0];
                for (int i = 1; i < users.Count; i++)
                {
                    if (andLists)
                        results = results.Intersect(users[i]).ToList();
                    else
                        results = results.Union(users[i]).ToList();
                }

                //// DEBUG
                //r.message += "Result: ";
                //foreach (int i in results)
                //{
                //    r.message += i.ToString() + " ";
                //}
                //r.message += "\n";
                //r.error = true;
                //// END DEBUG

                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Combine Lists error: " + e.ToString();
                return r;
            }
        }
        private Response EvaluateAchievement(int DJID, Achievement a, DatabaseConnectivity db)
        {
            string sqlText;
            List<SqlCommand> cmds;
            List<List<int>> results;
            Response r = AchievementParser.CreateAchievementSQL(a, DJID, out sqlText, out cmds);
            if (r.error)
                return r;

            r = db.EvaluateAchievementStatements(DJID, cmds, out results);
            if (r.error)
                return r;

            if (results.Count == 0)
            {
                r.error = true;
                r.message = "EvaulateAchievement: List is of size zero, something went wrong";
                return r;

            }

            List<int> qualifiedUsers;
            r = CombineLists(results, out qualifiedUsers, a.statementsAnd);
            if (r.error)
                return r;

            if (!a.isPermanant)
            {
                r = db.DeleteAchievementsByID(a.ID);
                if (r.error)
                    return r;
            }

            foreach (int userID in qualifiedUsers)
            {
                r = db.AwardAchievement(userID, a.ID);
                if (r.error)
                    return r;
            }


            return r;
        }

        /// <summary>
        /// Check the status of the DJ. Returns whether the desired status was found.
        /// </summary>
        /// <param name="DJID">The DJID for the DJ.</param>
        /// <param name="desiredStatus">The desired status, 2 or !1 etc.</param>
        /// <param name="db">The database object to use.</param>
        /// <returns>The outcome of the operation.</returns>
        private Response DJValidateStatus(int DJID, string desiredStatus, DatabaseConnectivity db)
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
        private Response DJKeyToID(long DJKey, out int DJID, DatabaseConnectivity db)
        {
            DJID = -1;
            Response r  = db.DJGetIDFromKey(DJKey);
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

        /// <summary>
        /// Convert a DJID to a DJKey.
        /// </summary>
        /// <param name="DJID">The DJID</param>
        /// <param name="DJKey">OUT parameter for the DJKey</param>
        /// <returns></returns>
        private Response DJGenerateKey(int DJID, out long DJKey, DatabaseConnectivity db)
        {
            DJKey = -1;
            Response r = new Response();
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            Random rand = new Random();
            byte[] randomBytes = new byte[64];
            byte[] result;
            long tempKey;
            for (; ; )
            {
                rand.NextBytes(randomBytes);
                result = sha.ComputeHash(randomBytes);
                tempKey = BitConverter.ToInt64(result, 0);
                r = db.DJGetIDFromKey(tempKey);
                if (r.error)
                    return r;
                if (r.message.Trim().Length != 0)
                    continue;
                r = db.DJSetKey(DJID, tempKey);
                if (r.error)
                    return r;
                DJKey = tempKey;
                return r;
            }
        }

        #endregion
    }
}
