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
using System.Data.SqlClient;

namespace KServer
{
    /// <summary>
    /// Collection of methods useful to the mobile, dj, and web services.
    /// </summary>
    public static class Common
    {
        #region VarConstEnum
        // The guesstimated time between singers.
        internal static readonly int TIME_BETWEEN_REQUESTS;
        // A unique and constant deliminator to use when needed.
        internal static readonly string DELIMINATOR;
        // ID numbers used for Google Cloud Messaging.
        internal static readonly string SENDER_ID;
        internal static readonly string APPLICATION_ID;

        /// <summary>
        /// Initialize the settings used in common to the values in Settings.resx.
        /// For settings that will break the program if invalid, default values.
        /// are set if the settings file is invalid.
        /// </summary>
        static Common()
        {
            int tbr = 0;
            int.TryParse(Settings.TIME_BETWEEN_REQUESTS, out tbr);
            TIME_BETWEEN_REQUESTS = tbr;

            if (Settings.DELIMINATOR.Length > 0)
                DELIMINATOR = Settings.DELIMINATOR;
            else
                DELIMINATOR = "#~Q";

            SENDER_ID = Settings.GCM_SENDER_ID;
            APPLICATION_ID = Settings.GCM_APPLICATION_ID;
        }
        /// <summary>
        /// Which log file to write to.
        /// </summary>
        public enum LogFile
        {
            Mobile,
            DJ,
            Debug,
            Web,
            Messages
        }
        #endregion

        #region Misc

        /// <summary>
        /// Returns a 0 or a 1 from a booleans.
        /// Returns 0 if false, 1 if true.
        /// </summary>
        /// <param name="boolean">The boolean.</param>
        /// <returns>The resulting int.</returns>
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

        #endregion

        #region SongRequestsandSongInfo

