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

namespace Festa.Client.Logic
{
	public class LeaveChatRoomProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ChatViewModel ChatViewModel => ClientMain.instance.getViewModel().Chat;
		private LocalChatDataManager LocalChatData => ClientMain.instance.getLocalChatData();

		private ChatRoomViewModel _roomVM;

		public static LeaveChatRoomProcessor create(ChatRoomViewModel roomVM)
		{
			LeaveChatRoomProcessor processor = new LeaveChatRoomProcessor();
			processor.init(roomVM);
			return processor;
		}

		private void init(ChatRoomViewModel roomVM)
		{
			base.init();

			_roomVM = roomVM;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqLeaveRoom);
			_stepList.Add(applyViewModel);
			_stepList.Add(saveToLocal);
		}

		private void reqLeaveRoom(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Chat.LeaveChatRoomReq);
			req.put("id", _roomVM.ID);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		private void applyViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			_roomVM.RoomData.status = ClientAccountChatRoom.Status.leaved;

			ChatViewModel.removeChatRoom(_roomVM);

			handler(Future.succeededFuture());
		}

		private void saveToLocal(Handler<AsyncResult<Module.Void>> handler)
		{
			LDB_AccountChatRoom chatRoom = LDB_AccountChatRoom.create(_roomVM);
			LocalChatData.writeAccountChatRoom(chatRoom, handler);
		}

	}
}
