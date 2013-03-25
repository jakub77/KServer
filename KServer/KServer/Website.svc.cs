using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;

namespace KServer
{
    public class Website : IWebsite
    {
        static readonly string mobiokeUsername = "mobioke@live.com";
        static readonly string mobiokePassword = "TeamWarpZone";

        /// <summary>
        /// Sends the username associated with the email address listed to the email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="role">The role: DJ or Mobile</param>
        /// <returns>The outcome of the operation.</returns>
        public Response SendEmailWithUsername(string email)
        {
            Response r;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return r;

                List<string> DJUsernames;
                List<string> mobileUsernames;

                r = db.DJGetUsernamesByEmail(email, out DJUsernames);
                if (r.error)
                    return r;

                r = db.MobileGetUsernamesByEmail(email, out mobileUsernames);
                if (r.error)
                    return r;

                List<string> usernames = new List<string>();
                List<string> roles = new List<string>();

                foreach (string djUsername in DJUsernames)
                {
                    usernames.Add(djUsername);
                    roles.Add("DJ");
                }
                foreach (string mobileUsername in mobileUsernames)
                {
                    usernames.Add(mobileUsername);
                    roles.Add("Singer");
                }

                try
                {
                    MailMessage mail = GenerateUsernameEmail(email, usernames, roles);
                    SmtpClient mailServer = new SmtpClient("smtp.live.com");
                    mailServer.Port = 25;
                    mailServer.UseDefaultCredentials = false;
                    mailServer.Credentials = new System.Net.NetworkCredential(mobiokeUsername, mobiokePassword);
                    mailServer.EnableSsl = true;
                    mailServer.Send(mail);
                    return r;
                }
                catch (Exception e)
                {
                    r.error = true;
                    r.message = "Exception in SendEmailWithUsername: " + e.Message;
                    return r;
                }                             
            }
        }

