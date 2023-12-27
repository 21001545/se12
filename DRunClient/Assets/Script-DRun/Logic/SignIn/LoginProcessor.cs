using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Firebase.Crashlytics;
using UnityEngine;

namespace DRun.Client.Logic.SignIn
{
	public class LoginProcessor : BaseStepProcessor
	{
		private string _email;

		private MapPacket _ack;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public bool isNewAccount()
		{
			return (bool)_ack.get(MapPacketKey.is_newaccount);
		}

		public static LoginProcessor create(string email)
		{
			LoginProcessor p = new LoginProcessor();
			p.init(email);
			return p;
		}

		private void init(string email)
		{
			base.init();
			_email = email;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
			_stepList.Add(processClientData);
		}

		private string getDeviceID()
		{
			return GlobalConfig.getDeviceID();
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = MapPacket.createWithMsgID(CSMessageID.Account.LoginReq);

			req.put("firebase_id", _email);
			req.put("phone_number", "n/a");
			req.put("provider_id", "local");
			req.put("device_id", getDeviceID());
			req.put("timezone_offset", TimeUtil.timezoneOffset());

			// 
			req.put("device_model", SystemInfo.deviceModel);
			req.put("operating_system", SystemInfo.operatingSystem);
			req.put("app_version", Application.version);

			Network.call(req, ack => { 
				if( ack.getResult() != Festa.Client.ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_ack = ack;

					handler(Future.succeededFuture());
				}
			});
		}

		private void processClientData(Handler<AsyncResult<Void>> handler)
		{
			int token = (int)_ack.get("token");
			int account_id = (int)_ack.get("account_id");

			Network.setSession(account_id, token);
			ViewModel.setupFromLogin(account_id,_ack);

			// 2022.02.03 Firebase Crashlytics
			Crashlytics.SetCustomKey("account_id", account_id.ToString());
			Crashlytics.SetCustomKey("token", token.ToString());
			Crashlytics.SetUserId(account_id.ToString());

			handler(Future.succeededFuture());
		}
	}
}
