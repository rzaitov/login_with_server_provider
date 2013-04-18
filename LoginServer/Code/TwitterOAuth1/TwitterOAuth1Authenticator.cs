using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;

namespace LoginServer.Code
{
	/// <summary>
	/// Документация по всем методам API https://dev.twitter.com/docs/api/1.1
	/// </summary>
	public class TwitterAuthenticator : OAuth1Authenticator
	{
		#region RequestToken
		//Подробнее об соответствующем методу API — https://dev.twitter.com/docs/api/1/post/oauth/request_token
		private const string _requestTokenUrl = @"https://api.twitter.com/oauth/request_token";
		public override string RequestTokenUrl
		{
			get { return _requestTokenUrl; }
		}

		private TwitterRequestTokenResult _requestTokenResult;
		public override string RequestToken
		{
			get
			{
				return _requestTokenResult.oauth_token;
			}
		}

		public override string RequestTokenSecret
		{
			get
			{
				return _requestTokenResult.oauth_token_secret;
			}
		}
		#endregion

		private const string _userAuthorizationBaseUrl = @"https://api.twitter.com/oauth/authenticate";
		public override string UserAuthorizationBaseUrl
		{
			get { return _userAuthorizationBaseUrl; }
		}

		#region AccessToken
		private const string _requestAccessTokenUrl = "https://api.twitter.com/oauth/access_token";
		public override string AccessTokenUrl
		{
			get { return _requestAccessTokenUrl; }
		}
		#endregion

		private const string _GetUsersShowUrl = @"http://api.twitter.com/1/users/show.json";

		public string OAuthVerifier { get; private set; }

		public TwitterAuthenticator(string consumerKey, string consumerSecret, string callbackUrl)
			:base(consumerKey, consumerSecret, callbackUrl)
		{

		}

		public new BaseRequestTokenResult GetRequestToken()
		{
			string result = base.GetRequestToken();

			_requestTokenResult = new TwitterRequestTokenResult(result);
			return _requestTokenResult;
		}

		public override Dictionary<string, string> GetAdditionalHeaderParametersForRequestToken()
		{
			Dictionary<string, string> headerParameters = new Dictionary<string, string>()
			{
				{"oauth_callback", CallbackUrl}
			};

			return headerParameters;
		}

		protected override Dictionary<string, string> GetRequestParametersForAccessToken()
		{
			Dictionary<string, string> requestParameters = new Dictionary<string, string>()
			{
				{"oauth_verifier", OAuthVerifier}
			};

			return requestParameters;
		}

		public string UsersShow(string screen_name, string oauth_token_secret)
		{
			string requestMethod = "GET";
			string getUsersShowUrl = string.Format("{0}?screen_name={1}", _GetUsersShowUrl, screen_name);

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(getUsersShowUrl);
			httpRequest.Method = requestMethod;

			Dictionary<string, string> requestParameters = new Dictionary<string, string>()
			{
				{"screen_name", screen_name},
				{"include_entities", "true"}
			};

			AuthorizationHeaderInfo authorizationHeaderInfo = GetAuthorizationHeaderInfo(requestMethod, _requestAccessTokenUrl, null, requestParameters, oauth_token_secret);
			httpRequest.Headers.Add(authorizationHeaderInfo.HeaderName, authorizationHeaderInfo.HeaderValue);

			return ReadResponseFrom(httpRequest);
		}

		#region ProviderServiceSpecific
		public void SetOauthVerifier(string oauth_verifier)
		{
			OAuthVerifier = oauth_verifier;
		}
		#endregion
	}
}