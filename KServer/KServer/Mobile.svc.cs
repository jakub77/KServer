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
                // Attempt to open connection to DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

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

                // Information seems valid, sign up DJ and return successfulness.
                r = db.MobileSignUp(username, password);
                return r;
            }
        }
        public LogInResponse MobileSignIn(string username, string password)
        {
            int MobileID = -1, MobileStatus = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to conenct to DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return new LogInResponse(r);

                // See if the username/password combination is valid.
                // If it is valid, the DJID will be stored in r.message.
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

                // Get the DJID stored in r.message.
                if (!int.TryParse(r.message.Trim(), out MobileID))
                {
                    r.error = true;
                    r.message = "Exception in MobileSignIn: Unable to parse MobileID from DB!";
                    return new LogInResponse(r);
                }

                // Get the current status of the DJ
                r = db.MobileGetStatus(MobileID);
                if (r.error)
                    return new LogInResponse(r);

                // Parse the status from the DB.
                if (!int.TryParse(r.message.Trim(), out MobileStatus))
                {
                    r.error = true;
                    r.message = "Exception in MobileSignIn: Unable to parse status from DB!";
                    return new LogInResponse(r);
                }

                // If either the status or the DJID are not yet set, return error.
                if (MobileStatus == -1 || MobileID == -1)
                {
                    r.error = true;
                    r.message = "Exception in MobileSignIn: MobileStatus or MobileID are not yet set!";
                    return new LogInResponse(r);
                }

                // Code to not allow a signed in user to sign in again, disabled since signouts
                // are not yet guarenteed upon a client disconnecting.
                //if(MobileStatus != 0)
                //{
                //    r.error= true;
                //    r.message= "You are already signed in.";
                //    return new LogInResponse(r);
                //}

                // Information seems valid, attempt to sign in.
                r = db.MobileSignIn(MobileID);
                if (r.error)
                    return new LogInResponse(r);

                // Attempt to change the DJID into a userKey
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
            int MobileID, MobileStatus;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to open the connection to the DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Convert the DJKey to a DJID
                r = MobileKeyToID(userKey, out MobileID);
                if (r.error)
                    return r;

                // Get the current status of the DJ
                r = db.MobileGetStatus(MobileID);
                if (r.error)
                    return r;

                // Try to parse the status of the DJ.
                if (!int.TryParse(r.message.Trim(), out MobileStatus))
                {
                    r.error = true;
                    r.message = "Exception in MobileSignIn: Unable to parse status from DB!";
                    return r;
                }

                // If the DJ is not signed in, inform them.
                if (MobileStatus <= 0)
                {
                    r.error = true;
                    r.message = "You are not signed in.";
                    return r;
                }

                // A sign out seems to be valid.
                r = db.MobileSignOut(MobileID);
                return r;
            }
        }

        public List<Song> MobileSongSearch(string title, string artist, int venueID)
        {
            List<Song> s = new List<Song>();
            return s;
        }
        public List<Song> MobileSongBrowse(char firstLetter, bool isArtist, int start, int count, int venueID)
        {
            List<Song> s = new List<Song>();
            Song song = new Song();
            song.artist = "Glitch Mob";
            song.title = "Fortune Days";
            song.pathOnDisk = "C:\\Songs\\Fortune Days";
            s.Add(song);
            Song song2 = new Song();
            song.artist = "Rammstein";
            song.title = "Du Hast";
            song.pathOnDisk = "C:\\Songs\\Du Hast";
            s.Add(song2);
            return s;
        }
        public Response MobileSongRequest(int songID, long userKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "MobileSongRequest is not yet implemented.";
            r.result = songID;
            return r;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MobileKey"></param>
        /// <param name="MobileID"></param>
        /// <returns></returns>
        private Response MobileKeyToID(long MobileKey, out int MobileID)
        {
            // Conver the DJKey to a DJID. (Temporary implementation).
            MobileID = (int)MobileKey;

            // Validate that the DJID is valid.
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Attempt to connect to DB.
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Try to validate DJID using DB.
                r = db.MobileValidateID(MobileID);
                if (r.error)
                    return r;

                // See if that DJID exists.
                if (r.message.Trim() == String.Empty)
                {
                    string s = r.message.Trim();
                    r.error = true;
                    r.message = "Exception in MobileKeytoID: ID could not be validated!";
                    return r;
                }

                // DJID exists, return success.
                return r;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MobileID"></param>
        /// <param name="MobileKey"></param>
        /// <returns></returns>
        private Response MobileIDToKey(int MobileID, out long MobileKey)
        {
            // Temporary implementation, always success.
            MobileKey = (long)MobileID;
            return new Response();
        }
    }
}
