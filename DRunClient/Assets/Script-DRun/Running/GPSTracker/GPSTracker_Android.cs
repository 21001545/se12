using DRun.Client.Android;
using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.Running
{
	public class GPSTracker_Android : AbstractGPSTracker
	{
		public override bool checkPermission()
		{
			throw new NotImplementedException();
		}

		public override void getLocationFrom(long begin, List<ClientLocationLog> resultList)
		{
			resultList.Clear();

			JsonArray jsonArray = CoreServicePlugin.getInstance().getLocationFrom(begin);
			parseJson(jsonArray, resultList);

			if( resultList.Count > 0)
			{
				DateTime dtTime = TimeUtil.dateTimeFromUnixTimestamp(begin);
				Debug.Log($"getLocationFrom:time[{dtTime.ToString()}] count[{resultList.Count}]");
			}
		}

		public override void getLocationRange(long begin, long end, List<ClientLocationLog> resultList)
		{
			resultList.Clear();

			JsonArray jsonArray = CoreServicePlugin.getInstance().getLocationRange(begin, end);
			parseJson(jsonArray, resultList);
		}

		public override void initDevice()
		{
			//nothing
		}

		public override void start()
		{
			_running = true;
			_statusInfo.reset();
			CoreServicePlugin.getInstance().startLocation();
		}

		public override void stop()
		{
			_running = false;
			CoreServicePlugin.getInstance().stopLocation();
		}

		private void parseJson(JsonArray jsonArray, List<ClientLocationLog> resultList)
		{
			try
			{
				for (int i = 0; i < jsonArray.size(); ++i)
				{
					JsonObject json = jsonArray.getJsonObject(i);

					long time = json.getLong("time");
					double longitude = json.getDouble("longitude");
					double latitude = json.getDouble("latitude");
					double altitude = json.getDouble("altitude");
					double h_accuracy = json.getDouble("h_accuracy");
					double v_accuracy = json.getDouble("v_accuracy");

					resultList.Add(ClientLocationLog.create(longitude, latitude, altitude, h_accuracy, -1, -1, time));
				}

				resultList.Sort((a, b) => { 
					if( a.event_time < b.event_time )
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
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
