using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MobiokeWebSite.ServiceReference1;

namespace MobiokeWebSite.Account
{
    public partial class Manage2 : System.Web.UI.Page
    {
        WebsiteClient proxy;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["ID"] == null)
            {
                Response.Redirect("~/Account/Login2.aspx");
                return;
            }
            proxy = new WebsiteClient();
            AccountNameType.Text = "&nbspfor " + Session["Username"];
            if (((string)Session["Role"]) == "DJ")
                AccountNameType.Text += " (DJ)";
            else
                AccountNameType.Text += " (singer)";
            
        }

        protected void PasswordSubmit_Click(object sender, EventArgs e)
        {
            clearPasswordErrors();

            string oldPassword = OldPasswordTextBox.Text;
            string newPassword = NewPasswordTextBox.Text;
            string confirmNewPassword = ConfirmNewPasswordTextBox.Text;

            if(ErrorInPasswordInput(oldPassword, newPassword, confirmNewPassword))
                return;

            int ID;
            Response r = proxy.Login((string)Session["username"], oldPassword, (string)Session["role"], out ID);
            if (r.error)
            {
                if (r.message.Contains("Username/Password is incorrect."))
                    PasswordResultLabel.Text = "Old password is incorrect.";
                else
                    PasswordResultLabel.Text = "A Server error occured, please try again later.";
                return;
            }

            r = proxy.ChangePassword(ID, (string)Session["role"], newPassword);
            if (r.error)
            {
                PasswordResultLabel.Text = r.message;
                return;
            }

            PasswordResultLabel.Text = "Success!";
            return;
        }

        protected void EmailSubmit_Click(object sender, EventArgs e)
        {
            clearEmailErrors();
            string email = NewEmailTextBox.Text;
            if (ErrorInEmailInput(email))
                return;

            Response r = proxy.ChangeEmail((int)Session["ID"], (string)Session["role"], email);
            if (r.error)
            {
                EmailResultLabel.Text = r.message;
                return;
            }
            EmailResultLabel.Text = "Success!";
            return;
        }

        private bool ErrorInEmailInput(string email)
        {
            bool errorInForm = false;
            if (email.Length == 0)
            {
                NewEmailErrorLabel.Text = "You must enter an email address.";
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
                    NewEmailErrorLabel.Text = "That email address doesn't appear to be valid.";
                    errorInForm = true;
                }
            }
            return errorInForm;
        }

        private bool ErrorInPasswordInput(string oldPassword, string newPassword, string confirmNewPassword)
        {
            bool errorInForm = false;
            if (oldPassword.Length == 0)
            {
                OldPasswordErrorLabel.Text = "You must enter your old password";
                errorInForm = true;
            }

            if (newPassword.Length == 0)
            {
                NewPasswordErrorLabel.Text = "You must enter a password.";
                errorInForm = true;
            }
            else if (newPassword.Length < 6)
            {
                NewPasswordErrorLabel.Text = "Your password must be at least 6 characters long";
                errorInForm = true;
            }
            else if (newPassword.Length > 20)
            {
                NewPasswordErrorLabel.Text = "Your password may at most be twenty characters long.";
                errorInForm = true;
            }

            if (confirmNewPassword.Length == 0)
            {
                ConfirmNewPasswordErrorLabel.Text = "You must confirm your password.";
                errorInForm = true;
            }
            else if (!newPassword.Equals(confirmNewPassword))
            {
                ConfirmNewPasswordErrorLabel.Text = "Your passwords do not match.";
                errorInForm = true;
            }
            return errorInForm;
        }

        private void clearPasswordErrors()
        {
            OldPasswordErrorLabel.Text = string.Empty;
            NewPasswordErrorLabel.Text = string.Empty;
            ConfirmNewPasswordErrorLabel.Text = string.Empty;
        }

        private void clearEmailErrors()
        {
            NewEmailErrorLabel.Text = string.Empty;
        }
    }
}