using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Firebase;
using Firebase.Messaging;
using System;
using UnityEngine;

namespace Festa.Client
{
	public class ClientFirebasePushManager
	{
		private ClientNetwork _network;
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public static ClientFirebasePushManager create()
		{
			ClientFirebasePushManager m = new ClientFirebasePushManager();
			m.init();
			return m;
		}

		private void init()
		{
			_network = ClientMain.instance.getNetwork();
		}

		public void start()
		{
			FirebaseMessaging.TokenReceived += onTokenReceived;
			FirebaseMessaging.MessageReceived += onMessageReceived;
		}

		private void onTokenReceived(object sender,TokenReceivedEventArgs token)
		{
			Debug.Log("push token received : " + token.Token);

			MapPacket req = _network.createReq(CSMessageID.Account.RegisterPushTokenReq);
			req.put("push_token", token.Token);

			_network.call(req, ack => { });
		}

		private void onMessageReceived(object sender,MessageReceivedEventArgs message)
		{
			FirebaseMessage fbMessage = message.Message;
			Debug.Log($"push message received : {fbMessage.From}");

			string meta_data;
			if( fbMessage.Data.TryGetValue("meta_data", out meta_data))
			{
				Debug.Log(meta_data);

				try
				{
					JsonObject jsonMetaData = new JsonObject(meta_data);

					int type = jsonMetaData.getInteger("type");
					if( type == PushDataDefine.Type.chatroom_message)
					{
						//meta_data.put("type", PushDataDefine.Type.chatroom_message);
						//meta_data.put("chatroom_id", _chatroom_id);
						//meta_data.put("sender_id", _account_id);
						//meta_data.put("sender_type", ChatRoomLog.SenderType.account);
						//meta_data.put("payload", _payload);
						//meta_data.put("time", TimeUtil.nowUTC());

						JsonObject data = jsonMetaData.getJsonObject("data");

						long chatroom_id = data.getLong("chatroom_id");	// 채팅방 번호
						ClientChatRoomLog log = ClientChatRoomLog.createFromPushMetaData(data);
						onChatRoomLog(chatroom_id, log);
					}
					else if( type == PushDataDefine.Type.activity)
					{
						//JsonObject json = new JsonObject();
						//json.put("slot_id", slot_id);
						//json.put("event_time", event_time.getTime());
						//json.put("event_type", event_type);
						//json.put("agent_account_id", agent_account_id);
						//json.put("param1", param1);
						//json.put("param2", param2);
						//json.put("string_param", string_param);
						//json.put("reward_type", reward_type);
						//json.put("reward_refid", reward_refid);
						//json.put("reward_amount", reward_amount);
						//json.put("claim_status", claim_status);

						ClientActivity activity = ClientActivity.createFromPushMetaData(jsonMetaData.getJsonObject("data"));
						onActivity(activity);
					}
				}
				catch(System.Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		//
		private void onChatRoomLog(long chatroom_id,ClientChatRoomLog log)
		{
			ChatRoomViewModel roomVM = ViewModel.Chat.findRoom(chatroom_id);

			if (roomVM != null)
			{
				// 해당 채팅방의 logid를 최신화하여 TotalUnreadCount를 갱신함
				QueryChatRoomLatestLogIDProcessor step = QueryChatRoomLatestLogIDProcessor.create(roomVM);
				step.run(result => { });
			}
			else
			{
				// 방목록자체를 갱신해야 될것 같은뎅
				RefreshChatRoomListProcessor step = RefreshChatRoomListProcessor.create();
				step.run(result => { });
			}
		}

		private void onActivity(ClientActivity activity)
		{

		}
	}
}
