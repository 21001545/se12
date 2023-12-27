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
	// 로직상 채팅 방 열기가 따로 있어야 될것 같음

	public class OpenChatRoomProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ChatViewModel ChatViewModel => ClientMain.instance.getViewModel().Chat;
		private LocalChatDataManager LocalChatData => ClientMain.instance.getLocalChatData();
		private ClientProfileCacheManager ProfileCache => ClientMain.instance.getProfileCache();
		
		private int _target_account_id;

		private ChatRoomViewModel _chatRoomViewModel;
		private ClientAccountChatRoom _chatRoom;
		private bool _saveChatRoomToLocal;

		public ChatRoomViewModel getRoomViewModel()
		{
			return _chatRoomViewModel;
		}

		public static OpenChatRoomProcessor create(int target_account_id)
		{
			OpenChatRoomProcessor processor = new OpenChatRoomProcessor();
			processor.init(target_account_id);
			return processor;
		}
		
		private void init(int target_account_id)
		{
			base.init();
			_target_account_id = target_account_id;
			_saveChatRoomToLocal = false;
		}

		protected override void buildSteps()
		{
			_stepList.Add(checkLocal);
			_stepList.Add(reqOpenChatRoom);
			_stepList.Add(prepareDMTargetProfile);
			_stepList.Add(makeViewModel);
			_stepList.Add(saveChatRoomToLocal);
		}

		private void checkLocal(Handler<AsyncResult<Module.Void>> handler)
		{
			// 이미 열린 채팅방
			ChatRoomViewModel chatRoomVM = ChatViewModel.findDirectMessageRoom(_target_account_id);
			if( chatRoomVM != null)
			{
				_chatRoomViewModel = chatRoomVM;
			}

			handler(Future.succeededFuture());
		}

		private void reqOpenChatRoom(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _chatRoomViewModel != null)
			{
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.Chat.OpenDMChatRoomReq);
			req.put("target", _target_account_id);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					// 비용처리가 들어 있다
					ClientMain.instance.getViewModel().updateFromPacket(ack);

					_chatRoom = (ClientAccountChatRoom)ack.get("chatroom");
					_saveChatRoomToLocal = true;

					handler(Future.succeededFuture());
				}
			});
		}

		private void prepareDMTargetProfile(Handler<AsyncResult<Module.Void>> handler)
		{
			if(_saveChatRoomToLocal == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ProfileCache.getProfileCache(_target_account_id, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_chatRoom._dmTargetProfile = result.result();
					handler(Future.succeededFuture());
				}
			});
		}
		
		private void makeViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			if (_saveChatRoomToLocal == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			_chatRoomViewModel = ChatRoomViewModel.create(_chatRoom);
			ChatViewModel.addChatRoom(_chatRoomViewModel);
			
			handler(Future.succeededFuture());
		}

		private void saveChatRoomToLocal(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _saveChatRoomToLocal == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			LocalChatData.writeAccountChatRoom(LDB_AccountChatRoom.create(_chatRoomViewModel), handler);
		}
	}
}
