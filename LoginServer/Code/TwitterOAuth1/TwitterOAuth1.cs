using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;

using TechnosilaMock.Code.OAuth1;

namespace TechnosilaMock.Code
{
	/// <summary>
	/// Документация по всем методам API https://dev.twitter.com/docs/api/1.1
	/// </summary>
	public class TwitterOAuth1
	{
		//Подробнее об соответствующем методу API — https://dev.twitter.com/docs/api/1/post/oauth/request_token
		private const string _requestTokenUrl = @"https://api.twitter.com/oauth/request_token";
		
		private const string _requestAccessTokenUrl = "https://api.twitter.com/oauth/access_token";

		private const string _GetUsersShowUrl = @"http://api.twitter.com/1/users/show.json";

		public string CallbackUrl { get; private set; }

		public string ConsumerKey { get; private set; }
		public string ConsumerSecret { get; private set; }

		public TwitterOAuth1(string consumerKey, string consumerSecret, string callbackUrl)
		{
			ConsumerKey = consumerKey;
			ConsumerSecret = consumerSecret;

			CallbackUrl = callbackUrl;
		}

		/// <summary>
		/// Метод получения маркера от сервeра Twitter. Этот маркер еще никак не привязан к пользователю, но привязан к приложению от чьего имени выполняется этот запрос.
		/// Этот метод необходимо вызывать самым первым в процессе авторизации.
		/// Подробнее об соответствующем методу API — https://dev.twitter.com/docs/api/1/post/oauth/request_token
		/// </summary>
		/// <returns>Информация о маркере</returns>
		public RequestTokenResult GetRequestToken()
		{
			string requestMethod = "POST";

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(_requestTokenUrl);
			httpRequest.Method = requestMethod;

			Dictionary<string, string> additionalHeaderParameters = new Dictionary<string, string>()
			{
				{"oauth_callback", CallbackUrl}
			};

			string authorizationHeaderValue = GetAuthorizationHeaderValue(requestMethod, _requestTokenUrl, additionalHeaderParameters);
			httpRequest.Headers.Add("Authorization", authorizationHeaderValue);

			string result = ReadResponseFrom(httpRequest);

			RequestTokenResult rtr = new RequestTokenResult();
			rtr.Parse(result);
			
			return rtr;
		}

		/// <summary>
		/// Метод обмена маркера на маркер доступа. Т.е. получение маркера с помощью которого можно будет производить запросы к API Twitter'а от имени пользователя.
		/// <summary>
		/// <param name="oauth_token">маркер который необходимо обменять</param>
		/// <param name="oauth_token_secret">секрет обмениваемого маркера. Участвует в подписи запроса</param>
		/// <param name="oauth_verifier">верификатор. Добавляется в параметры заголовка авторизации</param>
		/// <returns></returns>
		public AccessTokenResult ExchangeRequestTokenToAccessToken(string oauth_token, string oauth_token_secret, string oauth_verifier)
		{
			string requestMethod = "POST";
			string getAccessTokenUrl = string.Format("{0}?oauth_verifier={1}", _requestAccessTokenUrl, oauth_verifier);

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(getAccessTokenUrl);
			httpRequest.Method = requestMethod;

			Dictionary<string, string> requestParameters = new Dictionary<string, string>()
			{
				{"oauth_verifier", oauth_verifier}
			};

			Dictionary<string, string> additionalHeaderParameters = new Dictionary<string, string>()
			{
				{"oauth_token", oauth_token}
			};

			string authorizationHeaderValue = GetAuthorizationHeaderValue(requestMethod, _requestAccessTokenUrl, additionalHeaderParameters, requestParameters, oauth_token_secret);
			httpRequest.Headers.Add("Authorization", authorizationHeaderValue);

			string result = ReadResponseFrom(httpRequest);

			AccessTokenResult accessTokenResult = new AccessTokenResult();
			accessTokenResult.Parse(result);

			return accessTokenResult;
		}

