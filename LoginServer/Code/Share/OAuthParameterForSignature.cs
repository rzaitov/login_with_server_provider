using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	public class OAuthParameterForSignature
	{
		public string OriginalName;
		public string OriginalValue;

		public string UrlEncodedName
		{
			get
			{
				return Uri.EscapeDataString(OriginalName);
			}
		}

		public string UrlEncodedValue
		{
			get
			{
				// http://msdn.microsoft.com/ru-ru/library/system.uri.unescapedatastring.aspx
				return Uri.EscapeDataString(OriginalValue).Replace("+", " ");
			}
		}
	}
}