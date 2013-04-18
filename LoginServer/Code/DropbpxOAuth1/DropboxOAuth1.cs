using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code.OAuth1
{
	/*
	public class DropboxOAuth1 : OAuth1Authenticator
	{
		private const string _requestTokenUrl = @"https://api.dropbox.com/1/oauth/request_token";
		public override string RequestTokenUrl
		{
			get { return _requestTokenUrl; }
		}

		public override string AccessTokenUrl
		{
			get { throw new NotImplementedException(); }
		}

		public override string UserAuthorizationBaseUrl
		{
			get { throw new NotImplementedException(); }
		}

		public override string RequestToken
		{
			get { throw new NotImplementedException(); }
		}

		public override string RequestTokenSecret
		{
			get { throw new NotImplementedException(); }
		}

		public new BaseRequestTokenResult GetRequestToken()
		{
			string response = base.GetRequestToken();

			BaseRequestTokenResult result = new BaseRequestTokenResult(response);
			return result;
		}
	}
	*/
}