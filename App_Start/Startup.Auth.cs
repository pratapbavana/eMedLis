using System;
using System.Text;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Jwt;
using Microsoft.IdentityModel.Tokens;
using Owin;
using eMedLis.Models;

namespace eMedLis
{
    public partial class Startup
    {
        // ✅ JWT Configuration (hardcoded - no web.config needed)
        private const string JwtSecretKey = "GTYGDhgyuteYTYE56785HESF879EFUGEDFYH32UGDJKHukuyerewh";
        private const string JwtIssuer = "eMedLis";
        private const string JwtAudience = "eMedLisUsers";
        private const int JwtExpirationMinutes = 60;

        public void ConfigureAuth(IAppBuilder app)
        {
            // ============ EXISTING IDENTITY CONFIGURATION ============
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // ============ NEW: JWT BEARER AUTHENTICATION ============
            var keyBytes = Encoding.UTF8.GetBytes(JwtSecretKey);   // byte[]
            var signingKey = new SymmetricSecurityKey(keyBytes);

            app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions
            {
                AuthenticationMode = AuthenticationMode.Active,
                AllowedAudiences = new[] { JwtAudience },
                IssuerSecurityKeyProviders = new[]
                {
        // ✅ pass byte[] here, not SymmetricSecurityKey
        new SymmetricKeyIssuerSecurityKeyProvider(JwtIssuer, keyBytes)
    },
                TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = JwtIssuer,

                    ValidateAudience = true,
                    ValidAudience = JwtAudience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }
            });
        }

        // ✅ Helper method to get JWT settings (for use in JwtTokenHandler)
        public static class JwtSettings
        {
            public static string SecretKey => JwtSecretKey;
            public static string Issuer => JwtIssuer;
            public static string Audience => JwtAudience;
            public static int ExpirationMinutes => JwtExpirationMinutes;
        }
    }
}
