using AsyncOAuth;
using Fitbit.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security;
using System.Security.Principal;
using System.Security.Cryptography;

namespace Fitbit.Api
{
    public class Authenticator
    {
        const string FitBitBaseUrl = "https://api.fitbit.com";

        private string ConsumerKey;
        private string ConsumerSecret;
        private string RequestTokenUrl;
        private string AccessTokenUrl;
        private string AuthorizeUrl;

        //note: these removed as part of a major breaking change refactor
        //https://github.com/aarondcoleman/Fitbit.NET/wiki/Breaking-Change-on-1-24-2014-as-a-result-of-OAuth-update-in-Fitbit-API
        //private string RequestToken;
        //private string RequestTokenSecret;

        private readonly IRestClient client;

        public Authenticator(string ConsumerKey, string ConsumerSecret, string RequestTokenUrl, string AccessTokenUrl,
                             string AuthorizeUrl, IRestClient restClient = null)
        {
            this.ConsumerKey = ConsumerKey;
            this.ConsumerSecret = ConsumerSecret;
            this.RequestTokenUrl = RequestTokenUrl;
            this.AccessTokenUrl = AccessTokenUrl;
            this.AuthorizeUrl = AuthorizeUrl;
            client = restClient ?? new RestClient(FitBitBaseUrl);
            
            OAuthUtility.ComputeHash = (key, buffer) => { using (var hmac = new HMACSHA1(key)) { return hmac.ComputeHash(buffer); } };


        }

        public string GenerateAuthUrlFromRequestToken(Fitbit.Models.RequestToken token, bool forceLogoutBeforeAuth, bool showMobileVersion)
        {
            /*
            RestRequest request = null;

            if (forceLogoutBeforeAuth)
                request = new RestRequest("oauth/logout_and_authorize"); //this url will force the user to type in username and password
            else
                request = new RestRequest("oauth/authorize");           //this url will show allow/deny if a user is currently logged in
            request.AddParameter("oauth_token", token.Token);
            
            
            var url = client.BuildUri(request).ToString();
            */

            string url = FitBitBaseUrl;

            if (forceLogoutBeforeAuth)
                url += "/oauth/logout_and_authorize"; //this url will force the user to type in username and password
            else
                url += "/oauth/authorize";           //this url will show allow/deny if a user is currently logged in

            url += "?oauth_token=" + token.Token;

            if (showMobileVersion)
                url += "&display=touch";

            return url;
        }

        /// <summary>
        /// First step in the OAuth process is to ask Fitbit for a temporary request token. 
        /// From this you should store the RequestToken returned for later processing the auth token.
        /// </summary>
        /// <returns></returns>
        public async Task<Fitbit.Models.RequestToken> GetRequestTokenAsync()
        {
            //client.Authenticator = OAuth1Authenticator.ForRequestToken(this.ConsumerKey, this.ConsumerSecret);

            // create authorizer
            var authorizer = new OAuthAuthorizer(this.ConsumerKey, this.ConsumerSecret);

            // get request token
            var tokenResponse = await authorizer.GetRequestToken(FitBitBaseUrl + "/oauth/request_token");
            var requestToken = tokenResponse.Token;


            Fitbit.Models.RequestToken token = new Fitbit.Models.RequestToken();

            token.Token = requestToken.Key;
            token.Secret = requestToken.Secret;

            var data = tokenResponse.ExtraData;
            //if (tokenResponse.StatusCode != System.Net.HttpStatusCode.OK)
            //    throw new Exception("Request Token Step Failed");

            return token;
        }

        public async Task<AuthCredential> ProcessApprovedAuthCallbackAsync(Fitbit.Models.RequestToken token)
        {
            if (string.IsNullOrWhiteSpace(token.Token))
                throw new Exception("RequestToken.Token must not be null");
            //else if 

            var authorizer = new OAuthAuthorizer(this.ConsumerKey, this.ConsumerSecret);

            AsyncOAuth.RequestToken oauthRequestToken = new AsyncOAuth.RequestToken(token.Token, token.Secret);

            var authToken = await authorizer.GetAccessToken(this.AccessTokenUrl, oauthRequestToken, token.Verifier);

            /*
            if (response.StatusCode != HttpStatusCode.OK)
                throw new FitbitException(response.Content, response.StatusCode);
            */


            return new AuthCredential()
            {
                AuthToken = authToken.Token.Key,
                AuthTokenSecret = authToken.Token.Secret,
                UserId = authToken.ExtraData["encoded_user_id"].FirstOrDefault()
            };

            //Assert.NotNull(response);
            //Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            //request = new RestRequest("statuses/update.json", Method.POST);
            //request.AddParameter("status", "Hello world! " + DateTime.Now.Ticks.ToString());
            //client.Authenticator = OAuth1Authenticator.ForProtectedResource(
            //    consumerKey, consumerSecret, oauth_token, oauth_token_secret
            //);

            //response = client.Execute(request);

            //Assert.NotNull(response);
            //Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }


    }
}
