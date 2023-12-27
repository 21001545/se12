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
	public class QueryChatLogProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private LocalChatDataManager LocalChatCache => ClientMain.instance.getLocalChatData();

		private ChatRoomViewModel _roomViewModel;
		private int _begin;
		private int _end;

		private List<ClientChatRoomLog> _logList;

		public static QueryChatLogProcessor create(ChatRoomViewModel vm,int begin,int end)
		{
			QueryChatLogProcessor p = new QueryChatLogProcessor();
			p.init(vm, begin, end);
			return p;
		}

		private void init(ChatRoomViewModel vm, int begin,int end)
		{
			base.init();

			_roomViewModel = vm;
			_begin = begin;
			_end = end;

			Debug.Log($"start query chatlog: room_id[{_roomViewModel.ID}] begin[{_begin}] end[{_end}]");
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryLog);
			_stepList.Add(saveLocalDB_Log);
			_stepList.Add(saveLocalDB_ChatRoom);
			_stepList.Add(applyViewModel);
			_stepList.Add(updateTotalUnreadCount);
		}

		private void queryLog(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Chat.QueryChatRoomLogReq);
			req.put("id", _roomViewModel.ID);
			req.put("begin", _begin);
			req.put("end", _end);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_logList = ack.getList<ClientChatRoomLog>("data");

					//foreach(ClientChatRoomLog log in _logList)
					//{
					//	Debug.Log(log.payload.getString("msg"));
					//}

					handler(Future.succeededFuture());
				}
			});
		}

		// 굳이 기다릴 필요가 있을까?
		private void saveLocalDB_Log(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _logList.Count == 0)
			{
				// 2022.06.08 이강희 메세지를 받은지 3일이 넘도록 확인을 하지 않은 경우가 있을 수 있다
				_roomViewModel.LocalLastLogID = _end;

				handler(Future.succeededFuture());
				return;
			}

			List<LDB_ChatRoomLog> local_log_list = LDB_ChatRoomLog.createList(_roomViewModel.ID, _logList);
			LocalChatCache.writeChatLog(local_log_list, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_roomViewModel.LocalLastLogID = _logList[_logList.Count - 1].log_id;

					handler(Future.succeededFuture());
				}
			});
		}

		private void saveLocalDB_ChatRoom(Handler<AsyncResult<Module.Void>> handler)
		{
			LDB_AccountChatRoom localChatRoom = LDB_AccountChatRoom.create(_roomViewModel);
			LocalChatCache.writeAccountChatRoom(localChatRoom, result_room => {

				if (result_room.failed())
				{
					handler(Future.failedFuture(result_room.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		private void applyViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			_roomViewModel.appendLogs(_logList);
		}

		private void updateTotalUnreadCount(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.Chat.updateTotalUnreadCount();
			handler(Future.succeededFuture());
		}
	}
}
