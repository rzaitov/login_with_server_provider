using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	public class TwitterAccessTokenResult : BaseAccessTokenResult
	{
		public string user_id { get; set; }
		public string screen_name { get; set; }

		public TwitterAccessTokenResult(string responseString)
			: base(responseString)
		{
			user_id = GetParamValueByName("user_id");
			screen_name = GetParamValueByName("screen_name");
		}
	}
}