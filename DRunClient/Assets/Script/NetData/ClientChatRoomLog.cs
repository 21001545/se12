using Festa.Client.LocalDB;
using Festa.Client.Module;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.NetData
{
	public class ClientChatRoomLog
	{
		public int log_id;
		public int sender_type;
		public int sender_id;
		public JsonObject payload;
		public DateTime create_time;

		public static ClientChatRoomLog create(LDB_ChatRoomLog localLog)
		{
			ClientChatRoomLog log = new ClientChatRoomLog();
			log.log_id = localLog.log_id;
			log.sender_type = localLog.sender_type;
			log.sender_id = localLog.sender_id;
			log.payload = new JsonObject(localLog.payload);
			log.create_time = localLog.create_time;
			return log;
		}

		public static ClientChatRoomLog createFromPushMetaData(JsonObject data)
		{
			ClientChatRoomLog log = new ClientChatRoomLog();
			log.log_id = data.getInteger("log_id");
			log.sender_type = data.getInteger("sender_type");
			log.sender_id = data.getInteger("sender_id");
			log.payload = data.getJsonObject("payload");
			log.create_time = TimeUtil.dateTimeFromUnixTimestamp(data.getLong("create_time"));
			return log;
		}

		public static List<ClientChatRoomLog> create(List<LDB_ChatRoomLog> localLogList)
		{
			List<ClientChatRoomLog> log_list = new List<ClientChatRoomLog>();
			foreach(LDB_ChatRoomLog localLog in localLogList)
			{
				try
				{
					ClientChatRoomLog log = create(localLog);
					log_list.Add(log);
				}
				catch(Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			return log_list;
		}

		public static string getFileURL(string key)
		{
			return $"{GlobalConfig.fileserver_url}/chatfile/{key}";
		}
	}
}
