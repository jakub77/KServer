using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MobiokeWebSite.Account
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Session["ID"] = null;
            Session["Role"] = null;
            Session["Username"] = null;
            Response.Headers.Add("Refresh", "5;URL=/");
        }
    }
}