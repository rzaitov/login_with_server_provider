using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

namespace TechnosilaMock.Code
{
	public class FileLog
	{
		public FileLog()
		{

		}

		public static object sync = new object();

		public static void Log(string format, params object[] args)
		{
			Log(string.Format(format, args));
		}

		public static void Log(string message)
		{
			lock (sync)
			{
				Directory.CreateDirectory("C:\\technosilaMock\\");
				StreamWriter s = new StreamWriter(new FileStream("C:\\technosilaMock\\log.log", FileMode.Append | FileMode.OpenOrCreate));

				s.WriteLine("[" + DateTime.Now + "] " + message);
				s.Close();
			}
		}
	}
}