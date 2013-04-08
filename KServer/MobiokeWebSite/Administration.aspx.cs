using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MobiokeWebSite.ServiceReference1;
using System.Diagnostics;

namespace MobiokeWebSite
{
    public partial class Administration : System.Web.UI.Page
    {
        WebsiteClient proxy;
        protected void Page_Load(object sender, EventArgs e)
        {
            proxy = new WebsiteClient();
            try
            {
                upTimeLabel.Text = "Uptime at least: " + TimeSpan.FromMilliseconds(Environment.TickCount).ToString();
            }
            catch (Exception)
            {
                upTimeLabel.Text = "Uptime unknown";
            }
        }

        protected void enableRegistration_Click(object sender, EventArgs e)
        {
            if (adminUsername.Text != "jakub" || adminPassword.Text != "topsecret")
            {
                resultLabel.Text = "Bad credentials";
                return;
            }
            Response r = proxy.EnableDisableRegistration(true);
            if (r.error)
            {
                resultLabel.Text = "WCF Error";
                return;
            }
            resultLabel.Text = "Success";
        }

        protected void disableRegistration_Click(object sender, EventArgs e)
        {
            if (adminUsername.Text != "jakub" || adminPassword.Text != "topsecret")
            {
                resultLabel.Text = "Bad credentials";
                return;
            }
            Response r = proxy.EnableDisableRegistration(false);
            if (r.error)
            {
                resultLabel.Text = "WCF Error";
                return;
            }
            resultLabel.Text = "Success";
        }
    }
}