        /// <summary>
        /// Changes the object representation of a queue to a representation that is stored in the database.
        /// </summary>
        /// <param name="queue">The queue to work off of.</param>
        /// <param name="raw">Out parameter is set to a compressed representation of the queue.</param>
        /// <returns>Returns a response indicating no error.</returns>
        internal static ExpResponse MinimalListToDB(List<queueSinger> queue, out string raw)
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
            return new ExpResponse();
        }
        /// <summary>
        /// Takes the database representation of the queue and expands it into a queue that is minimally
        /// filled with data. The queue is only filled to contain user IDs and song IDs. 
        /// </summary>
        /// <param name="raw">The database representation of the queue.</param>
        /// <param name="queue">Out object represenation of the queue.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static ExpResponse DBToMinimalList(string raw, out List<queueSinger> queue)
        {
            int count = 0;
            ExpResponse r = new ExpResponse();

            queue = new List<queueSinger>();
            string[] clientRequests = raw.Split('`');
            for (int i = 0; i < clientRequests.Length; i++)
            {
                string[] parts = clientRequests[i].Split('~');
                if (parts.Length == 0)
                {
                    r.setErMsgStk(true, "Error in DBToMinimalList, parts.length == 0.", Environment.StackTrace);
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
        internal static ExpResponse DBToFullList(string raw, out List<queueSinger> queue, int DJID, DatabaseConnectivity db)
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
                    r.setErMsgStk(true, "Error in DBtoList 1", Environment.StackTrace);
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
                    r.setErMsgStk(true, "DB Username lookup exception in DBToFullList!", Environment.StackTrace);
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
        internal static ExpResponse GetSongInformation(int songID, int venueID, int mobileID, out Song song, DatabaseConnectivity db, bool includePath = false)
        {
            song = new Song();
            ExpResponse r = db.SongInformation(venueID, songID);
            if (r.error)
                return r;

            if (r.message.Trim().Length == 0)
            {
                r.setErMsgStk(true, "Could not find song", Environment.StackTrace);
                return r;
            }

            string[] songParts = splitByDel(r.message);
            if (songParts.Length < 4)
            {
                r.setErMsgStk(true, "Song lacked 4 parts", Environment.StackTrace);
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
                r.setErMsgStk(true, "Could not parse duration", Environment.StackTrace);
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
        internal static ExpResponse LoadSongRating(ref Song song, int mobileID, DatabaseConnectivity db)
        {
            int rating;
            ExpResponse r = db.MobileGetSongRating(mobileID, song.ID);
            if (r.error)
                return r;

            if (r.message.Trim().Length == 0)
            {
                song.rating = -1;
                return r;
            }

            if (!int.TryParse(r.message.Trim(), out rating))
            {
                r.setErMsgStk(true, "Could not parse song rating", Environment.StackTrace);
                return r;
            }
            song.rating = rating;
            r.message = rating.ToString();
            r.result = rating;
            return r;
        }
        #endregion

        #region PushMessages


        /// <summary>
        /// Push a message to all eligable users in the queue. Temp users and users without registered deviceIDs will not recieve this message.
        /// </summary>
        /// <param name="queue">The queue.</param>
        /// <param name="message">The message to send.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static ExpResponse PushMessageToUsersOfDJ(int DJID, string message, DatabaseConnectivity db)
        {
            ExpResponse r = new ExpResponse();
            List<int> clients;
            r = db.DJGetAssociatedClients(DJID, out clients);
            if (r.error)
                return r;

            foreach (int clientID in clients)
            {
                r = PushMessageToMobile(clientID, message, db);
                if (r.error)
                    return r;
            }
            return r;
        }
        /// <summary>
        /// Pushes a notification to the mobile device of a client.
        /// </summary>
        /// <param name="mobileID">The mobileID of the client.</param>
        /// <param name="message">The message to push.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static ExpResponse PushMessageToMobile(int mobileID, string message, DatabaseConnectivity db)
        {
            ExpResponse r = new ExpResponse();
            if (mobileID < 1)
                return r;

            string deviceID;
            r = db.MobileGetDeviceID(mobileID, out deviceID);
            if (r.error)
                return r;

            if (deviceID.Trim().Length == 0)
            {
                r.setErMsg(false, "Not sending message to " + mobileID + " no device ID associated");
                return Common.LogErrorRetGen<ExpResponse>(r, new ExpResponse(), LogFile.Messages);
            }

            r = PushAndroidNotification(deviceID, message);
            if (r.error)
            {
                r.setErMsgStk(true, "Message had error sending to: " + mobileID + " response was: " + r.message, string.Empty);
                return Common.LogErrorRetGen<ExpResponse>(r, new ExpResponse(), LogFile.Messages);
            }
            r.setErMsgStk(false, "Message sent without error to: " + mobileID + " response was: " + r.message, message);
            return Common.LogErrorRetGen<ExpResponse>(r, new ExpResponse(), LogFile.Messages);
        }
        /// <summary>
        /// Pushes a notification to an android device.
        /// </summary>
        /// <param name="deviceID">The deviceID of the device.</param>
        /// <param name="message">The message to push.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static ExpResponse PushAndroidNotification(string deviceID, string message)
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

            ExpResponse r = new ExpResponse();
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
            if (sResponseFromServer.ToLower().Contains("error"))
                r.setErMsg(true, sResponseFromServer);
            else
                r.setErMsg(false, sResponseFromServer);
            return r;
        }

        #endregion

        #region Logging

        /// <summary>
        /// Log an ExpResponse to a file and return a new Response with the given message and error set to true.
        /// </summary>
        /// <param name="r">The ExpResponse to write.</param>
        /// <param name="clientMsgToRet">The message to return.</param>
        /// <param name="logFile">Which log to write to.</param>
        /// <returns>The response set with the given message.</returns>
        internal static Response LogErrorRetNewMsg(ExpResponse r, string clientMsgToRet, LogFile logFile)
        {
            switch (logFile)
            {
                case LogFile.Mobile:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\mobile_log.txt");
                    break;
                case LogFile.DJ:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\dj_log.txt");
                    break;
                case LogFile.Debug:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\debug.txt");
                    break;
                case LogFile.Web:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\web.txt");
                    break;
                case LogFile.Messages:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\messages.txt");
                    break;
            }
            return new Response(true, clientMsgToRet);
        }
        /// <summary>
        /// Log an ExpResponse to a file and pass the given object through.
        /// </summary>
        /// <typeparam name="T">The type of object to pass through.</typeparam>
        /// <param name="r">The ExpResponse to write.</param>
        /// <param name="passThrough">The object to pass through.</param>
        /// <param name="logFile">Which log to write to.</param>
        /// <returns>The passed through object.</returns>
        internal static T LogErrorRetGen<T>(ExpResponse r, T passThrough, LogFile logFile)
        {
            switch (logFile)
            {
                case LogFile.Mobile:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\mobile_log.txt");
                    break;
                case LogFile.DJ:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\dj_log.txt");
                    break;
                case LogFile.Debug:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\debug.txt");
                    break;
                case LogFile.Web:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\web.txt");
                    break;
                case LogFile.Messages:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\messages.txt");
                    break;
            }
            return passThrough;
        }
        /// <summary>
        /// Logs and expresponse and passes the object back.
        /// </summary>
        /// <param name="r">The expresponse to log.</param>
        /// <param name="logFile">Which logfile to write to.</param>
        /// <returns>The expresponse passed in.</returns>
        internal static ExpResponse LogErrorPassThru(ExpResponse r, LogFile logFile)
        {
            switch (logFile)
            {
                case LogFile.Mobile:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\mobile_log.txt");
                    break;
                case LogFile.DJ:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\dj_log.txt");
                    break;
                case LogFile.Debug:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\debug.txt");
                    break;
                case LogFile.Web:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\web.txt");
                    break;
                case LogFile.Messages:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\messages.txt");
                    break;
            }
            return r;
        }
        /// <summary>
        /// Log a simple message to a logFile, does not return anything.
        /// </summary>
        /// <param name="logFile">The logfile to write to.</param>
        /// <param name="mes1">The first message</param>
        /// <param name="mes2">Optional second message</param>
        internal static void LogSimpleError(LogFile logFile, String mes1, string mes2 = "")
        {
            ExpResponse r = new ExpResponse(false, mes1, mes2);
            switch (logFile)
            {
                case LogFile.Mobile:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\mobile_log.txt");
                    break;
                case LogFile.DJ:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\dj_log.txt");
                    break;
                case LogFile.Debug:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\debug.txt");
                    break;
                case LogFile.Web:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\web.txt");
                    break;
                case LogFile.Messages:
                    writeExpRspToFile(r, "C:\\inetpub\\ftproot\\log\\messages.txt");
                    break;
            }
            return;
        }
        /// <summary>
        /// DEPRECIATED - Logs an error message and passes an object through.
        /// </summary>
        /// <param name="messagePart1">The message</param>
        /// <param name="messagePart2">The stack</param>
        /// <param name="passThru">A parameter to return</param>
        /// <param name="Case">0 = mobile_log, 1 = dj_log, 2 = debug</param>
        /// <returns>The passThru object</returns>
        //[Obsolete("Please use LogErrorRetNewMsg, LogErrorRetGen, or LogErrorPassThru now", false)]
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
            if (messagePart1.Length > 0)
                w.WriteLine(messagePart1);
            if (messagePart2.Length > 0)
                w.WriteLine(messagePart2);
            w.WriteLine("--------------------------------------------------");
            w.WriteLine();
            w.Close();
        }
        /// <summary>
        /// Appends an ExpResponse to a file.
        /// </summary>
        /// <param name="r">The ExpResponse</param>
        /// <param name="file">The output file path to append to.</param>
        private static void writeExpRspToFile(ExpResponse r, string file)
        {
            StreamWriter w = File.AppendText(file);
            w.WriteLine(DateTime.Now.ToString() + "\t Result: " + r.result);
            if (r.message.Length > 0)
                w.WriteLine(r.message);
            if (r.stackTrace.Length > 0)
                w.WriteLine(r.stackTrace);
            w.WriteLine("--------------------------------------------------");
            w.WriteLine();
            w.Close();
        }

        #endregion

        #region AchievementRunning
        /// <summary>
        /// Run the evaulation of the given DJ's achievement.
        /// </summary>
        /// <param name="VenueID">The ID of the DJ.</param>
        /// <param name="db">Database connectivity.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static ExpResponse RunAchievements(int VenueID, DatabaseConnectivity db)
        {
            ExpResponse r;
            List<Achievement> achievements;
            r = db.DJViewAchievements(VenueID, out achievements);
            if (r.error)
                return r;


            foreach (Achievement a in achievements)
            {
                r = EvaluateAchievement(VenueID, a, db);
                if (r.error)
                    return r;
            }

            return r;
        }
        /// <summary>
        /// Joins lists of a list of ints together based on a union or intersection.
        /// </summary>
        /// <param name="users">The list of list of ints.</param>
        /// <param name="results">Out the resulting list after the operation.</param>
        /// <param name="andLists">Whether to intersect(true), or union(false)</param>
        /// <returns>The outcome of the operation.</returns>
        private static ExpResponse CombineLists(List<List<int>> users, out List<int> results, bool andLists)
        {
            ExpResponse r = new ExpResponse();
            results = new List<int>();
            try
            {
                if (users.Count < 1)
                {
                    r.setErMsgStk(true, "Exception in AndLists, list size is < 1.", Environment.StackTrace);
                    return r;
                }

                results = users[0];
                for (int i = 1; i < users.Count; i++)
                {
                    if (andLists)
                        results = results.Intersect(users[i]).ToList();
                    else
                        results = results.Union(users[i]).ToList();
                }

                return r;
            }
            catch (Exception e)
            {
                r.setErMsgStk(true, "Combine Lists error: " + e.ToString(), Environment.StackTrace);
                return r;
            }
        }
        /// <summary>
        /// Evaluate and award a single achievement to all users who qualify.
        /// </summary>
        /// <param name="VenueID">The DJ's unique ID.</param>
        /// <param name="a">The achievement to evaluate.</param>
        /// <param name="db">Database conenctivity.</param>
        /// <returns>The outcome of the operation.</returns>
        internal static ExpResponse EvaluateAchievement(int VenueID, Achievement a, DatabaseConnectivity db)
        {
            string sqlText;
            List<SqlCommand> cmds;
            List<List<int>> results;
            ExpResponse r = AchievementParser.CreateAchievementSQL(a, VenueID, out sqlText, out cmds);
            if (r.error)
                return r;

            r = db.EvaluateAchievementStatements(VenueID, cmds, out results);
            if (r.error)
                return r;

            if (results.Count == 0)
            {
                r.setErMsgStk(true, "EvaulateAchievement: List is of size zero, something went wrong", Environment.StackTrace);
                return r;

            }

            List<int> qualifiedUsers;
            r = CombineLists(results, out qualifiedUsers, a.statementsAnd);
            if (r.error)
                return r;

            if (!a.isPermanant)
            {
                r = db.DeleteEarnedAchievementsByID(a.ID);
                if (r.error)
                    return r;
            }

            foreach (int userID in qualifiedUsers)
            {
                r = db.AwardAchievement(userID, a.ID);
                if (r.error)
                    return r;
            }

            return r;
        }

        #endregion
    }
}