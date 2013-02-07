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
            string baseAddress = "http://sunny.eng.utah.edu:81/Mobile.svc";
            // The key that describes the user. Is not set until signup is called!
            long userKey = -1;
            // Wether to display JSON or clearer object messages.
            bool displayJSON = false;
            // The venue of the location. Currently the server ignores this until venues are implemented.
            int venueID = 1;

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
                    Console.WriteLine("help<h>, JSONOut<j>, quit<q>, listMobile<lm>, signUp<su>, signIn<si>, \nsignOut<so>, songSearch<ss>,songBrowse<sb>, songRequest<sr>, listQueue<lq>");
                    Console.WriteLine("SongRequestChange<sc>, SongRequestRemove<sx>");
                }
                else if (command.StartsWith("j")) // TOGGLE JSON/OBJECT OUTPUT
                {
                    displayJSON = !displayJSON;
                }
                else if (command.StartsWith("q")) // QUIT
                {
                    return;
                }
                else if(command.StartsWith("lm"))
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
                    string username, password;
                    Console.Write("Username: ");
                    username = Console.ReadLine();
                    Console.Write("Password: ");
                    password = Console.ReadLine();

                    // Form the URL and get results from the server.
                    Stream data = client.OpenRead(baseAddress + "/MobileSignUp/?username=" + username.Trim() + "&password=" + password.Trim());
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
                    Stream data = client.OpenRead(baseAddress + "/MobileSignIn/?username=" + username.Trim() + "&password=" + password.Trim());
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
                        Stream data = client.OpenRead(baseAddress + "/MobileSongSearch/?title=" + title.Trim() + "&artist=" + artist.Trim() + "&venueID=" + venueID);
                        StreamReader reader = new StreamReader(data);
                        string s = reader.ReadToEnd();

                        if(s == "")
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
                                Console.WriteLine(song.ID + ", " + song.title + ", " + song.artist);
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
                        Stream data = client.OpenRead(baseAddress + "/MobileSongBrowse/?firstLetter=" + firstLetter + "&isArtist=" + isArtist + "&start=" + start + "&count=" + count + "&venueID=" + venueID);
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
                                Console.WriteLine(song.ID + ", " + song.title + ", " + song.artist);
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
                        Stream data = client.OpenRead(baseAddress + "/MobileChangeSongRequest/?oldSongID=" + oldSongID + "&newSongID=" + newSongID + "&userKey="+ userKey);
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
                        List<queueSinger> queue = strToJSON<List<queueSinger>>(s);
                        if (displayJSON)
                            Console.WriteLine("JSON: " + s);
                        else
                        {
                            foreach (queueSinger qs in queue)
                            {
                                Console.WriteLine("\nID: " + qs.user.userID + ", Name: " + qs.user.userName);
                                foreach (Song song in qs.songs)
                                    Console.WriteLine("\t" + song.ID + ", " + song.title + ", " + song.artist);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
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
