using Google_Calender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;
using static Google_Calender.Models.Comman;

namespace Google_Calender.Controllers
{
    public class AccountController : Controller
    {
      [AllowAnonymous]
      public ActionResult Login(string returnurl)
        {
            ViewBag.ReturnUrl = returnurl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel login,string returnUrl)
        {
            if (ModelState.IsValid && WebSecurity.Login(login.UserName,login.Password,persistCookie:login.RememberMe))
            {
                return RedirectToAction("Index","Home");
            }
            ModelState.AddModelError("", "UserName and Password Invalid");
            return View(login);
        }
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { Email = model.Email });
                    WebSecurity.Login(model.UserName, model.Password);
                    string url = GoogleAutherizationHelper.GetAuthorizationUrl(model.Email);
                    Response.Redirect(url);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return View();
        }
        public ActionResult GoogleAuthorization(string code)
        {
            // Retrieve the authenticator and save it in session for future use
            var authenticator = GoogleAutherizationHelper.GetAuthenticator(code);
            Session["authenticator"] = authenticator;

            // Save the refresh token locally
            using (var dbContext = new CalSiteDbContext())
            {
                var userName = User.Identity.Name;
                var userRegistry = dbContext.GoogleRefreshTokens.FirstOrDefault(c => c.UserName == userName);

                if (userRegistry == null)
                {
                    dbContext.GoogleRefreshTokens.Add(
                        new GoogleRefreshToken()
                        {
                            UserName = userName,
                            RefreshToken = authenticator.RefreshToken
                        });
                }
                else
                {
                    userRegistry.RefreshToken = authenticator.RefreshToken;
                }

                dbContext.SaveChanges();
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            WebSecurity.Logout();

            return RedirectToAction("Index", "Home");
        }


    }
}