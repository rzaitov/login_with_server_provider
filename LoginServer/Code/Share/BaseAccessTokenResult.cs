using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	public class BaseAccessTokenResult : BaseResult
	{
		public string oauth_token { get; set; }
		public string oauth_token_secret { get; set; }

		public BaseAccessTokenResult(string responseString)
			: base(responseString)
		{
			oauth_token = GetParamValueByName("oauth_token");
			oauth_token_secret = GetParamValueByName("oauth_token_secret");
		}
	}
}