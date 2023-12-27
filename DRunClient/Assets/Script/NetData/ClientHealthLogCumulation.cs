using System;

namespace Festa.Client.NetData
{
	public class ClientHealthLogCumulation
	{
		public long total;
		public long today_total;
		public int today_goal;
		public DateTime last_recorded_time;
	}
}
