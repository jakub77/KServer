// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// This contains all the methods a mobile user can call on the server.
// This implemented the service contract defined in IMobile.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, AddressFilterMode = AddressFilterMode.Any)]
    public class Mobile : IMobile
    {
        #region SignInOutEtc

        /// <summary>
        /// Sign a client up for the service. Will fail if username is already in user, or email is not formatted validly.
        /// </summary>
        /// <param name="username">Client username.</param>
        /// <param name="password">Client password.</param>
        /// <param name="email">Client email.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSignUp(string username, string password, string email)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Escape to allow the MobileTestClient to list all Mobile information
                // WILL BE REMOVED FOR RELEASE!
                if (username.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    ExpResponse listResponse = db.MobileListMembers();
                    if (listResponse.error)
                        return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                    return listResponse;
                }

                // Validate that username and password are not blank.
                if (username.Length == 0 || password.Length == 0)
                {
                    r.setErMsg(true, Messages.ERR_CRED_BLANK);
                    return r;
                }

                // Validate that username and password are not too long.
                if (username.Length > 20 || password.Length > 20)
                {
                    r.setErMsg(true, Messages.ERR_CRED_LONG);
                    return r;
                }

                // Validate the email address.
                try
                {
                    var address = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    r.setErMsg(true, Messages.ERR_BAD_EMAIL);
                    return r;
                }

                // Try to see if the username already exists. If it does, inform the client.
                r = db.MobileValidateUsername(username);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (r.message.Trim() != string.Empty)
                {
                    r.setErMsg(true, Messages.ERR_CRED_TAKEN);
                    return r;
                }

                // Create salt and hashed/salted password;
                string salt = Common.CreateSalt(16);
                string hashSaltPassword = Common.CreatePasswordHash(password, salt);

                // Information seems valid, sign up client and return successfulness.
                r = db.MobileSignUp(username, hashSaltPassword, email, salt);
                if(r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                return r;
            }
        }
        /// <summary>
        /// Sign in a mobile user into the system. The client's userKey to use is stored in the loginresponse.
        /// </summary>
        /// <param name="username">client username.</param>
        /// <param name="password">client password.</param>
        /// <param name="deviceID">The device ID the of the hardware the client is using.</param>
        /// <returns>Returns the outcome of the operation.</returns>
        public LogInResponse MobileSignIn(string username, string password, string deviceID)
        {
            int MobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return new LogInResponse(Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, 0));

                // Get the salt from the database and salt/hash the password.
                string salt;
                r = db.MobileGetSalt(username, out salt);
                if (r.error)
                    return new LogInResponse(Common.LogErrorRetNewMsg(r, Messages.ERR_CRED_WRONG, 0));
                string saltHashPassword = Common.CreatePasswordHash(password, salt);

                // See if the username/password combination is valid.
                // If it is valid, the userkey will be stored in r.message.
                // If it is not valid, r.message will be empty.
                r = db.MobileValidateUsernamePassword(username, saltHashPassword);
                if (r.error)
                    return new LogInResponse(Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, 0));

                // If the username/password couldn't be found, inform user.
                if (r.message.Trim() == string.Empty)
                {
                    r.setErMsg(true, Messages.ERR_CRED_WRONG);
                    return new LogInResponse(r);
                }

                // Get the client ID stored in r.message.
                if (!int.TryParse(r.message.Trim(), out MobileID))
                {
                    r.setErMsgStk(true, "Unable to parse MobileID from DB", "MobileSignIn");
                    return new LogInResponse(Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, 0));
                }

                // Make sure the client is not logged in. RIGHT NOW: JUST DON'T CHECK ANYTHING USEFUL TO ALLOW FOR LOGINS TO OCCUR WHEN LOGGED IN!
                bool validStatus;
                r = MobileCheckStatus(MobileID, "!4", db, out validStatus);
                if (r.error)
                    return new LogInResponse(Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, 0));
                if(!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_ALREADY_IN);
                    return new LogInResponse(r);
                }

                r = db.MobileSignIn(MobileID, deviceID);
                if (r.error)
                    return new LogInResponse(Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, 0));

                // Attempt to change the MobileID into a userKey
                long userKey;
                r = MobileGenerateKey(MobileID, out userKey, db);
                if (r.error)
                    return new LogInResponse(Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, 0));

                // If there was no error, create a loginResponse with the successful information.
                LogInResponse lr = new LogInResponse();
                lr.result = r.result;
                lr.userKey = userKey;
                return lr;
            }
        }
        /// <summary>
        /// Signs a mobile user out of the system.
        /// </summary>
        /// <param name="userKey">The client's key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSignOut(long userKey)
        {
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_ALREADY_OUT);
                    return r;
                }

                // A sign out seems to be valid. Also clears any device id found.
                r = db.MobileSignOut(mobileID);
                if(r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Remove the key from the DB.
                r = db.MobileSetKey(mobileID, null);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Clear the venue from the DB
                r = db.MobileSetVenue(mobileID, null);
                if(r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                return r;
            }
        }

        #endregion

        #region Songs

        /// <summary>
        /// Allows a client to search for songs in a venue. If title or artist are blank, they are not used in the search.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="artist">The artist.</param>
        /// <param name="start">The index of the first result.</param>
        /// <param name="count">The number of results to return.</param>
        /// <param name="venueID">The venueID to search from.</param>
        /// <param name="userKey">The userKey of the mobile user.</param>
        /// <returns>The outcome of the operation.</returns>
        public List<Song> MobileSongSearch(string title, string artist, int start, int count, int venueID, long userKey)
        {
            int venueStatus;
            int mobileID = -1;
            List<Song> songs = new List<Song>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsgStk(true, "User: " + mobileID + " has invalid status", Environment.StackTrace);
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                }

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.setErMsgStk(true, "VenueID parse fail", Environment.StackTrace);
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                }

                // Complete the search.
                r = db.MobileSearchSongs(title.Trim(), artist.Trim(), venueID, start, count);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                if (r.message.Trim() == string.Empty)
                    return songs;

                string[] songLines = r.message.Trim().Split('\n');
                foreach (string songLine in songLines)
                {
                    string[] songParts = Common.splitByDel(songLine);
                    Song song = new Song();
                    int id;
                    if (!int.TryParse(songParts[0], out id))
                    {
                        r.setErMsgStk(true, "Could not parse song ID", Environment.StackTrace);
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                    }

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
        /// <summary>
        /// Allows a mobile user to browse through a DJ's songs. Depending on isArtist, returns songs whose title/artist start with firstletter.
        /// </summary>
        /// <param name="firstLetter">The string to start matching on.</param>
        /// <param name="isArtist">Whether firstLetter is an artist or title.</param>
        /// <param name="start">The index of the first result.</param>
        /// <param name="count">The number of results.</param>
        /// <param name="venueID">The venueID to search songs from.</param>
        /// <param name="userKey">The userKey of the client.</param>
        /// <returns>The outcome of the operation.</returns>
        public List<Song> MobileSongBrowse(string firstLetter, bool isArtist, int start, int count, int venueID, long userKey)
        {
            int venueStatus;
            int mobileID = -1;
            List<Song> songs = new List<Song>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsgStk(true, "User: " + mobileID + " has invalid status", Environment.StackTrace);
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                }

                // Check to make sure the venue exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.setErMsgStk(true, "VenueID parse fail", Environment.StackTrace);
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                }

                r = db.MobileBrowseSongs(firstLetter, isArtist, start, count, venueID);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

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
                    {
                        r.setErMsgStk(true, "Could not parse song ID", Environment.StackTrace);
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                    }
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
        /// <summary>
        /// View the song history of the client.
        /// </summary>
        /// <param name="start">The index of the first result.</param>
        /// <param name="count">The number of results to return.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public List<SongHistory> MobileViewSongHistory(int start, int count, long userKey)
        {
            int mobileID = -1;
            List<SongHistory> songHistory = new List<SongHistory>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsgStk(true, "User: " + mobileID + " has invalid status", Environment.StackTrace);
                    return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);
                }

                r = db.MobileGetSongHistory(mobileID, start, count, out songHistory);
                if (r.error)
                    return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);

                for (int i = 0; i < songHistory.Count; i++)
                {
                    r = db.GetVenueName(songHistory[i].venue.venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);
                    songHistory[i].venue.venueName = r.message.Trim();
                    r = db.GetVenueAddress(songHistory[i].venue.venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);
                    songHistory[i].venue.venueAddress = r.message.Trim();

                    Song song;
                    r = Common.GetSongInformation(songHistory[i].song.ID, songHistory[i].venue.venueID, mobileID, out song, db, false);
                    if (r.error)
                        return Common.LogErrorRetGen<List<SongHistory>>(r, null, Common.LogFile.Mobile);
                    songHistory[i].song = song;
                }

                return songHistory;
            }
        }
        /// <summary>
        /// Rate a song.
        /// </summary>
        /// <param name="songID">The songID.</param>
        /// <param name="rating">The rating -1 to 5.</param>
        /// <param name="venueID">The venueID of the song.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileRateSong(int songID, int rating, int venueID, long userKey)
        {
            int mobileID = -1;
            int venueStatus;
            int songExists;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                ExpResponse r = new ExpResponse();
                if (rating < -1 || rating > 5)
                {
                    r.setErMsg(true, "Rating must be between -1 and 5 (inclusive).");
                    return r;
                }

                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.setErMsgStk(true, "MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.setErMsgStk(true, "Could not find song", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                // Set the song rating.
                r = db.MobileSetSongRating(mobileID, songID, rating);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                return r;
            }
        }
        /// <summary>
        /// View a song rating.
        /// </summary>
        /// <param name="songID">The songID.</param>
        /// <param name="venueID">The venueID.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileViewSongRating(int songID, int venueID, long userKey)
        {
            int mobileID = -1;
            int venueStatus;
            int songExists;
            int rating;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.setErMsgStk(true, "MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.setErMsgStk(true, "Could not find song", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                // Set the song rating.
                r = db.MobileGetSongRating(mobileID, songID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                if (r.message.Trim().Length == 0)
                {
                    r.setErMsgRes(false, "-1", -1);
                    return r;
                }

                if (!int.TryParse(r.message.Trim(), out rating))
                {
                    r.setErMsgStk(true, "Could not parse song rating song", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                r.setErMsgRes(false, rating.ToString(), rating);
                return r;
            }
        }
        /// <summary>
        /// Gets the most popular songs associated with a venue or all venues.
        /// </summary>
        /// <param name="venueID">The venueID of the venue, if -1, searches all venues.</param>
        /// <param name="start">The starting index of results.</param>
        /// <param name="count">The count of results.</param>
        /// <returns></returns>
        public List<Song> MobileGetMostPopularSongs(int venueID, int start, int count)
        {
            List<Song> songs = new List<Song>();
            List<int> counts = new List<int>();
            List<Song> songIDs;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                // Validate venueID if the any venueID not given.
                if (venueID != -1)
                {
                    // Make sure the venueID exists.
                    r = db.DJGetStatus(venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                    int venueStatus;
                    if (!int.TryParse(r.message.Trim(), out venueStatus))
                    {
                        r.setErMsgStk(true, "Could not parse venueID", Environment.StackTrace);
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                    }
                }


                r = db.GetMostPopularSongs(venueID, start, count, out songIDs, out counts);
                if (r.error)
                    return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                foreach (Song s in songIDs)
                {
                    Song fullSong;
                    Common.GetSongInformation(s.ID, venueID, -1, out fullSong, db, false);
                    songs.Add(fullSong);
                }
                return songs;
            }
        }
        /// <summary>
        /// Suggest songs for the user to sing. Based off of the song history of the current user.
        /// Some song suggestions given based on collaborative filtering with other user's song histories.
        /// Some suggestions based on other songs by artists sang. Returns most popular songs if prior
        /// information not available.
        /// </summary>
        /// <param name="venueID">The venue to suggest songs at.</param>
        /// <param name="userKey">The user's key.</param>
        /// <param name="start">The starting position of song suggestions.</param>
        /// <param name="count">The number of song suggestions.</param>
        /// <returns>The outcome of the operation</returns>
        public List<Song> MobileGetSongSuggestions(int venueID, long userKey, int start, int count)
        {
            start = 0;
            int count1 = count - count / 2;
            try
            {
                // Get all the songs I have sung.
                // Find people who have sung the same songs.
                // Get the songs these people have sung
                // If these songs have the same band as some song I've sung, award 2x points
                // If multiple people have sung the same song, points are cumalitive.
                // If we aren't finding songs, look up artists I have sung in the DB, and get other songs by that artist.
                // Insert a popular song into the mix for variablility.

                int mobileID = -1;
                using (DatabaseConnectivity db = new DatabaseConnectivity())
                {
                    List<Song> finalSuggestions = new List<Song>();
                    #region SongSuggestionBoilerPlate
                    // Try to establish a database connection
                    ExpResponse r = db.OpenConnection();
                    if (r.error)
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                    // Convert the userKey to MobileID
                    r = MobileKeyToID(userKey, out mobileID, db);
                    if (r.error)
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                    // Validate venueID if the any venueID not given.
                    if (venueID != -1)
                    {
                        // Make sure the venueID exists.
                        r = db.DJGetStatus(venueID);
                        if (r.error)
                            return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                        int venueStatus;
                        if (!int.TryParse(r.message.Trim(), out venueStatus))
                        {
                            r.setErMsgStk(true, "MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace);
                            return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                        }
                    }

                    #endregion

                    // The user's distinct song history.
                    List<KeyValuePair<string[], int>> songsAndCount; // Song Title/Artist and how many times it shows up.
                    r = db.MobileGetDistictSongHistory(mobileID, 0, 10, out songsAndCount);
                    if (r.error)
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                    // Get song suggestions based on what other people sang.
                    List<Song> suggestCollab;
                    r = SuggestCollabFilter(songsAndCount, mobileID, venueID, count1, out suggestCollab, db);
                    if (r.error)
                        return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                    // Add these songs to fill up to half of the final suggestions.
                    for (int i = 0; i < suggestCollab.Count; i++)
                    {
                        Song s = suggestCollab[i];
                        r = Common.LoadSongRating(ref s, mobileID, db);
                        if (r.error)
                            return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                        finalSuggestions.Add(s);
                    }

                    if (finalSuggestions.Count < count)
                    {
                        // Get song suggestions based off of artist
                        List<Song> suggestByArtist;
                        // Suggest songs not sung by the user, but which have an artist the user has sung.
                        r = SuggestSongsNotSungByMostSungArtists(count - finalSuggestions.Count, mobileID, venueID, out suggestByArtist, db);
                        if (r.error)
                            return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                        // Add the artist suggestions to fill out the suggetsions.
                        foreach (Song s in suggestByArtist)
                        {
                            Song song;
                            r = Common.GetSongInformation(s.ID, venueID, mobileID, out song, db, false);
                            if (r.error)
                                return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                            finalSuggestions.Add(song);
                        }
                    }

                    if (finalSuggestions.Count < count)
                    {
                        // If we are lacking songs still, get from popular songs.
                        List<Song> popSongs;
                        List<int> popCounts;
                        r = db.GetMostPopularSongs(venueID, 0, count - finalSuggestions.Count, out popSongs, out popCounts);
                        if (r.error)
                            return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);

                        foreach (Song s in popSongs)
                        {
                            Song song;
                            r = Common.GetSongInformation(s.ID, venueID, mobileID, out song, db, false);
                            if (r.error)
                                return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
                            finalSuggestions.Add(song);
                        }
                    }

                    return finalSuggestions;
                }
            }
            catch (Exception e)
            {
                ExpResponse r = new ExpResponse(true, "Exception in Suggest Song:", e.StackTrace);
                return Common.LogErrorRetGen<List<Song>>(r, null, Common.LogFile.Mobile);
            }
        }
        #endregion

        #region SongRequests

        /// <summary>
        /// Create a song request.
        /// </summary>
        /// <param name="songID">The songID</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileSongRequest(int songID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_VENUE_NO_SESSION);
                    return r;
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.setErMsgStk(true, "Could not find song", Environment.StackTrace);
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                string requests = r.message;
                string newRequests = string.Empty;

                if (requests.Trim().Length == 0)
                {
                    requests = mobileID.ToString() + "~" + songID.ToString();
                    r = db.SetSongRequests(venueID, requests);
                    if (r.error)
                        return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                    //Common.PushMessageToMobile(mobileID, "queue", db);
                    Common.PushMessageToUsersOfDJ(venueID, "queue", db);
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

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
                                r.setErMsg(true, "You are already in queue to sing that song");
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
                            return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                        Common.PushMessageToMobile(mobileID, "queue", db);
                        //Common.PushMessageToUsersOfDJ(venueID, "queue", db);

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
                r = Common.MinimalListToDB(queue, out newRequests);
                if(r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                r = db.SetSongRequests(venueID, newRequests);
                if(r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                Common.PushMessageToUsersOfDJ(venueID, "queue", db);

                return r;
            }
        }
        /// <summary>
        /// Changes a user's song request to a new song.
        /// </summary>
        /// <param name="oldSongID">The old request.</param>
        /// <param name="newSongID">The new request.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileChangeSongRequest(int oldSongID, int newSongID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            bool songChangeMade = false;
            bool requireSendToAll = false;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_VENUE_NO_SESSION);
                    return r;
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, newSongID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.setErMsg(true, "Could not find new song");
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                string requests = r.message;
                string newRequests = string.Empty;

                // If there are no song requests.
                if (requests.Trim().Length == 0)
                {
                    r.setErMsg(true, "There are no song requests.");
                    return r;

                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

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
                                r.setErMsg(true, "You are already singing the new song");
                                return r;
                            }
                            // If we found the old song.
                            if (queue[i].songs[j].ID == oldSongID)
                            {
                                queue[i].songs[j].ID = newSongID;
                                songChangeMade = true;
                                if (j == 0)
                                    requireSendToAll = true;
                            }

                        }

                        if (songChangeMade)
                        {
                            r = Common.MinimalListToDB(queue, out newRequests);
                            if(r.error)
                                return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                            r = db.SetSongRequests(venueID, newRequests);
                            if(r.error)
                                return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                            if (requireSendToAll)
                                Common.PushMessageToUsersOfDJ(venueID, "queue", db);
                            else
                                Common.PushMessageToMobile(mobileID, "queue", db);

                            return r;
                        }
                        // If we couldn't find the old song, inform user.
                        r.setErMsg(true, Messages.MSG_SONG_NOT_FOUND);
                        return r;
                    }
                }

                // If we couldn't find the user.
                r.setErMsg(true, Messages.MSG_USER_NO_SONG_REQUESTS);
                return r;
            }
        }
        /// <summary>
        /// Move a user's song to the front of his/her songs.
        /// </summary>
        /// <param name="songID">The ID of the song.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileMoveSongRequestToTop(int songID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_VENUE_NO_SESSION);
                    return r;
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.setErMsg(true, Messages.MSG_SONG_NOT_FOUND);
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                string requests = r.message;
                string newRequests = string.Empty;

                // If there are no song requests.
                if (requests.Trim().Length == 0)
                {
                    r.setErMsg(true, Messages.MSG_NO_SONG_REQUESTS);
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

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
                                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                                r = db.SetSongRequests(venueID, newRequests);
                                if (r.error)
                                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                                Common.PushMessageToUsersOfDJ(venueID, "queue", db);

                                return r;
                            }
                        }  
                    }
                }

                // If we couldn't find the user.
                r.setErMsg(true, Messages.MSG_SONG_NOT_FOUND);
                return r;
            }
        }
        /// <summary>
        /// Removes a song from a user's song requests.
        /// </summary>
        /// <param name="songID">The ID of the song.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileRemoveSongRequest(int songID, long userKey)
        {
            int venueID = -1;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileIDx
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_VENUE_NO_SESSION);
                    return r;
                }

                // Get the current song Requests
                r = db.GetSongRequests(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                string requests = r.message;
                string newRequests = string.Empty;

                if (requests.Trim().Length == 0)
                {
                    r.setErMsg(true, Messages.MSG_NO_SONG_REQUESTS);
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                List<queueSinger> queue;
                r = Common.DBToMinimalList(requests, out queue);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

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
                                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                                if (j == 0)
                                    Common.PushMessageToUsersOfDJ(venueID, "queue", db);
                                else
                                    Common.PushMessageToMobile(mobileID, "queue", db);

                                return r;
                            }

                        }
                        // If we can't find the current song.
                        r.setErMsg(true, Messages.MSG_SONG_NOT_FOUND);
                        return r;
                    }
                }

                r.setErMsg(true, Messages.MSG_USER_NOT_IN_QUEUE);
                return r;
            }
        }
        /// <summary>
        /// View the queue of the DJ the client is associated with.
        /// </summary>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public List<queueSinger> MobileViewQueue(long userKey)
        {
            int venueID = -1;
            int mobileID = -1;
            List<queueSinger> queue = new List<queueSinger>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsgStk(true, "Bad user status", Environment.StackTrace);
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);
                }

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsgStk(true, "Bad venue status for viewing queue.", Environment.StackTrace);
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);
                }

                r = db.GetSongRequests(venueID);
                if (r.error)
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    return queue;
                }

                r = DBToNearlyFullList(raw, out queue, venueID, mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<queueSinger>>(r, null, Common.LogFile.Mobile);
                return queue;
            }
        }

        #endregion

        #region VenueInformation
        /// <summary>
        /// Allow a client to join a venue via QR code.
        /// </summary>
        /// <param name="QR">The QR code of the venue.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileJoinVenue(string QR, long userKey)
        {
            int mobileID = -1;
            int venueID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Get the venue of this qr string.
                r = db.GetVenueIDByQR(QR.Trim());
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                if (r.message.Trim().Length == 0)
                {
                    r.setErMsg(true, Messages.ERR_QR_BAD);
                    return r;
                }

                // Parse the venueID.
                if (!int.TryParse(r.message.Trim(), out venueID))
                {
                    r.setErMsgStk(true, "QR code couldnot be parsed from db", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_VENUE_NO_SESSION);
                    return r;
                }

                // Check if the user is banned
                bool userBanned;
                r = db.MobileIsBanned(venueID, mobileID, out userBanned);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                if (userBanned)
                {
                    r.setErMsg(true, Messages.MSG_USER_BANNED);
                    return r;
                }

                // Set the venue of the client
                r = db.MobileSetVenue(mobileID, venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                r = db.GetVenueName(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                r.result = venueID;
                return r;
            }
        }
        /// <summary>
        /// Get the estimated wait time until the user can sing.
        /// </summary>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileGetWaitTime(long userKey)
        {
            int venueID = -1;
            int mobileID = -1;
            List<queueSinger> queue = new List<queueSinger>();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                
                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }
                   

                // Get the venueID
                r = MobileGetVenue(mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                venueID = r.result;

                // Make sure the venue is accepting songs.
                r = VenueCheckStatus(venueID, "2", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_VENUE_NO_SESSION);
                    return r;
                }

                r = db.GetSongRequests(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                string raw = r.message;
                if (raw.Trim() == "")
                {
                    r.setErMsgRes(false, "0", 0);
                    return r;
                }

                // Since there is a list of requests, call to parse the raw string data into an list of queuesingers.
                r = DBToNearlyFullList(raw, out queue, venueID, mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                int time = 0;
                for (int i = 0; i < queue.Count; i++)
                {
                    if (queue[i].user.userID == mobileID)
                        break;
                    else
                        time += queue[i].songs[0].duration + Common.TIME_BETWEEN_REQUESTS;
                }

                r.setErMsgRes(false, time.ToString(), time);
                return r;
            }
        }

        #endregion

        #region playlists

        /// <summary>
        /// Create a playlist. Returns the ID of the playlist in message.
        /// </summary>
        /// <param name="name">Playlist Name</param>
        /// <param name="venueID">VenueID the playlist is associated with.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileCreatePlaylist(string name, int venueID, long userKey)
        {
            ExpResponse r = new ExpResponse();
            if (name.Length < 1 || name.Length > 20)
            {
                r.setErMsg(true, Messages.ERR_PLYLST_NAME_LONG);
                return r;
            }

            int mobileID = -1;
            int venueStatus;
            
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Make sure the venueID exists.
                r = db.DJGetStatus(venueID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                if (!int.TryParse(r.message.Trim(), out venueStatus))
                {
                    r.setErMsg(true, Messages.ERR_BAD_SERVER_INPUT);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                r = db.MobileCreatePlaylist(name, venueID, mobileID, DateTime.Now);
                if(r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                return r;
            }
        }
        /// <summary>
        /// Delete a playlist.
        /// </summary>
        /// <param name="playListID">The Id of the playlist.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileDeletePlaylist(int playListID, long userKey)
        {
            int mobileID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                r = db.MobileDeletePlaylist(playListID, mobileID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                if (r.result == 0)
                {
                    r.setErMsg(true, Messages.ERR_BAD_SERVER_INPUT);
                    return r;
                }
                return r;
            }
        }
        /// <summary>
        /// Add a song to a playlist.
        /// </summary>
        /// <param name="songID">The songID</param>
        /// <param name="playListID">The PlaylistID</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileAddSongToPlaylist(int songID, int playListID, long userKey)
        {
            int venueID = -1;
            int songExists;
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Get the venue information from the playlist in DB.
                r = db.MobileGetVenueFromPlaylist(playListID, mobileID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out venueID))
                {
                    r.setErMsgStk(true, "Could not figure out Venue from DB", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                }

                // Check to see if song exists.
                r = db.SongExists(venueID, songID);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!int.TryParse(r.message.Trim(), out songExists))
                {
                    r.setErMsg(true, Messages.MSG_SONG_NOT_FOUND);
                    return r;
                }

                // Object to represent the song to add.
                Song song = new Song();
                song.ID = songID;

                // Get the current songs in the playlist.
                List<Song> songs;
                r = db.MobileGetSongsFromPlaylist(playListID, mobileID, out songs);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Check to see if the song already exists.
                foreach (Song s in songs)
                {
                    if (s.ID == song.ID)
                    {
                        r.setErMsg(true, Messages.ERR_PLYLST_DUP_SONG);
                        return r;
                    }
                }

                // Otherwise, add this to the playlist.
                songs.Add(song);
                r = db.MobileSetPlaylistSongs(playListID, mobileID, songs);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                return r;
            }
        }       
        /// <summary>
        /// Remove a song from a playlist.
        /// </summary>
        /// <param name="songID">The SongID</param>
        /// <param name="playListID">The PlaylistID</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileRemoveSongFromPlaylist(int songID, int playListID, long userKey)
        {
            int mobileID;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return r;
                }

                // Get the current songs in the playlist.
                List<Song> songs;
                r = db.MobileGetSongsFromPlaylist(playListID, mobileID, out songs);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);


                // Find the song to remove and remove it.
                for (int i = 0; i < songs.Count; i++)
                {
                    if (songs[i].ID == songID)
                    {
                        songs.RemoveAt(i);
                        r = db.MobileSetPlaylistSongs(playListID, mobileID, songs);
                        if (r.error)
                            return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Mobile);
                        return r;
                    }
                }

                // If we didn't find the song to remove.
                r.setErMsg(true, Messages.MSG_SONG_NOT_FOUND);
                return r;
            }
        }
        /// <summary>
        /// Get the playlists for a client.
        /// </summary>
        /// <param name="venueID">The venue to select playlists from. If set to -1, all venues are used.</param>
        /// <param name="userKey">client mobile key.</param>
        /// <returns>The outcome of the opearation.</returns>
        public List<Playlist> MobileGetPlayLists(int venueID, long userKey)
        {
            int mobileID = -1;
            int venueStatus;
            List<Playlist> playLists;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);

                // Make sure the client isn't already logged out.
                bool validStatus;
                r = MobileCheckStatus(mobileID, "!0", db, out validStatus);
                if (r.error)
                    return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);
                if (!validStatus)
                {
                    r.setErMsg(true, Messages.ERR_STATUS_IS_NOT_IN);
                    return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);
                }

                // Validate venueID if the any venueID not given.
                if (venueID != -1)
                {
                    // Make sure the venueID exists.
                    r = db.DJGetStatus(venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);

                    if (!int.TryParse(r.message.Trim(), out venueStatus))
                    {
                        r.setErMsgStk(true, "MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace);
                        return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);
                    }
                }

                r = db.MobileGetPlaylists(venueID, mobileID, out playLists);
                if (r.error)
                    return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);

                // Finish inserting information into the playlists.
                foreach (Playlist p in playLists)
                {
                    // Insert venue information.
                    r = db.GetVenueName(p.venue.venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);
                    p.venue.venueName = r.message.Trim();

                    r = db.GetVenueAddress(p.venue.venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);
                    p.venue.venueAddress = r.message.Trim();

                    for (int i = 0; i < p.songs.Count; i++)
                    {
                        Song fullSong;
                        r = Common.GetSongInformation(p.songs[i].ID, p.venue.venueID, mobileID, out fullSong, db);
                        if (r.error)
                            return Common.LogErrorRetGen<List<Playlist>>(r, null, Common.LogFile.Mobile);
                        p.songs[i] = fullSong;
                    }
                }
                return playLists;
            }
        }
        #endregion

        #region Achievements

        public List<MobileAchievement> MobileGetAchievements(int venueID, long userKey, int start, int count)
        {
            int mobileID = -1;
            int venueStatus;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                // Validate venueID if the any venueID not given.
                if (venueID != -1)
                {
                    // Make sure the venueID exists.
                    r = db.DJGetStatus(venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                    if (!int.TryParse(r.message.Trim(), out venueStatus))
                    {
                        r.setErMsgStk(true, "MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace);
                        return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);
                    }
                }

                List<Achievement> achievements;

                r = db.MobileGetAchievements(mobileID, venueID, out achievements, start, count);
                if (r.error)
                    return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                List<MobileAchievement> mobileAchievements = new List<MobileAchievement>();
                foreach (Achievement a in achievements)
                {
                    MobileAchievement ma = new MobileAchievement();
                    ma.ID = a.ID;
                    ma.name = a.name;
                    ma.description = a.description;
                    ma.image = a.image.ToString() + ".png";
                    mobileAchievements.Add(ma);
                }


                return mobileAchievements;
            }
        }
        public List<MobileAchievement> MobileGetUnearnedAchievements(int venueID, long userKey, int start, int count)
        {
            int mobileID = -1;
            int venueStatus;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                // Convert the userKey to MobileID
                r = MobileKeyToID(userKey, out mobileID, db);
                if (r.error)
                    return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                // Validate venueID if the any venueID not given.
                if (venueID != -1)
                {
                    // Make sure the venueID exists.
                    r = db.DJGetStatus(venueID);
                    if (r.error)
                        return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                    if (!int.TryParse(r.message.Trim(), out venueStatus))
                    {
                        r.setErMsgStk(true, "MobileGetPlayLists venueID parse fail (Bad venueID given?)", Environment.StackTrace);
                        return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);
                    }
                }

                List<Achievement> unearnedAchievements;

                r = db.MobileGetUnearnedVisibleAchievements(mobileID, venueID, out unearnedAchievements, start, count);
                if (r.error)
                    return Common.LogErrorRetGen<List<MobileAchievement>>(r, null, Common.LogFile.Mobile);

                List<MobileAchievement> mobileUnearnedAchievements = new List<MobileAchievement>();
                foreach (Achievement a in unearnedAchievements)
                {
                    MobileAchievement ma = new MobileAchievement();
                    ma.ID = a.ID;
                    ma.name = a.name;
                    ma.description = a.description;
                    ma.image = a.image.ToString() + ".png";
                    mobileUnearnedAchievements.Add(ma);
                }

                return mobileUnearnedAchievements;
            }
        }
        #endregion

        #region PrivateMethods
        /// <summary>
        /// Suggests songs based on collaborative filtering of the user's song history with other
        /// singers' song histories. Songs objects returned are pre-populated except for path on disk.
        /// </summary>
        /// <param name="userSongsAndCount">The user's songs and how often sung.</param>
        /// <param name="mobileID">The mobileID of the user.</param>
        /// <param name="venueID">The venueID</param>
        /// <param name="count">How many songs to suggest.</param>
        /// <param name="suggestCollab">Out parameter of suggested songs.</param>
        /// <param name="db">Database connectivity.</param>
        /// <returns>The outcome of the operation.</returns>
        private ExpResponse SuggestCollabFilter(List<KeyValuePair<string[], int>> userSongsAndCount, int mobileID, int venueID, int count, out List<Song> suggestCollab, DatabaseConnectivity db)
        {
            // Assign potential songs a value. If a user has 3 songs in common with me, every song suggestable should get 3 pts.
            // If 3 users have songs in common with me, and they each have a song in commong with themselves, that song gets 3 pts since it's from 3 users.
            // Later on rank by points.
            ExpResponse r = new ExpResponse();
            suggestCollab = new List<Song>();

            // Go through user history, get all the users who sang songs that I sang.
            // List of userIds, and how many songs we have in common.
            List<KeyValuePair<int, int>> userCountInCommon;
            r = GetUsersSongsInCommon(userSongsAndCount, mobileID, db, out userCountInCommon);
            if (r.error)
                return r;
            
            // Stores each user, how similar to them we are, what songs they have sung, and how often they have sung those songs.
            List<UserAndSongs> userAndSongs = new List<UserAndSongs>();

            // Get songs that the in common users have sung at this venue.
            foreach (KeyValuePair<int, int> other in userCountInCommon)
            {
                List<KeyValuePair<string[], int>> OSAC;

                r = db.MobileGetOtherDistictSongHistory(other.Key, 0, 2*count, out OSAC);
                if (r.error)
                    return r;

                // Remove any songs from other user's history that we have already sang.
                foreach (KeyValuePair<string[], int> excludeSong in userSongsAndCount)
                {
                    for (int i = 0; i < OSAC.Count; i++)
                    {
                        if (OSAC[i].Key[0].Equals(excludeSong.Key[0]) && OSAC[i].Key[1].Equals(excludeSong.Key[1]))
                        {
                            OSAC.RemoveAt(i);
                            break;
                        }
                    }
                }

                userAndSongs.Add(new UserAndSongs(other.Key, other.Value, OSAC));             
            }


            Random rand = new Random(DateTime.Now.Millisecond);
            while (userAndSongs.Count > 0)
            {
                int userIndex = selectRandWeightedUser(rand, userAndSongs);
                int songIndex = selectRandomSong(rand, userAndSongs[userIndex].songs);
                string title = userAndSongs[userIndex].songs[songIndex].Key[0];
                string artist = userAndSongs[userIndex].songs[songIndex].Key[1];
                userAndSongs[userIndex].songs.RemoveAt(songIndex);
                if (userAndSongs[userIndex].songs.Count == 0)
                    userAndSongs.RemoveAt(userIndex);

                Song song;
                r = db.MobileGetSongFromTitleArtist(title, artist, venueID, out song);
                if (r.error)
                    return r;
                if (song != null)
                    suggestCollab.Add(song);
                if (suggestCollab.Count >= count)
                    return r;
            }
            // Get count songs.


            //// Now weighted randomly select users, then weighted randomly select songs out of there.
            //foreach (UserAndSongs uas in userAndSongs)
            //{
            //    foreach (KeyValuePair<string[], int> s in uas.songs)
            //    {
            //        Song song;
            //        r = db.MobileGetSongFromTitleArtist(s.Key[0], s.Key[1], venueID, out song);
            //        if (r.error)
            //            return r;
            //        if(song != null)
            //            suggestCollab.Add(song);
            //        if (suggestCollab.Count >= count)
            //            return r;
            //    }
            //}
            return r;
        }
        /// <summary>
        /// Select a random song from a list of weigted songs.
        /// </summary>
        /// <param name="rand">Randon number generator to use.</param>
        /// <param name="all">The weighted list of songs.</param>
        /// <returns>The index of the random song in the collection.</returns>
        private int selectRandomSong(Random rand, List<KeyValuePair<string[], int>> all)
        {
            int totalSongScore = 0;
            foreach (KeyValuePair<string[], int> s in all)
                totalSongScore += s.Value;

            int sum = 0;
            int rn = rand.Next(1, totalSongScore);
            int count = 0;
            foreach (KeyValuePair<string[], int> s in all)
            {
                sum += s.Value;
                if (rn <= sum)
                    return count;
                count++;
            }

            Common.LogError("selectRandomSong logic fail", "Had to select first song", null, 2);
            return 0;
        }
        /// <summary>
        /// Returns a random user from a weigted collection of users.
        /// </summary>
        /// <param name="rand">The random number generator to use.</param>
        /// <param name="all">The weighted collection of users.</param>
        /// <returns>The index of the random user in the collection.</returns>
        private int selectRandWeightedUser(Random rand, List<UserAndSongs> all)
        {
            int totalUserScore = 0;
            foreach (UserAndSongs uas in all)
                totalUserScore += uas.commonScore;

            int sum = 0;
            int rn = rand.Next(1, totalUserScore);
            int count = 0;
            foreach (UserAndSongs uas in all)
            {
                sum += uas.commonScore;
                if (rn <= sum)
                    return count;
                count++;
            }
            Common.LogError("SelectRandWeigtedUser logic fail", "Had to select first user", null, 2);
            return 0;
        }
        /// <summary>
        /// Returns a list of keyvaluepairs where the key is the userID of a user who has a song in common with us, and the value is the number of songs they have in common.
        /// </summary>
        /// <param name="userHistory"></param>
        /// <param name="mobileID"></param>
        /// <param name="db"></param>
        /// <param name="songsInCommonList"></param>
        /// <returns></returns>
        private ExpResponse GetUsersSongsInCommon(List<KeyValuePair<string[], int>> songsAndCount, int mobileID, DatabaseConnectivity db, out List<KeyValuePair<int, int>> songsInCommonList)
        {
            songsInCommonList = new List<KeyValuePair<int, int>>();
            ExpResponse r = new ExpResponse();
            // For each user, see how many songs we have in common.
            IDictionary<int, int> songsInCommonDict = new Dictionary<int, int>(); // UserID, count

            // Loop through all of my songs.
            foreach (KeyValuePair<string[],int> sAc in songsAndCount)
            {
                SangSong ss;
                // Get everyone that shares this song.
                r = db.MobileGetOthersWhoSangSong(mobileID, sAc.Key[0], sAc.Key[1], 10, out ss);
                if (r.error)
                    return r;
                foreach (KeyValuePair<int, int> users in ss.userIDsAndCount)
                {
                    if (songsInCommonDict.ContainsKey(users.Key))
                        songsInCommonDict[users.Key] += Math.Min(users.Value, sAc.Value);
                    else
                        songsInCommonDict.Add(users.Key, Math.Min(users.Value, sAc.Value));
                }
            }

            // List of users and the number of songs we have in common, sort them in order of people we have most in common with.
            songsInCommonList = songsInCommonDict.ToList();
            songsInCommonList.Sort((one, two) => { return one.Value.CompareTo(two.Value); });
            return r;
        }
        /// <summary>
        /// Generates up to maxSuggestsions song suggestions for a user based off of their song history.
        /// Suggested songs are from the same artists as songs they have previously sung with a bias
        /// to artists they have sung more often. Suggestions are always song they have not sung yet.
        /// </summary>
        /// <param name="maxSuggestions"></param>
        /// <param name="mobileID"></param>
        /// <param name="DJID"></param>
        /// <param name="songs"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        private ExpResponse SuggestSongsNotSungByMostSungArtists(int maxSuggestions, int mobileID, int DJID, out List<Song> songs, DatabaseConnectivity db)
        {
            List<SongAndCount> sAc;
            songs = new List<Song>();
            ExpResponse r;

            // Get up to 10 unique artists this user has sung, ordered by the most sung first.
            r = db.MobileGetUniqueArtistsSung(mobileID, 0, 10, out sAc);
            if (r.error)
                return r;

            // Loop through the unique artists.
            // store total number of song sings in singCount
            // Store the iteration singCount in tmp for each sAc.
            int singCount = 0;
            for (int i = 0; i < sAc.Count; i++)
            {
                singCount += sAc[i].count;
                sAc[i].tmp = singCount;
            }

            // Generate a random number between 0 and the number of total sings.
            // Loop through the artists, if the random number is less than the 
            // store singCount iteration number, choose to suggest a song based off that artist.
            Random rand = new Random(DateTime.Now.Millisecond);
            
            for (int i = 0; i < maxSuggestions; i++)
            {
                int rn = rand.Next(0, singCount);
                for (int j = 0; j < sAc.Count; j++)
                {
                    if (rn < sAc[j].tmp)
                    {
                        sAc[j].tmp2++;
                        break;
                    }
                }
            }

            // For each artist, if we selected to suggest based off them, do so the number of times requested.
            foreach (SongAndCount s in sAc)
            {
                if (s.tmp2 > 0)
                {
                    List<int> songIDs;
                    r = db.MobileGetRandomSongsFromExactArtistNeverSung(s.song.artist, DJID, s.tmp2, mobileID, out songIDs);
                    if (r.error)
                        return r;
                    foreach (int id in songIDs)
                    {
                        Song song = new Song();
                        song.ID = id;
                        songs.Add(song);
                    }
                }
            }

            return r;
        }
        /// <summary>
        /// Check to see if a status of a venue is equal to the desired status. 
        /// </summary>
        /// <param name="venueID">The venueID.</param>
        /// <param name="desiredStatus">The desired status "2" or "!3" etc.</param>
        /// <param name="db">The database connectivity.</param>
        /// <returns>The outcome of the operation.</returns>
        private ExpResponse VenueCheckStatus(int venueID, string desiredStatus, DatabaseConnectivity db, out bool validStatus)
        {
            ExpResponse r;
            int DJStatus, desired;
            bool notStatus = false;
            validStatus = false;
            // Get the status of the DJ.
            r = db.DJGetStatus(venueID);
            if (r.error)
                return r;

            // Attempt to parse that status of the DJ.
            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.setErMsgStk(true, "Exception in VenueCheckStatus: Unable to parse status from DB!", Environment.StackTrace);
                return r;
            }

            if (desiredStatus[0] == '!')
            {
                notStatus = true;
                desiredStatus = desiredStatus.Substring(1);
            }

            if (!int.TryParse(desiredStatus, out desired))
            {
                r.setErMsgStk(true, "Exception in VenueCheckStatus: Cannot parse desired Status", Environment.StackTrace);
                return r;
            }

            if (!notStatus)
            {
                if (DJStatus != desired)
                {
                    return r;
                }
            }
            else if (DJStatus == desired)
            {
                return r;
            }

            r.result = DJStatus;
            validStatus = true;
            return r;
        }
        /// <summary>
        /// Convert the database representation of a queue to the object representation. Fill all fields except for path on disk.
        /// </summary>
        /// <param name="raw">The database representation.</param>
        /// <param name="queue">The out parameter to store the queue in.</param>
        /// <param name="DJID">The ID of the venue.</param>
        /// <param name="mobileID">The ID of the client.</param>
        /// <param name="db">The databse conenctivity to use.</param>
        /// <returns>The outcome of the operation.</returns>
        private ExpResponse DBToNearlyFullList(string raw, out List<queueSinger> queue, int DJID, int mobileID, DatabaseConnectivity db)
        {
            queue = new List<queueSinger>();
            ExpResponse r = new ExpResponse();
            int count = 0;

            string[] clientRequests = raw.Split('`');
            for (int i = 0; i < clientRequests.Length; i++)
            {
                string[] parts = clientRequests[i].Split('~');
                if (parts.Length == 0)
                {
                    r.setErMsgStk(true, "Error in DBToNearlyFullList", Environment.StackTrace);
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
                   r.setErMsgStk(true, "DB Username lookup exception in DJGetQueue!", Environment.StackTrace);
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
        /// <summary>
        /// Check whether the status of a mobile user is as desired.
        /// </summary>
        /// <param name="mobileID">The client ID.</param>
        /// <param name="desiredStatus">The desired status of the client.</param>
        /// <param name="db">The database conenctivity to use.</param>
        /// <returns>The outcome of the operation.</returns>
        private ExpResponse MobileCheckStatus(int mobileID, string desiredStatus, DatabaseConnectivity db, out bool validStatus)
        {
            ExpResponse r;
            validStatus = false;
            int MobileStatus, desired;
            bool notStatus = false;
            // Get the status of the DJ.
            r = db.MobileGetStatus(mobileID);
            if (r.error)
                return r;

            // Attempt to parse that status of the DJ.
            if (!int.TryParse(r.message.Trim(), out MobileStatus))
            {
                r.setErMsgStk(true, "Exception in MobileCheckStatus: Unable to parse status from DB!", Environment.StackTrace);
                return r;
            }

            if (desiredStatus[0] == '!')
            {
                notStatus = true;
                desiredStatus = desiredStatus.Substring(1);
            }

            if (!int.TryParse(desiredStatus, out desired))
            {
                r.setErMsgStk(true, "Cannot parse desired status", Environment.StackTrace);
                return r;
            }

            if (!notStatus)
            {
                if (MobileStatus != desired)
                {
                    return r;
                }
            }
            else if (MobileStatus == desired)
            {
                return r;
            }

            r.result = MobileStatus;
            validStatus = true;
            return r;
        }
        /// <summary>
        /// Conver t amobile Key to a mobileID.
        /// </summary>
        /// <param name="MobileKey">The mobileKey.</param>
        /// <param name="MobileID">Out parameter mobileID.</param>
        /// <returns>The outcome of the operation.</returns>
        private ExpResponse MobileKeyToID(long MobileKey, out int MobileID, DatabaseConnectivity db)
        {
            MobileID = -1;
            ExpResponse r = db.MobileGetIDFromKey(MobileKey);
            if (r.error)
                return r;
            if (r.message.Trim().Length == 0)
            {
                r.setErMsgStk(true, "Invalid MobileKey: " + MobileKey, Environment.StackTrace);
                return r;
            }
            if (!int.TryParse(r.message.Trim(), out MobileID))
            {
                r.setErMsgStk(true, "Exception in MobileKeyToID: Failed to parse ID from 'key' in DB: '" + r.message.Trim() + "'", Environment.StackTrace);
                return r;
            }
            return r;
        }
        /// <summary>
        /// Convert a mobile ID to a key.
        /// </summary>
        /// <param name="MobileID">The mobileID.</param>
        /// <param name="MobileKey">Out parameter of the mobile key.</param>
        /// <returns>The outcome of the operation.</returns>
        private ExpResponse MobileGenerateKey(int MobileID, out long MobileKey, DatabaseConnectivity db)
        {
            MobileKey = -1;
            ExpResponse r = new ExpResponse();
            System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
            Random rand = new Random();
            byte[] randomBytes = new byte[64];
            byte[] result;
            long tempKey;
            for(;;)
            {
                rand.NextBytes(randomBytes);
                result = sha.ComputeHash(randomBytes);
                tempKey = BitConverter.ToInt64(result, 0);
                r = db.MobileGetIDFromKey(tempKey);
                if(r.error)
                    return r;
                if(r.message.Trim().Length != 0)
                    continue;
                r = db.MobileSetKey(MobileID, tempKey);
                if(r.error)
                    return r;
                MobileKey = tempKey;
                return r;
            }
        }
        /// <summary>
        /// Get the venue that is associated with the mobile ID. Set result and message to the venue if able.
        /// </summary>
        /// <param name="mobileID">The mobile ID of the client.</param>
        /// <param name="db">The databse conenctivity to use.</param>
        /// <returns>The outcome of the operation.</returns>
        private ExpResponse MobileGetVenue(int mobileID, DatabaseConnectivity db)
        {
            int venueID = -1;
            ExpResponse r = db.MobileGetVenue(mobileID);
            if (r.error)
                return r;

            if (!int.TryParse(r.message.Trim(), out venueID))
            {
                r.setErMsgStk(true, "Could not parse venueID from DB", Environment.StackTrace);
                return r;
            }

            r.result = venueID;
            return r;
        }
        #endregion
    }
}