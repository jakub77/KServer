using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Website" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Website.svc or Website.svc.cs at the Solution Explorer and start debugging.
    public class Website : IWebsite
    {
        /// <summary>
        /// Validate a DJ's username. Result is stored in response.result.
        /// 1 indicates a valid username, 0 indicates invalid.
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The outcome of the operation</returns>
        public Response DJValidateUsername(string username)
        {
            return null;
        }

        /// <summary>
        /// Validate a Mobile username. Result is stores in response.result
        /// 1 indicates a valid username, 0 indicates invalid.
        /// </summary>
        /// <param name="username">The username</param>
        /// <returns>The outcome of the operation</returns>
        public Response MobileValidateUsername(string username)
        {
            return null;
        }

        /// <summary>
        /// Registers a DJ for the Mobioke service.
        /// If an error occurs, the response will describe the error.
        /// </summary>
        /// <param name="username">The username to use. Must not be in use by the service already</param>
        /// <param name="password">The password to use.</param>
        /// <param name="venue">Object that describes the DJ's venue.</param>
        /// <param name="email">The email address of the DJ</param>
        /// <returns>A Response object indicating the result of the operation.</returns>
        public Response DJSignUp(string username, string password, Venue venue, string email)
        {
            return null;
        }

        /// <summary>
        /// Sign a client up for the service. Will fail if username is already in user, or email is not formatted validly.
        /// </summary>
        /// <param name="username">Client username.</param>
        /// <param name="password">Client password.</param>
        /// <param name="email">Client email.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileSignUp(string username, string password, string email)
        {
            return null;
        }

        /// <summary>
        /// Create a request for a password change. If no error is returned, the message will have
        /// the unique key to pass into CompletePasswordChange to actually change the password.
        /// </summary>
        /// <param name="username">The username of the DJ.</param>
        /// <param name="password">The new password.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response DJRequestPasswordChange(string username, string password)
        {
            return null;
        }

        /// <summary>
        /// Create a request for a password change. If no error is returned, the message will have
        /// the unique key to pass into CompletePasswordChange to actually change the password.
        /// </summary>
        /// <param name="username">The username of the Mobile user.</param>
        /// <param name="password">The new password.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response MobileRequestPasswordChange(string username, string password)
        {
            return null;
        }

        /// <summary>
        /// Actually changes a user's password. Called after DJRequestPasswordChange or MobileRequestPasswordChange.
        /// Passed in the unique key sent from these methods.
        /// </summary>
        /// <param name="key">The unique key for this password reset.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response CompletePasswordChange(string key)
        {
            return null;
        }
    }
}