        /// <summary>
        /// Starts the password reset process for users who forgot their passwords.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="key">Out parameter for the unique key this user will temporarily be associated with.</param>
        /// <param name="role">The role: DJ or Mobile</param>
        /// <returns>The outcome of the operation.</returns>
        public Response StartPasswordReset(string email, string username, bool isDJ, string websiteAddress)
        {
            Response r = new Response();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return r;

                int ID;
                if (isDJ)
                {
                    r = db.DJValidateUsernameEmail(username, email, out ID);
                    if (r.error)
                        return r;
                }
                else
                {
                    r = db.MobileValidateUsernameEmail(username, email, out ID);
                    if (r.error)
                        return r;
                }

                if(ID == -1)
                {
                    r.error=true;
                    r.message="Username / email / Are you a DJ incorrect";
                    return r;
                }

                string random = Common.CreateSalt(32);
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                random = rgx.Replace(random, "x");
                int uniqueIsNegOne = 0;

                while (uniqueIsNegOne != -1)
                {
                    if (isDJ)
                    {
                        r = db.DJGetPasswordResetID(random, out uniqueIsNegOne);
                        if (r.error)
                            return r;
                    }
                    else
                    {
                        r = db.MobileGetPasswordResetID(random, out uniqueIsNegOne);
                        if (r.error)
                            return r;
                    }
                    random = Common.CreateSalt(32);
                    random = rgx.Replace(random, "x");
                }

                if (isDJ)
                {
                    r = db.DJSetPasswordReset(ID, random);
                    if (r.error)
                        return r;
                }
                else
                {
                    r = db.MobileSetPasswordReset(ID, random);
                    if (r.error)
                        return r;
                }

                try
                {
                    string resetURL = websiteAddress + "?DJ=" + isDJ.ToString() + "&key=" + random;
                    MailMessage mail = GeneratePasswordResetEmail(email, resetURL);
                    SmtpClient mailServer = new SmtpClient("smtp.live.com");
                    mailServer.Port = 25;
                    mailServer.UseDefaultCredentials = false;
                    mailServer.Credentials = new System.Net.NetworkCredential(mobiokeUsername, mobiokePassword);
                    mailServer.EnableSsl = true;
                    mailServer.Send(mail);
                    return r;
                }
                catch (Exception e)
                {
                    r.error = true;
                    r.message = "Exception in SendEmailWithUsername: " + e.Message;
                    return r;
                } 
            }
        }

        public Response ValidatePasswordResetKey(string key, bool isDJ, out int ID)
        {
            ID = -1;
            return new Response();
        }


        /// <summary>
        /// "Weblogin" to the system. Returns the user's ID upon success.
        /// </summary>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <param name="role">The role, DJ or Mobile</param>
        /// <param name="ID">Our parameter of the user ID.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response Login(string username, string password, string role, out int ID)
        {
            ID = 0;
            Response r = new Response();
            if (!role.Equals("DJ") && !role.Equals("Mobile"))
            {
                r.error = true;
                r.message = "Bad role";
                return r;
            }

            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return r;

                // Get the salt from the database and salt/hash the password.
                string salt;
                if (role == "DJ")
                    r = db.DJGetSalt(username, out salt);
                else
                    r = db.MobileGetSalt(username, out salt);
                if (r.error)
                    return r;
                string saltHashPassword = Common.CreatePasswordHash(password, salt);

                // Check validity of username/password.
                if (role == "DJ")
                    r = db.DJValidateUsernamePassword(username, saltHashPassword);
                else
                    r = db.MobileValidateUsernamePassword(username, saltHashPassword);
                if (r.error)
                    return r;

                // If the username/password couldn't be found, inform user.
                if (r.message.Trim() == string.Empty)
                {
                    r.error = true;
                    r.message = "Username/Password is incorrect.";
                    return r;
                }

                // Get the ID
                if (!int.TryParse(r.message.Trim(), out ID))
                {
                    r.error = true;
                    r.message = "Exception in ChangeEmail: Unable to parse ID from DB!";
                    return r;
                }

                return r;
            }
        }

        /// <summary>
        /// Change a user's password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="oldPassword">The old password.</param>
        /// <param name="newPassword">The new password.</param>
        /// <param name="role">The role: DJ or Mobile</param>
        /// <returns>The outcome of the operation.</returns>
        public Response ChangePassword(int ID, string role, string newPassword)
        {
            Response r = new Response();
            if (!role.Equals("DJ") && !role.Equals("Mobile"))
            {
                r.error = true;
                r.message = "Bad role";
                return r;
            }

            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return r;

                // Get the salt from the database and salt/hash the password.
                string salt = Common.CreateSalt(16);

                if (role == "DJ")
                    r = db.DJSetSalt(ID, salt);
                else
                    r = db.MobileSetSalt(ID, salt);

                if (r.error)
                    return r;

                string saltHashPassword = Common.CreatePasswordHash(newPassword, salt);

                if (role == "DJ")
                    r = db.DJSetPassword(ID, saltHashPassword);
                else
                    r = db.MobileSetPassword(ID, saltHashPassword);

                if (r.error)
                    return r;

                return r;
            }
        }

        /// <summary>
        /// Change a user's email.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="newEmail">The new email address.</param>
        /// <param name="role">The role, DJ or mobile.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response ChangeEmail(int ID, string role, string newEmail)
        {
            Response r = new Response();
            if (!role.Equals("DJ") && !role.Equals("Mobile"))
            {
                r.error = true;
                r.message = "Bad role";
                return r;
            }

            // Validate the email address.
            try
            {
                var address = new System.Net.Mail.MailAddress(newEmail);
            }
            catch
            {
                r.error = true;
                r.message = "Email address is not valid";
                return r;
            }

            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return r;

                if (role == "DJ")
                    r = db.DJSetEmail(ID, newEmail);
                else
                    r = db.MobileSetEmail(ID, newEmail);

                if (r.error)
                    return r;

                return r;
            }
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

        /// <summary>
        /// Enable or disable website registration to occur.
        /// </summary>
        /// <param name="enableRegistration">Whether registration is allowed.</param>
        /// <returns>The outcome of the operation.</returns>
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

        /// <summary>
        /// Check to see if website registration is allowed.
        /// </summary>
        /// <param name="registrationAllowed">Out parameter whether registration is allowed.</param>
        /// <returns>The outcome of the operation.</returns>
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

        private MailMessage GenerateUsernameEmail(string toEmailAddress, List<string> usernames, List<string> roles)
        {
            StringBuilder body = new StringBuilder();
            if(usernames.Count == 1)
                body.Append("Hello, we've got your Mobioke username!\n\n");
            if (usernames.Count > 1)
            {
                body.Append("Hello, we've got your Mobioke usernames!\n\n");
            }
            for (int i = 0; i < usernames.Count; i++)
            {
                if (roles[i] == "DJ")
                    body.Append("A DJ Account:\t\t" + usernames[i] + "\n");
                else
                    body.Append("A Singer Account:\t" + usernames[i] + "\n");
            }
            body.Append("\nRemember to properly check the \"Are you a DJ\" checkbox when logging in.\n\n");
            body.Append("We can't wait to see you singing again!\n\n");
            body.Append("  -Team Warp Zone");

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("Mobioke@live.com", "Mobioke");
            mail.To.Add(toEmailAddress);
            mail.Subject = "Your Mobioke username";
            mail.IsBodyHtml = false;
            mail.Body = body.ToString();
            return mail;
        }

        private MailMessage GeneratePasswordResetEmail(string toEmailAddress, string url)
        {
            StringBuilder body = new StringBuilder();
            body.Append("Hello, we're ready to reset your password in a couple easy steps.<br><br>");
            body.Append("Simply click on the following link:");
            body.Append("<a href=\"" + url + "\">Reset Password</a><br><br>");
            body.Append("After clicking, you will be prompted to enter a new password<br><br>");
            body.Append("If the link did not work, navigate to the following address using your web browser: ");
            body.Append(url + "<br><br>");
            body.Append("We can't wait to see you singing again!<br><br>");
            body.Append("  -Team Warp Zone");

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress("Mobioke@live.com", "Mobioke");
            mail.To.Add(toEmailAddress);
            mail.Subject = "Your Mobioke Password";
            mail.IsBodyHtml = true;
            mail.Body = body.ToString();
            return mail;
        }
    }
}

