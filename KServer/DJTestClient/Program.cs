using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DJTestClient.ServiceReference1;
using System.ServiceModel;
using System.Security.Cryptography;
using System.IO;

namespace DJTestClient
{
    //public class DJCallbacks : IDJCallback
    //{
    //    void IDJCallback.DJQueueChanged(queueSinger[] queue)
    //    {
    //        Console.WriteLine("Got a callback");
    //        return;
    //    }
    //}

    class Program
    {
        static void Main(string[] args)
        {
            //InstanceContext site = new InstanceContext(new DJCallbacks());
            //DJClient proxy = new DJClient(site);

            // User the proxy to communicate with server.
            DJClient proxy = new DJClient();

            // This is the DJKey that identifies the DJ. It is not set until a signin request is made.
            long DJKey = -1;

            Console.WriteLine("Remember to sign in before using commands that need a DJKey");
            for (; ; )
            {
                // Get user input for what to do. 
                Console.WriteLine("Enter command or [help]");
                string command = Console.ReadLine();

                if (command.StartsWith("h")) //HELP
                {
                    Console.WriteLine("help<h>, quit<q>, insertDJ<id>, signinDJ<si>, signoutDJ <so>, \nlistDJS<ld>, addSong<as>, removeSong<rs>, listSongs<ls>, \nlistQueue<lq>, popQueue<pq>");
                }
                else if (command.StartsWith("x"))
                {
                    try
                    {
                        string[] parts = command.Split(' ');
                        int num = int.Parse(parts[1]);
                        System.Security.Cryptography.SHA1 sha = new System.Security.Cryptography.SHA1CryptoServiceProvider();
                        byte[] res = sha.ComputeHash(BitConverter.GetBytes(num));
                        long l = BitConverter.ToInt64(res, 0);
                        Console.WriteLine(l.ToString());

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error: " + e.Message);
                    }

                }
                else if (command.StartsWith("q")) // QUIT
                {
                    return;
                }
                else if (command.StartsWith("ld")) // LIST DJS [This is a temporary function, not for release]
                {
                    Response r;
                    try
                    {
                        r = proxy.DJSignUp("list", String.Empty);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("id")) // INSERT DJ
                {
                    Response r;
                    string username, password;
                    Console.Write("Username: ");
                    username = Console.ReadLine();
                    Console.Write("Password: ");
                    password = Console.ReadLine();
                    try
                    {
                        r = proxy.DJSignUp(username, password);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("si")) // Signin a DJ
                {
                    LogInResponse r;
                    string username, password;
                    Console.Write("Username: ");
                    username = Console.ReadLine();
                    Console.Write("Password: ");
                    password = Console.ReadLine();
                    try
                    {
                        r = proxy.DJSignIn(username, password);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                        DJKey = r.userKey;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("so")) // Signout a DJ
                {
                    Response r;
                    try
                    {
                        r = proxy.DJSignOut(DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("as")) // Add a song
                {
                    Response r;
                    string title, artist, pathOnDisk;
                    Console.WriteLine("Title: ");
                    title = Console.ReadLine();
                    Console.WriteLine("Artist: ");
                    artist = Console.ReadLine();
                    Console.WriteLine("Path on Disk: ");
                    pathOnDisk = Console.ReadLine();
                    try
                    {
                        Song s = new Song();
                        s.artist = artist;
                        s.title = title;
                        s.pathOnDisk = pathOnDisk;
                        List<Song> songs = new List<Song>();
                        songs.Add(s);
                        r = proxy.DJAddSongs(songs.ToArray(), DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("rs")) // Remove a song
                {
                    Response r;
                    string title, artist, pathOnDisk;
                    Console.WriteLine("Title: ");
                    title = Console.ReadLine();
                    Console.WriteLine("Artist: ");
                    artist = Console.ReadLine();
                    Console.WriteLine("Path on Disk: ");
                    pathOnDisk = Console.ReadLine();
                    try
                    {
                        Song s = new Song();
                        s.artist = artist;
                        s.title = title;
                        s.pathOnDisk = pathOnDisk;
                        List<Song> songs = new List<Song>();
                        songs.Add(s);
                        r = proxy.DJRemoveSongs(songs.ToArray(), DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("ls")) // List all Songs owned by the logged in DJ.
                {
                    try
                    {
                        Song[] songs;
                        Response r = proxy.DJListSongs(out songs, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                        Console.WriteLine("Songs: ");
                        if (!r.error)
                            foreach (Song s in songs)
                                Console.WriteLine(s.ID + ", " + s.title + ", " + s.artist + ", " + s.pathOnDisk);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("lq")) // List the song queue for the loggin in DJ.
                {
                    try
                    {
                        queueSinger[] queue;
                        Response r = proxy.DJGetQueue(out queue, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                        Console.WriteLine("Queue: ");
                        if (!r.error)
                            foreach (queueSinger qs in queue)
                            {
                                string id = "ID: " + qs.user.userID + ", Name: " + qs.user.userName;

                                Console.WriteLine("\n" + id.Trim());
                                foreach (Song s in qs.songs)
                                {
                                    string s2 = s.ID + ", " + s.title + ", " + s.artist + ", " + s.pathOnDisk;
                                    Console.WriteLine("  " + s2.Trim());
                                }
                            }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("pq")) // Pop the first song request off the queue for the loggin in DJ.
                {
                    try
                    {
                        SongRequest sr = new SongRequest();
                        User u = new User();
                        Console.WriteLine("Enter clientID:");
                        u.userID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter SongID:");
                        sr.songID = int.Parse(Console.ReadLine().Trim());
                        sr.user = u;

                        Response r = proxy.DJPopQueue(sr, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else
                {
                    Console.WriteLine("Invalid Command!");
                }
            }
        }
    }
}
