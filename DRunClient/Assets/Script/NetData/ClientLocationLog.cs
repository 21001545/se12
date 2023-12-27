using Festa.Client.MapBox;
using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientLocationLog
	{
		public DateTime event_time;
		public double longitude;
		public double latitude;
		public double altitude;
		public double accuracy;
		public double speed;
		public double speed_accuracy;

		public MBLongLatCoordinate toMBLocation()
		{
			return new MBLongLatCoordinate(longitude, latitude);
		}

		public static ClientLocationLog createFromNow(MBLongLatCoordinate pos,double altitude)
		{
			ClientLocationLog log = new ClientLocationLog();
			log.longitude = pos.lon;
			log.latitude = pos.lat;
			log.altitude = altitude;
			log.accuracy = 0;
			log.speed = -1;
			log.speed_accuracy = -1;
			log.event_time = DateTime.UtcNow;
			return log;
		}

		public static ClientLocationLog create(double lon,double lat,double alt,double accuracy,double speed,double speed_accuracy,long timestamp)
		{
			ClientLocationLog log = new ClientLocationLog();
			log.longitude = lon;
			log.latitude = lat;
			log.altitude = alt;
			log.accuracy = accuracy;
			log.speed = speed;
			log.speed_accuracy = speed_accuracy;
			log.event_time = TimeUtil.dateTimeFromUnixTimestamp(timestamp);
			return log;
		}

		public static double calcDistance(List<ClientLocationLog> logList)
		{
			double distance = 0;
			for(int i = 1; i < logList.Count; ++i)
			{
				ClientLocationLog prev = logList[i - 1];
				ClientLocationLog next = logList[i];

				distance += calcDistance(prev, next);
			}
			return distance;
		}

		public static double calcDistance(ClientLocationLog prev,ClientLocationLog next)
		{
			return MapBoxUtil.distance(prev.longitude, prev.latitude, next.longitude, next.latitude);
		}

		public static double calcDeltaTime(ClientLocationLog prev,ClientLocationLog next)
		{
			return (next.event_time - prev.event_time).TotalSeconds;
		}



	}
}
