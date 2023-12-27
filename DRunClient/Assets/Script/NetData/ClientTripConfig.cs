using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientTripConfig
	{
		public int status;
		public DateTime trip_begin_time;
		public double distance_total;
		public double calorie_total;
		public double last_latitude;
		public double last_longitude;
		public double last_altitude;
		public int step_amount;
		public int trip_type;
		public int current_path_id;
		public int next_trip_id;

		public static class StatusType
		{
			public static int none = 0;
			public static int trip = 1;
			public static int paused = 2;
		}

		public static class TripType
		{
			public static int none = -1;
			public static int walking = 0;
			public static int running = 1;
			public static int hiking = 2;
			public static int cycling = 3;
		}
	}
}
