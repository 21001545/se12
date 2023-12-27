using System;
using UnityEngine;

namespace Service
{
	public class LocationServiceFactory
	{
		public static BgLocationService GetService() {
			BgLocationService service = null;
			#if UNITY_ANDROID 
			service = new AndroidLocationService();
			#elif UNITY_IOS
			service = new IodLocationService();
			#else
			service = new Stub();
			#endif

			return service;
		}

		private class Stub: BgLocationService {
			public void initDevice()
			{

			}


			public void start () {
				Debug.Log("Stub: Service started");
			}

			public void stop () {
				Debug.Log("Stub: Service stop");
			}
			public string getLocations(long time){
				Debug.Log("Stub: Service getLocations");
				return "";
			}
			public void deleteOld (long time) {
				Debug.Log ("Stub: Service deleteOld");
			}

			public void oneTimeRequest()
			{

			}

			public void requestPermission()
            {

            }

			public int currentAuthorizationStatus()
			{
				return 0;
			}
		}
	}
}

