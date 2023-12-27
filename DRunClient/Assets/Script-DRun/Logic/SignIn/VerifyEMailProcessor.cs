using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.SignIn
{
	public class VerifyEMailProcessor : BaseStepProcessor
	{
		private string _email;
		private string _code;

		private bool _check_result;

		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public bool getCheckResult()
		{
			return _check_result;
		}

		public static VerifyEMailProcessor create(string email,string code)
		{
			VerifyEMailProcessor p = new VerifyEMailProcessor();
			p.init(email, code);
			return p;
		}

		private void init(string email,string code)
		{
			base.init();
			_email = email;
			_code = code;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReqWithoutSession(CSMessageID.Auth.VerifyEMailReq);
			req.put("email", _email);
			req.put("code", _code);

			Network.call(req, ack => { 
				if( ack.getResult() != Festa.Client.ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_check_result = (bool)ack.get("check_result");
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
