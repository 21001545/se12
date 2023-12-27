using Festa.Client.Module;

namespace DRun.Client.Running
{
	public class GPSStatusInfo
	{
		public static class Status
		{
			public const int no_signal = 0;
			public const int weak = 1;
			public const int normal = 2;
			public const int good = 3;
		}

		public int status;
		public long lastLocationTime;
		public double lastAccuracy;

		public void reset()
		{
			status = Status.no_signal;
			lastLocationTime = TimeUtil.unixTimestampUtcNow();
			lastAccuracy = -1;
		}
	}
}
