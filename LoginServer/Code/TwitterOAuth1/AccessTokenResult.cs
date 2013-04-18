using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	public class AccessTokenResult
	{
		public string oauth_token { get; set; }
		public string oauth_token_secret { get; set; }
		public string user_id { get; set; }
		public string screen_name { get; set; }

		public void Parse(string responseString)
		{
			string[] resultParams = responseString.Split(new char[] { '&' });

			oauth_token = resultParams.First(s => s.StartsWith("oauth_token")).Substring("oauth_token=".Length);
			oauth_token_secret = resultParams.First(s => s.StartsWith("oauth_token_secret")).Substring("oauth_token_secret=".Length);
			user_id = resultParams.First(s => s.StartsWith("user_id")).Substring("user_id=".Length);
			user_id = resultParams.First(s => s.StartsWith("screen_name")).Substring("screen_name=".Length);
		}
	}
}