using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code.OAuth1
{
	public class DropboxOAuth1 : OAuth1Authenticator
	{
		#region RequestToken
		private const string _requestTokenUrl = @"https://api.dropbox.com/1/oauth/request_token";
		public override string RequestTokenUrl
		{
			get { return _requestTokenUrl; }
		}

		BaseRequestTokenResult _requestTokenResult;
		public override string RequestToken
		{
			get { return _requestTokenResult.oauth_token; }
		}

		public override string RequestTokenSecret
		{
			get { return _requestTokenResult.oauth_token_secret; }
		}
		#endregion

		public override string AccessTokenUrl
		{
			get { throw new NotImplementedException(); }
		}

		public override string UserAuthorizationBaseUrl
		{
			get { throw new NotImplementedException(); }
		}

		public DropboxOAuth1(string consumerKey, string consumerSecret, string callbackUrl)
			:base(consumerKey, consumerSecret, callbackUrl)
		{
		}

		public new BaseRequestTokenResult GetRequestToken()
		{
			string response = base.GetRequestToken();

			_requestTokenResult = new BaseRequestTokenResult(response);
			return _requestTokenResult;
		}
	}
}