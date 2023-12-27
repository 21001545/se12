using System;

namespace Festa.Client.NetData
{
	public class ClientDailyBoost
	{
		public int status;
		public DateTime last_used_time;
		public int used_count;

		public class StatusType
		{
			public static int off = 0;
			public static int on = 1;
		}
	}
}
