using Festa.Client.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.Android
{
	public class CoreServicePlugin
	{
		private AndroidJavaObject objActivity;
		private AndroidJavaObject objPlugin;

		private static CoreServicePlugin _instance;
		public static CoreServicePlugin getInstance()
		{
			if( _instance == null)
			{
				_instance = create();
			}

			return _instance;
		}

		private static CoreServicePlugin create()
		{
			CoreServicePlugin plugin = new CoreServicePlugin();
			plugin.init();
			return plugin;
		}

		private void init()
		{
			try
			{
				AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				objActivity = activityClass.GetStatic<AndroidJavaObject>("currentActivity");

				AndroidJavaClass classPlugin = new AndroidJavaClass("com.lifefesta.drun.coreservice.CoreServicePlugin");
				objPlugin = classPlugin.CallStatic<AndroidJavaObject>("create", objActivity);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public void startService()
		{
			if (objPlugin == null)
				return;

			objPlugin.Call("startService", "DRun", "Tracking Step Count");
		}

		public void stopService()
		{
			if (objPlugin == null)
				return;

			objPlugin.Call("stopService");
		}

		public void startLocation()
		{
			if (objPlugin == null)
				return;

			objPlugin.Call("startLocation");
		}

		public void stopLocation()
		{
			if (objPlugin == null)
				return;

			objPlugin.Call("stopLocation");
		}

		public int getStepCountRange(long begin,long end)
		{
			if (objPlugin == null)
				return 0;

			try
			{
				string jsonData = objPlugin.Call<string>("getStepCountRange", begin, end);
				return sumStepCount(jsonData);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return 0;
			}

		}

		public int getStepCountFrom(long begin)
		{
			if (objPlugin == null)
				return 0;

			try
			{
				string jsonData = objPlugin.Call<string>("getStepCountFrom", begin);
				return sumStepCount(jsonData);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
				return 0;
			}
		}

		public int sumStepCount(string jsonData)
		{
			int sum = 0;
			JsonArray array = new JsonArray(jsonData);
			for(int i = 0; i < array.size(); ++i)
			{
				JsonObject obj = array.getJsonObject(i);
				sum +=obj.getInteger("count");
			}

			return sum;
		}

		public JsonArray getLocationRange(long begin,long end)
		{
			if (objPlugin == null)
				return new JsonArray();

			try
			{
				string jsonData = objPlugin.Call<string>("getLocationRange", begin, end);
				return new JsonArray(jsonData);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
				return new JsonArray();
			}
		}

		public JsonArray getLocationFrom(long begin)
		{
			if (objPlugin == null)
				return new JsonArray();

			try
			{
				string jsonData = objPlugin.Call<string>("getLocationFrom", begin);
				return new JsonArray(jsonData);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
				return new JsonArray();
			}
		}
		
	}
}


