using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.BasicMode
{
	public class ExpireDailyRewardProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public static ExpireDailyRewardProcessor create()
		{
			ExpireDailyRewardProcessor p = new ExpireDailyRewardProcessor();
			p.init();
			return p;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.BasicMode.ExpireDailyRewardReq);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
