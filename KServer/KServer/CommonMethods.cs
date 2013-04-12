// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// Common is a class that contains common functions used in both the DJ and Mobile
// parts of the server system. It also contains some variable definitions that are
// used throughout the server application.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Security.Cryptography;

namespace KServer
{
    public static class Common
    {
        // The guesstimated time between singers.
        internal static readonly int TIME_BETWEEN_REQUESTS = 30;
        // A unique and constant deliminator to use when needed.
        internal static readonly string DELIMINATOR = "#~Q";
        // ID numbers used for Google Cloud Messaging.
        internal static readonly string SENDER_ID = "599874388677";
        internal static readonly string APPLICATION_ID = "AIzaSyCGoaZFOiMsz0Hxo5_52y1EU0aNUimeYbw";

        internal static int GetBitFromBool(bool boolean)
        {
            if (boolean)
                return 1;
            return 0;
        }

        /// <summary>
        /// Creates a random string to use as a salt for password hashing.
        /// </summary>
        /// <param name="size">The size of the salt to use.</param>
        /// <returns></returns>
        internal static string CreateSalt(int size)
        {
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buf = new byte[size];
            rng.GetBytes(buf);
            return Convert.ToBase64String(buf);
        }

        /// <summary>
        /// Creates a hash of the password plus the salt and returns it as a string.
        /// </summary>
        /// <param name="plainPassword">The plain text password.</param>
        /// <param name="salt">The salt associated with this user.</param>
        /// <returns>The salted and hashed password.</returns>
        internal static string CreatePasswordHash(string plainPassword, string salt)
        {
            HashAlgorithm algorithm = new SHA256Managed();
            string preHash = plainPassword + salt;
            byte[] postHash = algorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(preHash));
            return Convert.ToBase64String(postHash);
        }

        /// <summary>
        /// Splits a string up by the global DELIMINATOR.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <returns>An array of strings.</returns>
        internal static string[] splitByDel(string s)
        {
            return s.Split(new string[] { DELIMINATOR }, StringSplitOptions.None);
        }

        /// <summary>
        /// Changes the object representation of a queue to a representation that is stored in the database.
        /// </summary>
        /// <param name="queue">The queue to work off of.</param>
        /// <param name="raw">Out parameter is set to a compressed representation of the queue.</param>
        /// <returns>Returns a response indicating no error.</returns>
        internal static Response MinimalListToDB(List<queueSinger> queue, out string raw)
        {
            raw = string.Empty;
            foreach (queueSinger qs in queue)
            {
                raw += qs.user.userID.ToString();
                foreach (Song s in qs.songs)
                {
                    raw += "~" + s.ID;
                }
                raw += "`";
            }
            if (raw.Length > 0)
                raw = raw.Substring(0, raw.Length - 1);
            return new Response();
        }

        /// <summary>
        /// Takes the database representation of the queue and expands it into a queue that is minimally
        /// filled with data. The queue is only filled to contain user IDs and song IDs. 
        /// </summary>
        /// <param name="raw">The database representation of the queue.</param>
        /// <param name="queue">Out object represenation of the queue.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static Response DBToMinimalList(string raw, out List<queueSinger> queue)
        {
            int count = 0;
            Response r = new Response();

            queue = new List<queueSinger>();
            string[] clientRequests = raw.Split('`');
            for (int i = 0; i < clientRequests.Length; i++)
            {
                string[] parts = clientRequests[i].Split('~');
                if (parts.Length == 0)
                {
                    r.error = true;
                    r.message = "Error in DBtoList 1";
                    return r;
                }

                queueSinger qs = new queueSinger();
                qs.songs = new List<Song>();
                User u = new User();
                u.userID = int.Parse(parts[0]);
                qs.user = u;

                for (int j = 1; j < parts.Length; j++)
                {
                    Song s = new Song();
                    s.ID = int.Parse(parts[j]);
                    qs.songs.Add(s);

                }
                queue.Add(qs);
                count++;
            }
            return r;
        }

