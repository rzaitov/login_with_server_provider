using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using System.Net;
using System.IO;
using TechnosilaMock.Code;

using LoginServer.Code;
using Newtonsoft.Json;

namespace TechnosilaMock.Controllers
{
    public class LoginController : Controller
    {
		private static BaseRequestTokenResult requestTokenResult;
		private static TwitterAccessTokenResult accessTokenResult;

		private static TwitterAuthenticator twOAuth;
        //
        // GET: /Login/

        public ActionResult Index()
        {
            return View();
        }

		public string ConfirmLogin(int userId, string access_token, int expires_in)
		{
			return string.Format(
@"userId: {0}
access_token: {1}
expires_in: {2}", userId, access_token, expires_in);
		}

		public string ConfirmCode(string code)
		{
			FileLog.Log(code);
			string accessToken = GetAccessToken("3541654", "ASgUuQdakGXgzDmsfaMy", code, "http://188.138.108.97/Login/ConfirmCode");

			return string.Format("code: {0}", code);
		}

		private string GetAccessToken(string appId, string clientSecret, string code, string redirectUri)
		{
			string getTockenUrl = @"https://oauth.vk.com/access_token?client_id={0}&client_secret={1}&code={2}&redirect_uri={3}&";
			string requestString = string.Format(getTockenUrl, appId, clientSecret, code, redirectUri);

			string result = null;
			WebRequest request = WebRequest.Create(requestString);
			using (WebResponse response = request.GetResponse())
			{
				using (Stream s = response.GetResponseStream())
				{
					using (StreamReader sReader = new StreamReader(s))
					{
						result = sReader.ReadToEnd();
					}
				}

				response.Close();
			}

			FileLog.Log(result);
			VkAuthInfo aInfo = JsonConvert.DeserializeObject<VkAuthInfo>(result);

			FileLog.Log(aInfo.ToString());

			return aInfo.access_token;
		}

		public string LoginFb(string state, string code)
		{
			throw new NotImplementedException();
		}

		private string ExchangeTheCodeForAnAccessToken(string appId, string appSecret, string code, string redirectUri)
		{
			string getTokenUrl = @"https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";
			string requestString = string.Format(getTokenUrl, appId, redirectUri, appId, code);

			string result = null;
			WebRequest request = WebRequest.Create(requestString);
			using (WebResponse response = request.GetResponse())
			{
				using (Stream s = response.GetResponseStream())
				{
					using (StreamReader sReader = new StreamReader(s))
					{
						result = sReader.ReadToEnd();
					}
				}

				response.Close();
			}

			FileLog.Log(result);

			return "HelloFromFacebook";

			throw new NotImplementedException();
		}

		public ActionResult GetRequestToken()
		{
			twOAuth = new TwitterAuthenticator(
				// real consumer key
				"VPV4S0Yly38DQGCnjzAJQ",
				// fake consumer key
				//"cChZNFj6T5R0TigYB9yd1w",

				// real consumer secret 
				"fg99T7lvSoAXLBcW0eqgPVcAHQHlWEKEso2WBP7E4lA",
				// fake consumer secret
				//"L8qq9PZyRg6ieKGEKhZolGC0vJWLw8iEJ88DRdyOg",

				// real callback
				"http://mysite.ru:1083/Login/TwitterCallback/"
				// face callback
				//"http://localhost/sign-in-with-twitter/"
				);

			requestTokenResult = twOAuth.GetRequestToken();

			return new RedirectResult(twOAuth.UserAuthorizationUrl);
		}

		public string TwitterCallback(string oauth_token, string oauth_verifier)
		{
			twOAuth.SetOauthVerifier(oauth_verifier);
			accessTokenResult = twOAuth.ExchangeRequestTokenToAccessToken();

			return accessTokenResult.oauth_token;
		}

		public string UserShow(string screen_name)
		{
			string result = twOAuth.UsersShow(screen_name, accessTokenResult.oauth_token_secret);

			return result;
		}
	}

	public class VkAuthInfo
	{
		public string access_token { get; set; }
		public int expires_in { get; set; }
		public int user_id { get; set; }
		public string error { get; set; }
		public string error_description { get; set; }

		public override string ToString()
		{
			return string.Format("access_token: {0}	expires_in: {1}	user_id: {2}	error: {3}	error_description: {4}", access_token, expires_in, user_id, error, error_description);
		}
	}
}
