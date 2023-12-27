using Festa.Client;
using Festa.Client.Module;
using Festa.Client.NetData;
using Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace DRun.Client.Running
{
	public class GPSTracker_iOS : AbstractGPSTracker
	{
		private BgLocationService _locationService;
		//private KalmanLatLong _filter;

		public override bool checkPermission()
		{
			return true;
		}

		protected override void init()
		{
			base.init();

			_locationService = LocationServiceFactory.GetService();
		}

		public override void getLocationFrom(long begin, List<ClientLocationLog> resultList)
		{
			resultList.Clear();

			try
			{
				string json_string = _locationService.getLocations(begin);
				JsonArray json_array = new JsonArray(json_string);

				parseJson(json_array, resultList);
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		// 생각해보니 당장 없어도 되네
		public override void getLocationRange(long begin, long end, List<ClientLocationLog> resutList)
		{
			throw new NotImplementedException();
		}

		public override void initDevice()
		{
			_locationService.initDevice();
		}

		public override void start()
		{
			//_filter = null;
			_statusInfo.reset();
			_locationService.start();
		}

		public override void stop()
		{
			_locationService.stop();
		}

		private void parseJson(JsonArray json_array,List<ClientLocationLog> resultList)
		{
			for (int i = 0; i < json_array.size(); ++i)
			{
				JsonObject json = json_array.getJsonObject(i);

				long time = json.getLong("time");
				double longitude = json.getDouble("longitude");
				double latitude = json.getDouble("latitude");
				double altitude = json.getDouble("altitude");
				double accuracy = json.getDouble("h_accuracy");
				double speed = json.getDouble("speed");
				double speed_accuracy = json.getDouble("speed_accuracy");

				//if (_filter == null)
				//{
				//	_filter = new KalmanLatLong(5.0f);
				//	_filter.SetState(latitude, longitude, (float)accuracy, time);
				//}
				//else
				//{
				//	_filter.Process(latitude, longitude, (float)accuracy, time);

				//	longitude = _filter.Lng;
				//	latitude = _filter.Lat;
				//}

				resultList.Add(ClientLocationLog.create(longitude, latitude, altitude, accuracy, speed, speed_accuracy, time));
			}

			resultList.Sort((a, b) => { 
				if( a.event_time < b.event_time)
				{
					return -1;
				}
				else if( a.event_time > b.event_time)
				{
					return 1;
				}

				return 0;
			});

			updateGPSStatusInfo(resultList);
		}
	}
}
