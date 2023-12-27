using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Service {

	public interface BgLocationService {
		void initDevice();
		void start ();
		void stop ();
		void oneTimeRequest();
		string getLocations(long time);
		void deleteOld (long time);
		int currentAuthorizationStatus();
		void requestPermission();
	} 
}
