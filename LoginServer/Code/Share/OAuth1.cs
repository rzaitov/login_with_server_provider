using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace LoginServer.Code
{
	public abstract class OAuth1Authenticator
	{
		public abstract string RequestTokenUrl { get; }
		public abstract string AccessTokenUrl { get; }

		public string ConsumerKey { get; private set; }
		public string ConsumerSecret { get; private set; }
		public string CallbackUrl { get; private set; }

		public abstract string UserAuthorizationBaseUrl { get; }
		public abstract string RequestToken { get; }
		public abstract string RequestTokenSecret { get; }
		/// <summary>
		/// На этот адрес необходимо отправить пользователя, когда будет получен RequestToken
		/// </summary>
		public virtual string UserAuthorizationUrl
		{
			get
			{
				return string.Format("{0}?oauth_token={1}", UserAuthorizationBaseUrl, RequestToken);
			}
		}

		public OAuth1Authenticator(string consumerKey, string consumerSecret, string callbackUrl)
		{
			ConsumerKey = consumerKey;
			ConsumerSecret = consumerSecret;
			CallbackUrl = callbackUrl;
		}

		/// <summary>
		/// Метод получения маркера от провайдера серсива (ServiceProvider). Этот маркер еще никак не привязан к пользователю, но привязан к приложению от чьего имени выполняется этот запрос.
		/// Этот метод необходимо вызывать самым первым в процессе авторизации.
		/// Подробнее об соответствующем методу в API twitter'а — https://dev.twitter.com/docs/api/1/post/oauth/request_token
		/// </summary>
		/// <returns>Информация о маркере</returns>
		public virtual string GetRequestToken()
		{
			string requestMethod = "POST";

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(RequestTokenUrl);
			httpRequest.Method = requestMethod;

			Dictionary<string, string> additionalHeaderParameters = GetAdditionalHeaderParametersForRequestToken();
			Dictionary<string, string> requestParameters = GetRequestParametersForRequestToken();

			string authorizationHeaderValue = GetAuthorizationHeaderValue(requestMethod, RequestTokenUrl, additionalHeaderParameters, requestParameters);
			httpRequest.Headers.Add("Authorization", authorizationHeaderValue);

			string response = ReadResponseFrom(httpRequest);

			return response;
		}

		public virtual Dictionary<string, string> GetAdditionalHeaderParametersForRequestToken()
		{
			return null;
		}

		public virtual Dictionary<string, string> GetRequestParametersForRequestToken()
		{
			return null;
		}

		/// <summary>
		/// Метод обмена маркера на маркер доступа. Т.е. получение маркера с помощью которого можно будет производить запросы к API ServiceProvider'а от имени пользователя.
		/// <summary>
		/// <returns></returns>
		public AccessTokenResult ExchangeRequestTokenToAccessToken()
		{
			string requestMethod = "POST";

			// Получаем специфичные для провайдера сервиса параметры запроса
			Dictionary<string, string> requestParameters = GetRequestParametersForAccessToken();
			StringBuilder requestSb = new StringBuilder(AccessTokenUrl);
			if (requestParameters != null)
			{
				requestSb.Append("?");
				foreach (var kvp in requestParameters)
				{
					AppendOAuthParamToQuery(requestSb, kvp.Key, kvp.Value, true);
				}

				// удаляем последнюю запятую
				requestSb.Remove(requestSb.Length - 1, 1);
			}

			string getAccessTokenUrl = requestSb.ToString();

			HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(getAccessTokenUrl);
			httpRequest.Method = requestMethod;

			// Дополнительные параметры для данного запроса
			Dictionary<string, string> additionalHeaderParameters = new Dictionary<string, string>()
			{
				{"oauth_token", RequestToken}
			};

			AuthorizationHeaderInfo authorizationHeaderInfo = GetAuthorizationHeaderInfo(requestMethod, AccessTokenUrl, additionalHeaderParameters, requestParameters, RequestTokenSecret);
			httpRequest.Headers.Add(authorizationHeaderInfo.HeaderName, authorizationHeaderInfo.HeaderValue);

			string result = ReadResponseFrom(httpRequest);

			AccessTokenResult accessTokenResult = new AccessTokenResult();
			accessTokenResult.Parse(result);

			return accessTokenResult;
		}

		protected virtual Dictionary<string, string> GetRequestParametersForAccessToken()
		{
			return null;
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

			string oauth_nonce = CreateNonce();
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_nonce", oauth_nonce);

			string oauth_signature_method = "HMAC-SHA1";
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_signature_method", oauth_signature_method);

			string oauth_timestamp = GetUnixTimestamp(DateTime.UtcNow).ToString();
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_timestamp", oauth_timestamp);

			string oauth_version = "1.0";
			AppendOAuthParamToHeader(authorizationHeaderSb, "oauth_version", oauth_version);

			Dictionary<string, string> signatureParameters = new Dictionary<string, string>()
			{
				// Базовые параметры, которые есть в любом запросе авторизации
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

		protected static void AppendOAuthParamToHeader(StringBuilder paramSb, string unescapedName, string unescepedValue, bool shouldAppendComma = true)
		{
			string escapedName = Uri.EscapeDataString(unescapedName);
			string escapedValue = Uri.EscapeDataString(unescepedValue);

			paramSb.Append(escapedName)
				.Append("=")
				.Append('"').Append(escapedValue).Append('"');

			if (shouldAppendComma)
			{
				paramSb.Append(", ");
			}
		}

		protected static void AppendOAuthParamToQuery(StringBuilder querySb, string unescapedName, string unescepedValue, bool shouldAppendComma = true)
		{
			string escapedName = Uri.EscapeDataString(unescapedName);
			string escapedValue = Uri.EscapeDataString(unescepedValue);

			querySb.Append(escapedName)
				.Append("=")
				.Append(escapedValue);

			if (shouldAppendComma)
			{
				querySb.Append(",");
			}
		}

		protected static string CreateNonce()
		{
			string oauth_nonce = Guid.NewGuid().ToString().Replace("-", "");
			oauth_nonce = Uri.EscapeDataString(oauth_nonce);

			return oauth_nonce;
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

		protected string ReadResponseFrom(HttpWebRequest httpRequest)
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

		/*
		/// <summary>
		/// Метод установки токена. Он сделан открытым, т.к. токен может приходить не в качестве тела ответа, а в параметре обратного вызова.
		/// Во втором случае маркер должен установить клиент, использующий этот класс.
		/// </summary>
		public void SetRequestToken(string requestToken)
		{

		}

		/// <summary>
		/// Метод установки секрета. Он сделан открытым, т.к. секрет может приходить не в качестве тела ответа, а в параметре обратного вызова.
		/// Во втором случае маркер должен установить клиент, использующий этот класс.
		/// </summary>
		public void SetRequestTokenSecret(string requestTokenSecret)
		{

		}
		*/
	}
}