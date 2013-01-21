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
        public Response DJSignUp(string username, string password)
        {
            DatabaseConnectivity db = new DatabaseConnectivity();
            Response r = db.OpenConnection();
            if (r.error)
                return r;

            if (username.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                Response listResponse = db.DJListMembers();
                if (listResponse.error)
                    return listResponse;
                r = db.Close();
                if (r.error)
                    return r;
                return listResponse;         
            }

            r = db.DJValidateUsername(username);
            if (r.error)
                return r;
            if (r.message.Trim() != string.Empty)
            {
                r.error = true;
                r.message = "Error: That username already exists.";
                return r;
            }
  
            r = db.DJSignUp(username, password);

            if (r.error)
                return r;

            db.Close();
            return r;        
        }

        public LogInResponse DJSignIn(string username, string password)
        {
            int DJID = -1;
            int DJStatus = -1;
            DatabaseConnectivity db = new DatabaseConnectivity();
            Response r = db.OpenConnection();


            if (r.error)
                return new LogInResponse(r);

            r = db.DJValidateUsernamePassword(username, password);
            if (r.error)
                return new LogInResponse(r);

            if (r.message.Trim() == string.Empty)
            {
                r.error = true;
                r.message = "Error: Username/Password incorrect.";
                return new LogInResponse(r);
            }

            if (!int.TryParse(r.message.Trim(), out DJID))
            {
                r.error = true;
                r.message = "Exception in DJSignIn: Unable to parse DJID from DB!";
                return new LogInResponse(r);
            }

            r = db.DJGetStatus(DJID);
            if (r.error)
                return new LogInResponse(r);

            if(!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in DJSignIn: Unable to parse status from DB!";
                return new LogInResponse(r);
            }

            if (DJStatus == -1 || DJID == -1)
            {
                r.error = true;
                r.message = "DJStatus or DJID is still not set correctly\n";
                r.message += "DJStatus: " + DJStatus + " DJID: " + DJID + "\n";
                return new LogInResponse(r);
            }

            //if(DJStatus != 0)
            //{
            //    r.error= true;
            //    r.message= "You are already signed in.";
            //    return new LogInResponse(r);
            //}

            r = db.DJSignIn(DJID);
            if (r.error)
                return new LogInResponse(r);
            
            r = db.Close();
            if(r.error)
                return new LogInResponse(r);

            LogInResponse lr = new LogInResponse();
            long userKey;
            DJIDToKey(DJID, out userKey);
            lr.userKey = userKey;
            User u = new User();
            u.userName = username;
            u.userID = DJID;
            return lr;
        }

        public Response DJSignOut(long DJKey) 
        {
            int DJID;
            int DJStatus;
            DatabaseConnectivity db = new DatabaseConnectivity();
            Response r = db.OpenConnection();

            if (r.error)
                return r;

            DJKeyToID(DJKey, out DJID);
            r = db.DJGetStatus(DJID);

            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in DJSignIn: Unable to parse status from DB!";
                return r;
            }

            if (DJStatus <= 0)
            {
                r.error = true;
                r.message = "You are not signed in.";
                r.message += "\nID: " + DJID + " DJStatus: " + DJStatus + "\n";
                return r;
            }

            r = db.DJSignOut(DJID);
            if (r.error)
                return r;

            db.Close();
            return r;     
        }
        public Session DJCreateSession(long DJKey) { return null; }

        // Song management
        public Response DJAddSongs(List<Song> songs, long DJKey)
        {
            DatabaseConnectivity db = new DatabaseConnectivity();
            Response r = db.OpenConnection();

            if (r.error)
                return r;

            int DJID;
            DJKeyToID(DJKey, out DJID);
            int DJStatus = -1;
            r = db.DJGetStatus(DJID);

            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in DJSignIn: Unable to parse status from DB!";
                return r;
            }

            if (DJStatus == 0)
            {
                r.error = true;
                r.message = "Error: DJ is not logged in!";
                return r;
            }

            r = db.DJAddSongsIgnoringDuplicates(songs, DJID);

            if (r.error)
                return r;

            db.Close();
            return r;
        }

        public Response DJRemoveSongs(List<Song> songs, long DJKey)
        {
            DatabaseConnectivity db = new DatabaseConnectivity();
            Response r = db.OpenConnection();

            if (r.error)
                return r;

            int DJID;
            r = DJKeyToID(DJKey, out DJID);
            if (r.error)
                return r;

            int DJStatus = -1;
            r = db.DJGetStatus(DJID);

            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in DJRemoveSongs: Unable to parse status from DB!";
                return r;
            }

            if (DJStatus == 0)
            {
                r.error = true;
                r.message = "Error: DJ is not logged in!";
                return r;
            }

            r = db.DJRemoveSongs(songs, DJID);

            if (r.error)
                return r;

            db.Close();
            return r;
        }

        // Get all the songs that belong the the specified DJ and put into songs.
        // Return the successfullness of the operation.
        public Response DJListSongs(out List<Song> songs, long DJKey)
        {
            songs = null;
            DatabaseConnectivity db = new DatabaseConnectivity();
            Response r = db.OpenConnection();

            if (r.error)
                return r;

            int DJID;
            r = DJKeyToID(DJKey, out DJID);
            if (r.error)
                return r;

            int DJStatus = -1;
            r = db.DJGetStatus(DJID);

            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in DJListSongs: Unable to parse status from DB!";
                return r;
            }

            if (DJStatus == 0)
            {
                r.error = true;
                r.message = "Error: DJ is not logged in!";
                return r;
            }

            r = db.DJListSongs(out songs, DJID);

            if (r.error)
                return r;

            db.Close();
            return r;
        }

        // Queue management
        public Response DJAddQueue(SongRequest sr, int queueIndex, int sessionID, long DJKey) { return null; }
        public Response DJRemoveSongRequest(SongRequest sr, int sessionID, long DJKey) { return null; }
        public Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey) { return null; }
        public Response DJRemoveUser(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey) { return null; }
        public Response DJMoveUser(int userID, int sessionID, long DJKey) { return null; }

        private Response DJKeyToID(long DJKey, out int DJID)
        {
            DJID = (int)DJKey;
            DatabaseConnectivity db = new DatabaseConnectivity();
            Response r = db.OpenConnection();
            if (r.error)
                return r;

            r = db.DJValidateDJID(DJID);
            if (r.error)
                return r;
            if (r.message.Trim() == String.Empty)
            {
                string s = r.message.Trim();
                r.error = true;
                r.message = "Error DJKeyToID: Could not validate generated DJID\n";
                r.message += "DJKey: " + DJKey + " DJID: " + DJID + " Message: \"" + s + "\"";
                return r;
            }

            return r;
        }
        private Response DJIDToKey(int DJID, out long DJKey)
        {
            Response r = new Response();
            DJKey = (long)DJID;
            return r;
        }

    }
}
