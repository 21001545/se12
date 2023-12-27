using Festa.Client.LocalDB;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class ChatRoomViewModel : AbstractViewModel
	{
		private ClientAccountChatRoom _roomData;
		private List<ClientChatRoomLog> _logList;
		private Dictionary<int, ClientChatRoomLog> _logMap; // 중복 제거를 위해 
		private ObservableList<JobProgressItemViewModel> _sendingJobList;

		private int			_localLastLogID;
		private int			_unreadCount;
		private int			_lastLogID;
		private DateTime	_latestLogTime;
		private string _recentMessage;

		public ClientAccountChatRoom RoomData
		{
			get
			{
				return _roomData;
			}
			set
			{
				Set(ref _roomData, value);
			}
		}

		public long ID => _roomData.chatroom_id;

		// 읽지 않은 메세지 갯수
		public int UnreadCount
		{
			get
			{
				return _unreadCount;
			}
			set
			{
				Set(ref _unreadCount, value);
			}
		}

		// 마지막 채팅 시간
		public DateTime LatestLogTime
		{
			get
			{
				return _latestLogTime;
			}
			set
			{
				Set(ref _latestLogTime, value);
			}
		}

		//
		public int LastLogID
		{
			get
			{
				return _lastLogID;
			}
			set
			{
				Set(ref _lastLogID, value);
			}
		}

		//
		public int LocalLastLogID
		{
			get
			{
				return _localLastLogID;
			}
			set
			{
				Set(ref _localLastLogID, value);

				updateUnreadCount();
			}
		}

		public string RecentMessage
		{
			get
			{
				return _recentMessage;
			}
			set
			{
				Set(ref _recentMessage, value);
			}
		}

		public List<ClientChatRoomLog> LogList
		{
			get
			{
				return _logList;
			}
			set
			{
				Set(ref _logList, value);
			}
		}

		public ObservableList<JobProgressItemViewModel> SendingJobList => _sendingJobList;

		public int PushConfig
		{
			get
			{
				return _roomData.push_config;
			}
			set
			{
				_roomData.push_config = value;
				notifyPropetyChanged();
			}
		}

		public Vector2Int getVMLogIDRange()
		{
			Vector2Int range = Vector2Int.zero;

			if( _logList.Count > 0)
			{
				range.x = _logList[0].log_id;
				range.y = _logList[_logList.Count - 1].log_id;
			}

			return range;
		}

		// DM채팅방에서의 상대방 프로필 
		public ClientProfileCache DMTargetProfile => _roomData._dmTargetProfile;

		public static ChatRoomViewModel create(ClientAccountChatRoom chatRoom)
		{
			ChatRoomViewModel vm = new ChatRoomViewModel();
			vm.init(chatRoom);
			return vm;
		}

		protected void init(ClientAccountChatRoom chatRoom)
		{
			base.init();

			_roomData = chatRoom;
			_localLastLogID = 0;
			_unreadCount = 0;
			_lastLogID = 0;
			_latestLogTime = DateTime.UtcNow;
			_logList = new List<ClientChatRoomLog>();
			_logMap = new Dictionary<int, ClientChatRoomLog>();
			_recentMessage = "";
			_sendingJobList = new ObservableList<JobProgressItemViewModel>();
		}

		public void updateLatest(MapPacket ack)
		{
			LastLogID = (int)ack.get("last_id");
			updateUnreadCount();

			if( ack.contains("message") && LastLogID >= RoomData.begin_log_id)
			{
				ClientChatRoomLog log = (ClientChatRoomLog)ack.get("message");
				if (log.payload.contains("msg"))
				{
					RecentMessage = log.payload.getString("msg");
				}

				LatestLogTime = (DateTime)ack.get("last_time");
			}
			//else
			//{
			//	LatestLogTime = DateTime.UtcNow;
			//}
		}

		public void updateUnreadCount()
		{
			int unreadCount = _lastLogID - System.Math.Max(_localLastLogID,_roomData.begin_log_id);
			if( unreadCount < 0)
			{
				unreadCount = 0;
			}

			UnreadCount = unreadCount;
		}

		public class LogComparerer : IComparer<ClientChatRoomLog>
		{
			public int Compare(ClientChatRoomLog x, ClientChatRoomLog y)
			{
				if( x.log_id < y.log_id)
				{
					return -1;
				}
				else if( x.log_id > y.log_id)
				{
					return 1;
				}

				return 0;
			}
		}

		private static LogComparerer _logComparer = new LogComparerer();

		public void appendLog(ClientChatRoomLog log)
		{
			if( _logMap.ContainsKey(log.log_id))
			{
				return;
			}

			_logList.Add(log);
			_logMap.Add(log.log_id, log);

			updateRecentMessage();
			notifyPropetyChanged("LogList");
		}

		public void appendLogs(List<ClientChatRoomLog> log_list)
		{
			foreach(ClientChatRoomLog log in log_list)
			{
				if( _logMap.ContainsKey(log.log_id))
				{
					continue;
				}
				_logList.Add(log);
				_logMap.Add(log.log_id, log);
			}
			_logList.Sort(_logComparer);

			updateRecentMessage();
			notifyPropetyChanged("LogList");
		}

		private void updateRecentMessage()
		{
			if( _logList.Count == 0)
			{
				RecentMessage = "";
			}
			else
			{
				// 2022.05.03 이게 마지막 텍스트 채팅을 기록해야 되는걸까?
				ClientChatRoomLog latestLog = _logList[_logList.Count - 1];
				if( latestLog.payload.getInteger("type") == 1)
				{
					RecentMessage = latestLog.payload.getString("msg");
                }
                else if (latestLog.payload.getInteger("type") == 2)
                {
					RecentMessage = GlobalRefDataContainer.getStringCollection().get("messageroom.recent.picture", 0);
                }
            }
		}
	}
}
