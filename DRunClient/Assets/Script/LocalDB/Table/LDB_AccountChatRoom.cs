using Festa.Client.NetData;
using SQLite;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.LocalDB
{
	public class LDB_AccountChatRoom
	{
		[PrimaryKey]
		public long chatroom_id { get; set; }
		[Indexed]
		public int type { get; set; }
		[Indexed]
		public int target_id { get; set; }

		public int status { get; set; }
		public int begin_log_id { get; set; }
		public int push_config { get; set; }
		public int last_log_id { get; set; }

		public static LDB_AccountChatRoom create(ChatRoomViewModel roomVM)
		{
			ClientAccountChatRoom data = roomVM.RoomData;

			LDB_AccountChatRoom l = new LDB_AccountChatRoom();
			l.chatroom_id = data.chatroom_id;
			l.type = data.type;
			l.target_id = data.target_id;
			l.status = data.status;
			l.begin_log_id = data.begin_log_id;
			l.push_config = data.push_config;
			l.last_log_id = roomVM.LocalLastLogID;

			return l;
		}
	}
}