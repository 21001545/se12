using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientChatRoomEntrant
	{
		public int account_id;
		public int push_config;

		public static class PushConfig
		{
			public static int disable = 0;
			public static int enable = 1;
		}
	}
}
