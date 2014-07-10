using AsyncOAuth;
using Fitbit.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Fitbit
{
    public class FitbitClient
    {
        private string consumerKey;
        private string consumerSecret;
        private string accessToken;
        private string accessSecret;
        private IRestClient restClient;

        private string baseApiUrl = "https://api.fitbit.com";

        public FitbitClient(IRestClient restClient)
        {
            this.restClient = restClient;
            //restClient.Authenticator = OAuth1Authenticator.ForProtectedResource(this.consumerKey, this.consumerSecret, this.accessToken, this.accessSecret);

        }

        public FitbitClient(string consumerKey, string consumerSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
        }


        /// <summary>
        /// Initialize the FitbitClient using the provided access and the default API endpoint and RestSharp RestClient
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessSecret"></param>
        public FitbitClient(string consumerKey, string consumerSecret, string accessToken, string accessSecret)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.accessToken = accessToken;
            this.accessSecret = accessSecret;
            this.restClient = new RestClient(baseApiUrl);

            //restClient.Authenticator = authenticator; //OAuth1Authenticator.ForProtectedResource(this.consumerKey, this.consumerSecret, this.accessToken, this.accessSecret);

        }

        /// <summary>
        /// Initialize the FitbitClient using the provided access and specifying an IRestClient (good for testing)
        /// </summary>
        /// <param name="consumerKey"></param>
        /// <param name="consumerSecret"></param>
        /// <param name="accessToken"></param>
        /// <param name="accessSecret"></param>
        /// <param name="restClient"></param>
        public FitbitClient(string consumerKey, string consumerSecret, string accessToken, string accessSecret, IRestClient restClient)
        {
            this.consumerKey = consumerKey;
            this.consumerSecret = consumerSecret;
            this.accessToken = accessToken;
            this.accessSecret = accessSecret;
            this.restClient = restClient;

            //restClient.Authenticator = OAuth1Authenticator.ForProtectedResource(this.consumerKey, this.consumerSecret, this.accessToken, this.accessSecret);
        }

        /// <summary>
        /// Get TimeSeries data for this authenticated user as integer
        /// </summary>
        /// <param name="timeSeriesResourceType"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<TimeSeriesDataListInt> GetTimeSeriesIntAsync(TimeSeriesResourceType timeSeriesResourceType, DateTime startDate, DateTime endDate)
        {
            return await GetTimeSeriesIntAsync(timeSeriesResourceType, startDate, endDate, null);
        }

        public async Task<TimeSeriesDataListInt> GetTimeSeriesIntAsync(TimeSeriesResourceType timeSeriesResourceType, DateTime startDate, DateTime endDate, string userId)
        {
            return await GetTimeSeriesIntAsync(timeSeriesResourceType, startDate, endDate.ToString("yyyy-MM-dd"), userId);
        }

        public async Task<TimeSeriesDataListInt> GetTimeSeriesIntAsync(TimeSeriesResourceType timeSeriesResourceType, DateTime endDate, DateRangePeriod period)
        {
            return await GetTimeSeriesIntAsync(timeSeriesResourceType, endDate, period, null);
        }

        public async Task<TimeSeriesDataListInt> GetTimeSeriesIntAsync(TimeSeriesResourceType timeSeriesResourceType, DateTime endDate, DateRangePeriod period, string userId)
        {
            return await GetTimeSeriesIntAsync(timeSeriesResourceType, endDate, StringEnum.GetStringValue(period), userId);
        }

        public async Task<TimeSeriesDataListInt> GetTimeSeriesIntAsync(TimeSeriesResourceType timeSeriesResourceType, DateTime baseDate, string endDateOrPeriod, string userId)
        {
            string userSignifier = "-"; //used for current user
            if (!string.IsNullOrWhiteSpace(userId))
                userSignifier = userId;

            string requestUrl = string.Format("/1/user/{0}{1}/date/{2}/{3}.xml", userSignifier, StringEnum.GetStringValue(timeSeriesResourceType), baseDate.ToString("yyyy-MM-dd"), endDateOrPeriod);
            RestRequest request = new RestRequest(requestUrl);


            AsyncOAuth.AccessToken accessToken = new AccessToken(this.accessToken, this.accessSecret);
            //var authParameters = OAuthUtility.BuildBasicParameters(this.consumerKey, this.consumerSecret, baseApiUrl + requestUrl, System.Net.Http.HttpMethod.Post, accessToken, null);

            System.Net.Http.HttpClient httpClient = OAuthUtility.CreateOAuthClient(this.consumerKey, this.consumerSecret, accessToken, null);

            
            HttpResponseMessage response = await httpClient.GetAsync(baseApiUrl + requestUrl);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();


            var deserializer = new RestSharp.Deserializers.XmlDeserializer();
            //var deserializer = new RestSharp.Deserializers.JsonDeserializer();

            //TimeSeriesResourceType type = TimeSeriesResourceType.Steps.GetRootElement();

            deserializer.RootElement = "activities-steps";


            List<TimeSeriesDataListInt.Data> result = deserializer.Deserialize<List<TimeSeriesDataListInt.Data>>(new RestResponse() { Content = responseBody });

            TimeSeriesDataListInt dataList = new TimeSeriesDataListInt();
            dataList.DataList = result;

            /*
            foreach(var authParam in authParameters)
            {
                request.AddHeader(authParam.Key, authParam.Value);
            }
             */

            /*
            request.OnBeforeDeserialization = resp =>
            {
                throw new Exception("data is:" + resp.Content);
                XDocument doc = XDocument.Parse(resp.Content);
                //IEnumerable<XElement> links = doc.Descendants("result");
                var rootElement = doc.Descendants("result").FirstOrDefault().Descendants().FirstOrDefault();

                request.RootElement = rootElement.Name.LocalName;

                //foreach (XElement link in links)
                //{
                //RemoveDuplicateElement(link, "category"); 
                //RemoveDuplicateElement(link, "click-commission"); 
                //RemoveDuplicateElement(link, "creative-height"); 
                //RemoveDuplicateElement(link, "creative-width"); 
                //}


            };

             */

            //request.RootElement = timeSeriesResourceType.GetRootElement();

            /*
            var response = await restClient.ExecuteAsync<TimeSeriesDataListInt>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
                throw new Exception(response.Content);

            //HandleResponseCode(response.StatusCode);
            /*
            */
            return dataList;

        }
    }
}
