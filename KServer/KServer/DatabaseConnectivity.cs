﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Diagnostics;
//Adding songs increase speed. Combine queries into one. Use a single connections instead of opening/closing.

namespace KServer
{
    public class DatabaseConnectivity : IDisposable
    {
        private string SQLServerAddress = "localhost";
        private string DBUsername = "karaoke";
        private string DBPassword = "topsecret";
        private string database = "KaraokeDB";
        private int connectionTimeOut = 10;
        private string connectionString = String.Empty;

        public DatabaseConnectivity()
        {
            connectionString = string.Empty;
            connectionString += "user id=" + DBUsername + ";";
            connectionString += "pwd=" + DBPassword + ";";
            connectionString += "server=" + SQLServerAddress + ";";
            connectionString += "database=" + database + ";";
            connectionString += "connection timeout=" + connectionTimeOut + ";";
        }
        void IDisposable.Dispose()
        {
            return;
        }

        private Response DBNonQuery(SqlCommand cmd)
        {
            Response r = new Response();
            int affectedRows = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    cmd.Connection = con;
                    affectedRows = cmd.ExecuteNonQuery();
                    r.result = affectedRows;
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DBNonQuery\n " + e.Message + e.StackTrace;
                return r;
            }
        }
        private Response DBScalar(SqlCommand cmd)
        {
            Response r = new Response();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    cmd.Connection = con;
                    var v = cmd.ExecuteScalar();
                    r.result = int.Parse(v.ToString());
                }
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in DBScalar\n " + e.Message + e.StackTrace;
                return r;
            }
        }
        private Response DBQuery(SqlCommand cmd, string[] columns)
        {
            Response r = new Response();
            r.result = 0;
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    cmd.Connection = con;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            r.result++;
                            for (int i = 0; i < columns.Length - 1; i++)
                                r.message += reader[columns[i]].ToString().Trim() + Common.deliminator;
                            if (columns.Length > 0)
                                r.message += reader[columns[columns.Length - 1]].ToString().Trim();
                            r.message += "\n";
                        }
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

        public Response SongExists(int DJID, int SongID)
        {
            SqlCommand cmd = new SqlCommand("select SongID from DJSongs where SongID = @songID and DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@songID", SongID);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "SongID" });
        }
        public Response SongInformation(int DJID, int SongID)
        {
            SqlCommand cmd = new SqlCommand("select Title, Artist, PathOnDisk, Duration from DJSongs where SongID = @songID and DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@songID", SongID);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[4] { "Title", "Artist", "PathOnDisk", "Duration" });
        }


        public Response DJListMembers()
        {
            SqlCommand cmd = new SqlCommand("select * from DJUsers;");
            Response r = DBQuery(cmd, new string[4] { "ID", "Username", "Password", "Status" });
            r.message = r.message.Replace(Common.deliminator, ",");
            return r;
        }
        // Check to see if a DJ's username and password are valid.
        // If credentials are valid, returns the unique DJID in message.
        public Response DJValidateUsernamePassword(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where Username = @username and Password = @password;");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            return DBQuery(cmd, new string[1] { "ID" });
        }
        // Check to see if a DJ's username is valid.
        // If username is valid, returns the unique DJID in message.
        public Response DJValidateUsername(string username)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where Username = @username;");
            cmd.Parameters.AddWithValue("@username", username);
            return DBQuery(cmd, new string[1] { "ID" });
        }
        public Response DJValidateDJID(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select Status from DJUsers where ID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "Status" });
        }
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
        public Response DJGetIDFromKey(long DJKey)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where KeyHash = @DJKey;");
            cmd.Parameters.AddWithValue("@DJKey", DJKey);
            return DBQuery(cmd, new string[] { "ID" });
        }
        public Response DJGetStatus(int DJID)
        {
            return DJValidateDJID(DJID);
        }
        // Adds a new DJ to the system.
        // Returns whether it occured successfully.
        public Response DJSignUp(string username, string password, string email, string venueName, string venueAddress)
        {
            SqlCommand cmd = new SqlCommand("insert into DJUsers (Username, Password, Status, QR, Email, VenueName, VenueAddress)");
            cmd.CommandText += "Values (@username, @password, @status, @QR, @email, @venueName, @venueAddress);";
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            cmd.Parameters.AddWithValue("@status", 0);
            cmd.Parameters.AddWithValue("@QR", "");
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@venueName", venueName);
            cmd.Parameters.AddWithValue("@venueAddress", venueAddress);
            return DBNonQuery(cmd);
        }
        public Response DJAddTempUser(string name, int DJID)
        {
            SqlCommand cmd = new SqlCommand("insert into TempUsers (Name, Venue) Values (@name, @venue);");
            cmd.Parameters.AddWithValue("@name", name.Trim());
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBNonQuery(cmd);
        }
        public Response DJValidateTempUserName(string name, int DJID)
        {
            SqlCommand cmd = new SqlCommand("select ID from TempUsers where Name = @name and Venue = @venue;");
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBQuery(cmd, new string[1] { "ID" });
        }
        public Response DJGetTempUserName(int tempID, int DJID)
        {
            SqlCommand cmd = new SqlCommand("select Name from TempUsers where ID = @tempID and Venue = @venue;");
            cmd.Parameters.AddWithValue("@tempID", tempID);
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBQuery(cmd, new string[1] { "Name" });
        }
        public Response DJRemoveTempUser(int tempID, int DJID)
        {
            SqlCommand cmd = new SqlCommand("Delete from TempUsers where ID = @tempID and Venue = @venue;");
            cmd.Parameters.AddWithValue("@tempID", tempID);
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBNonQuery(cmd);
        }
        public Response DJRemoveAllTempUsers(int DJID)
        {
            SqlCommand cmd = new SqlCommand("Delete from TempUsers where Venue = @venue;");
            cmd.Parameters.AddWithValue("@venue", DJID);
            return DBNonQuery(cmd);
        }
        public Response DJCloseSongRequests(int DJID)
        {
            SqlCommand cmd = new SqlCommand("delete from DJSongRequests where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }
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
        public Response DJSetQR(string QR, int DJID)
        {
            SqlCommand cmd = new SqlCommand("update DJUsers set QR = @QR where ID = @DJID;");
            cmd.Parameters.AddWithValue("@QR", QR);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }
        public Response DJGetQR(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select QR from DJUsers where ID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "QR" });
        }
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
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
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in AddSongs: " + e.Message;
                return r;
            }
        }
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

        public Response DJListSongs(int DJID , out List<Song> songs)
        {
            Response r = new Response();
            songs = new List<Song>();
            SqlCommand cmd = new SqlCommand("select * from DJSongs where DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    cmd.Connection = con;
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

        public Response MobileListMembers()
        {
            SqlCommand cmd = new SqlCommand("select * from MobileUsers;");
            Response r = DBQuery(cmd, new string[4] { "ID", "Username", "Password", "Status" });
            r.message = r.message.Replace(Common.deliminator, ",");
            return r;
        }
        // Check to see if a mobile username is valid.
        // If username is valid, returns the unique DJID in message.
        public Response MobileValidateUsername(string username)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Username = @username;");
            cmd.Parameters.AddWithValue("@username", username);
            return DBQuery(cmd, new string[1] { "ID" });
        }
        // Check to see if a DJ's username and password are valid.
        // If credentials are valid, returns the unique DJID in message.
        public Response MobileValidateUsernamePassword(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Username = @username and Password = @password;");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            return DBQuery(cmd, new string[1] { "ID" });
        }
        public Response MobileIDtoUsername(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Username from MobileUsers where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBQuery(cmd, new string[1] { "Username" });
        }
        public Response MobileValidateID(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Status from MobileUsers where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBQuery(cmd, new string[1] { "Status" });
        }
        public Response MobileGetStatus(int MobileID)
        {
            return MobileValidateID(MobileID);
        }
        // Adds a new mobile client user to the DB.
        // Returns whether it occured successfully.
        public Response MobileSignUp(string username, string password, string email)
        {
            SqlCommand cmd = new SqlCommand("insert into MobileUsers (Username, Password, Status, Email, DeviceID) Values (@username, @password, @status, @email, @deviceID);");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            cmd.Parameters.AddWithValue("@status", 0);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@deviceID", String.Empty);
            return DBNonQuery(cmd);
        }

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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("update MobileUsers set DeviceID = '', Status = '0' where ID = @mobileID;", con);
                    cmd.Parameters.AddWithValue("@mobileID", mobileID);
                    r.result = cmd.ExecuteNonQuery();
                    return r;
                }
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("update MobileUsers set DeviceID = @deviceID, Status = '1' where ID = @mobileID", con);
                    cmd.Parameters.AddWithValue("@deviceID", deviceID);
                    cmd.Parameters.AddWithValue("@mobileID", mobileID);
                    r.result = cmd.ExecuteNonQuery();
                    return r;
                }
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
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
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
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception in MobileSetDeviceID: " + e.Message;
                return r;
            }

        }

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
        public Response GetVenueIDByQR(string QR)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where QR = @QR;");
            cmd.Parameters.AddWithValue("@QR", QR);
            return DBQuery(cmd, new string[1] { "ID" });
        }
        public Response GetVenueName(int venueID)
        {
            SqlCommand cmd = new SqlCommand("select VenueName from DJUsers where ID = @ID;");
            cmd.Parameters.AddWithValue("@ID", venueID);
            return DBQuery(cmd, new string[1] { "Venuename" });
        }
        public Response GetVenueAddress(int venueID)
        {
            SqlCommand cmd = new SqlCommand("select VenueAddress from DJUsers where ID = @ID;");
            cmd.Parameters.AddWithValue("@ID", venueID);
            return DBQuery(cmd, new string[1] { "VenueAddress" });
        }
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
        public Response MobileGetVenue(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Venue from MobileUsers where ID = @MobileID;");
            cmd.Parameters.AddWithValue("@MobileID", MobileID);
            return DBQuery(cmd, new string[1] { "Venue" });
        }
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
        public Response MobileGetIDFromKey(long MobileKey)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where KeyHash = @MobileKey;");
            cmd.Parameters.AddWithValue("@MobileKey", MobileKey);
            return DBQuery(cmd, new string[] { "ID" });
        }
        public Response GetSongRequests(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select List from DJSongRequests where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBQuery(cmd, new string[1] { "List" });
        }
        public Response SetSongRequests(int DJID, string requestString)
        {
            SqlCommand cmd = new SqlCommand("update DJSongRequests set List = @list where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@list", requestString);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonQuery(cmd);
        }
        public Response MobileCreatePlaylist(string name, int venueID, int userID, DateTime time)
        {
            SqlCommand cmd = new SqlCommand("insert into PlayLists (VenueID, MobileID, Name, Songs, DateCreated) Values (@venueID, @userID, @name, '', @time); select SCOPE_IDENTITY();");
            cmd.Parameters.AddWithValue("@venueID", venueID);
            cmd.Parameters.AddWithValue("@userID", userID);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@time", time);
            return DBScalar(cmd);
        }
        public Response MobileDeletePlaylist(int playListID, int userID)
        {
            SqlCommand cmd = new SqlCommand("delete from PlayLists where ID = @playListID and MobileID = @userID;");
            cmd.Parameters.AddWithValue("@playListID", playListID);
            cmd.Parameters.AddWithValue("@userID", userID);
            return DBNonQuery(cmd);
        }
        public Response MobileGetVenueFromPlaylist(int playListID, int userID)
        {
            SqlCommand cmd = new SqlCommand("select VenueID from PlayLists where ID = @playListID and MobileID = @userID;");
            cmd.Parameters.AddWithValue("@playListID", playListID);
            cmd.Parameters.AddWithValue("@userID", userID);
            return DBQuery(cmd, new string[1] { "VenueID" });
        }
        public Response MobileGetSongsFromPlaylist(int playListID, int userID)
        {
            SqlCommand cmd = new SqlCommand("select Songs from PlayLists where ID = @playListID and MobileID = @userID;");
            cmd.Parameters.AddWithValue("@playListID", playListID);
            cmd.Parameters.AddWithValue("@userID", userID);
            return DBQuery(cmd, new string[1] { "Songs" });
        }
        public Response MobileSetPlaylistSongs(int playListID, int userID, string songString)
        {
            SqlCommand cmd = new SqlCommand("update PlayLists set Songs = @songs where ID = @playListID and MobileID = @userID;");
            cmd.Parameters.AddWithValue("@songs", songString);
            cmd.Parameters.AddWithValue("@playListID", playListID);
            cmd.Parameters.AddWithValue("@userID", userID);
            return DBNonQuery(cmd);
        }
        public Response MobileGetPlaylists(int venueID, int userID)
        {
            SqlCommand cmd = new SqlCommand("select ID, Name, Songs, DateCreated, VenueID from PlayLists where MobileID = @userID");
            cmd.Parameters.AddWithValue("@userID", userID);
            if (venueID != -1)
            {
                cmd.CommandText += " and VenueID = @venueID";
                cmd.Parameters.AddWithValue("@venueID", venueID);
            }
            cmd.CommandText += ";";
            return DBQuery(cmd, new string[5] { "ID", "Name", "Songs", "DateCreated", "VenueID" });
        }
        public Response MobileGetSongHistory(int userID, int start, int count)
        {
            SqlCommand cmd = new SqlCommand("select A.* from MobileSongHistory A inner join (select ROW_NUMBER() over(order by DateSung DESC) as 'RN', * from MobileSongHistory where MobileID = @userID) B on A.ID = B.ID and B.rn between @start and @count order by DateSung DESC;");
            cmd.Parameters.AddWithValue("@userID", userID);
            cmd.Parameters.AddWithValue("@start", start + 1);
            cmd.Parameters.AddWithValue("@count", start + count);
            return DBQuery(cmd, new string[5] { "ID", "VenueID", "MobileID", "SongID", "DateSung" });
        }
        public Response MobileAddSongHistory(int mobileID, int venueID, int songID, DateTime dateSung)
        {
            SqlCommand cmd = new SqlCommand("insert into MobileSongHistory (VenueID, MobileID, SongID, DateSung) values (@venueID, @mobileID, @songID, @dateSung);");
            cmd.Parameters.AddWithValue("@venueID", venueID);
            cmd.Parameters.AddWithValue("@mobileID", mobileID);
            cmd.Parameters.AddWithValue("@songID", songID);
            cmd.Parameters.AddWithValue("@dateSung", dateSung);
            return DBNonQuery(cmd);
        }
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
        public Response MobileGetSongRating(int mobileID, int songID)
        {
            SqlCommand cmd = new SqlCommand("select Rating from MobileSongRatings where MobileID = @mobileID and SongID = @songID;");
            cmd.Parameters.AddWithValue("@mobileID", mobileID);
            cmd.Parameters.AddWithValue("@songID", songID);
            return DBQuery(cmd, new string[1] { "Rating" });
        }
    }
}