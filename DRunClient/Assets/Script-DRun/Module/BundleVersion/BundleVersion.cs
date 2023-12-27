using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

public static class BundleVersion
{
	public static string getVersion()
	{
		return Application.version;
	}

	public static int getVersionNumber()
	{
#if !UNITY_EDITOR
		
#if UNITY_ANDROID
		return getVersionNumber_Android();
#elif UNITY_IOS
		return getVersionNumber_iOS();
#endif

#else
		return 1;
#endif
	}

#if !UNITY_EDITOR && UNITY_IOS

	[DllImport("__Internal")]
	extern static private int getVersionNumber_iOS();

#endif

#if !UNITY_EDITOR && UNITY_ANDROID
	private static int getVersionNumber_Android()
	{
		try
		{
			AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			var ca = up.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject packageManager = ca.Call<AndroidJavaObject>("getPackageManager");
			var pInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo", Application.identifier, 0);

			return pInfo.Get<int>("versionCode");
		}
		catch(System.Exception e)
		{
			Debug.LogException(e);
			return 1;
		}
	}
#endif
}
