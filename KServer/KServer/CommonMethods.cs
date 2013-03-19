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

namespace KServer
{
    public static class Common
    {
        // The guesstimated time between singers.
        public static readonly int TIME_BETWEEN_REQUESTS = 30;
        // A unique and constant deliminator to use when needed.
        public static readonly string DELIMINATOR = "#~Q";
        // ID numbers used for Google Cloud Messaging.
        public static readonly string SENDER_ID = "599874388677";
        public static readonly string APPLICATION_ID = "AIzaSyCGoaZFOiMsz0Hxo5_52y1EU0aNUimeYbw";

        /// <summary>
        /// Splits a string up by the global DELIMINATOR.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <returns>An array of strings.</returns>
        public static string[] splitByDel(string s)
        {
            return s.Split(new string[] { DELIMINATOR }, StringSplitOptions.None);
        }

        /// <summary>
        /// Changes the object representation of a queue to a representation that is stored in the database.
        /// </summary>
        /// <param name="queue">The queue to work off of.</param>
        /// <param name="raw">Out parameter is set to a compressed representation of the queue.</param>
        /// <returns>Returns a response indicating no error.</returns>
        public static Response MinimalListToDB(List<queueSinger> queue, out string raw)
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
        public static Response DBToMinimalList(string raw, out List<queueSinger> queue)
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
        public static Response DBToFullList(string raw, out List<queueSinger> queue, int DJID, DatabaseConnectivity db)
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
        /// <param name="venueID">The ID of the venue that has this song.</param>
        /// <param name="mobileID">The mobile ID of client.</param>
        /// <param name="song">The out parameter that is filled with song information.</param>
        /// <param name="db">The conenctivity to the database.</param>
        /// <param name="includePath">Whether or not to include the pathOnDisk in the song.</param>
        /// <returns>The outcome of the operation.</returns>
        public static Response GetSongInformation(int songID, int venueID, int mobileID, out Song song, DatabaseConnectivity db, bool includePath = false)
        {
            song = new Song();
            Response r = db.SongInformation(venueID, songID);
            if (r.error)
                return r;

            if(r.message.Trim().Length == 0)
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
        public static Response LoadSongRating(ref Song song, int mobileID, DatabaseConnectivity db)
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
                LogError("The message is: '" + r.message.Trim() + "'", "", null, 2);
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
        /// Pushes a notification to the mobile device of a client.
        /// </summary>
        /// <param name="mobileID">The mobileID of the client.</param>
        /// <param name="message">The message to push.</param>
        /// <returns>The outcome of the operation.</returns>
        public static Response PushMessageToMobile(int mobileID, string message)
        {
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                string deviceID;
                r = db.MobileGetDeviceID(mobileID, out deviceID);
                if (r.error)
                    return r;

                if (deviceID.Trim().Length == 0)
                {
                    r.message = "Warning: No DeviceID was registered for this device";
                    return r;
                }

                r = PushAndroidNotification(deviceID, message);
                if (r.message.ToLower().Contains("error"))
                    r.error = true;
                return r;
            }
        }

        /// <summary>
        /// Pushes a notification to an android device.
        /// </summary>
        /// <param name="deviceID">The deviceID of the device.</param>
        /// <param name="message">The message to push.</param>
        /// <returns>The outcome of the operation.</returns>
        public static Response PushAndroidNotification(string deviceID, string message)
        {
            Response r = new Response();
            var value = message;
            WebRequest tRequest;
            tRequest = WebRequest.Create("https://android.googleapis.com/gcm/send");
            tRequest.Method = "post";
            tRequest.ContentType = " application/x-www-form-urlencoded;charset=UTF-8";
            tRequest.Headers.Add(string.Format("Authorization: key={0}", APPLICATION_ID));
            tRequest.Headers.Add(string.Format("Sender: id={0}", SENDER_ID));
            string postData = "collapse_key=score_update&time_to_live=108&delay_while_idle=1&data.message=" + value + "&data.time=" + System.DateTime.Now.ToString() + "&registration_id=" + deviceID + "";
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
        /// <param name="message">The message</param>
        /// <param name="stack">The stack</param>
        /// <param name="passThru">A parameter to return</param>
        /// <param name="Case">0 = mobile_log, 1 = dj_log, 2 = debug</param>
        /// <returns>The passThru object</returns>
        public static object LogError(string message, string stack, object passThru, int Case)
        {
            switch (Case)
            {
                case 0:
                    writeToFile(message, stack, "C:\\inetpub\\ftproot\\log\\mobile_log.txt");
                    break;
                case 1:
                    writeToFile(message, stack, "C:\\inetpub\\ftproot\\log\\dj_log.txt");
                    break;
                case 2:
                    writeToFile(message, stack, "C:\\inetpub\\ftproot\\log\\debug.txt");
                    break;
            }
            return passThru;
        }

        /// <summary>
        /// Writes a message to a file
        /// </summary>
        /// <param name="message">The first message to write.</param>
        /// <param name="stack">The second message to write.</param>
        /// <param name="file">The path of the file to write to.</param>
        private static void writeToFile(string message, string stack, string file)
        {
            StreamWriter w = File.AppendText(file);
            w.WriteLine(DateTime.Now.ToString());
            w.WriteLine(message);
            w.WriteLine(stack);
            w.WriteLine("--------------------------------------------------");
            w.WriteLine();
            w.Close();
        }
    }
}