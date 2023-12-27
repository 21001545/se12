using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientDoNotDisturb
	{
		public int status;
		public int begin_time;
		public int end_time;

		public int LocalBeginTime
		{
			get
			{
				return toLocal(begin_time);
			}
			set
			{
				begin_time = toUTC(value);
			}
		}

		public int LocalEndTime
		{
			get
			{
				return toLocal(end_time);
			}
			set
			{
				end_time = toUTC(value);
			}
		}

		public static string getTimeString_12hr(int time)
        {
            int hour = time / 60;
            int min = time % 60;
			string ampm;

            // am pm + 시간 보정
            if (time < 720)
            {
				ampm = "AM";
                if (hour == 0) hour = 12;
            }
            else
            {
				ampm = "PM";
                if (hour != 12) hour -= 12;
            }

			return string.Format("{0}:{1} {2}", hour, min.ToString("D2"), ampm);
		}

		public static int toLocal(int utc_time)
		{
			int tz_offset = (int)(TimeUtil.timezoneOffset() / TimeUtil.msMinute);
			int local_time = utc_time + tz_offset;
			if (local_time > 60 * 24)
			{
				local_time -= 60 * 24;
			}
			return local_time;
		}

		public static int toUTC(int local_time)
		{
			int tz_offset = (int)(TimeUtil.timezoneOffset() / TimeUtil.msMinute);
			int utc_time = local_time - tz_offset;
			if(utc_time < 0)
			{
				utc_time += 60 * 24;
			}
			return utc_time;
		}

		public class Status
		{
			public static int disable = 0;
			public static int enable = 1;
		}
	}
}
