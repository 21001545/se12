using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Module
{
	public static class Logger
	{
		public static void log(string msg)
		{
			Debug.Log(msg);
		}

		public static void logWarning(string msg)
		{
			Debug.LogWarning(msg);
		}

		public static void logError(string msg)
		{
			Debug.LogError(msg);
		}

		public static void logException(System.Exception e)
		{
			Debug.LogException(e);
		}
	}
}

