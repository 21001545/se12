using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class ChatViewModel : AbstractViewModel
	{
		private ClientAccountChatRoomConfig _config;
		private List<ChatRoomViewModel> _roomList;
		private int _totalUnreadCount;

		public ClientAccountChatRoomConfig Config
		{
			get { return _config; }
			set
			{
				Set(ref _config, value);
			}
		}

		public List<ChatRoomViewModel> RoomList
		{
			get
			{
				return _roomList;
			}
		}

		public int TotalUnreadCount
		{
			get
			{
				return _totalUnreadCount;
			}
			set
			{
				Set(ref _totalUnreadCount, value);
			}
		}

		public static ChatViewModel create()
		{
			ChatViewModel vm = new ChatViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_roomList = new List<ChatRoomViewModel>();
			_totalUnreadCount = 0;
		}

		public ChatRoomViewModel findDirectMessageRoom(int target_account_id)
		{
			foreach(ChatRoomViewModel room in _roomList)
			{
				if( room.RoomData.type == ClientAccountChatRoom.Type.direct_message &&
					room.RoomData.target_id == target_account_id)
				{
					return room;
				}
			}

			return null;
		}

		public ChatRoomViewModel findRoom(long chatroom_id)
		{
			foreach(ChatRoomViewModel room in _roomList)
			{
				if( room.ID == chatroom_id)
				{
					return room;
				}
			}

			return null;
		}

		public class RoomComparer : IComparer<ChatRoomViewModel>
		{
			public int Compare(ChatRoomViewModel x, ChatRoomViewModel y)
			{
				// 기획서가 없음으로 일단 임의로 구현
				if( x.LatestLogTime < y.LatestLogTime)
				{
					return 1;
				}
				else if( x.LatestLogTime > y.LatestLogTime )
				{
					return -1;
				}

				return 0;
			}
		}

		private RoomComparer _roomComparer = new RoomComparer();

		public void addChatRoom(ChatRoomViewModel roomVM)
		{
			_roomList.Add(roomVM);
			_roomList.Sort(_roomComparer);
			notifyPropetyChanged("RoomList");
		}

		public void updateChatRoomList(List<ChatRoomViewModel> roomList)
		{
			Debug.Log($"update chatRoomList:[{roomList.Count}]");

			_roomList = roomList;
			_roomList.Sort(_roomComparer);
			notifyPropetyChanged("RoomList");
		}

		public void removeChatRoom(ChatRoomViewModel roomVM)
		{
			_roomList.Remove(roomVM);
			notifyPropetyChanged("RoomList");
		}

		// 대화 상대 이름으로 채팅방 찾기
		public List<ChatRoomViewModel> searchRoom(string text)
		{
			List<ChatRoomViewModel> roomList = new List<ChatRoomViewModel>();
			foreach(ChatRoomViewModel chatRoomViewModel in _roomList)
			{
				ClientProfileCache profile = chatRoomViewModel.RoomData._dmTargetProfile;
				if( profile != null)
				{
					if( profile.Profile.name.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0)
					{
						roomList.Add(chatRoomViewModel);
					}
				}
			}
			return roomList;
		}

		public override void updateFromAck(MapPacket ack)
		{
			if( ack.contains(MapPacketKey.ClientAck.chatroom_config))
			{
				Config = (ClientAccountChatRoomConfig)ack.get(MapPacketKey.ClientAck.chatroom_config);
			}
		}

		public void updateTotalUnreadCount()
		{
			int total_count = 0;
			foreach(ChatRoomViewModel roomVM in _roomList)
			{
				total_count += roomVM.UnreadCount;
			}

			TotalUnreadCount = total_count;
		}
	}
}
