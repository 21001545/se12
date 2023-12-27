using Festa.Client.LocalDB;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class QueryChatRoomLatestLogIDProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		//private LocalChatDataManager LocalChatData => ClientMain.instance.getLocalChatData();

		private ChatRoomViewModel _roomVM;
		//private bool _saveToLocal;

		public static QueryChatRoomLatestLogIDProcessor create(ChatRoomViewModel roomVM)
		{
			QueryChatRoomLatestLogIDProcessor p = new QueryChatRoomLatestLogIDProcessor();
			p.init(roomVM);
			return p;
		}

		private void init(ChatRoomViewModel roomVM)
		{
			base.init();

			_roomVM = roomVM;
			//_saveToLocal = false;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryLatest);
			_stepList.Add(updateTotalUnreadCount);
			//_stepList.Add(saveToLocal);
		}

		private void queryLatest(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Chat.GetChatRoomLatestLogIDReq);
			req.put("id", _roomVM.ID);
			req.put("last_id", _roomVM.LastLogID);

			Network.call(req, ack => { 
				if( ack.getResult() == ResultCode.ok)
				{
					_roomVM.updateLatest(ack);
				}

				handler(Future.succeededFuture());
			});
		}

		private void updateTotalUnreadCount(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.Chat.updateTotalUnreadCount();
			handler(Future.succeededFuture());
		}

		//private void saveToLocal(Handler<AsyncResult<Module.Void>> handler)
		//{
		//	if( _saveToLocal == false)
		//	{
		//		return;
		//	}

		//	LDB_AccountChatRoom localChatRoom = LDB_AccountChatRoom.create(_roomVM.RoomData);
		//	LocalChatData.writeAccountChatRoom(localChatRoom, handler);
		//}
	}
}
