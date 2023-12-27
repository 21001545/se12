using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.SignIn
{
	public class ChangeNameProcessor : BaseStepProcessor
	{
		private string _name;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public static ChangeNameProcessor create(string name)
		{
			ChangeNameProcessor p = new ChangeNameProcessor();
			p.init(name);
			return p;
		}

		private void init(string name)
		{
			base.init();
			_name = name;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Account.ChangeNameReq);
			req.put("name", _name);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.Profile.Profile.name = _name;
					handler(Future.succeededFuture());
				}
			});
		}

		
	}
}
