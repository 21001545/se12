using Festa.Client.NetData;
using Service;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module;
using Festa.Client.MapBox;

namespace Festa.Client
{
	public class LocationDevice_Mobile : AbstractLocationDevice
	{
		private BgLocationService _locationService;
		private long _lastQueryTime;
		private string _pref_key = "LocationDevice.LastQueryTime";
		private MultiThreadWorker _threadWorker;
		private List<DeviceLocationInfo> _tempList;
		private bool _firstReceived;

		public override long getLastQueryTime()
		{
			return _lastQueryTime;
		}

		public static LocationDevice_Mobile create(MultiThreadWorker threadWorker)
		{
			LocationDevice_Mobile device = new LocationDevice_Mobile();
			device.init(threadWorker);
			return device;
		}

		private void init(MultiThreadWorker threadWorker)
		{
			base.init();

			// 
			_lastLocation = _lastLocation_NotFiltered = new MBLongLatCoordinate(127.0543616485475, 37.50637354896867);
			_lastAltitude = 0;
			_tempList = new List<DeviceLocationInfo>();
			_firstReceived = false;

			_threadWorker = threadWorker;
			_locationService = LocationServiceFactory.GetService();
			readLastQueryTime();
		}

		private void readLastQueryTime()
		{
			if( PlayerPrefs.HasKey( _pref_key) == false)
			{
				_lastQueryTime = TimeUtil.unixTimestampUtcNow();
			}
			else
			{
				if( long.TryParse( PlayerPrefs.GetString( _pref_key), out _lastQueryTime) == false)
				{
					_lastQueryTime = TimeUtil.unixTimestampUtcNow();
				}
			}
		}

		private void saveLastQueryTime()
		{
			PlayerPrefs.SetString(_pref_key, _lastQueryTime.ToString());
		}

		public override void initDevice(Action<bool> callback)
		{
			_locationService.initDevice();
			
			// 2022.6.30 이강희
			//_locationService.start();
			callback(true);
		}

		public override void startService()
		{
			_locationService.start();
		}

		public override void stopService()
		{
			_locationService.stop();
		}

		public override void oneTimeRequest()
		{
			_locationService.oneTimeRequest();
		}

		public override int currentAuthorizationStatus()
        {
			return _locationService.currentAuthorizationStatus();
        }

		public override void requestPermission()
        {
			_locationService.requestPermission();
        }

        public override void queryLocationLog(List<ClientLocationLog> result_list, Action action)
		{
			try
			{
				string json_str = _locationService.getLocations(_lastQueryTime);
				JsonArray json_array = new JsonArray(json_str);
				_tempList.Clear();

				for(int i = 0; i < json_array.size(); ++i)
				{
					JsonObject log = json_array.getJsonObject(i);

					DeviceLocationInfo li = new DeviceLocationInfo();
					li.timestamp = log.getLong("time");
					li.longitude = log.getDouble("longitude");
					li.latitude = log.getDouble("latitude");
					li.altitude = log.getDouble("altitude");
					li.accuracy = (float)log.getDouble("h_accuracy");

					_tempList.Add(li);

					_lastQueryTime = System.Math.Max( li.timestamp, _lastQueryTime );
				}

				if( json_array.size() > 0)
				{
					//Debug.Log(json_str);
					_firstReceived = true;
				}
				else
				{
					// 2021.09.03 음.. 일단 현재 위치를 줘 보자
					if( _firstReceived)
					{
						DeviceLocationInfo li = new DeviceLocationInfo();
						li.longitude = _lastLocation_NotFiltered.lon;
						li.latitude = _lastLocation_NotFiltered.lat;
						li.altitude = _lastAltitude;
						li.accuracy = 1.0f;
						li.timestamp = TimeUtil.unixTimestampUtcNow();

						_tempList.Add(li);
					}
				}

				processFilter(_tempList, result_list);
			}
			catch (Exception e)
			{
				Debug.LogException(e);				
			}

			//_lastQueryTime = TimeUtil.unixTimestampUtcNow();
			saveLastQueryTime();
			action();


			//_threadWorker.execute<Module.Void>(promise => { 
			//	try
			//	{
			//		string json_str = _locationService.getLocations(_lastQueryTime);
			//		Debug.Log(json_str);
			//	}
			//	catch(Excetion e)
			//	{
			//		promise.fail(e);
			//	}
			
			//}, result => {
			//	_lastQueryTime = TimeUtil.unixTimestampUtcNow();
			//	saveLastQueryTime();
			//	action();
			//});
		}


	}
}
