using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MobiokeWebSite.ServiceReference1;

namespace MobiokeWebSite.Account
{
    public partial class FinishPasswordReset : System.Web.UI.Page
    {
        WebsiteClient proxy;
        int userResetID;
        bool isDJ;

        protected void Page_Load(object sender, EventArgs e)
        {
            proxy = new WebsiteClient();
            string key = Request.QueryString["key"];
            if(key == null || !bool.TryParse(Request.QueryString["DJ"], out isDJ))
            {
                Response.Redirect("~/Error.aspx");
                return;
            }

            Response r;
    
            r = proxy.ValidatePasswordResetKey(key, isDJ, out userResetID);
            if (userResetID == -1)
            {
                Response.Redirect("~/Error.aspx");
                return;
            }            
        }

        protected void Submit_Click(object sender, EventArgs e)
        {
            ClearErrors();
            string password = PasswordTextBox.Text;
            string confirmPassword = PasswordConfirmationTextBox.Text;
            if (ErrorInInput(password, confirmPassword))
                return;

            Response r;
            if (isDJ)
                r = proxy.ChangePassword(userResetID, "DJ", password);
            else
                r = proxy.ChangePassword(userResetID, "Mobile", password);
            if (r.error)
            {
                ResultLabel.Text = r.message;
                return;
            }
            ResultLabel.Text = "Your password has been changed.";
        }


        private bool ErrorInInput(string password, string confirmPassword)
        {
            bool errorInForm = false;

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
            return errorInForm;
        }

        private void ClearErrors()
        {
            PasswordErrorLabel.Text = string.Empty;
            PasswordConfirmationErrorLabel.Text = string.Empty;
        }
    }
}
