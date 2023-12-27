using System;
using UnityEngine;

namespace Service
{
	public class AndroidLocationService : BgLocationService
	{
		private AndroidJavaObject activityObj;
		private AndroidJavaObject pluginObj;

		public AndroidLocationService() {
			Debug.Log("AndroidBackgroundLocationService init");
			try {
				AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				activityObj = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

				pluginObj = (new AndroidJavaClass("me.devhelp.unityplugin_drun.BGLocationPlugin")).CallStatic<AndroidJavaObject>("create", activityObj);

			}
			catch (Exception e) {
				#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX
				Debug.Log(e.Message);
				#else
				Debug.LogError(e.Message);
				#endif
			}
		}

		public void initDevice()
		{
			if (activityObj == null)
			{
				return;
			}

			pluginObj.Call("initDevice");
		}

		public string getLocations(long time) {
			if (activityObj == null) {
				return "";
			}
			//Debug.Log("BackgroundLocationService getLocations");
			long lastUpdateTime = time; //TODO: should be replaced with actual value

			object[] method_args = new object[1];
			method_args[0] = lastUpdateTime;
			string json = pluginObj.Call<string>("getLocationsJson", method_args);
			//Debug.Log("Got json:" + json);
			return json;
		}

		public void start() {
			if (activityObj != null) {
				Debug.Log("BackgroundLocationService start");
				pluginObj.Call("startLocationService");
			}
		}

		public void stop() {
			if (activityObj != null) {
				Debug.Log("BackgroundLocationService stop");
				pluginObj.Call("stopLocationService");
			}
		}

		public void oneTimeRequest()
		{
			if( activityObj != null)
			{
				Debug.Log("request Location OneTime");
				pluginObj.Call("requestOneTime");
			}
		}

		public void deleteOld(long time) {
			if (activityObj != null) {
				Debug.Log("BackgroundLocationService clearOldData");
				object[] method_args = new object[1];
				method_args[0] = time;
				pluginObj.Call("deleteLocationsBefore", method_args);
			}
		}
		
		public void requestPermission()
        {
            if (activityObj != null)
            {
                Debug.Log("BackgroundLocationService requestPermission");
                pluginObj.Call("checkPermissions");
            }
        }

		public int currentAuthorizationStatus()
        {
            if (activityObj != null)
            {
                Debug.Log("BackgroundLocationService requestPermission");
                return pluginObj.Call<bool>("hasPermission") == true ? 4 : 2;
            }
            return 0;
		}
	}
}

