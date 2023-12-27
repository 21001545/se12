using Festa.Client.Module;
using Festa.Client.Module.Events;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace Festa.Client.LocalDB
{
	public class LocalChatDataManager
	{
		private MultiThreadWorker _multiThreadWorker;

		private string _base_path;
		private string _db_path;
		private SQLiteConnection _connection;

		public static LocalChatDataManager create()
		{
			LocalChatDataManager manager = new LocalChatDataManager();
			manager.init();
			return manager;
		}

		private void init()
		{
			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();

		}

		private int makeURLID(string baseURL)
		{
#if UNITY_EDITOR
			return EncryptUtil.makeHashCodePositive(Application.dataPath + ":" + baseURL);
#else
			return EncryptUtil.makeHashCodePositive(baseURL);
#endif
		}

		private void validateDirectory(Handler<AsyncResult<Module.Void>> handler)
		{
			if (Directory.Exists(_base_path) == false)
			{
				try
				{
					Directory.CreateDirectory(_base_path);
					handler(Future.succeededFuture());
				}
				catch (Exception e)
				{
					handler(Future.failedFuture(e));
				}
			}
			else
			{
				handler(Future.succeededFuture());
			}
		}

		public void start(string baseURL, Handler<AsyncResult<Module.Void>> handler)
		{
			int url_id = makeURLID(baseURL);

			_base_path = Application.temporaryCachePath + "/local";
			_db_path = _base_path + $"/chatdata_{url_id}";

			validateDirectory(directory_result => {
				if (directory_result.failed())
				{
					handler(Future.failedFuture(directory_result.cause()));
					return;
				}

				createDBConnection(conn_result =>{

					if (conn_result.failed())
					{
						handler(Future.failedFuture(conn_result.cause()));
						return;
					}

					prepareTables(table_result => {

						if( table_result.failed())
						{
							handler(Future.failedFuture(table_result.cause()));
						}
						else
						{
							handler(Future.succeededFuture());
						}
					});
				});
			});
		}

		private void createDBConnection(Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<SQLiteConnection>(promise => { 
				SQLiteConnection conn = new SQLiteConnection(_db_path);
				promise.complete(conn);
			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_connection = result.result();

					handler(Future.succeededFuture());
				}
			});
		}

		private void prepareTables(Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<Module.Void>(promise => { 

				System.Type[] table_list = new System.Type[] { 
					typeof(LDB_AccountChatRoom),
					typeof(LDB_ChatRoomEntrant),
					typeof(LDB_ChatRoomLog)
				};

				foreach(System.Type type in table_list)
				{
					CreateTableResult result = _connection.CreateTable(type);
				}

				promise.complete();

			}, result => {
				if (result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

#region useless

		// 다이렉트 메세지 채팅방 찾기
		public void findAccountChatRoom(int type,int target_id,Handler<AsyncResult<LDB_AccountChatRoom>> handler)
		{
			_multiThreadWorker.execute<LDB_AccountChatRoom>(promise => {

				List<LDB_AccountChatRoom> list = _connection.Query<LDB_AccountChatRoom>("select * from LDB_AccountChatRoom where type = ? and target_id = ?", type, target_id);

				if( list.Count == 0)
				{
					promise.complete(null);
				}
				else
				{
					promise.complete(list[0]);
				}

			}, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture<LDB_AccountChatRoom>(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture(result.result()));
				}

			});
		}


		public void queryAccountChatRoom(Handler<AsyncResult<List<LDB_AccountChatRoom>>> handler)
		{
			_multiThreadWorker.execute<List<LDB_AccountChatRoom>>(promise => {
				List<LDB_AccountChatRoom> list = _connection.Query<LDB_AccountChatRoom>("select * from LDB_AccountChatRoom");

				promise.complete(list);

			}, result => {
				if (result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture<List<LDB_AccountChatRoom>>(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture(result.result()));
				}
			});
		}

#endregion

		public void writeAccountChatRoom(LDB_AccountChatRoom chatRoom,Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<Module.Void>(promise => {
				Debug.Log($"write room to local: id[{chatRoom.chatroom_id}] last_log_id[{chatRoom.last_log_id}]");

				int count = _connection.InsertOrReplace(chatRoom);
				if( count == 0)
				{
					promise.fail(new Exception("insert fail"));
				}
				else
				{
					promise.complete();
				}
			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		public void writeAccountChatRoom(List<LDB_AccountChatRoom> roomList,Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<Module.Void>(promise => {

				int count = 0;
				foreach(LDB_AccountChatRoom room in roomList)
				{
					Debug.Log($"write room to local: id[{room.chatroom_id}] last_log_id[{room.last_log_id}]");

					count += _connection.InsertOrReplace(room);
				}
				
				if( count != roomList.Count)
				{
					promise.fail(new Exception("insert fail"));
				}
				else
				{
					promise.complete();
				}
			
			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}

			});
		}

		public void readAccountChatRoom(long chatroom_id, Handler<AsyncResult<LDB_AccountChatRoom>> handler)
		{
			_multiThreadWorker.execute<LDB_AccountChatRoom>(promise => {
				List<LDB_AccountChatRoom> list = _connection.Query<LDB_AccountChatRoom>("select * from LDB_AccountChatRoom where chatroom_id = ?", chatroom_id);
				if (list.Count == 0)
				{
					promise.complete(null);
				}
				else
				{
					promise.complete(list[0]);
				}
			}, result => {

				if (result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture<LDB_AccountChatRoom>(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture(result.result()));
				}
			});
		}

		public void writeChatLog(List<LDB_ChatRoomLog> log_list,Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<Module.Void>(promise => {

				foreach(LDB_ChatRoomLog log in log_list)
				{
					_connection.InsertOrReplace(log);
				}

				promise.complete();

			}, result => {
				if( result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		public void readChatLog(long chatroom_id,int begin,int end,Handler<AsyncResult<List<LDB_ChatRoomLog>>> handler)
		{
			_multiThreadWorker.execute<List<LDB_ChatRoomLog>>(promise => {

				List<LDB_ChatRoomLog> list = _connection.Query<LDB_ChatRoomLog>("select * from LDB_ChatRoomLog where chatroom_id = ? and log_id >= ? and log_id <= ?", chatroom_id, begin, end);
				promise.complete(list);

			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture<List<LDB_ChatRoomLog>>(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture(result.result()));
				}
			});
		}

		private string encodeString(string text)
        {
			StringBuilder builder = new StringBuilder();
			char[] array = text.ToCharArray();
            char[] array2 = array;
            foreach (char c in array2)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        continue;
                    case '\\':
                        builder.Append("\\\\");
                        continue;
                    case '\b':
                        builder.Append("\\b");
                        continue;
                    case '\f':
                        builder.Append("\\f");
                        continue;
                    case '\n':
                        builder.Append("\\n");
                        continue;
                    case '\r':
                        builder.Append("\\r");
                        continue;
                    case '\t':
                        builder.Append("\\t");
                        continue;
                }
                int num = Convert.ToInt32(c);
                if (num >= 32 && num <= 126)
                {
                    builder.Append(c);
                    continue;
                }
                builder.Append("\\u");
                builder.Append(num.ToString("x4"));
            }

			return builder.ToString();
		}

		// 속도가 얼마나 느릴지 알아보자
		public void searchChatLog(long chatroom_id,int begin,string text,Handler<AsyncResult<List<LDB_ChatRoomLog>>> handler)
		{
			_multiThreadWorker.execute<List<LDB_ChatRoomLog>>(promise => {
                var unicodeText = encodeString(text);
				unicodeText = $"%{unicodeText}%";
				List<LDB_ChatRoomLog> payload_list = _connection.Query<LDB_ChatRoomLog>("select * from LDB_ChatRoomLog where chatroom_id = ? and log_id >= ? and payload like ?", chatroom_id, begin, unicodeText);
				List<LDB_ChatRoomLog> list = new List<LDB_ChatRoomLog>();
				
				// payload로 걸러낸거라서 message로 다시 한번 걸러야 된다
				foreach(LDB_ChatRoomLog log in payload_list)
				{
					JsonObject json = new JsonObject(log.payload);

					int type = json.getInteger("type");
					if( type != 1)
					{
						continue;
					}

					string message = json.getString("msg");
					if( message.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						list.Add(log);
					}
				}
				
				promise.complete(list);

			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					handler(Future.failedFuture<List<LDB_ChatRoomLog>>(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture(result.result()));
				}
			});
		}
	}
}
