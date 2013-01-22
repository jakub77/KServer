using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DJTestClient.ServiceReference1;
using System.ServiceModel;

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
            DJClient proxy = new DJClient();
            long DJKey = -1;

            for (; ; )
            {
                Console.WriteLine("Enter command or [help]");
                string command = Console.ReadLine();

                if (command.StartsWith("h")) //HELP
                {
                    Console.WriteLine("help<h>, quit<q>, insertDJ<id>, signinDJ<si>, signoutDJ <so>, \nlistDJS<ld>, addSong<as>, removeSong<rs>, listSongs<ls>");
                }
                else if (command.StartsWith("q")) // QUIT
                {
                    return;
                }
                else if (command.StartsWith("ld")) // LIST DJS
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
                else
                {
                    Console.WriteLine("Invalid Command!");
                }
            } 
        }
    }
}
