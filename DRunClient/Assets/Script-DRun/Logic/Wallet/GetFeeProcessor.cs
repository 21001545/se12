using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Wallet
{
	public class GetFeeProcessor : BaseLogicStepProcessor
	{
		public static GetFeeProcessor create()
		{
			GetFeeProcessor processor = new GetFeeProcessor();
			processor.init();
			return processor;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.GetFeeReq);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.Wallet.WalletFee = (JsonObject)ack.get("data");

					handler(Future.succeededFuture());
				}
			});
		}
	}
}
