﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KServer
{
    public static class CommonMethods
    {
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
                r = db.MobileIDtoUsername(u.userID);
                if (r.error)
                    return r;
                if (r.message.Trim().Length == 0)
                {
                    r.error = true;
                    r.message = "DB Username lookup exception in DJGetQueue!";
                    return r;
                }

                u.userName = r.message.Trim();
                qs.user = u;

                for (int j = 1; j < parts.Length; j++)
                {
                    Song s = new Song();
                    s.ID = int.Parse(parts[j]);
                    r = db.SongInformation(DJID, s.ID);
                    if (r.error)
                        return r;
                    if (r.message.Trim().Length == 0)
                    {
                        r.error = true;
                        r.message = "DB Song lookup exception in DJGETQUEUE!";
                        return r;
                    }
                    string[] songParts = r.message.Split(',');
                    s.title = songParts[0];
                    s.artist = songParts[1];
                    s.pathOnDisk = songParts[2];
                    qs.songs.Add(s);

                }
                queue.Add(qs);
                count++;
            }
            return r;
        }


    }
}