using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TechnosilaMock.Code.OAuth1
{
	public class AuthorizationHeaderInfo
	{
		public string HeaderName { get; private set; }
		public string HeaderValue { get; set; }

		public AuthorizationHeaderInfo()
		{
			HeaderName = "Authorization";
		}
	}
}