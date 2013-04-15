// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// This is a program used for testing the Mobile functionality of the server.
// This is not intended to be used in the final product.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.ServiceModel;
using JakubMobileClient.ServiceReference1;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace JakubMobileClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // Used to make http requests.
            WebClient client = new WebClient();
            // Base address of all commands.
            string baseAddress = "http://sunny.eng.utah.edu:1718/Mobile.svc";
            // The key that describes the user. Is not set until signup is called!
            long userKey = -1;
            // Wether to display JSON or clearer object messages.
            bool displayJSON = false;
            // The venue of the location. Currently the server ignores this until venues are implemented.
            int venueID = -1;

            Console.WriteLine("Remember to signin before using commands that use a userKey");
            for (; ; )
            {
                // Get the command
                Console.WriteLine("Enter command or [help]");
                string command = Console.ReadLine();

                // Bit if/else if statement to determine waht the command is.
                if (command.StartsWith("h")) //HELP
                {
                    //Console.WriteLine("help<h>, quit<q>, insertDJ<id>, signinDJ<si>, signoutDJ <so>, \nlistDJS<ld>, addSong<as>, removeSong<rs>, listSongs<ls>, \nlistQueue<lq>, popQueue<pq>");
                    Console.WriteLine("help<h>, JSONOut<jo>, quit<q>, listMobile<lm>, signUp<su>, signIn<si>,");
                    Console.WriteLine("signOut<so>, songSearch<ss>,songBrowse<sb>, songRequest<sr>, listQueue<lq>,");
                    Console.WriteLine("SongRequestChange<sc>, SongRequestRemove<sx>, JoinVenue<jv>,");
                    Console.WriteLine("CreatePlaylist<cp>, DeletePlaylist<dp>, AddToPlaylist<ap>,");
                    Console.WriteLine("RemoveFromPlaylist<rp>, ListPlaylists<lp>, getWait<gw>, songHistory<sh>,");
                    Console.WriteLine("RateSong<rs>, GetRating<gr>, MoveSongRequestToTop<st>, mostPopular<mp>");
                    Console.WriteLine("GetAchievements<ga>, GetUnearnedAchievements<gu>, songSuggestions<sg>");
                }
                //MobileGetSongSuggestions/?venueID={venueID}&userKey={userKey}&start={start}&count={count}
                else if (command.StartsWith("sg"))
                {
                    try
                    {
                        int start = 0;
                        int count = 5;
                        Console.WriteLine("Count:");
                        count = int.Parse(Console.ReadLine());
                        Stream data = client.OpenRead(baseAddress + "/MobileGetSongSuggestions/?venueID=" + venueID + "&userKey=" + userKey + "&start=" + start + "&count=" + count);
                        
                        
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        if (s == "")
                        {
                            Console.WriteLine("An error occured");
                            continue;
                        }

                        List<Song> r = strToJSON<List<Song>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (Song song in r)
                            {
                                Console.WriteLine(song.ID + ", " + song.title + ", " + song.artist + ", " + song.duration + ", " + song.rating);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("gu"))
                {
                    ///MobileGetAchievements/?venueID={venueID}&userKey={userKey}
                    Console.WriteLine("Enter venueID or 0 to use last venue ID, or -1 for all venues");
                    int venueIDL = int.Parse(Console.ReadLine().Trim());
                    if (venueIDL == 0)
                        venueIDL = venueID;
                    Console.WriteLine("Enter start");
                    int start = int.Parse(Console.ReadLine());
                    Console.WriteLine("Enter count");
                    int count = int.Parse(Console.ReadLine());
                    Stream data = client.OpenRead(baseAddress + "/MobileGetUnearnedAchievements/?venueID=" + venueIDL + "&userKey=" + userKey + "&start=" + start + "&count=" + count);
                    StreamReader reader = new StreamReader(data);
                    string s = reader.ReadToEnd();
                    if (s == "")
                    {
                        Console.WriteLine("An error occured");
                        continue;
                    }

                    List<MobileAchievement> r = strToJSON<List<MobileAchievement>>(s);
                    if (displayJSON)
                        Console.WriteLine("JSON: " + s);
                    else
                    {
                        foreach (MobileAchievement ma in r)
                        {
                            Console.WriteLine(ma.ID + " " + ma.name + " " + ma.description + " " + ma.image);
                        }
                    }
                }
                else if (command.StartsWith("ga"))
                {
                    ///MobileGetAchievements/?venueID={venueID}&userKey={userKey}
                    Console.WriteLine("Enter venueID or 0 to use last venue ID, or -1 for all venues");
                    int venueIDL = int.Parse(Console.ReadLine().Trim());
                    if (venueIDL == 0)
                        venueIDL = venueID;
                    Console.WriteLine("Enter start");
                    int start = int.Parse(Console.ReadLine());
                    Console.WriteLine("Enter count");
                    int count = int.Parse(Console.ReadLine());
                    Stream data = client.OpenRead(baseAddress + "/MobileGetAchievements/?venueID=" + venueIDL + "&userKey=" + userKey + "&start=" + start + "&count=" + count);
                    StreamReader reader = new StreamReader(data);
                    string s = reader.ReadToEnd();
                    if (s == "")
                    {
                        Console.WriteLine("An error occured");
                        continue;
                    }

                    List<MobileAchievement> r = strToJSON<List<MobileAchievement>>(s);
                    if (displayJSON)
                        Console.WriteLine("JSON: " + s);
                    else
                    {
                        foreach (MobileAchievement ma in r)
                        {
                            Console.WriteLine(ma.ID + " " + ma.name + " " + ma.description + " " + ma.image);
                        }
                    }
                }
                else if (command.StartsWith("x"))
                {
                    try
                    {
                        string title, artist;
                        title = String.Empty;
                        Console.Write("Artist: ");
                        artist = Console.ReadLine();
                        int start = 0;
                        int count = 20;
                        Stream data = client.OpenRead(baseAddress + "/MobileSongSearch/?title=" + title.Trim() + "&artist=" + artist.Trim() + "&start=" + start + "&count=" + count + "&venueID=" + venueID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        if (s == "")
                        {
                            Console.WriteLine("An error occured");
                            continue;
                        }

                        List<Song> r = strToJSON<List<Song>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (Song song in r)
                            {
                                Console.WriteLine(song.ID + ", " + song.title + ", " + song.artist + ", " + song.duration + ", " + song.rating);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("jo")) // TOGGLE JSON/OBJECT OUTPUT
                {
                    displayJSON = !displayJSON;
                }
                else if (command.StartsWith("q")) // QUIT
                {
                    return;
                }
                else if (command.StartsWith("mp"))
                {
                    Console.WriteLine("Enter venueID or 0 to use last venue ID, or -1 for all venues");
                    int venueIDL = int.Parse(Console.ReadLine().Trim());
                    if (venueIDL == 0)
                        venueIDL = venueID;
                    Console.WriteLine("Enter start");
                    int start = int.Parse(Console.ReadLine());
                    Console.WriteLine("Enter count");
                    int count = int.Parse(Console.ReadLine());
                    Stream data = client.OpenRead(baseAddress + "/MobileGetMostPopularSongs/?venueID=" + venueIDL + "&start=" + start + "&count=" + count);
                    StreamReader reader = new StreamReader(data);
                    string s = reader.ReadToEnd();
                    if (s == "")
                    {
                        Console.WriteLine("An error occured");
                        continue;
                    }

                    List<Song> r = strToJSON<List<Song>>(s);
                    if (displayJSON)
                        Console.WriteLine("JSON: " + s);
                    else
                    {
                        foreach (Song song in r)
                        {
                            Console.WriteLine(song.ID + " " + song.title + " " + song.artist + " " + song.rating);
                        }
                    }
                }
                else if (command.StartsWith("lm"))
                {
                    Stream data = client.OpenRead(baseAddress + "/MobileSignUp/?username=list&password=ignore");
                    StreamReader reader = new StreamReader(data);
                    string s = reader.ReadToEnd();
                    Response r = strToJSON<Response>(s);
                    if (displayJSON)
                        Console.WriteLine("JSON: " + s);
                    else
                    {
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                }
                else if (command.StartsWith("su")) // SIGN UP
                {
                    // Get the username/password from the user.
                    string username, password, email;
                    Console.Write("Username: ");
                    username = Console.ReadLine();
                    Console.Write("Password: ");
                    password = Console.ReadLine();
                    Console.Write("Email: ");
                    email = Console.ReadLine();

                    // Form the URL and get results from the server.
                    Stream data = client.OpenRead(baseAddress + "/MobileSignUp/?username=" + username.Trim() + "&password=" + password.Trim() + "&email=" + email.Trim());
                    // Read the raw results and turn into a string.
                    StreamReader reader = new StreamReader(data);
                    string s = reader.ReadToEnd();

                    // Since MobileSignUp returns a Response object. Convert the JSON to a Response object.
                    Response r = strToJSON<Response>(s);

                    // If we are diplaying JSON, simply display the original string returned.
                    if (displayJSON)
                        Console.WriteLine("JSON: " + s);
                    // Otherwise, display values that comes from the response object.
                    else
                    {
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                }
                else if (command.StartsWith("si")) // SIGN IN
                {
                    string username, password;
                    Console.Write("Username: ");
                    username = Console.ReadLine();
                    Console.Write("Password: ");
                    password = Console.ReadLine();
                    Stream data = client.OpenRead(baseAddress + "/MobileSignIn/?username=" + username.Trim() + "&password=" + password.Trim() + "&deviceID=");
                    StreamReader reader = new StreamReader(data);
                    string s = reader.ReadToEnd();
                    LogInResponse r = strToJSON<LogInResponse>(s);
                    if (displayJSON)
                        Console.WriteLine("JSON: " + s);
                    else
                    {
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Key: " + r.userKey);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    userKey = r.userKey;
                }
                else if (command.StartsWith("so")) // SIGN OUT
                {
                    try
                    {
                        Stream data = client.OpenRead(baseAddress + "/MobileSignOut/?userKey=" + userKey.ToString());
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("ss")) // Song Search
                {
                    try
                    {
                        string title, artist;
                        Console.Write("Title: ");
                        title = Console.ReadLine();
                        Console.Write("Artist: ");
                        artist = Console.ReadLine();
                        Console.WriteLine("Start: ");
                        int start = int.Parse(Console.ReadLine());
                        Console.WriteLine("Count: ");
                        int count = int.Parse(Console.ReadLine());
                        ///MobileSongSearch/?title={title}&artist={artist}&start={start}&count={count}&venueID={venueID}&userKey={userKey}
                        Stream data = client.OpenRead(baseAddress + "/MobileSongSearch/?title=" + title.Trim() + "&artist=" + artist.Trim() + "&start=" + start + "&count=" + count + "&venueID=" + venueID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        if (s == "")
                        {
                            Console.WriteLine("An error occured");
                            continue;
                        }

                        List<Song> r = strToJSON<List<Song>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (Song song in r)
                            {
                                Console.WriteLine(song.ID + ", " + song.title + ", " + song.artist + ", " + song.duration + ", " + song.rating);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("sb")) // Song BROWSE
                {
                    try
                    {
                        string firstLetter, isArtist, start, count;
                        Console.Write("Title: ");
                        firstLetter = Console.ReadLine();
                        Console.Write("IsArtist (true/false): ");
                        isArtist = Console.ReadLine();
                        Console.Write("start: ");
                        start = Console.ReadLine();
                        Console.Write("count: ");
                        count = Console.ReadLine();
                        Stream data = client.OpenRead(baseAddress + "/MobileSongBrowse/?firstLetter=" + firstLetter + "&isArtist=" + isArtist + "&start=" + start + "&count=" + count + "&venueID=" + venueID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        if (s == "")
                        {
                            Console.WriteLine("An error occured");
                            continue;
                        }

                        List<Song> r = strToJSON<List<Song>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (Song song in r)
                            {
                                Console.WriteLine(song.ID + ", " + song.title + ", " + song.artist + ", " + song.duration + ", " + song.rating);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("sr")) // Song request
                {
                    try
                    {
                        string songID;
                        Console.Write("SongID: ");
                        songID = Console.ReadLine();
                        Stream data = client.OpenRead(baseAddress + "/MobileSongRequest/?songID=" + songID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("sc"))
                {
                    try
                    {
                        string oldSongID, newSongID;
                        Console.Write("Old Song ID: ");
                        oldSongID = Console.ReadLine();
                        Console.Write("New Song ID: ");
                        newSongID = Console.ReadLine();
                        Stream data = client.OpenRead(baseAddress + "/MobileChangeSongRequest/?oldSongID=" + oldSongID + "&newSongID=" + newSongID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("sx"))
                {
                    try
                    {
                        string songID;
                        Console.Write("Old Song ID: ");
                        songID = Console.ReadLine();
                        Stream data = client.OpenRead(baseAddress + "/MobileRemoveSongRequest/?songID=" + songID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("lq")) // List the Queue
                {
                    try
                    {
                        Stream data = client.OpenRead(baseAddress + "/MobileViewQueue/?userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        if (s.Length == 0)
                        {
                            Console.WriteLine("An error occured");
                            continue;   
                        }
                        List<queueSinger> queue = strToJSON<List<queueSinger>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (queueSinger qs in queue)
                            {
                                Console.WriteLine("\nID: " + qs.user.userID + ", Name: " + qs.user.userName);
                                foreach (Song song in qs.songs)
                                    Console.WriteLine("\t" + song.ID + ", " + song.title + ", " + song.artist + ", " + song.rating);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("jv"))
                {
                    try
                    {
                        Console.WriteLine("Enter QR string: or j to auto join Jakub, or h to auto join Hugo, r for rick");
                        string QR = Console.ReadLine().Trim();
                        if (QR.Equals("j"))
                            QR = "04e0df5f";
                        else if (QR.Equals("h"))
                            QR = "aaaaaaaa";
                        else if (QR.Equals("r"))
                            QR = "1eba658f";
                        Stream data = client.OpenRead(baseAddress + "/MobileJoinVenue/?QR=" + QR + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        venueID = r.result;
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("cp"))
                {
                    try
                    {
                        Console.WriteLine("Enter Name:");
                        string name = Console.ReadLine().Trim();
                        Console.WriteLine("Enter venueID or -1 to use last venue ID");
                        int venueIDL = int.Parse(Console.ReadLine().Trim());
                        if (venueIDL == -1)
                            venueIDL = venueID;
                        Stream data = client.OpenRead(baseAddress + "/MobileCreatePlaylist/?name=" + name + "&venueID=" + venueIDL + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("dp"))
                {
                    try
                    {
                        Console.WriteLine("Enter Playlist ID");
                        int playListID = int.Parse(Console.ReadLine().Trim());
                        Stream data = client.OpenRead(baseAddress + "/MobileDeletePlaylist/?playListID=" + playListID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("ap"))
                {
                    try
                    {
                        Console.WriteLine("Enter SongID");
                        int songID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter PlaylistID");
                        int playlistID = int.Parse(Console.ReadLine().Trim());

                        Stream data = client.OpenRead(baseAddress + "/MobileAddSongToPlaylist/?songID=" + songID + "&playListID=" + playlistID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("rp"))
                {
                    try
                    {
                        Console.WriteLine("Enter SongID");
                        int songID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter PlaylistID");
                        int playlistID = int.Parse(Console.ReadLine().Trim());

                        Stream data = client.OpenRead(baseAddress + "/MobileRemoveSongFromPlaylist/?songID=" + songID + "&playListID=" + playlistID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();
                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("lp"))
                {
                    try
                    {
                        Console.WriteLine("Enter venueID or 0 to use last venue ID or -1 to not specify a venueID");
                        int venueIDL = int.Parse(Console.ReadLine().Trim());
                        if (venueIDL == 0)
                            venueIDL = venueID;
                        Stream data = client.OpenRead(baseAddress + "/MobileGetPlayLists/?venueID=" + venueIDL + "&userKey=" + userKey);

                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        if (s == "")
                        {
                            Console.WriteLine("An error occured");
                            continue;
                        }

                        List<Playlist> r = strToJSON<List<Playlist>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (Playlist p in r)
                            {
                                Console.WriteLine(p.ID + ", " + p.name + ", " + p.venue.venueID + ", " + p.venue.venueName + "," + p.venue.venueAddress + ", " + p.dateCreated);
                                foreach (Song song in p.songs)
                                    Console.WriteLine("\t" + song.ID + ", " + song.title + ", " + song.artist + ", " + song.rating);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("gw"))
                {
                    try
                    {
                        Stream data = client.OpenRead(baseAddress + "/MobileGetWaitTime/?userKey=" + userKey);

                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("sh"))
                {
                    try
                    {
                        Console.WriteLine("Start index:");
                        int start = int.Parse(Console.ReadLine());
                        Console.WriteLine("Count: ");
                        int count = int.Parse(Console.ReadLine());
                        Stream data = client.OpenRead(baseAddress + "/MobileViewSongHistory/?start=" + start + "&count=" + count + " &userKey=" + userKey);

                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        if (s.Trim().Length == 0)
                        {
                            Console.WriteLine("An error occured");
                            break;
                        }

                        List<SongHistory> r = strToJSON<List<SongHistory>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (SongHistory sh in r)
                            {
                                Console.WriteLine(sh.venue.venueID + ", " + sh.venue.venueName + ", " + sh.venue.venueAddress);
                                Console.WriteLine("\t" + sh.song.ID + ", " + sh.song.title + ", " + sh.song.artist + ", " + sh.song.duration + "s, " + sh.date);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("rs"))
                {
                    try
                    {
                        Console.WriteLine("SongID:");
                        int songID = int.Parse(Console.ReadLine());
                        Console.WriteLine("Rating:");
                        int rating = int.Parse(Console.ReadLine());
                        Stream data = client.OpenRead(baseAddress + "/MobileRateSong/?songID=" + songID + "&rating=" + rating + "&venueID=" + venueID + "&userKey=" + userKey);

                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }

                }
                else if (command.StartsWith("gr"))
                {
                    try
                    {
                        Console.WriteLine("SongID:");
                        int songID = int.Parse(Console.ReadLine());
                        Stream data = client.OpenRead(baseAddress + "/MobileViewSongRating/?songID=" + songID + "&venueID=" + venueID + "&userKey=" + userKey);

                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("st"))
                {
                    ///MobileMoveSongRequestToTop/?songID={songID}&userKey={userKey}
                    try
                    {
                        Console.WriteLine("SongID:");
                        int songID = int.Parse(Console.ReadLine());
                        Stream data = client.OpenRead(baseAddress + "/MobileMoveSongRequestToTop/?songID=" + songID + "&userKey=" + userKey);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        Response r = strToJSON<Response>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            Console.WriteLine("Error: " + r.error);
                            Console.WriteLine("Result: " + r.result);
                            Console.WriteLine("Message:\n" + r.message);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Command");
                }
            }
        }

        // Convert json to compatable object.
        public static T strToJSON<T>(string json)
        {
            T obj = Activator.CreateInstance<T>();
            MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            obj = (T)serializer.ReadObject(ms);
            ms.Close();
            return obj;
        }
    }
}
