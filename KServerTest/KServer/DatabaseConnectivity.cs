using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace KServer
{
    public class DatabaseConnectivity
    {
        private string SQLServerAddress = "localhost";
        private string DBUsername = "karaoke";
        private string DBPassword = "topsecret";
        private string database = "KaraokeDB";
        private int connectionTimeOut = 10;

        SqlConnection DBConnection = null;

        // Open a connection to the databse server using the information defined above.
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
                r.message = "OpenConnection_Error\n" + e.Message;
                r.error = true;
                return r;
            }
        }

        // Execute the given command as a NonQuery to the database
        // If an error occurs, it is stored in reponse.
        // Response.result contains the number of rows effected by the command.
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
                r.message = "SQL_Non_Query_Error\n" + command + "\n" + e.Message;
                r.error = true;
                return r;
            }
        }

        // Execute the given command a a query to the database.
        // Return the values for any valid given columns.
        private Response DBQuery(string command, string[] columns)
        {
            Response r = new Response();
            try
            {
                SqlDataReader reader = null;
                SqlCommand c = new SqlCommand(command, DBConnection);
                reader = c.ExecuteReader();
                while (reader.Read())
                {
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
                r.message = "SQL_Query_Error\n" + command + "\n" + e.Message;
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
            string[] columns = new string[5] { "ID", "Username", "Password", "SongListID", "Status" };
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

        // Add songs to a DJ's library.
        public Response DJAddSongsIgnoringDuplicates(List<Song> songs, int DJID)
        {
            Response r = new Response();
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
                    continue;                

                command = "insert into DJSongs (DJListID, Title, Artist, PathOnDisk) Values (";
                command += "'" + DJID.ToString() + "',";
                command += "'" + s.title + "',";
                command += "'" + s.artist + "',";
                command += "'" + s.pathOnDisk + "'";
                command += ");";
                r = DBNonQuery(command);
                if (r.error)
                    return r;
            }
            return r;
        }

        // Remove songs form a DJ's library.
        public Response DJRemoveSongs(List<Song> songs, int DJID)
        {
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
            }
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
        
        public Response Close()
        {
            Response r = new Response();
            try
            {
                DBConnection.Close();
                return r;
            }
            catch (Exception e)
            {
                r.message = "Close_Error\n" + e.Message;
                r.error = true;
                return r;
            }
        }
    }
}