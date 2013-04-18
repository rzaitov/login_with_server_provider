using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	// Эти поля всегда возвращаются при удачном запроce RequestToken'а
	public class BaseRequestTokenResult : BaseResult
	{
		public string oauth_token { get; set; }
		public string oauth_token_secret { get; set; }

		public BaseRequestTokenResult(string responseString)
			:base(responseString)
		{
			oauth_token = GetParamValueByName("oauth_token");
			oauth_token_secret = GetParamValueByName("oauth_token_secret");
		}
	}
}