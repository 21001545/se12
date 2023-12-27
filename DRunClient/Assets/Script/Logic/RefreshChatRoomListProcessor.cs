using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class RefreshChatRoomListProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ChatViewModel ChatVM => ClientMain.instance.getViewModel().Chat;

		private int _prevRoomListRevision;

		public static RefreshChatRoomListProcessor create()
		{
			RefreshChatRoomListProcessor p = new RefreshChatRoomListProcessor();
			p.init();
			return p;
		}

		protected override void init()
		{
			base.init();
			_prevRoomListRevision = ChatVM.Config.roomlist_revision;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryConfig);
			_stepList.Add(refreshRoomList);
		}

		private void queryConfig(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Chat.GetChatRoomConfigReq);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ClientMain.instance.getViewModel().updateFromPacket(ack);
					handler(Future.succeededFuture());
				}

			});
		}

		private void refreshRoomList(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _prevRoomListRevision == ChatVM.Config.roomlist_revision)
			{
				handler(Future.succeededFuture());
				return;
			}

			Debug.Log($"ChatRoomList revision updated: {_prevRoomListRevision}->{ChatVM.Config.roomlist_revision}");

			QueryChatRoomListProcessor step = QueryChatRoomListProcessor.create();
			step.run(handler);
		}


	}
}
