using CSMHomePage.Classes;
using CSMHomePage.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace CSMHomePage
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            Database.SetInitializer<csmContext>(null);
        }
        protected void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            log.Error(ex.Message);
            EmailTheException(ex);
        }

        private void EmailTheException(Exception ex)
        {
            Email.EmailTheException(ex);
        }
    }
}
