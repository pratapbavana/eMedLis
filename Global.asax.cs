using eMedLis.Helpers;
using eMedLis.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace eMedLis
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            try
            {
                ConfigurationHelper.ValidateJwtConfiguration();
                Debug.WriteLine("✓ Application startup: JWT Configuration validated successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"✗ Application startup error: {ex.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie =
                Context.Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie == null || string.IsNullOrEmpty(authCookie.Value))
                return;

            FormsAuthenticationTicket ticket;

            try
            {
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return;
            }

            if (ticket == null || ticket.Expired)
                return;

            // Restore identity
            var identity = new FormsIdentity(ticket);

            // Restore roles from UserData
            string[] roles = new string[] { };

            if (!string.IsNullOrEmpty(ticket.UserData))
            {
                var userData = JsonConvert.DeserializeObject<AuthUserData>(ticket.UserData);
                roles = userData?.Roles?.ToArray() ?? new string[] { };
            }

            var principal = new GenericPrincipal(identity, roles);

            Context.User = principal;
        }
    }
}
