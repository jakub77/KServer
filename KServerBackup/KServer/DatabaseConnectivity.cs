﻿// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// DatabaseConnectivity is a collection of methods called by other
// parts of the project to interface with the database. These methods
// are specific to Microsoft SQL Server 2012. They additionally likely
// work with SQL Server 2008, but this is not tested.

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;

namespace KServer
{
    public class DatabaseConnectivity : IDisposable
    {
        // Descriptions of the connection information for the database.
        private string SQLServerAddress = "localhost";
        private string DBUsername = "karaoke";
        private string DBPassword = "topsecret";
        private string database = "KaraokeDB";
        private int connectionTimeOut = 10;
        private string connectionString = String.Empty;

        private SqlConnection con;

        /// <summary>
        /// Set up the connection string on creation.
        /// </summary>
        public DatabaseConnectivity()
        {
            connectionString = string.Empty;
            connectionString += "user id=" + DBUsername + ";";
            connectionString += "pwd=" + DBPassword + ";";
            connectionString += "server=" + SQLServerAddress + ";";
            connectionString += "database=" + database + ";";
            connectionString += "connection timeout=" + connectionTimeOut + ";";
            con = new SqlConnection(connectionString);
        }

        /// <summary>
        /// Open a connection to the database for all other methods to use.
        /// Call this before calling data retrieving funcitons.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        public Response OpenConnection()
        {
            Response r = new Response();
            try
            {
                con.Open();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Error opening SQL connection" + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Close the connection to the database. Call this when you are done
        /// using this connection. Alternatively dispose of the resource to 
        /// automatically call this method.
        /// </summary>
        /// <returns></returns>
        public Response CloseConnection()
        {
            Response r = new Response();
            try
            {
                con.Close();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Error opening SQL connection" + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Close the database connection.
        /// </summary>
        void IDisposable.Dispose()
        {
            CloseConnection();
            return;
        }

        /// <summary>
        /// A generic way to execute a non-query SQL command.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns>The outcome of the operation. Resposne.Result will contain the number of affected rows.</returns>
        private Response DBNonQuery(SqlCommand cmd)
        {
            Response r = new Response();
            r.result = 0;

            try
            {
                cmd.Connection = con;
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DBNonQuery\n " + e.Message + e.StackTrace;
                return r;
            }
        }

        /// <summary>
        /// A generic way to execute a scalar operation on the database.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <returns>The first fow of the result as an integer is stored in r.result.</returns>
        private Response DBScalar(SqlCommand cmd)
        {
            Response r = new Response();
            try
            {
                cmd.Connection = con;
                var v = cmd.ExecuteScalar();
                r.result = int.Parse(v.ToString());
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DBScalar\n " + e.Message + e.StackTrace;
                return r;
            }
        }

        /// <summary>
        /// A generic way to execute a query on the database.
        /// </summary>
        /// <param name="cmd">The command.</param>
        /// <param name="columns">The columns of results requested.</param>
        /// <returns>The outcome of the operation.</returns>
        private Response DBQuery(SqlCommand cmd, string[] columns)
        {
            Response r = new Response();
            r.result = 0;
            try
            {
                cmd.Connection = con;
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        r.result++;
                        for (int i = 0; i < columns.Length - 1; i++)
                            r.message += reader[columns[i]].ToString().Trim() + Common.DELIMINATOR;
                        if (columns.Length > 0)
                            r.message += reader[columns[columns.Length - 1]].ToString().Trim();
                        r.message += "\n";
                    }
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DBQuery: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Checks to see if a song exists, if it does, SongID is stored in message.
        /// </summary>
        /// <param name="DJID">The ID of the DJ/Venue.</param>
        /// <param name="SongID">The ID of the song.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response SongExists(int DJID, int SongID)
        {
            SqlCommand cmd = new SqlCommand("select SongID from DJSongs where SongID = @songID and DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@songID", SongID);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "SongID" });
        }

        /// <summary>
        /// Gets the song information of a song from the databse
        /// </summary>
        /// <param name="DJID">The ID of the DJ/Venue.</param>
        /// <param name="SongID">The ID of the song.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response SongInformation(int DJID, int SongID)
        {
            SqlCommand cmd = new SqlCommand("select Title, Artist, PathOnDisk, Duration from DJSongs where SongID = @songID and DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@songID", SongID);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[4] { "Title", "Artist", "PathOnDisk", "Duration" });
        }

        /// <summary>
        /// Get all the DJs from the database.
        /// </summary>
        /// <returns>The outcome of the operation with message describing the DJs.</returns>
        public Response DJListMembers()
        {
            SqlCommand cmd = new SqlCommand("select * from DJUsers;");
            Response r = DBQuery(cmd, new string[4] { "ID", "Username", "Password", "Status" });
            r.message = r.message.Replace(Common.DELIMINATOR, ",");
            return r;
        }

        /// <summary>
        /// Check to see if a DJ's username and password are valid.
        /// If credentials are valid, returns the unique DJID in message,
        /// otherwise, the message will be blank.
        /// </summary>
        /// <param name="username">The DJ's username.</param>
        /// <param name="password">The DJ's password</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJValidateUsernamePassword(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where Username = @username and Password = @password;");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            return DBQuery(cmd, new string[1] { "ID" });
        }

        /// <summary>
        /// Check to see if a DJ's username is valid/taken. If the username is valid,
        /// the DJID is in the message, otherwise message is blank.
        /// </summary>
        /// <param name="username">The DJ's username.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJValidateUsername(string username)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where Username = @username;");
            cmd.Parameters.AddWithValue("@username", username);
            return DBQuery(cmd, new string[1] { "ID" });
        }

        /// <summary>
        /// Validate whether a DJ with the given ID exists. If it does, message is set to his/her status.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJValidateDJID(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select Status from DJUsers where ID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "Status" });
        }

