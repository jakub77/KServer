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

        SqlConnection DBConnection = null;

        /// <summary>
        /// Open a connection to the DB.
        /// </summary>
        /// <returns></returns>
        public Response OpenConnection()
        {
            Response r = new Response();
            try
            {
                string connectionString = string.Empty;
                connectionString += "user id=" + DBUsername + ";";
                connectionString += "pwd=" + DBPassword + ";";
                connectionString += "server=" + SQLServerAddress + ";";
                connectionString += "database=" + database + ";";
                connectionString += "connection timeout=" + connectionTimeOut + ";";
                DBConnection = new SqlConnection(connectionString);
                DBConnection.Open();
                return r;
            }
            catch (Exception e)
            {
                r.error = true;
                r.message = "Exception SQL: Could not open a connection to the DB\n" + e.Message;
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
                r.message = "Exception SQL_NON_QUERY\n" + e.Message;
                r.error = true;
                return r;
            }
        }

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
                r.message = "Exception SQL_Query\n" + e.Message;
                r.error = true;
                return r;
            }
        }

        // List all DJ's and all direct information associated with them.
        // Returns the information in a response if no errors occured.
        public Response DJListMembers()
        {
            string command;
            command = "select * from DJUsers;";
            string[] columns = new string[4] { "ID", "Username", "Password", "Status" };
            return DBQuery(command, columns);
        }

        // Check to see if a DJ's username and password are valid.
        // If credentials are valid, returns the unique DJID in message.
        public Response DJValidateUsernamePassword(string username, string password)
        {
            string command;
            command = "select ID from DJUsers where ";
            command += "Username = '" + username.Trim() + "' and ";
            command += "Password = '" + password.Trim() + "';";
            string[] columns = new string[1] { "ID" };
            return DBQuery(command, columns);
        }

        // Check to see if a DJ's username is valid.
        // If username is valid, returns the unique DJID in message.
        public Response DJValidateUsername(string username)
        {
            string command;
            command = "select ID from DJUsers where ";
            command += "Username = '" + username.Trim() + "';";
            string[] columns = new string[1] { "ID" };
            return DBQuery(command, columns);
        }

        public Response DJValidateDJID(int DJID)
        {
            string command;
            command = "select Status from DJUsers where ";
            command += "ID = '" + DJID.ToString() + "';";
            string[] columns = new string[1] { "Status" };
            return DBQuery(command, columns);
        }

        // Get the current status of the given DJ.
        public Response DJGetStatus(int DJID)
        {
            string command;
            command = "select Status from DJUsers where ";
            command += "ID = '" + DJID.ToString() + "';";
            string[] columns = new string[1] { "Status" };
            return DBQuery(command, columns);

        }

        // Adds a new DJ to the system.
        // Returns whether it occured successfully.
        public Response DJSignUp(string username, string password)
        {
            string command;
            command = "insert into DJUsers (Username, Password, SongListID, Status) Values (";
            command += "'" + username.Trim() + "',";
            command += "'" + password.Trim() + "',";
            command += "'" + "7" + "',";
            command += "'" + "0" + "'";
            command += ");";
            return DBNonQuery(command);
        }

        // Signs a DJ into the system. Return whether it occured successfully.
        public Response DJSignIn(int DJID)
        {
            string command;
            command = "update DJUsers set Status = '1' where ";
            command += "ID = '" + DJID.ToString() + "';";
            return DBNonQuery(command);
        }

        // Sign a DJ out of the system. Return whether successful.
        public Response DJSignOut(int DJID)
        {
            string command;
            command = "update DJUsers set Status = '0' where ";
            command += "ID = '" + DJID.ToString() + "';";
            return DBNonQuery(command);
        }

        /// <summary>
        /// Add songs to a DJ's library. If a song already exists in that library, it is not added.
        /// Returns the number of songs actually added in Response.result.
        /// </summary>
        /// <param name="songs">Songs to add</param>
        /// <param name="DJID">DJ's ID.</param>
        /// <returns></returns>
        public Response DJAddSongsIgnoringDuplicates(List<Song> songs, int DJID)
        {
            bool songAlreadyExisted = false;
            int songsAdded = 0;
            Response r = new Response();
            r.result = 0;
            foreach (Song s in songs)
            {
                string command;
                command = "select SongID from DJSongs where ";
                command += "DJListID = '" + DJID.ToString() + "'";
                command += "and Title = '" + s.title + "'";
                command += "and Artist = '" + s.artist + "'";
                command += "and PathOnDisk = '" + s.pathOnDisk + "'";
                command += ";";
                string[] columns = new string[1] { "SongID" };
                r = DBQuery(command, columns);
                if (r.error)
                    return r;

                if (r.message.Trim() != string.Empty)
                {
                    songAlreadyExisted = true;
                    continue;
                }

                command = "insert into DJSongs (DJListID, Title, Artist, PathOnDisk) Values (";
                command += "'" + DJID.ToString() + "',";
                command += "'" + s.title + "',";
                command += "'" + s.artist + "',";
                command += "'" + s.pathOnDisk + "'";
                command += ");";
                r = DBNonQuery(command);
                if (r.error)
                    return r;
                songsAdded++;
            }
            if (songAlreadyExisted)
                r.message = "Warning: Song(s) were not added since they already existed";
            r.result = songsAdded;
            return r;
        }

        // Remove songs form a DJ's library.
        public Response DJRemoveSongs(List<Song> songs, int DJID)
        {
            bool songNotFound = false;
            int songsRemoved = 0;
            Response r = new Response();
            foreach (Song s in songs)
            {
                string command;
                command = "delete from DJSongs where ";
                command += "DJListID = '" + DJID.ToString() + "'";
                command += "and Title = '" + s.title + "'";
                command += "and Artist = '" + s.artist + "'";
                command += "and PathOnDisk = '" + s.pathOnDisk + "'";
                command += ";";
                r = DBNonQuery(command);
                if (r.error)
                    return r;
                if (r.result == 0)
                    songNotFound = true;
                else
                    songsRemoved++;
            }
            if(songNotFound)
                r.message = "Warning: Song(s) in the list were not found";
            r.result = songsRemoved;
            return r;
        }

        public Response DJListSongs(out List<Song> songs, int DJID)
        {
            Response r = new Response();
            songs = null;
            try
            {
                songs = new List<Song>();
                string command;
                command = "select * from DJSongs where ";
                command += "DJListID = '" + DJID.ToString() + "';";
                string[] columns = new string[3] { "Title", "Artist", "PathOnDisk" };
                r = DBQuery(command, columns);
                if (r.error)
                    return r;

                if (r.message.Trim() == string.Empty)
                    return r;

                string[] songLines = r.message.Trim().Split('\n');
                foreach (string songLine in songLines)
                {
                    string[] songParts = songLine.Split(',');
                    Song song = new Song();
                    song.title = songParts[0];
                    song.artist = songParts[1];
                    song.pathOnDisk = songParts[2];
                    songs.Add(song);
                }
                r.message = "";
                return r;
            }
            catch (Exception e)
            {

                r.message = "Exception " + e.Message;
                r.error = true;
                return r;
            }
        }

        /// <summary>
        /// Dispose resources.
        /// </summary>
        void IDisposable.Dispose()
        {
            try
            {
                DBConnection.Close();
            }
            catch (Exception) { }
        }
    }
}