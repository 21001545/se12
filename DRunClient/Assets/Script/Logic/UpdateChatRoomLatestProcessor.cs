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
	public class UpdateChatRoomLatestProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ChatRoomViewModel _roomViewModel;

		public static UpdateChatRoomLatestProcessor create(ChatRoomViewModel vm)
		{
			UpdateChatRoomLatestProcessor p = new UpdateChatRoomLatestProcessor();
			p.init(vm);
			return p;
		}

		private void init(ChatRoomViewModel vm)
		{
			base.init();

			_roomViewModel = vm;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryLatest);
		}

		private void queryLatest(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Chat.GetChatRoomLatestLogIDReq);
			req.put("id", _roomViewModel.ID);
			req.put("last_id", _roomViewModel.LastLogID);

			Network.call(req, ack => { 
				if (ack.getResult() == ResultCode.ok)
				{
					_roomViewModel.updateLatest(ack);
				}

				handler(Future.succeededFuture());
			});
		}

	}
}
