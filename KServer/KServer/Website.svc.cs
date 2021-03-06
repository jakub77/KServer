﻿// Jakub Szpunar - U of U Spring 2013 Senior Project - Team Warp Zone
// This contains all the methods the website can call on the server.
// This implemented the service contract defined in IWebsite.cs

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
        /// <summary>
        /// Sends the username associated with the email address listed to the email address.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="role">The role: DJ or Mobile</param>
        /// <returns>The outcome of the operation.</returns>
        public Response SendEmailWithUsername(string email)
        {
            ExpResponse r = new ExpResponse();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                List<string> DJUsernames;
                List<string> mobileUsernames;

                r = db.DJGetUsernamesByEmail(email, out DJUsernames);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                r = db.MobileGetUsernamesByEmail(email, out mobileUsernames);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                if (DJUsernames.Count == 0 && mobileUsernames.Count == 0)
                {
                    r.setErMsg(true, Messages.MSG_EMAIL_NOT_FOUND);
                    return r;
                }

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
                    mailServer.Credentials = new System.Net.NetworkCredential(Settings.EMAIL_ADR, Settings.EMAIL_PSWD);
                    mailServer.EnableSsl = true;
                    mailServer.Send(mail);
                    return r;
                }
                catch (Exception e)
                {
                    r.setErMsgStk(true, e.Message, e.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_EMAIL_SERVER, Common.LogFile.Web);
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
            ExpResponse r = new ExpResponse();
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                int ID;
                if (isDJ)
                    r = db.DJValidateUsernameEmail(username, email, out ID);
                else
                    r = db.MobileValidateUsernameEmail(username, email, out ID);

                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                if(ID == -1)
                {
                    r.setErMsg(true, Messages.MSG_EMAIL_NOT_FOUND);
                    return r;
                }

                string random = Common.CreateSalt(32);
                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                random = rgx.Replace(random, "x");
                int uniqueIsNegOne = 0;

                while (uniqueIsNegOne != -1)
                {
                    if (isDJ)
                        r = db.DJGetPasswordResetID(random, out uniqueIsNegOne);
                    else
                        r = db.MobileGetPasswordResetID(random, out uniqueIsNegOne);

                    if(r.error)
                        return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                    random = Common.CreateSalt(32);
                    random = rgx.Replace(random, "x");
                }

                if (isDJ)
                    r = db.DJSetPasswordReset(ID, random);
                else
                    r = db.MobileSetPasswordReset(ID, random);

                if(r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);


                try
                {
                    string resetURL = websiteAddress + "?DJ=" + isDJ.ToString() + "&key=" + random;
                    MailMessage mail = GeneratePasswordResetEmail(email, resetURL);
                    SmtpClient mailServer = new SmtpClient("smtp.live.com");
                    mailServer.Port = 25;
                    mailServer.UseDefaultCredentials = false;
                    mailServer.Credentials = new System.Net.NetworkCredential(Settings.EMAIL_ADR, Settings.EMAIL_PSWD);
                    mailServer.EnableSsl = true;
                    mailServer.Send(mail);
                    return r;
                }
                catch (Exception e)
                {
                    r.setErMsgStk(true, e.Message, e.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_EMAIL_SERVER, Common.LogFile.Web);
                } 
            }
        }
        /// <summary>
        /// Retrieves the DJ or MobileID associated with a password reset key and then removes it from the DB.
        /// ID will be set to -1 if it was invalid.
        /// </summary>
        /// <param name="key">The key to remove.</param>
        /// <param name="isDJ">Whether this is for DJs or mobile users.</param>
        /// <param name="ID">The userID associated with this user.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response UsePasswordResetKey(string key, bool isDJ, out int ID)
        {
            Response r1 = ValidatePasswordResetKey(key, isDJ, out ID);
            if (r1.error)
                return r1;

            ExpResponse r = new ExpResponse();
            if (ID != -1)
            {
                using (DatabaseConnectivity db = new DatabaseConnectivity())
                {
                    r = db.OpenConnection();
                    if (r.error)
                        return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
                    if (isDJ)
                        r = db.DJClearPasswordResetID(ID, key);
                    else
                        r = db.MobileClearPasswordResetID(ID, key);
                    if (r.error)
                        return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
                }
            }
            return r;
        }
        /// <summary>
        /// Validates a password reset key to be valid. If it is valid, ID is set to the ID it represents,
        /// if it is not valid, ID is set to -1.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="isDJ">Whether this is for DJs or Mobile.</param>
        /// <param name="ID">The ID for the key.</param>
        /// <returns>The outcome of the operation.</returns>
        public Response ValidatePasswordResetKey(string key, bool isDJ, out int ID)
        {
            ExpResponse r = new ExpResponse();
            ID = -1;
            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                if(isDJ)
                {
                    r = db.DJGetPasswordResetID(key, out ID);
                    if (r.error)
                        return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
                }
                else
                {
                    r = db.MobileGetPasswordResetID(key, out ID);
                    if (r.error)
                        return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
                }

                return r;
            }
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
            ExpResponse r = new ExpResponse();
            if (!role.Equals("DJ") && !role.Equals("Mobile"))
            {
                r.setErMsgStk(true, "Bad Role Given", Environment.StackTrace);
                return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
            }

            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                // Get the salt from the database and salt/hash the password.
                string salt;
                if (role == "DJ")
                    r = db.DJGetSalt(username, out salt);
                else
                    r = db.MobileGetSalt(username, out salt);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_CRED_WRONG, Common.LogFile.Web);
                string saltHashPassword = Common.CreatePasswordHash(password, salt);

                // Check validity of username/password.
                if (role == "DJ")
                    r = db.DJValidateUsernamePassword(username, saltHashPassword);
                else
                    r = db.MobileValidateUsernamePassword(username, saltHashPassword);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                // If the username/password couldn't be found, inform user.
                if (r.message.Trim() == string.Empty)
                {
                    r.setErMsg(true, Messages.ERR_CRED_WRONG);
                    return r;
                }

                // Get the ID
                if (!int.TryParse(r.message.Trim(), out ID))
                {
                    r.setErMsgStk(true, "Exception in ChangeEmail: Unable to parse ID from DB!", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
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
            ExpResponse r = new ExpResponse();
            if (!role.Equals("DJ") && !role.Equals("Mobile"))
            {
                r.setErMsgStk(true, "Bad Role Given", Environment.StackTrace);
                return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
            }

            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                // Get the salt from the database and salt/hash the password.
                string salt = Common.CreateSalt(16);

                if (role == "DJ")
                    r = db.DJSetSalt(ID, salt);
                else
                    r = db.MobileSetSalt(ID, salt);

                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_CRED_WRONG, Common.LogFile.Web);

                string saltHashPassword = Common.CreatePasswordHash(newPassword, salt);

                if (role == "DJ")
                    r = db.DJSetPassword(ID, saltHashPassword);
                else
                    r = db.MobileSetPassword(ID, saltHashPassword);

                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

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
            ExpResponse r = new ExpResponse();
            if (!role.Equals("DJ") && !role.Equals("Mobile"))
            {
                r.setErMsgStk(true, "Bad Role Given", Environment.StackTrace);
                return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
            }

            // Validate the email address.
            try
            {
                var address = new System.Net.Mail.MailAddress(newEmail);
            }
            catch
            {
                r.setErMsg(true, Messages.ERR_BAD_EMAIL);
                return r;
            }

            using (DatabaseConnectivity db = new DatabaseConnectivity())
            {
                // Try to establish a database connection
                r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                if (role == "DJ")
                    r = db.DJSetEmail(ID, newEmail);
                else
                    r = db.MobileSetEmail(ID, newEmail);

                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

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
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                // Validate that username and password are not blank.
                if (username.Length == 0 || password.Length == 0)
                {
                    r.setErMsg(true, Messages.ERR_CRED_BLANK);
                    return r;
                }

                // Validate that username and password are not too long.
                if (username.Length > 20 || password.Length > 20)
                {
                    r.setErMsg(true, Messages.ERR_CRED_LONG);
                    return r;
                }

                // Try to see if the username already exists. If it does, inform the client.
                r = db.DJValidateUsername(username);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
                if (r.message.Trim() != string.Empty)
                {
                    r.setErMsg(true, Messages.ERR_CRED_TAKEN);
                    return r;
                }

                // Validate the email address.
                try
                {
                    var address = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    r.setErMsg(true, Messages.ERR_BAD_EMAIL);
                    return r;
                }

                if (venue == null)
                {
                    r.setErMsg(true, Messages.ERR_VEN_INFO_MISSING);
                    return r;
                }

                if (venue.venueName == null || venue.venueName.Length == 0)
                {
                    r.setErMsg(true, Messages.ERR_VEN_INFO_MISSING);
                    return r;
                }

                if (venue.venueName.Length > 20)
                {
                    r.setErMsg(true, Messages.ERR_VEN_INFO_LONG);
                    return r;
                }

                if (venue.venueAddress == null || venue.venueAddress.Length == 0)
                {
                    r.setErMsg(true, Messages.ERR_VEN_INFO_MISSING);
                    return r;
                }

                if (venue.venueAddress.Length > 100)
                {
                    r.setErMsg(true, Messages.ERR_VEN_INFO_LONG);
                    return r;
                }

                // Information seems valid, create a salt and hash the password.
                string salt = Common.CreateSalt(16);
                string hashSaltPassword = Common.CreatePasswordHash(password, salt);

                // Sign up the user.
                r = db.DJSignUp(username, hashSaltPassword, email, venue.venueName, venue.venueAddress, salt);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

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
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                // Validate that username and password are not blank.
                if (username.Length == 0 || password.Length == 0)
                {
                    r.setErMsg(true, Messages.ERR_CRED_BLANK);
                    return r;
                }

                // Validate that username and password are not too long.
                if (username.Length > 20 || password.Length > 20)
                {
                    r.setErMsg(true, Messages.ERR_CRED_LONG);
                    return r;
                }

                // Validate the email address.
                try
                {
                    var address = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    r.setErMsg(true, Messages.ERR_BAD_EMAIL);
                    return r;
                }

                // Try to see if the username already exists. If it does, inform the client.
                r = db.MobileValidateUsername(username);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
                if (r.message.Trim() != string.Empty)
                {
                    r.setErMsg(true, Messages.ERR_CRED_TAKEN);
                    return r;
                }

                // Create salt and hashed/salted password;
                string salt = Common.CreateSalt(16);
                string hashSaltPassword = Common.CreatePasswordHash(password, salt);

                // Information seems valid, sign up client and return successfulness.
                r = db.MobileSignUp(username, hashSaltPassword, email, salt);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
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
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                r = db.SetSetting("webRegistration", enableRegistration.ToString());
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
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
                ExpResponse r = db.OpenConnection();
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                string registrationAllowedString;
                r = db.GetSetting("webRegistration", out registrationAllowedString);
                if (r.error)
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);

                if (!bool.TryParse(registrationAllowedString, out registrationAllowed))
                {
                    r.setErMsgStk(true, "Could not read setting", Environment.StackTrace);
                    return Common.LogErrorRetNewMsg(r, Messages.ERR_SERVER, Common.LogFile.Web);
                }
                return r;
            }
        }
        /// <summary>
        /// Creates an email to send to the user that will contain the user's usernames and roles.
        /// </summary>
        /// <param name="toEmailAddress">Where to send to.</param>
        /// <param name="usernames">List of usernames.</param>
        /// <param name="roles">List of roles associated with the username.</param>
        /// <returns>The outcome of the operation.</returns>
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
                    body.Append("A DJ Account:\t\t\t" + usernames[i] + "\n");
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
        /// <summary>
        /// Creates an email to be setnt to the user that will contain a link to reset his or her password.
        /// </summary>
        /// <param name="toEmailAddress">Where to send to.</param>
        /// <param name="url">The url link that allows for password resets.</param>
        /// <returns>The outcome of the operation.</returns>
        private MailMessage GeneratePasswordResetEmail(string toEmailAddress, string url)
        {
            StringBuilder body = new StringBuilder();
            body.Append("Hello, we're ready to reset your password in a couple easy steps.<br><br>");
            body.Append("Simply click on the following link: ");
            body.Append("<a href=\"" + url + "\">Reset Password</a><br><br>");
            body.Append("After clicking, you will be prompted to enter a new password<br><br>");
            body.Append("If the link did not work, navigate to the following address using your web browser: ");
            body.Append(url + "<br><br>");
            body.Append("Please note, this link will only work once. After being clicked on, the link will be invalidated and you will need to request a password reset again<br><br>");
            body.Append("We can't wait to see you singing again!<br>");
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

