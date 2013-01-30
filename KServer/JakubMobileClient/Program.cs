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

            for (; ; )
            {
                Console.WriteLine("Enter command or [help]");
                string command = Console.ReadLine();

                if (command.StartsWith("h")) //HELP
                {
                    //Console.WriteLine("help<h>, quit<q>, insertDJ<id>, signinDJ<si>, signoutDJ <so>, \nlistDJS<ld>, addSong<as>, removeSong<rs>, listSongs<ls>, \nlistQueue<lq>, popQueue<pq>");
                    Console.WriteLine("help<h>, JSONOut<j>, quit<q>, signIn<si>, signOut<so>");
                }
                else if (command.StartsWith("j")) // QUIT
                {
                    displayJSON = !displayJSON;
                }
                else if (command.StartsWith("q")) // QUIT
                {
                    return;
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
