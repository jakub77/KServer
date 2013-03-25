using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MobiokeWebSite.ServiceReference1;

namespace MobiokeWebSite.Account
{
    public partial class Login2 : System.Web.UI.Page
    {
        WebsiteClient proxy;
        protected void Page_Load(object sender, EventArgs e)
        {
            proxy = new WebsiteClient();
        }

        protected void LoginSubmit_Click(object sender, EventArgs e)
        {
            clearErrors();

            string username = UserNameTextBox.Text;
            string password = PasswordTextBox.Text;
            string role;
            int ID;

            if (ErrorInInput(username, password))
                return;

            if (IsDJCheckBox.Checked)
                role = "DJ";
            else
                role = "Mobile";

            Response r = proxy.Login(username, password, role, out ID);
            if (r.error)
            {
                ResultLabel.Text = r.message;
                return;
            }

            Session["ID"] = ID;
            Session["Role"] = role;
            Session["Username"] = username;

            Response.Redirect("~/Account/Manage2.aspx");

            return;
        }

        private bool ErrorInInput(string username, string password)
        {
            bool errorInForm = false;
            if (username.Length == 0)
            {
                UserNameErrorLabel.Text = "You must enter a user name.";
                errorInForm = true;
            }

            if (password.Length == 0)
            {
                PasswordErrorLabel.Text = "You must enter a password.";
                errorInForm = true;
            }
            return errorInForm;
        }

        private void clearErrors()
        {
            UserNameErrorLabel.Text = string.Empty;
            PasswordErrorLabel.Text = string.Empty;
        }
    }
}