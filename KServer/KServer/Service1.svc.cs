using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace KServer
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple, IncludeExceptionDetailInFaults = true)]
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class Service1 : IService1
    {
        public LogInResponse DJSignUp(string userName, string password){ return null; }
        public LogInResponse DJSignIn(string userName, string password) { return null; }
        public Response DJSignOut(long DJKey) { return null; }

        public Session DJCreateSession(long DJKey) { return null; }

        // Song management

        public Response DJAddSongs(List<Song> songs, long DJKey) { return null; }

        public Response DJRemoveSongs(List<Song> songs, long DJKey) { return null; }

        // Queue management

        public Response DJAddQueue(SongRequest sr, int queueIndex, int sessionID, long DJKey) { return null; }

        public Response DJRemoveSongRequest(SongRequest sr, int sessionID, long DJKey) { return null; }

        public Response DJChangeSongRequest(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey) { return null; }

        public Response DJRemoveUser(SongRequest newSR, SongRequest oldSR, int sessionID, long DJKey) { return null; }

        public Response DJMoveUser(int userID, int sessionID, long DJKey) { return null; }

        // CLIENT STUFF
        // Log in etc

        public LogInResponse ClientSignUp(string userName, string password) { return null; }

        public LogInResponse ClientSignIn(string userName, string password) { return null; }

        public Response ClientSignOut(long userKey) { return null; }

        // Song lists

        public List<Song> Search(string title, string artist, int venueID) { return null; }

        public List<Song> Browse(char firstLetter, bool isArtist, int start, int count, int venueID) { return null; }

        // Song Requests

        public Response ClientSongRequest(SongRequest sr, long userKey) { return null; }

        public Response ClientRemoveSongRequest(SongRequest sr, long userKey) { return null; }

        public Response ClientChangeSongRequest(int prevSongID, SongRequest sr, long userKey) { return null; }
    }
}