        /// <summary>
        /// Sets a DJ's unique key. The DJKey is a long if valid, and null if unassigned.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <param name="DJKey">The DJ's key, expected to be a long or null.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJSetKey(int DJID, object DJKey)
        {
            SqlCommand cmd = new SqlCommand("update DJUsers set KeyHash = @DJKey where ID = @DJID;");
            if (DJKey == null)
                cmd.Parameters.AddWithValue("@DJKey", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@DJKey", DJKey);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Converts a DJKey to a DJID.
        /// </summary>
        /// <param name="DJKey">The DJKey of the DJ.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJGetIDFromKey(long DJKey)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where KeyHash = @DJKey;");
            cmd.Parameters.AddWithValue("@DJKey", DJKey);
            return DBQuery(cmd, new string[] { "ID" });
        }

        /// <summary>
        /// Gets the status of a DJ and stores it in message. Message is blank if the DJ doesn't exist.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJGetStatus(int DJID)
        {
            return DJValidateDJID(DJID);
        }

        /// <summary>
        /// Adds a new DJ into the database.
        /// </summary>
        /// <param name="username">The username of the DJ.</param>
        /// <param name="password">The password of the DJ.</param>
        /// <param name="email">The email address of the DJ.</param>
        /// <param name="venueName">The DJ's venue name.</param>
        /// <param name="venueAddress">The DJ's venue address.</param>
        /// <returns>The outcome of the operation</returns>
        public Response DJSignUp(string username, string password, string email, string venueName, string venueAddress, string salt)
        {
            SqlCommand cmd = new SqlCommand("insert into DJUsers (Username, Password, Status, QR, Email, VenueName, VenueAddress, Salt)");
            cmd.CommandText += "Values (@username, @password, @status, @QR, @email, @venueName, @venueAddress, @Salt);";
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            cmd.Parameters.AddWithValue("@status", 0);
            cmd.Parameters.AddWithValue("@QR", "");
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@venueName", venueName);
            cmd.Parameters.AddWithValue("@venueAddress", venueAddress);
            cmd.Parameters.AddWithValue("@Salt", salt);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Sets a DJ's password to the new password.
        /// </summary>
        /// <param name="DJID">The DJ's unique ID.</param>
        /// <param name="password">The new hashed and salted password.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJSetPassword(int DJID, string password)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("Update DJUsers set Password = @password where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@ID", DJID);
            try
            {
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJSetPassword: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Update a DJ's email address.
        /// </summary>
        /// <param name="DJID">The DJ's unique ID.</param>
        /// <param name="email"></param>
        /// <returns>The otucome of the operation.</returns>
        public Response DJSetEmail(int DJID, string email)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("Update DJUsers set Email = @email where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@ID", DJID);
            try
            {
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJSetEmail: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Sets a Mobile User's password to the new password.
        /// </summary>
        /// <param name="mobileID">The username to match with.</param>
        /// <param name="password">The new hashed and salted password.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetPassword(int mobileID, string password)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("Update MobileUsers set Password = @password where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@password", password);
            cmd.Parameters.AddWithValue("@ID", mobileID);
            try
            {
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJSetPassword: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Update a Mobile user's email address.
        /// </summary>
        /// <param name="mobileID">The mobile ID.</param>
        /// <param name="email">The new email.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetEmail(int mobileID, string email)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("Update MobileUsers set Email = @email where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@ID", mobileID);
            try
            {
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJSetEmail: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Update a DJ's salt.
        /// </summary>
        /// <param name="DJID">The DJ's unique ID.</param>
        /// <param name="salt">The new salt.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJSetSalt(int DJID, string salt)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("Update DJUsers set Salt = @salt where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@ID", DJID);
            try
            {
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJSetPassword: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Set's a mobile users's password salt.
        /// </summary>
        /// <param name="mobileID">The mobile user's unique ID.</param>
        /// <param name="salt">The new salt.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetSalt(int mobileID, string salt)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("Update MobileUsers set Salt = @salt where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@salt", salt);
            cmd.Parameters.AddWithValue("@ID", mobileID);
            try
            {
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJSetPassword: " + e.Message;
                return r;
            }
        }


        /// <summary>
        /// Get all mobile client ids that are logged into this DJ.
        /// </summary>
        /// <param name="venueID">The id of the venue/DJ.</param>
        /// <param name="clients">Out list of client IDs.</param>
        /// <returns>The otucome of the operation.</returns>
        public Response DJGetAssociatedClients(int venueID, out List<int> clients)
        {
            clients = new List<int>();
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Venue = @venueID;", con);
            cmd.Parameters.AddWithValue("@venueID", venueID);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(int.Parse(reader[0].ToString()));
                    }
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJGetAssociatedClients: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Get the password salt associated with a DJ.
        /// </summary>
        /// <param name="username">The DJ's username</param>
        /// <param name="salt">Out parameter for the salt.</param>
        /// <returns>The outcome of the oepration.</returns>
        public Response DJGetSalt(string username, out string salt)
        {
            salt = string.Empty;
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select Salt from DJUsers where Username = @username;", con);
            cmd.Parameters.AddWithValue("@username", username);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        salt = reader[0].ToString();
                        return r;
                    }
                    else
                    {
                        r.error = true;
                        r.message = "Error in DJGetSalt: Username could not be found";
                        return r;
                    }
                }
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJGetSalt: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Adds a temporary user to the databse.
        /// </summary>
        /// <param name="name">The temporary user's name.</param>
        /// <param name="DJID">The DJID of the DJ the user belongs to.</param>
        /// <returns>The outcome of the operation</returns>
        public Response DJAddTempUser(string name, int DJID)
        {
            SqlCommand cmd = new SqlCommand("insert into TempUsers (Name, Venue) Values (@name, @venue);");
            cmd.Parameters.AddWithValue("@name", name.Trim());
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Validates wether a temp user exists. If it does, ID is stored in message.
        /// </summary>
        /// <param name="name">The temporary user's name.</param>
        /// <param name="DJID">The DJID of the DJ the user belongs to.</param>
        /// <returns>The outcome of the operation</returns>
        public Response DJValidateTempUserName(string name, int DJID)
        {
            SqlCommand cmd = new SqlCommand("select ID from TempUsers where Name = @name and Venue = @venue;");
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBQuery(cmd, new string[1] { "ID" });
        }

        /// <summary>
        /// Get the user name of the temporary user.
        /// </summary>
        /// <param name="tempID">The temporary userID.</param>
        /// <param name="DJID">The DJID of the DJ the user belongs to.</param>
        /// <returns>The outcome of the operation</returns>
        public Response DJGetTempUserName(int tempID, int DJID)
        {
            SqlCommand cmd = new SqlCommand("select Name from TempUsers where ID = @tempID and Venue = @venue;");
            cmd.Parameters.AddWithValue("@tempID", tempID);
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBQuery(cmd, new string[1] { "Name" });
        }

        /// <summary>
        /// Removes the temp user from the database.
        /// </summary>
        /// <param name="tempID">The temporary userID.</param>
        /// <param name="DJID">The DJID of the DJ the user belongs to.</param>
        /// <returns>The outcome of the operation</returns>
        public Response DJRemoveTempUser(int tempID, int DJID)
        {
            SqlCommand cmd = new SqlCommand("Delete from TempUsers where ID = @tempID and Venue = @venue;");
            cmd.Parameters.AddWithValue("@tempID", tempID);
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Removes all temp users belonging the the given user from the databse.
        /// </summary>
        /// <param name="DJID">The DJID of the DJ the user belongs to.</param>
        /// <returns>The outcome of the operation</returns>
        public Response DJRemoveAllTempUsers(int DJID)
        {
            SqlCommand cmd = new SqlCommand("Delete from TempUsers where Venue = @venue;");
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Close a DJ's song requests.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJCloseSongRequests(int DJID)
        {
            SqlCommand cmd = new SqlCommand("delete from DJSongRequests where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Open a DJ's song requests. Clears out any existing song requests.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJOpenSongRequests(int DJID)
        {
            SqlCommand cmd = new SqlCommand("delete from DJSongRequests where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            Response r = DBNonQuery(cmd);
            if (r.error)
                return r;

            cmd = new SqlCommand("insert into DJSongRequests (ListDJID, List) Values (@DJID, '');");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Store a DJ's QR code.
        /// </summary>
        /// <param name="QR">The QR code.</param>
        /// <param name="DJID">The DJ's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJSetQR(string QR, int DJID)
        {
            SqlCommand cmd = new SqlCommand("update DJUsers set QR = @QR where ID = @DJID;");
            cmd.Parameters.AddWithValue("@QR", QR);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Get a DJ's QR code. If the DJ exists, QR is stored in message.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJGetQR(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select QR from DJUsers where ID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "QR" });
        }

        /// <summary>
        /// Set a DJ's status.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <param name="status">The status.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJSetStatus(int DJID, int status)
        {
            SqlCommand cmd = new SqlCommand("update DJUsers set Status = @status where ID = @DJID;");
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }
        
        /// <summary>
        /// Add songs to a DJ's library. If a song with a matching artist and title exists,
        /// the path on disk and duration are updated to the new values. Otherwise, a new
        /// song is added to the library.
        /// </summary>
        /// <param name="songs">List of songs to add to library</param>
        /// <param name="DJID">DJ unique identifier</param>
        /// <returns>Response encoding the sucess of the operation</returns>
        public Response DJAddSongsUpdatingDuplicates(List<Song> songs, int DJID)
        {
            Response r = new Response();
            r.result = 0;
            try
            {
                string cmdText = @"Merge DJSongs as target
                                            using (values(@pathOnDisk, @duration))
	                                            as source (PathOnDisk, Duration)
	                                            on target.Title = @title and target.Artist = @title and DJListID = @DJID
                                            when matched then
	                                            update set PathOnDisk = source.PathOnDisk, Duration = source.Duration
                                            when not matched then
	                                            insert (DJListID, Title, Artist, PathOnDisk, Duration)
	                                            values (@DJID, @title, @artist, @pathOnDisk, @duration);";
                SqlCommand cmd = new SqlCommand(cmdText, con);

                foreach (Song s in songs)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@DJID", DJID);
                    cmd.Parameters.AddWithValue("@title", s.title);
                    cmd.Parameters.AddWithValue("@artist", s.artist);
                    cmd.Parameters.AddWithValue("@pathOnDisk", s.pathOnDisk);
                    cmd.Parameters.AddWithValue("@duration", s.duration);
                    cmd.Connection = con;
                    r.result += cmd.ExecuteNonQuery();
                }
                return r;

            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in AddSongs: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Remove the given songs from the DJ's library.
        /// </summary>
        /// <param name="songs">The songs to add.</param>
        /// <param name="DJID">The DJ's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJRemoveSongs(List<Song> songs, int DJID)
        {
            int songsNotFound = 0;
            int songsRemoved = 0;
            Response r = new Response();
            foreach (Song s in songs)
            {
                SqlCommand cmd = new SqlCommand("delete from DJSongs where DJListID = @DJID and Title = @title and Artist = @artist and PathOnDisk = @pathOnDisk;");
                cmd.Parameters.AddWithValue("@DJID", DJID);
                cmd.Parameters.AddWithValue("@title", s.title);
                cmd.Parameters.AddWithValue("@artist", s.artist);
                cmd.Parameters.AddWithValue("@pathOnDisk", s.pathOnDisk);

                r = DBNonQuery(cmd);
                if (r.error)
                    return r;
                if (r.result == 0)
                    songsNotFound++;
                else
                    songsRemoved++;
            }

            if (songsNotFound > 0)
                r.message = "Warning: " + songsNotFound + " song(s) in the list were not found and thus were not removed";
            r.result = songsRemoved;
            return r;
        }

        /// <summary>
        /// List all of a DJ's songs.
        /// </summary>
        /// <param name="DJID">The DJ's ID.</param>
        /// <param name="songs">Out parameter that will store all the songs.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJListSongs(int DJID, out List<Song> songs)
        {
            Response r = new Response();
            songs = new List<Song>();
            SqlCommand cmd = new SqlCommand("select * from DJSongs where DJListID = @DJID;", con);
            cmd.Parameters.AddWithValue("@DJID", DJID);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Song song = new Song();
                        song.ID = int.Parse(reader["SongID"].ToString());
                        song.title = reader["Title"].ToString();
                        song.artist = reader["Artist"].ToString();
                        song.pathOnDisk = reader["PathOnDisk"].ToString();
                        song.duration = int.Parse(reader["Duration"].ToString());
                        songs.Add(song);
                    }
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJListSongs: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// List all the MobileUsers.
        /// </summary>
        /// <returns>Outcome of the operation with message set to describe the mobile users.</returns>
        public Response MobileListMembers()
        {
            SqlCommand cmd = new SqlCommand("select * from MobileUsers;");
            Response r = DBQuery(cmd, new string[4] { "ID", "Username", "Password", "Status" });
            r.message = r.message.Replace(Common.DELIMINATOR, ",");
            return r;
        }

        /// <summary>
        /// Check to see if a mobile username is valid. Mobile ID stored in message if valid.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileValidateUsername(string username)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Username = @username;");
            cmd.Parameters.AddWithValue("@username", username);
            return DBQuery(cmd, new string[1] { "ID" });
        }
        // Check to see if a DJ's username and password are valid.
        // If credentials are valid, returns the unique DJID in message.

        /// <summary>
        /// Check to see if mobile credentials are valid. If they are, mobile ID is stored in message.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The Password.</param>
        /// <returns>The outcome of the opeation.</returns>
        public Response MobileValidateUsernamePassword(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Username = @username and Password = @password;");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            return DBQuery(cmd, new string[1] { "ID" });
        }

        /// <summary>
        /// Get a mobile user's username.
        /// </summary>
        /// <param name="MobileID">The mobileID of the user.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileIDtoUsername(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Username from MobileUsers where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBQuery(cmd, new string[1] { "Username" });
        }

        /// <summary>
        /// Validate a mobileID. if it is valid, message is set to the status of the user.
        /// </summary>
        /// <param name="MobileID">The mobileID of the user.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileValidateID(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Status from MobileUsers where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBQuery(cmd, new string[1] { "Status" });
        }

        /// <summary>
        /// Get the status of a mobile user. If it exists, message is set to status.
        /// </summary>
        /// <param name="MobileID">The mobileID of the user.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetStatus(int MobileID)
        {
            return MobileValidateID(MobileID);
        }

        /// <summary>
        /// Inserts a mobile user into the table of users.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="email">The email of the user.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSignUp(string username, string password, string email, string salt)
        {
            SqlCommand cmd = new SqlCommand("insert into MobileUsers (Username, Password, Status, Email, DeviceID, Salt) Values (@username, @password, @status, @email, @deviceID, @salt);");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            cmd.Parameters.AddWithValue("@status", 0);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@deviceID", String.Empty);
            cmd.Parameters.AddWithValue("@salt", salt);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Get the password salt associated with the mobile user.
        /// </summary>
        /// <param name="username">The mobile username.</param>
        /// <param name="salt">Out parmater of the salt.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetSalt(string username, out string salt)
        {
            salt = string.Empty;
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select Salt from MobileUsers where Username = @username;", con);
            cmd.Parameters.AddWithValue("@username", username);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        salt = reader[0].ToString();
                        return r;
                    }
                    else
                    {
                        r.error = true;
                        r.message = "Error in MobileGetSalt: Username could not be found";
                        return r;
                    }
                }
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileGetSalt: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Sets the status of a mobile user.
        /// </summary>
        /// <param name="MobileID">The id of the mobile client.</param>
        /// <param name="status">The status to set.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetStatus(int MobileID, int status)
        {
            SqlCommand cmd = new SqlCommand("update MobileUsers set Status = @status where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@status", status);
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Signs a Mobile user out of the sytem. Update the deviceID to be empty and sets the status to be logged out.
        /// </summary>
        /// <param name="mobileID">The mobile client ID.</param>
        /// <returns>The success of the operation.</returns>
        public Response MobileSignOut(int mobileID)
        {
            Response r = new Response();
            try
            {
                SqlCommand cmd = new SqlCommand("update MobileUsers set DeviceID = '', Status = '0' where ID = @mobileID;", con);
                cmd.Parameters.AddWithValue("@mobileID", mobileID);
                r.result = cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileSignOut: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Signs a Mobile user into the sytem. Updates the deviceID and updates status to be logged in.
        /// </summary>
        /// <param name="mobileID">The mobile client ID.</param>
        /// <param name="deviceID">The device id of the phone<./param>
        /// <returns>Response indicating the success of the operation.</returns>
        public Response MobileSignIn(int mobileID, string deviceID)
        {
            Response r = new Response();
            try
            {
                SqlCommand cmd = new SqlCommand("update MobileUsers set DeviceID = @deviceID, Status = '1' where ID = @mobileID", con);
                cmd.Parameters.AddWithValue("@deviceID", deviceID);
                cmd.Parameters.AddWithValue("@mobileID", mobileID);
                r.result = cmd.ExecuteNonQuery();
                return r;

            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileSignIn: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Get the Device ID of a Mobile Client's phone.
        /// </summary>
        /// <param name="mobileID">The mobile client id.</param>
        /// <param name="deviceID">Outputs the device id of the phone.</param>
        /// <returns>Response indicating the success of the operation.</returns>
        public Response MobileGetDeviceID(int mobileID, out string deviceID)
        {
            deviceID = String.Empty;
            Response r = new Response();
            try
            {
                SqlCommand cmd = new SqlCommand("select DeviceID from MobileUsers where ID = @mobileID", con);
                cmd.Parameters.AddWithValue("@mobileID", mobileID);
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        deviceID = reader["DeviceID"].ToString(); ;
                        return r;
                    }
                    r.error = true;
                    r.message = "MobileID invalid in MobileGetDeviceID";
                    return r;
                }

            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileSetDeviceID: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Search for songs. If title or artist is blank, does not seach by that term.
        /// </summary>
        /// <param name="title">The title to seach by.</param>
        /// <param name="artist">The artist to search by.</param>
        /// <param name="DJID">The ID of the DJ whose library is being searched.</param>
        /// <param name="start">The index of the first result.</param>
        /// <param name="count">The number of results to return.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSearchSongs(string title, string artist, int DJID, int start, int count)
        {
            title = title.Trim();
            artist = artist.Trim();
            SqlCommand cmd = new SqlCommand("select A.* from DJSongs A inner join (select ROW_NUMBER() over(order by");
            if (title.Length > 0 && artist.Length == 0)
                cmd.CommandText += " Title";
            else
                cmd.CommandText += " Artist";

            cmd.CommandText += (") as 'RN', * from DJSongs where DJListID = @DJID");
            cmd.Parameters.AddWithValue("@DJID", DJID);

            if (title.Length > 0)
            {
                cmd.CommandText += " and Title like @title";
                cmd.Parameters.AddWithValue("@title", "%" + title + "%");
            }
            if (artist.Length > 0)
            {
                cmd.CommandText += " and Artist like @artist";
                cmd.Parameters.AddWithValue("@artist", "%" + artist + "%");
            }
            cmd.CommandText += ") B on A.SongID = B.SongID and B.rn between @start and @end;";
            cmd.Parameters.AddWithValue("@start", (start + 1));
            cmd.Parameters.AddWithValue("@end", (start + count));
            return DBQuery(cmd, new string[4] { "SongID", "Title", "Artist", "Duration" });
        }

        /// <summary>
        /// Browse through a DJ's songs. Depending on the value of isArtist, either the artist, or the title must start with firstLetter.
        /// </summary>
        /// <param name="firstLetter">The string to start matching on.</param>
        /// <param name="isArtist">Whether we are matching by artist, or title.</param>
        /// <param name="start">The index of the first result.</param>
        /// <param name="count">The number of results to return.</param>
        /// <param name="DJID">The ID of the DJ's whose library is being serached.</param>
        /// <returns>The outcome of the opearation.</returns>
        public Response MobileBrowseSongs(string firstLetter, bool isArtist, int start, int count, int DJID)
        {
            int length = firstLetter.Length;

            // select A.* from DJSongs A inner join (select ROW_NUMBER() over(order by SongID) as 'RN', * 
            // from DJSongs where DJListID = '1') B on A.SongID = B.SongID and B.rn between 7 and 10;
            SqlCommand cmd = new SqlCommand("select A.* from DJSongs A inner join (select ROW_NUMBER() over(order by ");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            if (isArtist)
                cmd.CommandText += "Artist";
            else
                cmd.CommandText += "Title";

            cmd.CommandText += (") as 'RN', * from DJSongs where DJListID = @DJID");

            if (isArtist && length > 0)
            {
                cmd.CommandText += " and LEFT([Artist], @length) = @firstLetter";
                cmd.Parameters.AddWithValue("@length", length);
                cmd.Parameters.AddWithValue("@firstLetter", firstLetter);
            }
            else if (length > 0)
            {
                cmd.CommandText += " and LEFT([Title], @length) = @firstLetter";
                cmd.Parameters.AddWithValue("@length", length);
                cmd.Parameters.AddWithValue("@firstLetter", firstLetter);
            }

            cmd.CommandText += ") B on A.SongID = B.SongID and B.rn between @start and @end;";
            cmd.Parameters.AddWithValue("@start", (start + 1));
            cmd.Parameters.AddWithValue("@end", (start + count));

            return DBQuery(cmd, new string[4] { "SongID", "Title", "Artist", "Duration" });
        }

        /// <summary>
        /// Get the ID of the venue who has the given QR code. If it exists, message is set to the ID.
        /// </summary>
        /// <param name="QR">The QR code.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response GetVenueIDByQR(string QR)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where QR = @QR;");
            cmd.Parameters.AddWithValue("@QR", QR);
            return DBQuery(cmd, new string[1] { "ID" });
        }

        /// <summary>
        /// Get the venue name. If the venue exists, name is stored in message.
        /// </summary>
        /// <param name="venueID">The venueID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response GetVenueName(int venueID)
        {
            SqlCommand cmd = new SqlCommand("select VenueName from DJUsers where ID = @ID;");
            cmd.Parameters.AddWithValue("@ID", venueID);
            return DBQuery(cmd, new string[1] { "Venuename" });
        }

        /// <summary>
        /// Get the venue address. If the venue exists, address is stored in message.
        /// </summary>
        /// <param name="venueID">The venueID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response GetVenueAddress(int venueID)
        {
            SqlCommand cmd = new SqlCommand("select VenueAddress from DJUsers where ID = @ID;");
            cmd.Parameters.AddWithValue("@ID", venueID);
            return DBQuery(cmd, new string[1] { "VenueAddress" });
        }

        /// <summary>
        /// Set a mobile user's venue.
        /// </summary>
        /// <param name="MobileID">The mobile client's ID.</param>
        /// <param name="Venue">The venueID, either an int, or null if no venue.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetVenue(int MobileID, object Venue)
        {
            SqlCommand cmd = new SqlCommand("update MobileUsers set Venue = @Venue where ID = @MobileID;");
            if (Venue == null)
                cmd.Parameters.AddWithValue("@Venue", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@Venue", Venue);
            cmd.Parameters.AddWithValue("@MobileID", MobileID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Get the venue a mobile client is associated with. Result in message.
        /// </summary>
        /// <param name="MobileID">The mobile ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetVenue(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Venue from MobileUsers where ID = @MobileID;");
            cmd.Parameters.AddWithValue("@MobileID", MobileID);
            return DBQuery(cmd, new string[1] { "Venue" });
        }

        /// <summary>
        /// Set a mobile user's unique key.
        /// </summary>
        /// <param name="MobileID">The mobile ID.</param>
        /// <param name="MobileKey">The mobile Key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetKey(int MobileID, object MobileKey)
        {
            SqlCommand cmd = new SqlCommand("update MobileUsers set KeyHash = @MobileKey where ID = @MobileID;");
            if (MobileKey == null)
                cmd.Parameters.AddWithValue("@MobileKey", DBNull.Value);
            else
                cmd.Parameters.AddWithValue("@MobileKey", MobileKey);
            cmd.Parameters.AddWithValue("@MobileID", MobileID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Get a mobile user's ID from his/her key.
        /// </summary>
        /// <param name="MobileKey">The mobile key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetIDFromKey(long MobileKey)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where KeyHash = @MobileKey;");
            cmd.Parameters.AddWithValue("@MobileKey", MobileKey);
            return DBQuery(cmd, new string[] { "ID" });
        }

        /// <summary>
        /// Get the song requests of a DJ. They are stored in the message.
        /// </summary>
        /// <param name="DJID">The DJID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response GetSongRequests(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select List from DJSongRequests where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "List" });
        }

        /// <summary>
        /// Set the song requests of a DJ. 
        /// </summary>
        /// <param name="DJID">The DJID.</param>
        /// <param name="requestString">The new song requests.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response SetSongRequests(int DJID, string requestString)
        {
            SqlCommand cmd = new SqlCommand("update DJSongRequests set List = @list where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@list", requestString);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Create a playlist. Returns the playlist ID in r.message.
        /// </summary>
        /// <param name="name">The name of the playlist.</param>
        /// <param name="venueID">The venueID to associate the playlist with.</param>
        /// <param name="userID">The mobile client's ID.</param>
        /// <param name="time">The time the playlist was created.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileCreatePlaylist(string name, int venueID, int userID, DateTime time)
        {
            SqlCommand cmd = new SqlCommand("insert into PlayLists (VenueID, MobileID, Name, Songs, DateCreated) Values (@venueID, @userID, @name, '', @time); select SCOPE_IDENTITY();");
            cmd.Parameters.AddWithValue("@venueID", venueID);
            cmd.Parameters.AddWithValue("@userID", userID);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@time", time);
            return DBScalar(cmd);
        }

        /// <summary>
        /// Delete a playlist.
        /// </summary>
        /// <param name="playListID">The playlist ID.</param>
        /// <param name="userID">The mobile client's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileDeletePlaylist(int playListID, int userID)
        {
            SqlCommand cmd = new SqlCommand("delete from PlayLists where ID = @playListID and MobileID = @userID;");
            cmd.Parameters.AddWithValue("@playListID", playListID);
            cmd.Parameters.AddWithValue("@userID", userID);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Get the venue from a playlist. Stored in message if exists.
        /// </summary>
        /// <param name="playListID">The playlist ID.</param>
        /// <param name="userID">The mobile client's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetVenueFromPlaylist(int playListID, int userID)
        {
            SqlCommand cmd = new SqlCommand("select VenueID from PlayLists where ID = @playListID and MobileID = @userID;");
            cmd.Parameters.AddWithValue("@playListID", playListID);
            cmd.Parameters.AddWithValue("@userID", userID);
            return DBQuery(cmd, new string[1] { "VenueID" });
        }

        /// <summary>
        /// Get the songs from a playlist. Songs saved in message.
        /// </summary>
        /// <param name="playListID">The playlist ID.</param>
        /// <param name="userID">The mobile client's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetSongsFromPlaylist(int playListID, int userID, out List<Song> songs)
        {
            Response r = new Response();
            songs = new List<Song>();
            SqlCommand cmd = new SqlCommand("select SongID from PlayListSongs where PlayListID = @playListID;", con);
            cmd.Parameters.AddWithValue("@playListID", playListID);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Song song = new Song();
                        song.ID = int.Parse(reader[0].ToString());
                        songs.Add(song);
                    }
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileGetSongsFromPlaylist: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Set the songs of a playlist.
        /// </summary>
        /// <param name="playListID">The playlist ID.</param>
        /// <param name="userID">The mobile client's ID.</param>
        /// <param name="songString">The songs in string form.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetPlaylistSongs(int playListID, int userID, List<Song> songs)
        {
            Response r = new Response();
            try
            {
                SqlCommand cmd = new SqlCommand("delete from PlayListSongs where PlayListID = @playListID;", con);
                cmd.Parameters.AddWithValue("@playListID", playListID);
                cmd.ExecuteNonQuery();

                cmd = new SqlCommand("insert into PlayListSongs (PlayListID, SongID) values (@playListID, @songID);", con);
                cmd.Parameters.AddWithValue("@playListID", playListID);

                foreach (Song s in songs)
                {
                    cmd.Parameters.AddWithValue("@songID", s.ID);
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.RemoveAt(1);
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileSignOut: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Get a client's playlists. Out playlists parameter has playlist id, name, datecreated, songsIDs and venueIDs set.
        /// Additional song and venue information is NOT set.
        /// </summary>
        /// <param name="venueID">The venueID, if set to -1, all venues are included.</param>
        /// <param name="userID">The mobile client's ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetPlaylists(int venueID, int userID, out List<Playlist> playlists)
        {
            Response r = new Response();
            playlists = new List<Playlist>();
            SqlCommand cmd = new SqlCommand("select ID, Name, DateCreated, VenueID from PlayLists where MobileID = @userID", con);
            cmd.Parameters.AddWithValue("@userID", userID);
            if (venueID != -1)
            {
                cmd.CommandText += " and VenueID = @venueID";
                cmd.Parameters.AddWithValue("@venueID", venueID);
            }
            cmd.CommandText += ";";

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Playlist p = new Playlist();
                        p.ID = int.Parse(reader["ID"].ToString());
                        p.name = reader["Name"].ToString();
                        p.dateCreated = Convert.ToDateTime(reader["DateCreated"].ToString());
                        p.venue = new Venue();
                        p.venue.venueID = int.Parse(reader["VenueID"].ToString());
                        playlists.Add(p);
                    }
                }

                foreach (Playlist p in playlists)
                {
                    List<Song> songs;
                    r = MobileGetSongsFromPlaylist(p.ID, userID, out songs);
                    if (r.error)
                        return r;
                    p.songs = songs;
                }

                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileGetSongsFromPlaylist: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Get the song history for a mobile user.
        /// </summary>
        /// <param name="userID">The userID.</param>
        /// <param name="start">The starting index.</param>
        /// <param name="count">The number of results.</param>
        /// <returns>The outcome of the opeation.</returns>
        public Response MobileGetSongHistory(int userID, int start, int count)
        {
            SqlCommand cmd = new SqlCommand("select A.* from MobileSongHistory A inner join (select ROW_NUMBER() over(order by DateSung DESC) as 'RN', * from MobileSongHistory where MobileID = @userID) B on A.ID = B.ID and B.rn between @start and @count order by DateSung DESC;");
            cmd.Parameters.AddWithValue("@userID", userID);
            cmd.Parameters.AddWithValue("@start", start + 1);
            cmd.Parameters.AddWithValue("@count", start + count);
            return DBQuery(cmd, new string[5] { "ID", "VenueID", "MobileID", "SongID", "DateSung" });
        }

        /// <summary>
        /// Add a song to a mobile user's song history.
        /// </summary>
        /// <param name="mobileID">The mobile ID.</param>
        /// <param name="venueID">The venue/DJID.</param>
        /// <param name="songID">The songID.</param>
        /// <param name="dateSung">The date of singing.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileAddSongHistory(int mobileID, int venueID, int songID, DateTime dateSung)
        {
            SqlCommand cmd = new SqlCommand("insert into MobileSongHistory (VenueID, MobileID, SongID, DateSung) values (@venueID, @mobileID, @songID, @dateSung);");
            cmd.Parameters.AddWithValue("@venueID", venueID);
            cmd.Parameters.AddWithValue("@mobileID", mobileID);
            cmd.Parameters.AddWithValue("@songID", songID);
            cmd.Parameters.AddWithValue("@dateSung", dateSung);
            return DBNonQuery(cmd);
        }

        /// <summary>
        /// Set the song rating of a song.
        /// </summary>
        /// <param name="mobileID">The mobile id of the client setting the rating.</param>
        /// <param name="songID">The songID.</param>
        /// <param name="rating">The rating.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSetSongRating(int mobileID, int songID, int rating)
        {
            Response r;
            SqlCommand cmd = new SqlCommand("select ID from MobileSongRatings where MobileID = @mobileID and SongID = @songID;");
            cmd.Parameters.AddWithValue("@mobileID", mobileID);
            cmd.Parameters.AddWithValue("@songID", songID);
            r = DBQuery(cmd, new string[1] { "ID" });
            if (r.error)
                return r;
            // We are adding a new song rating.
            if (r.message.Trim().Length == 0)
            {
                cmd = new SqlCommand("insert into MobileSongRatings (SongID, MobileID, Rating) values (@songID, @mobileID, @rating);");
                cmd.Parameters.AddWithValue("@songID", songID);
                cmd.Parameters.AddWithValue("@mobileID", mobileID);
                cmd.Parameters.AddWithValue("@rating", rating);
                return DBNonQuery(cmd);
            }
            // We are updating a new song rating.
            else
            {
                cmd = new SqlCommand("update MobileSongRatings set Rating = @rating where SongID = @songID and MobileID = @mobileID;");
                cmd.Parameters.AddWithValue("@rating", rating);
                cmd.Parameters.AddWithValue("@songID", songID);
                cmd.Parameters.AddWithValue("@mobileID", mobileID);
                return DBNonQuery(cmd);
            }
        }

        /// <summary>
        /// Get the song rating of a song.
        /// </summary>
        /// <param name="mobileID">The mobileID of the client.</param>
        /// <param name="songID">The songID.</param>
        /// <returns>The outcome of the oepration.</returns>
        public Response MobileGetSongRating(int mobileID, int songID)
        {
            SqlCommand cmd = new SqlCommand("select Rating from MobileSongRatings where MobileID = @mobileID and SongID = @songID;");
            cmd.Parameters.AddWithValue("@mobileID", mobileID);
            cmd.Parameters.AddWithValue("@songID", songID);
            return DBQuery(cmd, new string[1] { "Rating" });
        }

        /// <summary>
        /// Set a setting the the settings table in the databse.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The value of the setting.</param>
        /// <returns>The outcome of the opeartion.</returns>
        public Response SetSetting(string name, string value)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("delete from Settings where Name = @name ;", con);
            cmd.Parameters.AddWithValue("@name", name);
            SqlCommand cmd2 = new SqlCommand("insert into Settings (Name, Value) values (@name, @value);", con);
            cmd2.Parameters.AddWithValue("@name", name);
            cmd2.Parameters.AddWithValue("@value", value);

            try
            {
                cmd.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileGetSalt: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Set a setting in the Settings table in the databse.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">Out value of the setting.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response GetSetting(string name, out string value)
        {
            value = string.Empty;
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select value from Settings where Name = @name ;", con);
            cmd.Parameters.AddWithValue("@name", name);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        value = reader[0].ToString();
                        return r;
                    }
                    else
                    {
                        r.error = true;
                        r.message = "Error in MobileGetSalt: Username could not be found";
                        return r;
                    }
                }
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileGetSalt: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Get all the mobile usernames associated with this email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <param name="usernames">Out usernames.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileGetUsernamesByEmail(string email, out List<string> usernames)
        {
            usernames = new List<string>();
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select Username from MobileUsers where Email = @email ;", con);
            cmd.Parameters.AddWithValue("@email", email);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        usernames.Add(reader[0].ToString());
                    }
                    return r;
                }
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileGetUsernamesByEmail: " + e.Message;
                return r;
            }
        }

        /// <summary>
        /// Get all the DJ usernames associated with this email address.
        /// </summary>
        /// <param name="email">The email address.</param>
        /// <param name="usernames">Out usernames.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJGetUsernamesByEmail(string email, out List<string> usernames)
        {
            usernames = new List<string>();
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select Username from DJUsers where Email = @email ;", con);
            cmd.Parameters.AddWithValue("@email", email);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usernames.Add(reader[0].ToString());
                    }
                    return r;
                }
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJGetUsernamesByEmail: " + e.Message;
                return r;
            }
        }

        public Response DJValidateUsernameEmail(string username, string email, out int DJID)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where Email = @email and Username = @username ;", con);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@username", username);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DJID = int.Parse(reader[0].ToString());
                        return r;
                    }

                    DJID = -1;
                    return r;
                }
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJValidateUsernameEmail: " + e.Message;
                DJID = -1;
                return r;
            }
        }

        public Response MobileValidateUsernameEmail(string username, string email, out int mobileID)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Email = @email and Username = @username ;", con);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@username", username);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        mobileID = int.Parse(reader[0].ToString());
                        return r;
                    }

                    mobileID = -1;
                    return r;
                }
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileValidateUsernameEmail: " + e.Message;
                mobileID = -1;
                return r;
            }
        }

        public Response DJSetPasswordReset(int DJID, string value)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("delete from DJPasswordResets where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@ID", DJID);
            cmd.ExecuteNonQuery();

            SqlCommand cmd2 = new SqlCommand("insert into DJPasswordResets(ID, Value) values (@ID, @value);", con);
            cmd2.Parameters.AddWithValue("@ID", DJID);
            cmd2.Parameters.AddWithValue("@value", value);
            cmd2.ExecuteNonQuery();

            try
            {
                cmd.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJSetPasswordReset: " + e.Message;
                return r;
            }
        }

        public Response MobileSetPasswordReset(int mobileID, string value)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("delete from MobilePasswordResets where ID = @ID;", con);
            cmd.Parameters.AddWithValue("@ID", mobileID);
            cmd.ExecuteNonQuery();

            SqlCommand cmd2 = new SqlCommand("insert into MobilePasswordResets(ID, Value) values (@ID, @value);", con);
            cmd2.Parameters.AddWithValue("@ID", mobileID);
            cmd2.Parameters.AddWithValue("@value", value);
            cmd2.ExecuteNonQuery();

            try
            {
                cmd.ExecuteNonQuery();
                cmd2.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileSetPasswordReset: " + e.Message;
                return r;
            }
        }

        public Response DJGetPasswordResetID(string value, out int DJID)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select ID from DJPasswordResets where Value = @value;", con);
            cmd.Parameters.AddWithValue("@value", value);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        DJID = int.Parse(reader[0].ToString());
                    }
                    else
                    {
                        DJID = -1;
                    }
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJGetPasswordResetID: " + e.Message;
                DJID = -1;
                return r;
            }
        }

        public Response MobileGetPasswordResetID(string value, out int mobileID)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("select ID from MobilePasswordResets where Value = @value;", con);
            cmd.Parameters.AddWithValue("@value", value);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        mobileID = int.Parse(reader[0].ToString());
                    }
                    else
                    {
                        mobileID = -1;
                    }
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileGetPasswordResetID: " + e.Message;
                mobileID = -1;
                return r;
            }
        }

        public Response DJClearPasswordResetID(int DJID, string value)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("delete from DJPasswordResets where ID = @DJID or Value = @value;", con);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            cmd.Parameters.AddWithValue("@value", value);

            try
            {
                cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DJClearPasswordResetID: " + e.Message;
                return r;
            }
        }

        public Response MobileClearPasswordResetID(int mobileID, string value)
        {
            Response r = new Response();
            SqlCommand cmd = new SqlCommand("delete from MobilePasswordResets where ID = @mobileID or Value = @value;", con);
            cmd.Parameters.AddWithValue("@mobileID", mobileID);
            cmd.Parameters.AddWithValue("@value", value);

            try
            {
                cmd.ExecuteNonQuery();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileClearPasswordResetID: " + e.Message;
                return r;
            }
        }
    }
}