using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.SignIn
{
	public class SignInProcessor : BaseStepProcessor
	{
		private string _email;
		private string _password;

		private int _errorCode;
		
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public int getErrorCode()
		{
			return _errorCode;
		}

		public static SignInProcessor create(string email,string password)
		{
			SignInProcessor p = new SignInProcessor();
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
			_stepList.Add(req);
			_stepList.Add(saveCredential);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Auth.SignInReq);
			req.put("email", _email);
			req.put("password", _password);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					_errorCode = ack.getResult();
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		private void saveCredential(Handler<AsyncResult<Void>> handler)
		{
			ViewModel.SignIn.EMail = _email;

			handler(Future.succeededFuture());
		}
	}
}
