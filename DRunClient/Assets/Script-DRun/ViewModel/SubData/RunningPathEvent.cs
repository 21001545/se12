using DRun.Client.Running;

namespace DRun.Client.ViewModel
{
	public class RunningPathEvent
	{
		public static class EventType
		{
			public const int reset = 1;
			public const int create_path = 2;
			public const int append_log = 3;
		}

		public int eventType;
		public RunningPathData pathData;

		public static RunningPathEvent create(int eventType, RunningPathData pathData)
		{
			RunningPathEvent e = new RunningPathEvent();
			e.eventType = eventType;
			e.pathData = pathData;
			return e;
		}
	}
}
