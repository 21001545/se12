using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;

#if UNITY_IPHONE

namespace Service
{
	public class IodLocationService : BgLocationService
	{
		public void initDevice()
		{
			initLocationDevice();
		}

		public string getLocations(long time) {
			//Debug.Log("Unity: BackgroundLocationService getLocations");
			var json = getLocationsJson(time);
			//Debug.Log("Unity: Got json:" + json);

			return json;
		}

		public void start() {
			Debug.Log("Unity: BackgroundLocationService start");
			startLocationService();
		}

		public void stop() {
			Debug.Log("Unity: BackgroundLocationService stop");
			stopLocationService();
		}

		public void oneTimeRequest()
		{
			requestLocation();
		}

		public void deleteOld(long time) {
			Debug.Log("Unity: BackgroundLocationService stop");
			deleteLocationsBefore(time);
		}

		public void requestPermission()
		{
			requestAlwaysAuthorization();
		}

		public int currentAuthorizationStatus()
		{
			return authorizationStatus();
		}

		[DllImport("__Internal")]
		extern static private void initLocationDevice();

		[DllImport("__Internal")]
		extern static private void startLocationService();

		[DllImport("__Internal")]
		extern static private void stopLocationService();

		[DllImport("__Internal")]
		extern static private void requestLocation();

		[DllImport("__Internal")]
		extern static private string getLocationsJson(double time);

		[DllImport("__Internal")]
		extern static private string deleteLocationsBefore(double time);

		[DllImport("__Internal")]
		extern static private int authorizationStatus();
		
		[DllImport("__Internal")]
		extern static private void requestAlwaysAuthorization();	
	}
}

#endif
