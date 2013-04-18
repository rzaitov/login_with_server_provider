using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LoginServer.Code
{
	public class BaseResult
	{
		protected string[] responseParams;

		public BaseResult(string responseString)
		{
			responseParams = responseString.Split(new char[] { '&' });
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
	}
}