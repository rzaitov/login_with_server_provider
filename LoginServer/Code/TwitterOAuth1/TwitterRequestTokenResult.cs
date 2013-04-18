using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	public class TwitterRequestTokenResult : BaseRequestTokenResult
	{
		public string oauth_callback_confirmed { get; set; }

		public TwitterRequestTokenResult(string response)
			: base(response)
		{
			oauth_callback_confirmed = GetParamValueByName("oauth_callback_confirmed");
		}
	}
}