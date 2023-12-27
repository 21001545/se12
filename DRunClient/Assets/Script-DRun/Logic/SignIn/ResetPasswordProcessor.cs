using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.SignIn
{
	public class ResetPasswordProcessor : BaseStepProcessor
	{
		private string _email;
		private string _code;
		private string _password;

		ClientNetwork Network => ClientMain.instance.getNetwork();

		public static ResetPasswordProcessor create(string email,string code,string password)
		{
			ResetPasswordProcessor p = new ResetPasswordProcessor();
			p.init(email, code, password);
			return p;
		}

		private void init(string email,string code,string password)
		{
			base.init();

			_email = email;
			_code = code;
			_password = password;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Auth.ResetPasswordReq);
			req.put("email", _email);
			req.put("code", _code);
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
	}
}