        /// <summary>
        /// Takes the database representaiton of the queue and expands it into a queue that is fully
        /// filled except for song ratings.
        /// </summary>
        /// <param name="raw">The databse representation of the queue.</param>
        /// <param name="queue">Out object representation of the queue.</param>
        /// <param name="DJID">The DJID of the DJ.</param>
        /// <param name="db">An object that allows for database connectivity.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static Response DBToFullList(string raw, out List<queueSinger> queue, int DJID, DatabaseConnectivity db)
        {
            queue = new List<queueSinger>();
            Response r = new Response();
            int count = 0;

            string[] clientRequests = raw.Split('`');
            for (int i = 0; i < clientRequests.Length; i++)
            {
                string[] parts = clientRequests[i].Split('~');
                if (parts.Length == 0)
                {
                    r.error = true;
                    r.message = "Error in DBtoList 1";
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
                    r.error = true;
                    r.message = "DB Username lookup exception in DBToFullList!";
                    return r;
                }

                u.userName = r.message.Trim();
                qs.user = u;

                for (int j = 1; j < parts.Length; j++)
                {
                    Song s = new Song();
                    r = GetSongInformation(int.Parse(parts[j]), DJID, -1, out s, db, true);
                    if (r.error)
                        return r;

                    qs.songs.Add(s);

                }
                queue.Add(qs);
                count++;
            }
            return r;
        }

        /// <summary>
        /// Gets the song information of a song from the database. If mobileID is -1, song rating is not included.
        /// If includePath is set to true, the pathondisk is set, otherwise it is not set.
        /// </summary>
        /// <param name="songID">The songID of the song.</param>
        /// <param name="venueID">ignored</param>
        /// <param name="mobileID">The mobile ID of client.</param>
        /// <param name="song">The out parameter that is filled with song information.</param>
        /// <param name="db">The conenctivity to the database.</param>
        /// <param name="includePath">Whether or not to include the pathOnDisk in the song.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static Response GetSongInformation(int songID, int venueID, int mobileID, out Song song, DatabaseConnectivity db, bool includePath = false)
        {
            song = new Song();
            Response r = db.SongInformation(venueID, songID);
            if (r.error)
                return r;

            if (r.message.Trim().Length == 0)
            {
                r.error = true;
                r.message = "Could not find song.";
                return r;
            }

            string[] songParts = splitByDel(r.message);
            if (songParts.Length < 4)
            {
                r.error = true;
                r.message = "Song did not have 4 parts";
                return r;
            }

            song.ID = songID;
            song.title = songParts[0];
            song.artist = songParts[1];
            if (includePath)
                song.pathOnDisk = songParts[2];

            int duration;
            if (!int.TryParse(songParts[3], out duration))
            {
                r.error = true;
                r.message = "Could not parse the duration";
                return r;
            }
            song.duration = duration;

            if (mobileID == -1)
                return r;

            return LoadSongRating(ref song, mobileID, db);
        }

        /// <summary>
        /// Loads the song rating into the referenced song.
        /// </summary>
        /// <param name="song">The referenced song to set the song rating of.</param>
        /// <param name="mobileID">The ID of the mobile user.</param>
        /// <param name="db">Connectivity of the database.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static Response LoadSongRating(ref Song song, int mobileID, DatabaseConnectivity db)
        {
            int rating;
            Response r = db.MobileGetSongRating(mobileID, song.ID);
            if (r.error)
                return r;

            if (r.message.Trim().Length == 0)
            {
                song.rating = -1;
                return r;
            }

            if (!int.TryParse(r.message.Trim(), out rating))
            {
                LogError("Load song rating fail, the message is: '" + r.message.Trim() + "'", "", null, 2);
                r.error = true;
                r.message = "Could not parse rating";
                return r;
            }
            song.rating = rating;
            r.message = rating.ToString();
            r.result = rating;
            return r;
        }

        /// <summary>
        /// Push a message to all eligable users in the queue. Temp users and users without registered deviceIDs will not recieve this message.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static Response PushMessageToUsersOfDJ(int DJID, string message, DatabaseConnectivity db)
        {
            Response r = new Response();
            List<int> clients;
            r = db.DJGetAssociatedClients(DJID, out clients);
            if (r.error)
                return r;

            foreach (int clientID in clients)
            {
                r = PushMessageToMobile(clientID, message, db);
                if (r.error)
                    LogError(r.message, Environment.StackTrace, null, 2);
                if (r.message.StartsWith("Warning:"))
                    LogError(r.message, "UserID: " + clientID, null, 2);
            }
            return r;
        }

