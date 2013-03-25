using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MobiokeWebSite.ServiceReference1;

namespace MobiokeWebSite.Account
{
    public partial class ForgotUsername : System.Web.UI.Page
    {
        WebsiteClient proxy;

        protected void Page_Load(object sender, EventArgs e)
        {
            proxy = new WebsiteClient();
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            ClearEmailErrors();
            string email = EmailTextBox.Text;
            if (ErrorInEmailInput(email))
                return;

            Response r = proxy.SendEmailWithUsername(email);
            if(r.error)
            {
                ResultLabel.Text = r.message;
                return;
            }
            ResultLabel.Text = "Please check your email for your account name";
        }


        private bool ErrorInEmailInput(string email)
        {
            bool errorInForm = false;
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
            EmailErrorLabel.Text = string.Empty;
        }


    }
}