using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.SignIn
{
	public class SendEMailVerifyCodeProcessor : BaseStepProcessor
	{
		private bool _checkSignUp;
		private string _email;

		private bool _checkSignUpResult;

		public bool checkSignUpResult()
		{
			return _checkSignUpResult;
		}

		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public static SendEMailVerifyCodeProcessor create(string email,bool check_signup)
		{
			SendEMailVerifyCodeProcessor p = new SendEMailVerifyCodeProcessor();
			p.init(email, check_signup);
			return p;
		}

		private void init(string email,bool checkSignUp)
		{
			base.init();
			_email = email;
			_checkSignUp = checkSignUp;
		}

		protected override void buildSteps()
		{
			_stepList.Add(checkSignUp);
			_stepList.Add(req);
		}

		private void checkSignUp(Handler<AsyncResult<Void>> handler)
		{
			if( _checkSignUp == false)
			{
				_checkSignUpResult = true;
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReqWithoutSession(CSMessageID.Auth.CheckSignUpReq);
			req.put("email", _email);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_checkSignUpResult = (bool)ack.get("check_result");
					
					if( _checkSignUpResult == false)
					{
						handler(Future.failedFuture("already sign-in email"));
					}
					else
					{
						handler(Future.succeededFuture());
					}
				}
			});
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReqWithoutSession(CSMessageID.Auth.SendEMailVerifyCodeReq);
			req.put("email", _email);

			Network.call(req, ack => { 
				if( ack.getResult() != Festa.Client.ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
