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
            //MobileClient proxy = new MobileClient();
            //LogInResponse r = proxy.MobileSignIn("Jakub", "topsecret");

            WebClient client = new WebClient();
            string baseAddress = "http://sunny.eng.utah.edu:81/Mobile.svc";
            long userKey = -1;
            bool displayJSON = true;
            int venueID = 1;

            for (; ; )
            {
                Console.WriteLine("Enter command or [help]");
                string command = Console.ReadLine();

                if (command.StartsWith("h")) //HELP
                {
                    //Console.WriteLine("help<h>, quit<q>, insertDJ<id>, signinDJ<si>, signoutDJ <so>, \nlistDJS<ld>, addSong<as>, removeSong<rs>, listSongs<ls>, \nlistQueue<lq>, popQueue<pq>");
                    Console.WriteLine("help<h>, JSONOut<j>, quit<q>, signUp<su>, signIn<si>, signOut<so>, songSearch<ss>,\nsongBrowse<sb>, songRequest<sr>, listQueue<lq>");
                }
                else if (command.StartsWith("j")) // TOGGLE JSON/OBJECT OUTPUT
                {
                    displayJSON = !displayJSON;
                }
                else if (command.StartsWith("q")) // QUIT
                {
                    return;
                }
                else if (command.StartsWith("su")) // SIGN UP
                {
                    string username, password;
                    Console.Write("Username: ");
                    username = Console.ReadLine();
                    Console.Write("Password: ");
                    password = Console.ReadLine();
                    Stream data = client.OpenRead(baseAddress + "/MobileSignUp/?username=" + username.Trim() + "&password=" + password.Trim());
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
                else if (command.StartsWith("si"))
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
                else if (command.StartsWith("so")) // Signout a DJ
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
                else if (command.StartsWith("sb")) // Song browse
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
                                Console.WriteLine("ID: " + qs.user.userID + ", Name: " + qs.user.userName);
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


                //
            }
        }


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