		public string UsersShow(string screen_name, string oauth_token_secret)
		{
			string requestMethod = "GET";
			string getUsersShowUrl = string.Format("{0}?screen_name={1}", _GetUsersShowUrl, screen_name);

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(getUsersShowUrl);
			httpRequest.Method = requestMethod;

			Dictionary<string, string> requestParameters = new Dictionary<string, string>()
			{
				{"screen_name", screen_name},
				{"include_entities", "true"}
			};

			string authorizationHeaderValue = GetAuthorizationHeaderValue(requestMethod, _requestAccessTokenUrl, null, requestParameters, oauth_token_secret);
			httpRequest.Headers.Add("Authorization", authorizationHeaderValue);

			return ReadResponseFrom(httpRequest);
		}

		private string ReadResponseFrom(HttpWebRequest httpRequest)
		{
			string result = null;
			using (WebResponse response = httpRequest.GetResponse())
			{
				using (Stream s = response.GetResponseStream())
				{
					using (StreamReader sReader = new StreamReader(s))
					{
						result = sReader.ReadToEnd();
					}
				}

				response.Close();
			}

			return result;
		}

		/// <summary>
		/// Возвращает заголовок авторизации, который используется для подписи запросов к серверу Twitter
		/// </summary>
		/// <param name="requestMethod">Тип запроса GET или POST</param>
		/// <param name="baseRequestUrl">Базовый адрес запроса (url запроса без параметров)</param>
		/// <param name="additionalHeaderParameters">Дополнительные параметры, которые необходимо включить в заголовок авторизации</param>
		/// <param name="requestParameters">Дополнительные параметры которые будут включены в строку запроса</param>
		/// <param name="tokenSecret">Секрет маркера. Будет участвовать в созлании подписи</param>
		/// <returns></returns>
		protected AuthorizationHeaderInfo GetAuthorizationHeaderInfo(string requestMethod, string baseRequestUrl, Dictionary<string, string> additionalHeaderParameters = null, Dictionary<string, string> requestParameters = null, string tokenSecret = "")
		{
			AuthorizationHeaderInfo hInfo = new AuthorizationHeaderInfo
			{
				HeaderValue = GetAuthorizationHeaderValue(requestMethod, baseRequestUrl, additionalHeaderParameters, requestParameters, tokenSecret)
			};

			return hInfo;
		}

		protected string GetAuthorizationHeaderValue(string requestMethod, string baseRequestUrl, Dictionary<string, string> additionalHeaderParameters = null, Dictionary<string, string> requestParameters = null, string tokenSecret = "")
		{
			StringBuilder authorizationHeaderSb = new StringBuilder();
			authorizationHeaderSb.Append("OAuth ");

			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_consumer_key", ConsumerKey);

			string oauth_nonce = Guid.NewGuid().ToString().Replace("-","");
			//string oauth_nonce = "ea9ec8429b68d6b77cd5600adbbb0456";
			oauth_nonce = Uri.EscapeDataString(oauth_nonce);
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_nonce", oauth_nonce);

			string oauth_signature_method = "HMAC-SHA1";
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_signature_method", oauth_signature_method);

			string oauth_timestamp = GetUnixTimestamp(DateTime.UtcNow).ToString();
			//string oauth_timestamp = "1318467427";
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_timestamp", oauth_timestamp);

			string oauth_version = "1.0";
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_version", oauth_version);

			Dictionary<string, string> signatureParameters = new Dictionary<string, string>()
			{
				// Базовые параметры, которые есть в любом запросе к Twitter API
				{"oauth_nonce", oauth_nonce},
				{"oauth_timestamp", oauth_timestamp},
				{"oauth_signature_method", oauth_signature_method},
				{"oauth_consumer_key", ConsumerKey},
				{"oauth_version", oauth_version}
			};


			if (additionalHeaderParameters != null)
			{
				foreach (var kvp in additionalHeaderParameters)
				{
					signatureParameters.Add(kvp.Key, kvp.Value);
					AppendOAuthParamToHeader(authorizationHeaderSb, kvp.Key, kvp.Value);
				}
			}

			if (requestParameters != null)
			{
				foreach (var kvp in requestParameters)
				{
					signatureParameters.Add(kvp.Key, kvp.Value);
				}
			}

			string oauth_signature = GetSignature(requestMethod, baseRequestUrl, signatureParameters, ConsumerSecret, tokenSecret);
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_signature", oauth_signature, false);

			return authorizationHeaderSb.ToString();
		}

