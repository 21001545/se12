using Festa.Client.Module;
using System;

namespace DRun.Client.NetData
{
	public class ClientRunningLogCumulation
	{
		public int running_type;
		public int running_sub_type;
		public int type;
		public long id;
		public DateTime begin_time;
		public DateTime end_time;
		public int log_count;
		public double total_distance;
		public double total_mine_distance;
		public long total_time;
		public long total_drn;
		public double total_calorie;
		public long total_stepcount;
		public int best_log_id;
		public double best_distance;
		public double best_mine_distance;
		public long best_time;
		public long best_drn;
		public long best_stepcount;

		public static class RunningType
		{
			public static int promode = 1;
			public static int marathon = 2;
		}

		public static class MarathonType
		{
			public static int _5k = 1;
			public static int _10k = 2;
			public static int _20k = 3;
			public static int _40k = 4;
			public static int _free_distance = 5;
			public static int _free_time = 6;
			public static int _sum = 100;
		}

		public static class TimeType
		{
			public static int day = 1;
			public static int week = 2;
			public static int month = 3;
			public static int year = 4;
			public static int total = 5;

			public static int[] array = new int[]{
				day,
				week,
				month,
				year,
				total
			};
		}

		public double getAvgDistance()
		{
			if( log_count > 0)
			{
				return total_distance / (double)log_count;
			}
			else
			{
				return 0;
			}
		}

		public double getAvgVelocity()
		{
			if( total_time > 0)
			{
				return total_distance / (double)total_time * 3600.0;
			}
			else
			{
				return 0;
			}
		}

		public TimeSpan getAvgTime()
		{
			if( log_count > 0)
			{
				return TimeSpan.FromSeconds(total_time / log_count);
			}
			else
			{
				return TimeSpan.FromSeconds(0);
			}
		}

		public static ClientRunningLogCumulation createEmpty(int running_type,int running_sub_type,int type,long id)
		{
			ClientRunningLogCumulation log = new ClientRunningLogCumulation();
			log.running_type = running_type;
			log.running_sub_type = running_sub_type;
			log.type = type;
			log.id = id;
			log.log_count = 0;
			log.total_distance = 0;
			log.total_mine_distance = 0;
			log.total_time = 0;
			log.total_drn = 0;
			log.best_log_id = 0;
			log.best_distance = 0;
			log.best_mine_distance = 0;
			log.best_time = 0;
			log.best_drn = 0;
			log.best_stepcount = 0;
			setupTimePeriod(type, id, log);

			return log;
		}

		private static void setupTimePeriod(int type,long id,ClientRunningLogCumulation log)
		{
			if( type == TimeType.day)
			{
				log.begin_time = TimeUtil.dateFromDayCountUTC(id, TimeUtil.timezoneOffset());
				log.end_time = log.begin_time.AddHours(12).AddSeconds(-1);
			}
			else if( type == TimeType.week)
			{
				log.begin_time = TimeUtil.dateFromWeekCountUTC(id, TimeUtil.timezoneOffset());
				log.end_time = log.begin_time.AddDays(7).AddSeconds(-1);
			}
			else if( type == TimeType.month)
			{
				log.begin_time = TimeUtil.dateFromMonthCountUTC(id, TimeUtil.timezoneOffset());
				log.end_time = log.begin_time.AddMonths(1).AddSeconds(-1);
			}
			else if( type == TimeType.year)
			{
				log.begin_time = TimeUtil.dateFromYearCountUTC(id, TimeUtil.timezoneOffset());
				log.end_time = log.begin_time.AddYears(1).AddSeconds(-1);
			}
			else if( type == TimeType.total)
			{
				int year = DateTime.Now.Year;

				log.begin_time = new DateTime( year, 1, 1).ToUniversalTime();
				log.end_time = new DateTime( year + 1, 1, 1).ToUniversalTime().AddSeconds(-1);
			}
		}
	}
}
