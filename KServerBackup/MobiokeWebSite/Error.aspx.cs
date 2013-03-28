using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MobiokeWebSite
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string error = Request.QueryString["error"];
            if (error != null)
                ErrorLabel.Text += error;
            else
                ErrorLabel.Text = string.Empty;
        }
    }
}