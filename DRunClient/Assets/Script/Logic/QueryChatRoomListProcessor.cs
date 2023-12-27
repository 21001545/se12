using Festa.Client.LocalDB;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class QueryChatRoomListProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientProfileCacheManager ProfileCache => ClientMain.instance.getProfileCache();
		private LocalChatDataManager LocalChatData => ClientMain.instance.getLocalChatData();

		private List<ClientAccountChatRoom> _roomList;
		private List<ChatRoomViewModel> _roomVMList;
		private List<LDB_AccountChatRoom> _saveRoomList;

		public static QueryChatRoomListProcessor create()
		{
			QueryChatRoomListProcessor p = new QueryChatRoomListProcessor();
			p.init();
			return p;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryList);
			_stepList.Add(prepareDMTargetProfiles);
			_stepList.Add(makeViewModel);
			_stepList.Add(queryLatest);
			_stepList.Add(applyViewModel);
			_stepList.Add(saveToLocal);
			_stepList.Add(updateTotalUnreadCount);
		}

		protected override void init()
		{
			base.init();
			_saveRoomList = new List<LDB_AccountChatRoom>();
		}

		private void queryList(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Chat.QueryAccountChatRoomReq);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_roomList = ack.getList<ClientAccountChatRoom>(MapPacketKey.ClientAck.chatroom_list);
					handler(Future.succeededFuture());
				}
			});
		}

		private void prepareDMTargetProfiles(Handler<AsyncResult<Module.Void>> handler)
		{
			prepareDMTargetProfileIter(_roomList.GetEnumerator(), handler);
		}

		private void prepareDMTargetProfileIter(List<ClientAccountChatRoom>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}
			
			//DM방만 해당
			ClientAccountChatRoom room = e.Current;
			if( room.type != ClientAccountChatRoom.Type.direct_message)
			{
				prepareDMTargetProfileIter(e, handler);
				return;
			}

			ProfileCache.getProfileCache(room.target_id, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					ClientProfileCache profile = result.result();
					room._dmTargetProfile = profile;

					prepareDMTargetProfileIter(e, handler);
				}
			});
		}

		private void makeViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			_roomVMList = new List<ChatRoomViewModel>();

			foreach (ClientAccountChatRoom room in _roomList)
			{
				ChatRoomViewModel vm = ChatRoomViewModel.create(room);
				_roomVMList.Add(vm);
			}

			handler(Future.succeededFuture());
		}

		private void queryLatest(Handler<AsyncResult<Module.Void>> handler)
		{
			queryLatestIter(_roomVMList.GetEnumerator(), handler);
		}

		private void queryLatestIter(List<ChatRoomViewModel>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ChatRoomViewModel vm = e.Current;

			// 로컬검색
			LocalChatData.readAccountChatRoom(vm.ID, local_result => { 
				if( local_result.failed())
				{
					handler(Future.failedFuture(local_result.cause()));
					return;
				}

				LDB_AccountChatRoom room = local_result.result();
				if( room != null)
				{
					vm.LocalLastLogID = System.Math.Max(room.last_log_id, room.begin_log_id);

					Debug.Log($"room[{room.chatroom_id}] localLastLogID[{room.last_log_id}]");
				}

				MapPacket req = Network.createReq(CSMessageID.Chat.GetChatRoomLatestLogIDReq);
				req.put("id", vm.ID);
				req.put("last_id", 0);

				Network.call(req, ack => {
					if (ack.getResult() == ResultCode.ok)
					{
						vm.updateLatest(ack);

						// local에 방이 없을때만 저장
						if( room == null)
						{
							_saveRoomList.Add(LDB_AccountChatRoom.create(vm));
						}
					}

					queryLatestIter(e, handler);
				});
			});
		}

		private void applyViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.Chat.updateChatRoomList(_roomVMList);
			handler(Future.succeededFuture());
		}

		private void saveToLocal(Handler<AsyncResult<Module.Void>> handler)
		{
			// 기다리지 말아볼까?
			//handler(Future.succeededFuture());

			List<LDB_AccountChatRoom> localChatRoomList = new List<LDB_AccountChatRoom>();
			LocalChatData.writeAccountChatRoom(localChatRoomList, handler);
		}

		private void updateTotalUnreadCount(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.Chat.updateTotalUnreadCount();
			handler(Future.succeededFuture());
		}
	}
}
