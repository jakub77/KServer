using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

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

        /// <summary>
        /// Open a connection to the DB.
        /// </summary>
        /// <returns></returns>
        public Response OpenConnection()
        {
            return new Response();
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            return;
        }


        /*
        // Execute the given command a a query to the database.
        // Return the values for any valid given columns.

        /// <summary>
        /// Execute the command as a Query.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="columns">The columns of information to return.</param>
        /// <returns>Outcome of attempt. If successful, the result is stored in response.message. 
        /// Newlines separates rows. Commas separate fields. Response.result stores the number of row.</returns>
        private Response DBQuery(string command, string[] columns)
        {
            Response r = new Response();
            try
            {
                r.result = 0;
                SqlDataReader reader = null;
                SqlCommand c = new SqlCommand(command, DBConnection);
                reader = c.ExecuteReader();
                while (reader.Read())
                {
                    r.result++;
                    for (int i = 0; i < columns.Length - 1; i++)
                        r.message += reader[columns[i]].ToString().Trim() + ",";
                    if (columns.Length > 0)
                        r.message += reader[columns[columns.Length - 1]].ToString().Trim();
                    r.message += "\n";                
                }
                reader.Close();
                return r;
            }
            catch (Exception e)
            {
                r.message = "Exception SQL_Query\n" + e.Message + "\n" + command;
                r.error = true;
                return r;
            }
        }

        /// <summary>
        /// Execute the command as a NonQuery.
        /// If successful, Response.result contains the number of rows effected.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <returns>Outcome of attempt.</returns>
        private Response DBNonQuery(string command)
        {
            Response r = new Response();
            int affectedRows = 0;
            try
            {
                SqlCommand c = new SqlCommand(command, DBConnection);
                affectedRows = c.ExecuteNonQuery();
                r.result = affectedRows;
                return r;
            }
            catch (Exception e)
            {
                r.message = "Exception SQL_NON_QUERY\n" + e.Message + "\n";
                r.message += command;
                r.error = true;
                return r;
            }
        }
        */

        private Response DBNonRead(SqlCommand cmd)
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
                r.message = "Exception in DBNonRead\n " + e.Message;
                    return r;
            }
        }

        private Response DBRead(SqlCommand cmd, string[] columns)
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
                                r.message += reader[columns[i]].ToString().Trim() + ",";
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
                r.message = "Exception in DBRead\n " + e.Message;
                return r;
            }
        }

        public Response SongExists(int DJID, int SongID)
        {
            SqlCommand cmd = new SqlCommand("select SongID from DJSongs where SongID = @songID and DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@songID", SongID);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBRead(cmd, new string[1] { "SongID" });
        }

        public Response SongInformation(int DJID, int SongID)
        {
            SqlCommand cmd = new SqlCommand("select Title, Artist, PathOnDisk from DJSongs where SongID = @songID and DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@songID", SongID);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBRead(cmd, new string[3] { "Title", "Artist", "PathOnDisk" });
        }

        #region DJStuff
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DJ STUFF
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////

        // List all DJ's and all direct information associated with them.
        // Returns the information in a response if no errors occured.
        public Response DJListMembers()
        {
            SqlCommand cmd = new SqlCommand("select * from DJUsers;");
            return DBRead(cmd, new string[4] { "ID", "Username", "Password", "Status" });
        }

        // Check to see if a DJ's username and password are valid.
        // If credentials are valid, returns the unique DJID in message.
        public Response DJValidateUsernamePassword(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where Username = @username and Password = @password;");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            return DBRead(cmd, new string[1] { "ID" });
        }

        // Check to see if a DJ's username is valid.
        // If username is valid, returns the unique DJID in message.
        public Response DJValidateUsername(string username)
        {
            SqlCommand cmd = new SqlCommand("select ID from DJUsers where Username = @username;");
            cmd.Parameters.AddWithValue("@username", username);
            return DBRead(cmd, new string[1] { "ID" });
        }

        public Response DJValidateDJID(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select Status from DJUsers where ID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBRead(cmd, new string[1] { "Status" });
        }

        // Get the current status of the given DJ.
        public Response DJGetStatus(int DJID)
        {
            return DJValidateDJID(DJID);
        }

        // Adds a new DJ to the system.
        // Returns whether it occured successfully.
        public Response DJSignUp(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("insert into DJUsers (Username, Password, SongListID, Status) Values (@username, @password, @songListID, @status);");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            cmd.Parameters.AddWithValue("@songListID", 0);
            cmd.Parameters.AddWithValue("@status", 0);
            return DBNonRead(cmd);
        }

        // Signs a DJ into the system. Return whether it occured successfully.
        public Response DJSignIn(int DJID)
        {
            SqlCommand cmd = new SqlCommand("update DJUsers set Status = '1' where ID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonRead(cmd);
        }

        // Sign a DJ out of the system. Return whether successful.
        public Response DJSignOut(int DJID)
        {
            SqlCommand cmd = new SqlCommand("update DJUsers set Status = '0' where ID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonRead(cmd);
        }

        /// <summary>
        /// Add songs to a DJ's library. If a song already exists in that library, it is not added.
        /// If the song exists in the library, but the path is different than the path supplied, the path in the library is updated.
        /// Returns the number of songs actually added in Response.result.
        /// </summary>
        /// <param name="songs">Songs to add</param>
        /// <param name="DJID">DJ's ID.</param>
        /// <returns></returns>
        public Response DJAddSongsIgnoringDuplicates(List<Song> songs, int DJID)
        {
            int songsAlreadyExisted = 0;
            int songPathsUpdated = 0;
            int songsAdded = 0;
            Response r = new Response();
            r.result = 0;
            foreach (Song s in songs)
            {
                // Get any song that matches exactly.
                SqlCommand cmd = new SqlCommand("select SongID from DJSongs where DJListID = @DJID and Title = @title and Artist = @artist and PathOnDisk = @pathOnDisk;");
                cmd.Parameters.AddWithValue("@DJID", DJID);
                cmd.Parameters.AddWithValue("@title", s.title);
                cmd.Parameters.AddWithValue("@artist", s.artist);
                cmd.Parameters.AddWithValue("@pathOnDisk", s.pathOnDisk);
                r = DBRead(cmd, new string[1] { "SongID" });
                if (r.error)
                    return r;

                // If a song matched exactly, no need to add it again/modify it.
                if (r.message.Trim() != string.Empty)
                {
                    songsAlreadyExisted++;
                    continue;
                }

                // Get any song that matches all criteria except path on disk.
                cmd = new SqlCommand("select SongID from DJSongs where DJListID = @DJID and Title = @title and Artist = @artist;");
                cmd.Parameters.AddWithValue("@DJID", DJID);
                cmd.Parameters.AddWithValue("@title", s.title);
                cmd.Parameters.AddWithValue("@artist", s.artist);
                r = DBRead(cmd, new string[1] { "SongID" });
                if (r.error)
                    return r;

                // If a song just has a differnt path on disk, update the path on disk.
                if (r.message.Trim() != string.Empty)
                {
                    cmd = new SqlCommand("update DJSongs set PathOnDisk = @pathOnDisk where DJListID = @DJID and Title = @title and Artist = @artist;");
                    cmd.Parameters.AddWithValue("@DJID", DJID);
                    cmd.Parameters.AddWithValue("@title", s.title);
                    cmd.Parameters.AddWithValue("@artist", s.artist);
                    cmd.Parameters.AddWithValue("@pathOnDisk", s.pathOnDisk);
                    r = DBNonRead(cmd);
                    if (r.error)
                        return r;
                    songPathsUpdated++;
                    continue;
                }

                // Otherwise, add the new song.
                cmd = new SqlCommand("insert into DJSongs (DJListID, Title, Artist, PathOnDisk) Values (@DJID, @title, @artist, @pathOnDisk);");
                cmd.Parameters.AddWithValue("@DJID", DJID);
                cmd.Parameters.AddWithValue("@title", s.title);
                cmd.Parameters.AddWithValue("@artist", s.artist);
                cmd.Parameters.AddWithValue("@pathOnDisk", s.pathOnDisk);
                r = DBNonRead(cmd);
                if (r.error)
                    return r;
                songsAdded++;
            }

            r.message = string.Empty;
            if (songsAlreadyExisted > 0)
                r.message += "Warning: " + songsAlreadyExisted + " song(s) were not added since they already existed\n";
            if (songPathsUpdated > 0)
                r.message += "Warning: " + songPathsUpdated + " song(s) were not added, but instead had pathOnDisk updated";
            r.result = songsAdded;
            return r;
        }

        // Remove songs form a DJ's library.
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
                r = DBNonRead(cmd);
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

        public Response DJListSongs(out List<Song> songs, int DJID)
        {
            Response r = new Response();
            songs = new List<Song>();
            SqlCommand cmd = new SqlCommand("select * from DJSongs where DJListID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            r = DBRead(cmd, new string[4] { "SongID", "Title", "Artist", "PathOnDisk" });
            if (r.error)
                return r;

            if (r.message.Trim() == string.Empty)
            {
                r.message = "Warning: No songs were found";
                return r;
            }

            try
            {
                string[] songLines = r.message.Trim().Split('\n');
                foreach (string songLine in songLines)
                {
                    string[] songParts = songLine.Split(',');
                    Song song = new Song();
                    int id;
                    if (!int.TryParse(songParts[0], out id))
                    {
                        r.error = true;
                        r.message = "Exception in SongListSQL: could not parse song id";
                        return r;
                    }
                    song.ID = id;
                    song.title = songParts[1];
                    song.artist = songParts[2];
                    song.pathOnDisk = songParts[3];
                    songs.Add(song);
                }
                r.message = "";
                return r;
            }
            catch (Exception e)
            {

                r.message = "Exception " + e.ToString();
                r.error = true;
                return r;
            }
        }

        #endregion
        #region mobileStuff

        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CLIENT STUFF
        /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////

        // List all DJ's and all direct information associated with them.
        // Returns the information in a response if no errors occured.
        public Response MobileListMembers()
        {
            SqlCommand cmd = new SqlCommand("select * from MobileUsers;");
            return DBRead(cmd, new string[4] { "ID", "Username", "Password", "Status" });
        }

        // Check to see if a mobile username is valid.
        // If username is valid, returns the unique DJID in message.
        public Response MobileValidateUsername(string username)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Username = @username;");
            cmd.Parameters.AddWithValue("@username", username);
            return DBRead(cmd, new string[1] { "ID" });
        }

        // Check to see if a DJ's username and password are valid.
        // If credentials are valid, returns the unique DJID in message.
        public Response MobileValidateUsernamePassword(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("select ID from MobileUsers where Username = @username and Password = @password;");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            return DBRead(cmd, new string[1] { "ID" });
        }

        public Response MobileIDtoUsername(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Username from MobileUsers where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBRead(cmd, new string[1] { "Username" });
        }

        public Response MobileValidateID(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("select Status from MobileUsers where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBRead(cmd, new string[1] { "Status" });
        }

        // Get the current status of the given mobile client.
        public Response MobileGetStatus(int MobileID)
        {
            return MobileValidateID(MobileID);
        }

        // Adds a new mobile client user to the DB.
        // Returns whether it occured successfully.
        public Response MobileSignUp(string username, string password)
        {
            SqlCommand cmd = new SqlCommand("insert into MobileUsers (Username, Password, Status) Values (@username, @password, @status);");
            cmd.Parameters.AddWithValue("@username", username.Trim());
            cmd.Parameters.AddWithValue("@password", password.Trim());
            cmd.Parameters.AddWithValue("@status", 0);
            return DBNonRead(cmd);
        }

        // Signs a mobile client into the system. Return whether it occured successfully.
        public Response MobileSignIn(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("update MobileUsers set Status = '1' where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBNonRead(cmd);
        }

        // Sign a client out of the system. Return whether successful.
        public Response MobileSignOut(int MobileID)
        {
            SqlCommand cmd = new SqlCommand("update MobileUsers set Status = '0' where ID = @mobileID;");
            cmd.Parameters.AddWithValue("@mobileID", MobileID);
            return DBNonRead(cmd);
        }

        public Response MobileSearchSongs(out List<Song> songs, string title, string artist, int DJID)
        {
            Response r = new Response();
            songs = new List<Song>();
            try
            {
                SqlCommand cmd = new SqlCommand("select * from DJSongs where DJListID = @DJID");
                cmd.Parameters.AddWithValue("@DJID", DJID);
                if (title.Trim().Length > 0)
                {
                    cmd.CommandText += " and Title = @title";
                    cmd.Parameters.AddWithValue("@title", title.Trim());
                }
                if (artist.Trim().Length > 0)
                {
                    cmd.CommandText += " and Artist = @artist";
                    cmd.Parameters.AddWithValue("@artist", artist.Trim());
                }
                cmd.CommandText += ";";

                r = DBRead(cmd, new string[3] { "SongID", "Title", "Artist" });

                if (r.error)
                    return r;

                if (r.message.Trim() == string.Empty)
                {
                    r.result = 0;
                    return r;
                }

                int count = 0;
                string[] songLines = r.message.Trim().Split('\n');
                foreach (string songLine in songLines)
                {
                    string[] songParts = songLine.Split(',');
                    Song song = new Song();
                    int id;
                    if (!int.TryParse(songParts[0], out id))
                    {
                        r.error = true;
                        r.message = "Exception in MobileListSongsSQL: could not parse song id";
                        return r;
                    }
                    song.ID = id;
                    song.title = songParts[1];
                    song.artist = songParts[2];
                    song.pathOnDisk = "";
                    songs.Add(song);
                    count++;
                }
                r.message = "";
                r.result = count;
                return r;
            }
            catch (Exception e)
            {
                r.message = "Exception " + e.Message;
                r.error = true;
                return r;
            }
        }

        public Response MobileBrowseSongs(out List<Song> songs, string firstLetter, bool isArtist, int start, int count, int DJID)
        {
            Response r = new Response();
            songs = new List<Song>();
            int length = firstLetter.Length;
            try
            {
                // select A.* from DJSongs A inner join (select ROW_NUMBER() over(order by SongID) as 'RN', * 
                // from DJSongs where DJListID = '1') B on A.SongID = B.SongID and B.rn between 7 and 10;
                SqlCommand cmd = new SqlCommand("select A.* from DJSongs A inner join (select ROW_NUMBER() over(order by SongID) as 'RN', * from DJSongs where DJListID = @DJID");
                cmd.Parameters.AddWithValue("@DJID", DJID);

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
                
                r = DBRead(cmd, new string[3] { "SongID", "Title", "Artist" });

                if (r.error)
                    return r;

                if (r.message.Trim() == string.Empty)
                {
                    r.result = 0;
                    return r;
                }

                int count2 = 0;
                string[] songLines = r.message.Trim().Split('\n');
                foreach (string songLine in songLines)
                {
                    string[] songParts = songLine.Split(',');
                    Song song = new Song();
                    int id;
                    if (!int.TryParse(songParts[0], out id))
                    {
                        r.error = true;
                        r.message = "Exception in MobileListSongsSQL: could not parse song id";
                        return r;
                    }
                    song.ID = id;
                    song.title = songParts[1];
                    song.artist = songParts[2];
                    song.pathOnDisk = "";
                    songs.Add(song);
                    count2++;
                }
                r.message = "";
                r.result = count2;
                return r;
            }
            catch (Exception e)
            {
                r.message = "Exception in MobileSongBrowse " + e.ToString();
                r.error = true;
                return r;
            }
        }

        public Response GetSongRequests(int DJID)
        {
            SqlCommand cmd = new SqlCommand("select List from DJSongRequests where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBRead(cmd, new string[1] { "List" });
        }

        public Response SetSongRequests(int DJID, string requestString)
        {
            SqlCommand cmd = new SqlCommand("update DJSongRequests set List = @list where ListDJID = @DJID;");
            cmd.Parameters.AddWithValue("@list", requestString);
            cmd.Parameters.AddWithValue("@DJID", DJID);
            return DBNonRead(cmd);
        }


        #endregion
    }
}