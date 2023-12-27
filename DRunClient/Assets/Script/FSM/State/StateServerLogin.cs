using Festa.Client.Data;
using Festa.Client.Module.FSM;
using Festa.Client.Module.Net;
using Firebase.Crashlytics;

namespace Festa.Client
{
	public class StateServerLogin : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.server_login;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("server login...", 41);
			startLogin();
		}

		private void startLogin()
		{
			MapPacket req = MapPacket.createWithMsgID(CSMessageID.Account.LoginReq);

			StartupContext ctx = _data.getStartupContext(); 

			req.put("firebase_id", ctx.firebase_id);
			req.put("phone_number", ctx.phone_number);
			req.put("provider_id", ctx.provider_id);
			req.put("device_id", ctx.device_id);
			req.put("timezone_offset", ctx.timezone_offset);

			_network.call(req, ack => {

				if (ack.getResult() != ResultCode.ok)
				{
					UIPopup.spawnOK("login fail", () => {

						startLogin();

					});
					//_owner.changeState(ClientStateType.sleep);
				}
				else
				{
					processClientData(ack);

					changeToNextState();
				}

			});
/*
	public static int firebase_id = EncryptUtil.makeHashCode("firebase_id");
		public static int account_id = EncryptUtil.makeHashCode("account_id");
		public static int device_id = EncryptUtil.makeHashCode("device_id");
		public static int phone_number = EncryptUtil.makeHashCode("phone_number");
		public static int provider_id = EncryptUtil.makeHashCode("provider_id");
		public static int is_newaccount = EncryptUtil.makeHashCode("is_newaccount");
*/
		}

		private void processClientData(MapPacket ack)
		{
			int token = (int)ack.get("token");
			int account_id = (int)ack.get("account_id");

			_network.setSession(account_id, token);
			
			_data.getStartupContext().is_newaccount = (bool)ack.get(MapPacketKey.is_newaccount);

			_viewModel.setupFromLogin(account_id,ack);

			// 2022.02.03 Firebase Crashlytics
			Crashlytics.SetCustomKey("account_id", account_id.ToString());
			Crashlytics.SetCustomKey("token", token.ToString());
			Crashlytics.SetUserId(account_id.ToString());
		}
	}
}
