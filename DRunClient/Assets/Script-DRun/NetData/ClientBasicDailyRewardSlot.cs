using DRun.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.NetData
{
	public class ClientBasicDailyRewardSlot
	{
		public int slot_id;
		public int status;
		public long reward_count;

		public static class Status
		{
			public const int none = 1;
			public const int wait_claim = 2;
			public const int claimed = 3;
		}
	}
}
