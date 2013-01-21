using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Mobile" in code, svc and config file together.
    //[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any, ConcurrencyMode = ConcurrencyMode.Multiple, SessionMode = SessionMode.)]
    //[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true, AddressFilterMode = AddressFilterMode.Any)]
    public class Mobile : IMobile
    {
        public Response test(string s)
        {
            Response r = new Response();
            for (int i = s.Length - 1; i >= 0; i--)
                r.message += s[i];
            r.result = s.Length;
            return r;
        }
        public Response MobileSignUp(string username, string password)
        {
            Response r = new Response();
            r.error = true;
            r.message = "MobileSignUp is not yet implemented.";
            return r;
        }
        public LogInResponse MobileSignIn(string username, string password)
        {
            LogInResponse r = new LogInResponse();
            r.error = true;
            r.message = "MobileSignIn is not yet implemented.";
            r.result = -1;
            r.userKey = 1337;
            User u = new User();
            u.userID = 3;
            u.userName = username;
            r.user = u;
            return r;
        }
        public Response MobileSignOut(long userKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "MobileSignOut is not yet implemented.";
            r.result = (int)userKey;
            return r;
        }

        public List<Song> MobileSongSearch(string title, string artist, int venueID)
        {
            List<Song> s = new List<Song>();
            return s;
        }
        public List<Song> MobileSongBrowse(char firstLetter, bool isArtist, int start, int count, int venueID)
        {
            List<Song> s = new List<Song>();
            Song song = new Song();
            song.artist = "Glitch Mob";
            song.title = "Fortune Days";
            song.pathOnDisk = "C:\\Songs\\Fortune Days";
            s.Add(song);
            Song song2 = new Song();
            song.artist = "Rammstein";
            song.title = "Du Hast";
            song.pathOnDisk = "C:\\Songs\\Du Hast";
            s.Add(song2);
            return s;
        }
        public Response MobileSongRequest(int songID, long userKey)
        {
            Response r = new Response();
            r.error = true;
            r.message = "MobileSongRequest is not yet implemented.";
            r.result = songID;
            return r;
        }
    }
}
