using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Wallet
{
	public class GetBalanceProcessor : BaseLogicStepProcessor
	{
		public static GetBalanceProcessor create()
		{
			GetBalanceProcessor processor = new GetBalanceProcessor();
			processor.init();
			return processor;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.GetBalanceReq);
			Network.call(req, ack => {
				if(ack.getResult() != ResultCode.ok)
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
