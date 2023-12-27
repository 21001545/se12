using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.Wallet
{
	public class SpendingToWalletProcessor : BaseLogicStepProcessor
	{
		private long _amount;

		public static SpendingToWalletProcessor create(long amount)
		{
			SpendingToWalletProcessor processor = new SpendingToWalletProcessor();
			processor.init(amount);
			return processor;
		}

		private void init(long amount)
		{
			base.init();
			_amount = amount;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.SpendingToWalletReq);
			req.put("count", _amount);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);

					// transaction처리를 빠르게 확인 할 수 있도록
					UIWallet.getInstance().getTimerIncompleteTransaction().setNext(1.0f);

					handler(Future.succeededFuture());
				}
			});
		}
	}
}
