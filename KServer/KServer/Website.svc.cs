using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace KServer
{
    public class Website : IWebsite
    {
        /// <summary>
        /// Sends the username associated with the email address listed to the email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="role">The role: DJ or Mobile</param>
        /// <returns>The outcome of the operation.</returns>
        public Response SendEmailWithUsername(string email, string role)
        {
            return null;
        }

        /// <summary>
        /// Starts the password reset process for users who forgot their passwords.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="key">Out parameter for the unique key this user will temporarily be associated with.</param>
        /// <param name="role">The role: DJ or Mobile</param>
        /// <returns>The outcome of the operation.</returns>
        public Response StartPasswordReset(string email, out string key, string role)
        {
            key = String.Empty;
            return null;
        }

        /// <summary>
        /// Completes that password reset process for the user.
        /// </summary>
        /// <param name="key">The key associated with the user from StartPasswordReset</param>
        /// <param name="newPassword">The new password.</param>
        /// <returns>The outcome of the opeartion.</returns>
        public Response CompletePasswordReset(string key, string newPassword)
        {
            return null;
        }

        /// <summary>
        /// Change a user's password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="role">The role: DJ or Mobile</param>
        /// <returns>The outcome of the operation.</returns>
        public Response ChangePassword(string username, string oldPassword, string newPassword, string role)
        {
            return null;
        }

        /// <summary>
        /// Change a user's email.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="newEmail">The new email address.</param>
        /// <param name="role">The role, DJ or mobile.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response ChangeEmail(string username, string password, string newEmail, string role)
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
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                // Escape to allow the DJTestClient to list all DJ information
                // WILL BE REMOVED FOR RELEASE!
                if (username.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    Response listResponse = db.DJListMembers();
                    if (listResponse.error)
                        return listResponse;
                    if (r.error)
                        return r;
                    return listResponse;
                }

                // Validate that username and password are not blank.
                if (username.Length == 0 || password.Length == 0)
                {
                    r.error = true;
                    r.message = "Username or password is blank.";
                    return r;
                }

                // Validate that username and password are not too long.
                if (username.Length > 20 || password.Length > 20)
                {
                    r.error = true;
                    r.message = "Username or password is longer than 20 characters.";
                    return r;
                }

                // Try to see if the username already exists. If it does, inform the client.
                r = db.DJValidateUsername(username);
                if (r.error)
                    return r;
                if (r.message.Trim() != string.Empty)
                {
                    r.error = true;
                    r.message = "That username already exists.";
                    return r;
                }

                // Validate the email address.
                try
                {
                    var address = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    r.error = true;
                    r.message = "Email address is not valid";
                    return r;
                }

                if (venue == null)
                {
                    r.error = true;
                    r.message = "Venue information must be passed in.";
                    return r;
                }

                if (venue.venueName == null || venue.venueName.Length == 0)
                {
                    r.error = true;
                    r.message = "Venue name must be set";
                    return r;
                }

                if (venue.venueName.Length > 20)
                {
                    r.error = true;
                    r.message = "Venue name is longer than 20 characters.";
                    return r;
                }

                if (venue.venueAddress.Length > 100)
                {
                    r.error = true;
                    r.message = "Venue address is longer than 100 characters";
                    return r;
                }

                if (venue.venueAddress == null || venue.venueAddress.Length == 0)
                {
                    r.error = true;
                    r.message = "Venue address must be set";
                    return r;
                }

                // Information seems valid, create a salt and hash the password.
                string salt = Common.CreateSalt(16);
                string hashSaltPassword = Common.CreatePasswordHash(password, salt);

                // Sign up the user.
                r = db.DJSignUp(username, hashSaltPassword, email, venue.venueName, venue.venueAddress, salt);
                if (r.error)
                    return r;

                return r;
            }
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
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);

                // Escape to allow the MobileTestClient to list all Mobile information
                // WILL BE REMOVED FOR RELEASE!
                if (username.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    Response listResponse = db.MobileListMembers();
                    if (listResponse.error)
                        return (Response)Common.LogError(listResponse.message, Environment.StackTrace, listResponse, 0);
                    return listResponse;
                }

                // Validate that username and password are not blank.
                if (username.Length == 0 || password.Length == 0)
                {
                    r.error = true;
                    r.message = "Username or password is blank.";
                    return r;
                }

                // Validate that username and password are not too long.
                if (username.Length > 20 || password.Length > 20)
                {
                    r.error = true;
                    r.message = "Username or password is longer than 20 characters.";
                    return r;
                }

                // Validate the email address.
                try
                {
                    var address = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    r.error = true;
                    r.message = "Email address is not valid";
                    return r;
                }

                // Try to see if the username already exists. If it does, inform the client.
                r = db.MobileValidateUsername(username);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                if (r.message.Trim() != string.Empty)
                {
                    r.error = true;
                    r.message = "That username already exists.";
                    return r;
                }

                // Create salt and hashed/salted password;
                string salt = Common.CreateSalt(16);
                string hashSaltPassword = Common.CreatePasswordHash(password, salt);

                // Information seems valid, sign up client and return successfulness.
                r = db.MobileSignUp(username, hashSaltPassword, email, salt);
                if (r.error)
                    return (Response)Common.LogError(r.message, Environment.StackTrace, r, 0);
                return r;
            }
        }

        public Response EnableDisableRegistration(bool enableRegistration)
        {
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                r = db.SetSetting("webRegistration", enableRegistration.ToString());
                if (r.error)
                    return r;
                return r;
            }
        }

        public Response IsRegistrationAllowed(out bool registrationAllowed)
        {
            registrationAllowed = false;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                Response r = db.OpenConnection();
                if (r.error)
                    return r;

                string registrationAllowedString;
                r = db.GetSetting("webRegistration", out registrationAllowedString);
                if (r.error)
                    return r;

                if (!bool.TryParse(registrationAllowedString, out registrationAllowed))
                {
                    r.error = true;
                    r.message = "Could not read setting";
                    return r;
                }
                return r;
            }
        }
    }
}

