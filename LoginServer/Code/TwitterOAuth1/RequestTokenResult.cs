using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	// Эти поля всегда возвращаются при удачном запроce RequestToken'а
	public class BaseRequestTokenResult
	{
		public string oauth_token { get; set; }
		public string oauth_token_secret { get; set; }

		protected string[] responseParams;

		public BaseRequestTokenResult(string responseString)
		{
			responseParams = responseString.Split(new char[] { '&' });

			oauth_token = GetParamValueByName("oauth_token");
			oauth_token_secret = GetParamValueByName("oauth_token_secret");

			SetAdditionalFields();
		}

		/// <summary>
		/// Получает значение параметра из ответа
		/// </summary>
		/// <param name="paramName">Имя параметра, значение которого необходимо получить</param>
		/// <param name="responseParams">Массив строк в формате paramName=paramValue</param>
		/// <returns></returns>
		public string GetParamValueByName(string paramName)
		{
			string paramValue = responseParams.First(s => s.StartsWith(paramName)).Substring(paramName.Length + 1);
			return paramValue;
		}

		public virtual void SetAdditionalFields()
		{
		}
	}

	public class TwitterRequestTokenResult : BaseRequestTokenResult
	{
		public string oauth_callback_confirmed { get; set; }

		public TwitterRequestTokenResult(string response)
			: base(response)
		{ }

		public override void SetAdditionalFields()
		{
			oauth_callback_confirmed = GetParamValueByName("oauth_callback_confirmed");
		}
	}
}