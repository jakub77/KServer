using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Security.Permissions;

namespace KServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]

    // Username and password must be the same to be valid (TEMP... of course)
    public class Service1 : IDJ
    {
        /// <summary>
        /// Sign a DJ up for the service.
        /// If an error occurs, the Response will have error set to true, and the error message will be in message.
        /// </summary>
        /// <param name="username">The requested username.</param>
        /// <param name="password">The requested password.</param>
        /// <returns>The success of the operation.</returns>
        public Response DJSignUp(string username, string password)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to open connection to DB.
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
            int DJID = -1, DJStatus = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to conenct to DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return new LogInResponse(r);

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

                // Get the current status of the DJ
                r = db.DJGetStatus(DJID);
                if (r.error)
                    return new LogInResponse(r);

                // Parse the status from the DB.
                if (!int.TryParse(r.message.Trim(), out DJStatus))
                {
                    r.error = true;
                    r.message = "Exception in DJSignIn: Unable to parse status from DB!";
                    return new LogInResponse(r);
                }

                // If either the status or the DJID are not yet set, return error.
                if (DJStatus == -1 || DJID == -1)
                {
                    r.error = true;
                    r.message = "Exception in DJSignIn: DJStatus or DJID are not yet set!";
                    return new LogInResponse(r);
                }

                // Code to not allow a signed in user to sign in again, disabled since signouts
                // are not yet guarenteed upon a client disconnecting.
                //if(DJStatus != 0)
                //{
                //    r.error= true;
                //    r.message= "You are already signed in.";
                //    return new LogInResponse(r);
                //}

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
            int DJID, DJStatus;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to open the connection to the DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Get the current status of the DJ
                r = db.DJGetStatus(DJID);
                if (r.error)
                    return r;

                // Try to parse the status of the DJ.
                if (!int.TryParse(r.message.Trim(), out DJStatus))
                {
                    r.error = true;
                    r.message = "Exception in DJSignIn: Unable to parse status from DB!";
                    return r;
                }

                // If the DJ is not signed in, inform them.
                if (DJStatus <= 0)
                {
                    r.error = true;
                    r.message = "You are not signed in.";
                    return r;
                }

                // A sign out seems to be valid.
                r = db.DJSignOut(DJID);
                return r;
            }
        }

        public Session DJCreateSession(long DJKey) { return null; }

        /// <summary>
        /// Add the given songs to the list of songs for the given DJ.
        /// Response.result contains the number of songs successfully added.
        /// </summary>
        /// <param name="songs">Songs to add.</param>
        /// <param name="DJKey">Unique DJKey that describes the DJ.</param>
        /// <returns>The outcome of the operaiton.</returns>
        public Response DJAddSongs(List<Song> songs, long DJKey)
        {
            int DJID = -1, DJStatus = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to conenct to DB
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Get the current status of the DJ.
                r = db.DJGetStatus(DJID);
                if (r.error)
                    return r;

                // Try to parse the status of the DJ.
                if (!int.TryParse(r.message.Trim(), out DJStatus))
                {
                    r.error = true;
                    r.message = "Exception in DJSignIn: Unable to parse status from DB!";
                    return r;
                }

                // If the DJ is not logged in, don't add songs.
                if (DJStatus == 0)
                {
                    r.error = true;
                    r.message = "You must be logged in to add songs.";
                    return r;
                }

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
            int DJID = -1, DJStatus = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to connect to the DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Attempt to convert DJKey to DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Get the status of the DJ.
                r = db.DJGetStatus(DJID);
                if (r.error)
                    return r;

                // Attempt to parse that status of the DJ.
                if (!int.TryParse(r.message.Trim(), out DJStatus))
                {
                    r.error = true;
                    r.message = "Exception in DJRemoveSongs: Unable to parse status from DB!";
                    return r;
                }

                // If the DJ is not logged in, inform DJ.
                if (DJStatus == 0)
                {
                    r.error = true;
                    r.message = "You must be logged in to remove songs.";
                    return r;
                }

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
            int DJID = -1, DJStatus = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to conenct to DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Conver the DJKey to a DJID
                r = DJKeyToID(DJKey, out DJID);
                if (r.error)
                    return r;

                // Get the status of the DJ.
                r = db.DJGetStatus(DJID);
                if (r.error)
                    return r;

                // Try to parse the status.
                if (!int.TryParse(r.message.Trim(), out DJStatus))
                {
                    r.error = true;
                    r.message = "Exception in DJListSongs: Unable to parse status from DB!";
                    return r;
                }

                // If the DJ is not logged in, don't list songs.
                if (DJStatus == 0)
                {
                    r.error = true;
                    r.message = "You are not logged in!";
                    return r;
                }

                // Information seems valid, list the songs.
                r = db.DJListSongs(out songs, DJID);
                return r;
            }
        }

        // Queue management
        public Response DJAddQueue(SongRequest sr, int queueIndex, int sessionID, long DJKey) { return null; }
        public Response DJRemoveSongRequest(SongRequest sr, int sessionID, long DJKey) { return null; }
        public Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey) { return null; }
        public Response DJRemoveUser(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey) { return null; }
        public Response DJMoveUser(int userID, int sessionID, long DJKey) { return null; }

        /// <summary>
        /// Convert a DJKey to a DJID.
        /// </summary>
        /// <param name="DJKey">The DJKey.</param>
        /// <param name="DJID">OUT parameter for the DJID.</param>
        /// <returns>The outcome of the operation.</returns>
        private Response DJKeyToID(long DJKey, out int DJID)
        {
            // Conver the DJKey to a DJID. (Temporary implementation).
            DJID = (int)DJKey;

            // Validate that the DJID is valid.
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to connect to DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Try to validate DJID using DB.
                r = db.DJValidateDJID(DJID);
                if (r.error)
                    return r;

                // See if that DJID exists.
                if (r.message.Trim() == String.Empty)
                {
                    string s = r.message.Trim();
                    r.error = true;
                    r.message = "Exception in DJKeyToID: The DJID could not be validated!";
                    return r;
                }

                // DJID exists, return success.
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
            // Temporary implementation, always success.
            DJKey = (long)DJID;
            return new Response();
        }

    }
}