		protected void AppendOAuthParamToHeader(StringBuilder headerBuilder, string unescapedName, string unescepedValue, bool shouldAppendComma = true)
		{
			string escapedName = Uri.EscapeDataString(unescapedName);
			string escapedValue = Uri.EscapeDataString(unescepedValue);

			headerBuilder.Append(escapedName)
				.Append("=")
				.Append('"').Append(escapedValue).Append('"');

			if (shouldAppendComma)
			{
				headerBuilder.Append(", ");
			}
		}

		protected static long GetUnixTimestamp(DateTime dateTime)
		{
			DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return (long)(dateTime - unixEpoch).TotalSeconds;
		}

		protected static string GetSignature(string requestMethod, string baseUrl, Dictionary<string, string> parameters, string consumerSecret, string oAuthTokenSecret)
		{
			string signatureBaseString = GetSignatureBaseString(requestMethod, baseUrl, parameters);
			string signingKey = GetSigningKey(consumerSecret, oAuthTokenSecret);

			byte[] input = Encoding.ASCII.GetBytes(signatureBaseString);
			byte[] key = Encoding.ASCII.GetBytes(signingKey);

			HMACSHA1 hmac_sha1 = new HMACSHA1(key);
			byte[] hash = hmac_sha1.ComputeHash(input);

			string signature = Convert.ToBase64String(hash);

			return signature;
		}

		/// <summary>
		/// Получение строки по которой будет получена подпись.
		/// Base string = [Method(GET|POST)]&[Percent encode the URL]&[Percent encode the parameter string]
		/// Подробнее в разделе "Creating the signature base string" по адресу https://dev.twitter.com/docs/auth/creating-signature
		/// </summary>
		/// <param name="requestMethod">Тип HTTP запроса GET или POST</param>
		/// <param name="baseUrl">Базовый адрес запроса (url без параметров)</param>
		/// <param name="parameters">Коллекция параметров и значений запроса</param>
		/// <returns></returns>
		protected static string GetSignatureBaseString(string requestMethod, string baseUrl, Dictionary<string, string> parameters)
		{
			StringBuilder signatureBaseStingSb = new StringBuilder();
			signatureBaseStingSb.Append(requestMethod.ToUpper()).Append("&");

			string encodedUrl = Uri.EscapeDataString(baseUrl);
			signatureBaseStingSb.Append(encodedUrl).Append("&");

			IOrderedEnumerable<OAuthParameterForSignature> _params = parameters.Select(kvp => new OAuthParameterForSignature { OriginalName = kvp.Key, OriginalValue = kvp.Value })
																  .OrderBy(oaParam => oaParam.UrlEncodedName);

			StringBuilder parameterSb = new StringBuilder();
			foreach (OAuthParameterForSignature p in _params)
			{
				parameterSb.Append(p.UrlEncodedName)
				  .Append("=")
				  .Append(p.UrlEncodedValue)
				  .Append("&");
			}
			// Удаляем последний &
			parameterSb.Remove(parameterSb.Length - 1, 1);

			string parameterString = parameterSb.ToString();
			string urlEncodedParameterString = Uri.EscapeDataString(parameterString);

			signatureBaseStingSb.Append(urlEncodedParameterString);

			return signatureBaseStingSb.ToString();
		}

		protected static string GetSigningKey(string consumerSecret, string oAuthTokenSecret)
		{
			return string.Format("{0}&{1}", Uri.EscapeDataString(consumerSecret), Uri.EscapeDataString(oAuthTokenSecret));
		}
	}
}