        /// <summary>
        /// Pushes a notification to the mobile device of a client.
        /// </summary>
        /// <param name="mobileID">The mobileID of the client.</param>
        /// <param name="message">The message to push.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static Response PushMessageToMobile(int mobileID, string message, DatabaseConnectivity db)
        {
            Response r = new Response();

            string deviceID;
            r = db.MobileGetDeviceID(mobileID, out deviceID);
            if (r.error)
                return r;

            if (deviceID.Trim().Length == 0)
            {
                //LogError("Message not sent to (No DeviceID associated): " + mobileID, String.Empty, null, 2);
                return r;
            }

            r = PushAndroidNotification(deviceID, message);
            if (r.message.ToLower().Contains("error"))
            {
                r.error = true;
                LogError("Message had error sending to: " + mobileID + " response was: " + r.message, message, null, 2);
            }
            LogError("Message sent without error to: " + mobileID + " response was: " + r.message, message, null, 2);
            return r;
        }

        /// <summary>
        /// Pushes a notification to an android device.
        /// </summary>
        /// <param name="deviceID">The deviceID of the device.</param>
        /// <param name="message">The message to push.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static Response PushAndroidNotification(string deviceID, string message)
        {
            string collapseKey = String.Empty;
            if (message == "queue")
                collapseKey = "queue";
            else if (message == "turn")
                collapseKey = "turn";
            else if (message == "next")
                collapseKey = "next";
            else
                collapseKey = "other";

            Response r = new Response();
            var value = message;
            WebRequest tRequest;
            tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = " application/x-www-form-urlencoded;charset=UTF-8";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", APPLICATION_ID));
            tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            string postData = "collapse_key=" + collapseKey + "&time_to_live=0&delay_while_idle=false&data.message=" + value + "&data.time=" + System.DateTime.Now.ToString() + "&registration_id=" + deviceID + "";
            Console.WriteLine(postData);
            Byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            tRequest.ContentLength = byteArray.Length;
            Stream dataStream = tRequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse tResponse = tRequest.GetResponse();
            dataStream = tResponse.GetResponseStream();
            StreamReader tReader = new StreamReader(dataStream);
            String sResponseFromServer = tReader.ReadToEnd();
            tReader.Close();
            dataStream.Close();
            tResponse.Close();
            r.message = sResponseFromServer;
            return r;
        }

        /// <summary>
        /// Log an error message.
        /// </summary>
        /// <param name="messagePart1">The message</param>
        /// <param name="messagePart2">The stack</param>
        /// <param name="passThru">A parameter to return</param>
        /// <param name="Case">0 = mobile_log, 1 = dj_log, 2 = debug</param>
        /// <returns>The passThru object</returns>
        internal static object LogError(string messagePart1, string messagePart2, object passThru, int Case)
        {
            switch (Case)
            {
                case 0:
                    writeToFile(messagePart1, messagePart2, "C:\\inetpub\\ftproot\\log\\mobile_log.txt");
                    break;
                case 1:
                    writeToFile(messagePart1, messagePart2, "C:\\inetpub\\ftproot\\log\\dj_log.txt");
                    break;
                case 2:
                    writeToFile(messagePart1, messagePart2, "C:\\inetpub\\ftproot\\log\\debug.txt");
                    break;
            }
            return passThru;
        }

        /// <summary>
        /// Writes a message to a file
        /// </summary>
        /// <param name="messagePart1">The first message to write.</param>
        /// <param name="messagePart2">The second message to write.</param>
        /// <param name="file">The path of the file to write to.</param>
        private static void writeToFile(string messagePart1, string messagePart2, string file)
        {
            StreamWriter w = File.AppendText(file);
            w.WriteLine(DateTime.Now.ToString());
            w.WriteLine(messagePart1);
            w.WriteLine(messagePart2);
            w.WriteLine("--------------------------------------------------");
            w.WriteLine();
            w.Close();
        }
    }
}