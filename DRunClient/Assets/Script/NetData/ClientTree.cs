using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientTree
	{
		public int tree_id;
		public int status;
		public long remain_time;
		public DateTime expire_time;

		public static class Status
		{
			public const int active = 1;
			public const int expired = 2;
			public const int reserved = 3;
		}
	}
}
