using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleWebMVC.Portable.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {

            if (Session["FitbitAuthToken"] == null ||
                Session["FitbitAuthTokenSecret"] == null ||
                Session["FitbitUserId"] == null)
            {
                ViewBag.FitbitConnected = false;
            }
            else
            {
                ViewBag.FitbitConnected = true; // "Welcome Fitbit User " + Session["FitbitUserId"].ToString();
            }

            return View();
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