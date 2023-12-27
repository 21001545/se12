using Festa.Client.NetData;
using SQLite;
using System;
using System.Collections.Generic;

namespace Festa.Client.LocalDB
{
	public class LDB_ChatRoomLog
	{
		[Indexed]
		public long chatroom_id { get; set; }
		[Indexed]
		public int log_id { get; set; }
		public int sender_type { get; set; }
		public int sender_id { get; set; }
		[Indexed] // ?
		public string payload { get; set; }
		public DateTime create_time { get; set; }
	
		public static LDB_ChatRoomLog create(long chatroom_id,ClientChatRoomLog log)
		{
			LDB_ChatRoomLog local_log = new LDB_ChatRoomLog();
			local_log.chatroom_id = chatroom_id;
			local_log.log_id = log.log_id;
			local_log.sender_type = log.sender_type;
			local_log.sender_id = log.sender_id;
			local_log.payload = log.payload.encode();
			local_log.create_time = log.create_time;
			return local_log;
		}

		public static List<LDB_ChatRoomLog> createList(long chatroom_id,List<ClientChatRoomLog> log_list)
		{
			List<LDB_ChatRoomLog> local_list = new List<LDB_ChatRoomLog>();
			foreach(ClientChatRoomLog log in log_list)
			{
				local_list.Add(create(chatroom_id,log));
			}

			return local_list;
		}
	}
}
