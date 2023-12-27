using Festa.Client.Module;
using System;

namespace Festa.Client.NetData
{
	public class ClientHealthLogData
	{
		public DateTime record_time;
		public int value;

		public static ClientHealthLogData create(int value)
		{
			ClientHealthLogData data = new ClientHealthLogData();
			data.record_time = DateTime.UtcNow;
			data.value = value;
			return data;
		}

		public static ClientHealthLogData create(long time,int value)
		{
			ClientHealthLogData data = new ClientHealthLogData();
			data.record_time = TimeUtil.dateTimeFromUnixTimestamp(time);
			data.value = value;
			return data;
		}

		public static ClientHealthLogData create(DateTime time,int value)
		{
			ClientHealthLogData data = new ClientHealthLogData();
			data.record_time = time;
			data.value = value;
			return data;
		}
	}
}
