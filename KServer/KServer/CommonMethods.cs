using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KServer
{
    public class CommonMethods : IDisposable
    {
        public CommonMethods()
        {

        }

        public Response DJCheckStatus(int DJID, string desiredStatus, DatabaseConnectivity db)
        {
            Response r;
            int DJStatus, desired;
            bool notStatus = false;
            // Get the status of the DJ.
            r = db.DJGetStatus(DJID);
            if (r.error)
                return r;

            // Attempt to parse that status of the DJ.
            if (!int.TryParse(r.message.Trim(), out DJStatus))
            {
                r.error = true;
                r.message = "Exception in DJCheckStatus: Unable to parse status from DB!";
                return r;
            }

            if (desiredStatus[0] == '!')
            {
                notStatus = true;
                desiredStatus = desiredStatus.Substring(1);
            }

            if (!int.TryParse(desiredStatus, out desired))
            {
                r.error = true;
                r.message = "Exception in DJCheckStatus: Cannot parse desired Status";
                return r;
            }

            if (!notStatus)
            {
                if (DJStatus != desired)
                {
                    r.error = true;
                    if (desired == 0)
                        r.message = "You are not signed out.";
                    else if (desired == 1)
                        r.message = "You are not signed in.";
                    else
                        r.message = "You are in the wrong state, possibly not created a session?";
                    return r;
                }
            }
            else if (DJStatus == desired)
            {
                r.error = true;
                if (desired == 0)
                    r.message = "You are signed out and cannot do that.";
                else if (desired == 1)
                    r.message = "You are signed in and cannot do that.";
                else
                    r.message = "You are in the wrong state, do you have a session running?";
                return r;
            }

            r.result = DJStatus;
            return r;
        }

        void IDisposable.Dispose()
        {
            return;
        }
    }
}