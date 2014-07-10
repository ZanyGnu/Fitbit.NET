﻿using Fitbit;
using Fitbit.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SampleWebMVC.Portable.Controllers
{
    public class FitbitController : Controller
    {
        //
        // GET: /Fitbit/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /FitbitAuth/
        // Setup - prepare the user redirect to Fitbit.com to prompt them to authorize this app.
        public async Task<ActionResult> Authorize()
        {

            //make sure you've set these up in Web.Config under <appSettings>:
            string ConsumerKey = ConfigurationManager.AppSettings["FitbitConsumerKey"];
            string ConsumerSecret = ConfigurationManager.AppSettings["FitbitConsumerSecret"];


            
            Fitbit.Api.Authenticator authenticator = new Fitbit.Api.Authenticator(ConsumerKey,
                                                                                    ConsumerSecret,
                                                                                    "http://api.fitbit.com/oauth/request_token",
                                                                                    "http://api.fitbit.com/oauth/access_token",
                                                                                    "http://api.fitbit.com/oauth/authorize");
             

            RequestToken token = await authenticator.GetRequestTokenAsync();
            Session.Add("FitbitRequestTokenSecret", token.Secret.ToString()); //store this somehow, like in Session as we'll need it after the Callback() action

            //note: at this point the RequestToken object only has the Token and Secret properties supplied. Verifier happens later.

            string authUrl = authenticator.GenerateAuthUrlFromRequestToken(token, true, false);


            return Redirect(authUrl);
        }

        //Final step. Take this authorization information and use it in the app
        public async Task<ActionResult> Callback()
        {
            RequestToken token = new RequestToken();
            token.Token = Request.Params["oauth_token"];
            token.Secret = Session["FitbitRequestTokenSecret"].ToString();
            token.Verifier = Request.Params["oauth_verifier"];

            string ConsumerKey = ConfigurationManager.AppSettings["FitbitConsumerKey"];
            string ConsumerSecret = ConfigurationManager.AppSettings["FitbitConsumerSecret"];

            //this is going to go back to Fitbit one last time (server to server) and get the user's permanent auth credentials

            //create the Authenticator object
            Fitbit.Api.Authenticator authenticator = new Fitbit.Api.Authenticator(ConsumerKey,
                                                                                    ConsumerSecret,
                                                                                    "http://api.fitbit.com/oauth/request_token",
                                                                                    "http://api.fitbit.com/oauth/access_token",
                                                                                    "http://api.fitbit.com/oauth/authorize");


            //execute the Authenticator request to Fitbit
            AuthCredential credential = await authenticator.ProcessApprovedAuthCallbackAsync(token);

            //here, we now have everything we need for the future to go back to Fitbit's API (STORE THESE):
            //  credential.AuthToken;
            //  credential.AuthTokenSecret;
            //  credential.UserId;

            // For demo, put this in the session managed by ASP.NET
            Session["FitbitAuthToken"] = credential.AuthToken;
            Session["FitbitAuthTokenSecret"] = credential.AuthTokenSecret;
            Session["FitbitUserId"] = credential.UserId;

            return RedirectToAction("Index", "Home");

        }

        public async Task<string> TestTimeSeries()
        {
            FitbitClient client = GetFitbitClient();

            var results = await client.GetTimeSeriesIntAsync(TimeSeriesResourceType.DistanceTracker, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, null);

            string sOutput = "";
            foreach (var result in results.DataList)
            {
                sOutput += result.DateTime.ToString() + " - " + result.Value.ToString();
            }

            return sOutput;

        }


        public async Task<ActionResult> LastWeekSteps()
        {
            FitbitClient client = GetFitbitClient();

            TimeSeriesDataListInt results = await client.GetTimeSeriesIntAsync(TimeSeriesResourceType.Steps, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            return View(results);

        }

        /*

        public ActionResult LastWeekDistance()
        {
            FitbitClient client = GetFitbitClient();

            TimeSeriesDataList results = client.GetTimeSeries(TimeSeriesResourceType.Distance, DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            return View(results);
        }


        //example using the direct API call getting all the individual logs
        public ActionResult MonthFat(string id)
        {
            DateTime dateStart = Convert.ToDateTime(id);

            FitbitClient client = GetFitbitClient();

            Fat fat = client.GetFat(dateStart, DateRangePeriod.OneMonth);

            if (fat == null || fat.FatLogs == null) //succeeded but no records
            {
                fat = new Fat();
                fat.FatLogs = new List<FatLog>();
            }
            return View(fat);

        }

        //example using the time series, one per day
        public ActionResult LastYearFat()
        {
            FitbitClient client = GetFitbitClient();

            TimeSeriesDataList fatSeries = client.GetTimeSeries(TimeSeriesResourceType.Fat, DateTime.UtcNow, DateRangePeriod.OneYear);

            return View(fatSeries);

        }

        //example using the direct API call getting all the individual logs
        public ActionResult MonthWeight(string id)
        {
            DateTime dateStart = Convert.ToDateTime(id);

            FitbitClient client = GetFitbitClient();

            Weight weight = client.GetWeight(dateStart, DateRangePeriod.OneMonth);

            if (weight == null || weight.Weights == null) //succeeded but no records
            {
                weight = new Weight();
                weight.Weights = new List<WeightLog>();
            }
            return View(weight);

        }

        //example using the time series, one per day

        public ActionResult LastYearWeight()
        {
            FitbitClient client = GetFitbitClient();

            TimeSeriesDataList weightSeries = client.GetTimeSeriesAsync(TimeSeriesResourceType.Weight, DateTime.UtcNow, DateRangePeriod.OneYear);

            return View(weightSeries);

        }
        */
        /// <summary>
        /// This requires the Fitbit staff approval of your app before it can be called
        /// </summary>
        /// <returns></returns>

        private FitbitClient GetFitbitClient()
        {
            FitbitClient client = new FitbitClient(ConfigurationManager.AppSettings["FitbitConsumerKey"],
                ConfigurationManager.AppSettings["FitbitConsumerSecret"],
                Session["FitbitAuthToken"].ToString(),
                Session["FitbitAuthTokenSecret"].ToString());

            return client;
        }
    }
}