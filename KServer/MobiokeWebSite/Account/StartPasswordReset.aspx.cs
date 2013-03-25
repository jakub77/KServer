using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MobiokeWebSite.ServiceReference1;

namespace MobiokeWebSite.Account
{
    public partial class StartPasswordReset : System.Web.UI.Page
    {
        WebsiteClient proxy;

        protected void Page_Load(object sender, EventArgs e)
        {
            proxy = new WebsiteClient();
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            ClearEmailErrors();
            string username = UserNameTextBox.Text;
            string email = EmailTextBox.Text;
            bool isDJ = IsDJCheckBox.Checked;
            if (ErrorInEmailInput(username, email))
                return;

            string serverAddress = "http://" + Request.Url.Host;
            if (Request.Url.Port != 80)
                serverAddress += ":" + Request.Url.Port;
            serverAddress += "/FinishPasswordReset/";
            Response r = proxy.StartPasswordReset(email, username, isDJ, serverAddress);
            if (r.error)
            {
                ResultLabel.Text = r.message;
                return;
            }
            ResultLabel.Text = "Please check your email.";
        }


        private bool ErrorInEmailInput(string username, string email)
        {
            bool errorInForm = false;

            if (username.Length == 0)
            {
                UserNameErrorLabel.Text = "You must enter a username";
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
            return errorInForm;
        }

        private void ClearEmailErrors()
        {
            UserNameErrorLabel.Text = string.Empty;
            EmailErrorLabel.Text = string.Empty;
        }
    }
}
