// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// This is a program used for testing the DJ functionality of the server.
// This is not intended to be used in the final product.

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
    class Program
    {
        static void Main(string[] args)
        {
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
                    Console.WriteLine("help<h>, quit<q>, insertDJ<id>, signinDJ<si>, signoutDJ <so>,");
                    Console.WriteLine("listDJS<ld>, addSong<as>, removeSong<rs>, listSongs<ls>, listQueue<lq>,"); 
                    Console.WriteLine("popQueue<pq>, getQR<gq>, generateNewQR<nq>, addRequest<ar>, removeRequest<rr>");
                    Console.WriteLine("changeRequest<cr>, moveUser<mu>, removeUser<ru>, createSession<cs>");
                    Console.WriteLine("newUserWaitTime<nw>, testQueueFill<tf>, moveSongRequest<mr>, mostPopular<mp>");
                }
                else if (command.StartsWith("mp"))
                {
                    Response r;
                    Song[] songs;
                    int[] counts;
                    bool limitToVenue;
                    Console.WriteLine("Do you wish to limit results to this venue? [0=no,1=yes]");
                    string input = Console.ReadLine();
                    if (input == "0")
                        limitToVenue = false;
                    else
                        limitToVenue = true;

                    Console.WriteLine("Starting index");
                    int start = int.Parse(Console.ReadLine());
                    Console.WriteLine("Count");
                    int count = int.Parse(Console.ReadLine());

                    try
                    {
                        r = proxy.DJGetMostPopularSongs(out songs, out counts, DJKey, limitToVenue, start, count);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                        for (int i = 0; i < songs.Length; i++)
                        {
                            Console.WriteLine(songs[i].ID + " " + songs[i].title + " " + songs[i].artist + " " + counts[i]);
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("tf"))
                {
                    Response r;
                    try
                    {
                        r = proxy.DJTestQueueFill(DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("nw"))
                {
                    Response r;
                    try
                    {
                        r = proxy.DJNewUserWaitTime(DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("cs"))
                {
                    Response r;
                    try
                    {
                        r = proxy.DJCreateSession(DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("gq"))
                {
                    Response r;
                    try
                    {
                        r = proxy.DJGetQRNumber(DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("nq"))
                {
                    Response r;
                    try
                    {
                        r = proxy.DJGenerateNewQRNumber(DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message);
                    }
                }
                else if (command.StartsWith("x"))
                {
                    try
                    {
                        Console.WriteLine("password");
                        string plainPassword = Console.ReadLine().Trim();
                        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
                        byte[] buf = new byte[16];
                        rng.GetBytes(buf);
                        string salt = Convert.ToBase64String(buf);
                        Console.WriteLine(salt);

                        HashAlgorithm algorithm = new SHA256Managed();
                        string preHash = plainPassword + salt;

                        byte[] postHash = algorithm.ComputeHash(System.Text.Encoding.UTF8.GetBytes(preHash));
                        Console.WriteLine(Convert.ToBase64String(postHash));

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
                        r = proxy.DJSignUp("list", String.Empty, new Venue(), String.Empty);
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
                    string username, password, email;
                    Venue venue = new Venue();
                    Console.WriteLine("Username: ");
                    username = Console.ReadLine();
                    Console.WriteLine("Password: ");
                    password = Console.ReadLine();
                    Console.WriteLine("Email:");
                    email = Console.ReadLine();
                    Console.WriteLine("Venue Name:");
                    venue.venueName = Console.ReadLine();
                    Console.WriteLine("Venue Address:");
                    venue.venueAddress = Console.ReadLine();

                    try
                    {
                        r = proxy.DJSignUp(username, password, venue, email);
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
                    try
                    {
                        Response r;
                        string title, artist, pathOnDisk;
                        Console.WriteLine("Title: ");
                        title = Console.ReadLine();
                        Console.WriteLine("Artist: ");
                        artist = Console.ReadLine();
                        Console.WriteLine("Path on Disk: ");
                        pathOnDisk = Console.ReadLine();
                        Console.WriteLine("Duration: ");
                        int duration = int.Parse(Console.ReadLine());
                        Song s = new Song();
                        s.artist = artist;
                        s.title = title;
                        s.pathOnDisk = pathOnDisk;
                        s.duration = duration;
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
                                Console.WriteLine(s.ID + ", " + s.title + ", " + s.artist + ", " + s.pathOnDisk + ", " + s.duration);
                        Console.WriteLine("Result: " + r.result);
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
                else if (command.StartsWith("ar")) // Add a song request.
                {
                    try
                    {
                        SongRequest sr = new SongRequest();
                        User u = new User();
                        Console.WriteLine("Type ClientID, or 0 to type in Client Name, or -1 to add a temp user.");
                        u.userID = int.Parse(Console.ReadLine().Trim());
                        if (u.userID == 0)
                        {
                            Console.WriteLine("Enter the client name:");
                            u.userName = Console.ReadLine().Trim();
                        }
                        if (u.userID == -1)
                        {
                            Console.WriteLine("Enter the temp client name:");
                            u.userName = Console.ReadLine().Trim();
                        }
                        Console.WriteLine("Enter SongID:");
                        sr.songID = int.Parse(Console.ReadLine().Trim());
                        sr.user = u;
                        Console.WriteLine("enter index to insert client into:");
                        int index = int.Parse(Console.ReadLine().Trim());

                        Response r = proxy.DJAddQueue(sr, index, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("rr")) // Remove song request.
                {
                    try
                    {
                        SongRequest sr = new SongRequest();
                        User u = new User();
                        Console.WriteLine("Enter ClientID:");
                        u.userID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter SongID:");
                        sr.songID = int.Parse(Console.ReadLine().Trim());
                        sr.user = u;

                        Response r = proxy.DJRemoveSongRequest(sr, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("cr")) // Change song request.
                {
                    try
                    {
                        SongRequest newSR = new SongRequest();
                        SongRequest oldSR = new SongRequest();
                        User u = new User();
                        Console.WriteLine("Enter ClientID:");
                        u.userID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter Old Song ID:");
                        oldSR.songID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter New Song ID:");
                        newSR.songID = int.Parse(Console.ReadLine().Trim());
                        oldSR.user = u;
                        newSR.user = u;

                        Response r = proxy.DJChangeSongRequest(newSR, oldSR, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("mr")) // Move a song request in the list of songs.
                {
                    try
                    {
                        SongRequest sr = new SongRequest();
                        User u = new User();
                        Console.WriteLine("Enter ClientID:");
                        u.userID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter song ID:");
                        sr.songID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter new index:");
                        int index = int.Parse(Console.ReadLine());
                        sr.user = u;
                        Response r = proxy.DJMoveSongRequest(sr, index, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("mu")) // Move user
                {
                    try
                    {
                        Console.WriteLine("Enter ClientID:");
                        int userID = int.Parse(Console.ReadLine().Trim());
                        Console.WriteLine("Enter new index:");
                        int index = int.Parse(Console.ReadLine().Trim());

                        Response r = proxy.DJMoveUser(userID, index, DJKey);
                        Console.WriteLine("Error: " + r.error);
                        Console.WriteLine("Result: " + r.result);
                        Console.WriteLine("Message:\n" + r.message);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Exception: " + e.Message + "\n" + e.ToString());
                    }
                }
                else if (command.StartsWith("ru")) // Remove user.
                {
                    try
                    {
                        Console.WriteLine("Enter ClientID:");
                        int userID = int.Parse(Console.ReadLine().Trim());

                        Response r = proxy.DJRemoveUser(userID, DJKey);
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
