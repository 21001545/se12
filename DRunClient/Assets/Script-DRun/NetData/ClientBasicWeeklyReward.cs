using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.NetData
{
	public class ClientBasicWeeklyReward
	{
		public int week_id;
		public int status;
		public long amount;
		public DateTime claim_time;
		public DateTime expire_time;

		public static class Status
		{
			public const int none = 1;
			public const int claimed = 2;
		}
	}
}
