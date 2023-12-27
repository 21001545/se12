using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientAccountChatRoom
	{
		public long chatroom_id;
		public int type;
		public int target_id;
		public int status;
		public int begin_log_id;
		public int push_config;

		public static class Type
		{
			public static int direct_message = 1;
		}

		// 상대방 유저의 프로필 Type이 DM일 경우에만 유효 
		[SerializeOption(SerializeOption.NONE)]
		public ClientProfileCache _dmTargetProfile;

		public static class Status
		{
			public static int leaved = 0;
			public static int entered = 1;
		}

		public static class PushConfig
		{
			public static int disable = 0;
			public static int enable = 1;
		}
	}
}
