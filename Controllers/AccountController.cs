using eMedLis.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace eMedLis.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        // GET: Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        // POST: Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            using (var client = new HttpClient())
            {
                var apiBase =
                    ConfigurationManager.AppSettings["ApiBaseUrl"];

                var payload = new
                {
                    userName = model.UserName,
                    password = model.Password
                };

                var json = JsonConvert.SerializeObject(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(
                    $"{apiBase}/api/auth/login",
                    content);

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }

                var result = JsonConvert.DeserializeObject<LoginResponse>(
                    await response.Content.ReadAsStringAsync());

                if (result == null || !result.Success)
                {
                    ModelState.AddModelError(
                        "",
                        result?.Message ?? "Login failed");
                    return View(model);
                }

                if (result.Data == null)
                {
                    ModelState.AddModelError("", "Invalid login response");
                    return View(model);
                }

                string userData = JsonConvert.SerializeObject(new
                {
                    Roles = result.Data.Roles,
                    Token = result.Data.Token
                });
                // Create auth ticket
                var authTicket = new FormsAuthenticationTicket(
                    1,
                    result.Data.UserName,
                    DateTime.Now,
                    DateTime.Now.AddHours(8),
                    false,
                    userData
                );

                var encryptedTicket =
                    FormsAuthentication.Encrypt(authTicket);

                var cookie = new HttpCookie(
                    FormsAuthentication.FormsCookieName,
                    encryptedTicket)
                {
                    HttpOnly = true,
                    Path = FormsAuthentication.FormsCookiePath
                };

                Response.Cookies.Add(cookie);

                var identity = new FormsIdentity(authTicket);
                var principal = new GenericPrincipal(identity, result.Data.Roles.ToArray());
                HttpContext.User = principal;
            }

            string returnUrl = Request.QueryString["ReturnUrl"];

            if (!string.IsNullOrEmpty(returnUrl)
                && Url.IsLocalUrl(returnUrl)
                && !returnUrl.Contains("/Account/Login"))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}
