using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.SignIn
{
	public class SignUpProcessor : BaseStepProcessor
	{
		private string _email;
		private string _password;

		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public static SignUpProcessor create(string email,string password)
		{
			SignUpProcessor p = new SignUpProcessor();
			p.init(email, password);
			return p;
		}

		private void init(string email,string password)
		{
			base.init();

			_email = email;
			_password = password;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqSignUp);
			_stepList.Add(loginToServer);
		}

		private void reqSignUp(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Auth.SignUpReq);
			req.put("email", _email);
			req.put("password", _password);

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

		private void loginToServer(Handler<AsyncResult<Void>> handler)
		{
			LoginProcessor step = LoginProcessor.create(_email);
			step.run(handler);
		}

	}
}
