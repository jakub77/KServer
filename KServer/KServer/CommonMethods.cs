using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace KServer
{
    public static class Common
    {
        public static readonly int TIME_BETWEEN_REQUESTS = 30;
        public static readonly string deliminator = "#~Q";
        public static string[] splitByDel(string s)
        {
            return s.Split(new string[] { deliminator }, StringSplitOptions.None);
        }
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
        /// Log an error message.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="stack">The stack</param>
        /// <param name="passThru">A parameter to return</param>
        /// <param name="Case">0 = mobile_log, 1 = dj_log, 2 = debug</param>
        /// <returns></returns>
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