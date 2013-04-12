using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TechnosilaMock.Code.OAuth1
{
	public class RequestTokenResult
	{
		public string oauth_token { get; set; }
		public string oauth_token_secret { get; set; }
		public string oauth_callback_confirmed { get; set; }

		public RequestTokenResult()
		{

		}

		public void Parse(string responseString)
		{
			string[] resultParams = responseString.Split(new char[] { '&' });

			oauth_token = resultParams.First(s => s.StartsWith("oauth_token")).Substring("oauth_token=".Length);
			oauth_token_secret = resultParams.First(s => s.StartsWith("oauth_token_secret")).Substring("oauth_token_secret=".Length);
			oauth_callback_confirmed = resultParams.First(s => s.StartsWith("oauth_callback_confirmed")).Substring("oauth_callback_confirmed=".Length);
		}
	}
}