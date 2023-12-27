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
	public class SendDirectMessageProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientViewModel _viewModel;
		private ChatViewModel _chatViewModel;
		private LocalChatDataManager _localChatData;

		private int _target_account_id;
		private JsonObject _payload;
		private string _pushMessage;

		private ChatRoomViewModel _chatRoomViewModel;
		private ClientChatRoomLog _log;

		public static SendDirectMessageProcessor create(int target_account_id,JsonObject payload,string push_message)
		{
			SendDirectMessageProcessor processor = new SendDirectMessageProcessor();
			processor.init(target_account_id, payload, push_message);
			return processor;
		}

		private void init(int target_account_id,JsonObject payload,string push_message)
		{
			base.init();

			_target_account_id = target_account_id;
			_payload = payload;
			_pushMessage = push_message;

			_network = ClientMain.instance.getNetwork();
			_viewModel = ClientMain.instance.getViewModel();
			_chatViewModel = _viewModel.Chat;
			_localChatData = ClientMain.instance.getLocalChatData();
		}

		protected override void buildSteps()
		{
			_stepList.Add(getChatRoomFromLocal);
			_stepList.Add(sendMessage);
			_stepList.Add(saveToLocalCache);
		}

		private void getChatRoomFromLocal(Handler<AsyncResult<Module.Void>> handler)
		{
			_chatRoomViewModel = _chatViewModel.findDirectMessageRoom(_target_account_id);
			if(_chatRoomViewModel == null)
			{
				handler(Future.failedFuture(new Exception("chatroom not opened")));
			}
			else
			{
				handler(Future.succeededFuture());
			}
		}

		private void sendMessage(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Chat.SendMessageReq);
			req.put("id", _chatRoomViewModel.ID);
			req.put("data", _payload);
			req.put("message", _pushMessage);	// 서버에서 푸시 보낼때 사용하는 문자열

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_log = (ClientChatRoomLog)ack.get("log");
					_chatRoomViewModel.appendLog(_log);

					//_log = log;
					handler(Future.succeededFuture());
				}
			});
		}

		private void saveToLocalCache(Handler<AsyncResult<Module.Void>> handler)
		{
			List<LDB_ChatRoomLog> log_list = new List<LDB_ChatRoomLog>();
			log_list.Add( LDB_ChatRoomLog.create( _chatRoomViewModel.ID,_log));
			_localChatData.writeChatLog(log_list, handler);
		}
	}
}
