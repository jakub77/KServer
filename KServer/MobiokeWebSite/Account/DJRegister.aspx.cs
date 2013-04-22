using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MobiokeWebSite.ServiceReference1;

namespace MobiokeWebSite.Account
{
    public partial class DJRegister : System.Web.UI.Page
    {
        WebsiteClient proxy;
        protected void Page_Load(object sender, EventArgs e)
        {
            proxy = new WebsiteClient();
            bool registrationAllowed;
            Response r = proxy.IsRegistrationAllowed(out registrationAllowed);
            if (r.error)
            {
                RegistrationLockedMessage.Text = r.message;
                RegisterSubmit.Enabled = false;
                return;
            }
            if (!registrationAllowed)
            {
                RegistrationLockedMessage.Text = "Registration is currently locked, please try back later.";
                RegisterSubmit.Enabled = false;
                return;
            }
            RegisterSubmit.Enabled = true;
        }



        protected void RegisterSubmit_Click(object sender, EventArgs e)
        {
            clearErrors();

            bool registrationAllowed;
            Response r = proxy.IsRegistrationAllowed(out registrationAllowed);
            if (r.error)
            {
                ResultLabel.Text = r.message;
                return;
            }
            if (!registrationAllowed)
            {
                ResultLabel.Text = "Registration is currently locked, please try back later.";
                return;
            }

            string username = UserNameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Text;
            string confirmPassword = PasswordConfirmationTextBox.Text;
            string venueName = VenueNameTextBox.Text;
            string venueAddress = VenueAddressTextBox.Text;
            if (ErrorInInput(username, email, password, confirmPassword, venueName, venueAddress))
                return;

            Venue v = new Venue();
            v.venueName = venueName;
            v.venueAddress = venueAddress;
            
            r = proxy.DJSignUp(username, password, v, email);
            if (r.error)
            {
                ResultLabel.Text = r.message;
                return;
            }

            int ID;
            r = proxy.Login(username, password, "Mobile", out ID);

            if (r.error)
            {
                Response.Redirect("~/Account/Login2.aspx");
                return;
            }

            Session["ID"] = ID;
            Session["Role"] = "Mobile";
            Session["Username"] = username;

            Response.Redirect("~/Account/Manage2.aspx");

            return;
        }

        private bool ErrorInInput(string username, string email, string password, string confirmPassword, string venueName, string venueAddress)
        {
            bool errorInForm = false;
            if (username.Length == 0)
            {
                UserNameErrorLabel.Text = "You must enter a user name.";
                errorInForm = true;
            }
            else if (username.Length > 20)
            {
                UserNameErrorLabel.Text = "Your username may at most be twenty characters long.";
                errorInForm = true;
            }

            if (email.Length == 0)
            {
                EmailErrorLabel.Text = "You must enter an email address.";
                errorInForm = true;
            }
            else
            {
                try
                {
                    var address = new System.Net.Mail.MailAddress(email);
                }
                catch
                {
                    EmailErrorLabel.Text = "That email address doesn't appear to be valid.";
                    errorInForm = true;
                }
            }

            if (password.Length == 0)
            {
                PasswordErrorLabel.Text = "You must enter a password.";
                errorInForm = true;
            }
            else if (password.Length < 6)
            {
                PasswordErrorLabel.Text = "Your password must be at least 6 characters long";
                errorInForm = true;
            }
            else if (password.Length > 20)
            {
                PasswordErrorLabel.Text = "Your password may at most be twenty characters long.";
                errorInForm = true;
            }

            if (confirmPassword.Length == 0)
            {
                PasswordConfirmationErrorLabel.Text = "You must confirm your password.";
                errorInForm = true;
            }
            else if (!password.Equals(confirmPassword))
            {
                PasswordConfirmationErrorLabel.Text = "Your passwords do not match.";
                errorInForm = true;
            }

            if (venueName.Length == 0)
            {
                VenueNameErrorLabel.Text = "You must enter a venue name, this name will be visiable to singers.";
                errorInForm = true;
            }
            else if (venueName.Length > 20)
            {
                VenueNameErrorLabel.Text = "Venue name may at most be twenty characters long.";
                errorInForm = true;
            }

            if (venueAddress.Length == 0)
            {
                VenueAddressErrorLabel.Text = "You must enter a venue address, this address will be visiable to singers.";
                errorInForm = true;
            }
            else if (venueAddress.Length > 100)
            {
                VenueAddressErrorLabel.Text = "Venue address may at most be one hundred characters long.";
                errorInForm = true;
            }
            return errorInForm;
        }
        private void clearErrors()
        {
            UserNameErrorLabel.Text = string.Empty;
            EmailErrorLabel.Text = string.Empty;
            PasswordErrorLabel.Text = string.Empty;
            PasswordConfirmationErrorLabel.Text = string.Empty;
            VenueNameErrorLabel.Text = string.Empty;
            VenueAddressErrorLabel.Text = string.Empty;
        }
    }
}