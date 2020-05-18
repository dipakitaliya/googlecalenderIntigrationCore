using Google_Calender.Models;
using Google_Calender.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Google_Calender.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            return ListEvents(DateTime.Now.Subtract(TimeSpan.FromDays(10)), DateTime.Now.AddHours(22 - DateTime.Now.Hour));
        }

        public ActionResult ListEvents(DateTime startDate, DateTime endDate)
        {
            UserProfile userProfile = null;
            using (var context = new CalSiteDbContext())
            {
                userProfile = context.UserProfiles.FirstOrDefault(c => c.UserName == User.Identity.Name);
            }

            if (userProfile == null) return RedirectToAction("Register", "Account");

            var authenticator = GetAuthenticator();

            var service = new GoogleCalendarServiceProxy(authenticator);
            var model = service.GetEvents(userProfile.Email, startDate, endDate);

            ViewBag.StartDate = startDate.ToShortDateString();
            ViewBag.EndDate = endDate.ToShortDateString();
            return View("Index", model);
        }
        private GoogleAuthenticator GetAuthenticator()
        {
            var authenticator = (GoogleAuthenticator)Session["authenticator"];

            if (authenticator == null || !authenticator.IsValid)
            {
                // Get a new Authenticator using the Refresh Token
                var refreshToken = new CalSiteDbContext()
                                        .GoogleRefreshTokens
                                        .FirstOrDefault(c => c.UserName == User.Identity.Name)
                                        .RefreshToken;
                authenticator = GoogleAutherizationHelper.RefreshAuthenticator(refreshToken);
                Session["authenticator"] = authenticator;
            }

            return authenticator;
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}