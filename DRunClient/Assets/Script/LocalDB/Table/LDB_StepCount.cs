using SQLite;

namespace Festa.Client.LocalDB
{
	public class LDB_StepCount
	{
		[Indexed]
		public long time { get; set; }

		public int value { get; set; }
	
		public static LDB_StepCount create(long time,int value)
		{
			LDB_StepCount log = new LDB_StepCount();
			log.time = time;
			log.value = value;

			return log;
		}
	}
}
