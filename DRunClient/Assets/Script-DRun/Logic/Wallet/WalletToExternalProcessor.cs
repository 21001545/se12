using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.Wallet
{
	public class WalletToExternalProcessor : BaseLogicStepProcessor
	{
		private string _receiverAddress;
		private int _assetType;
		private string _amount;
		
		public static WalletToExternalProcessor create(string receiverAddress,int assetType,string amount)
		{
			WalletToExternalProcessor processor = new WalletToExternalProcessor();
			processor.init(receiverAddress,assetType, amount);
			return processor;
		}

		private void init(string receiverAddress, int assetType,string amount)
		{
			base.init();
			_receiverAddress = receiverAddress;
			_assetType = assetType;
			_amount = amount;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.WalletToExternalReq);
			req.put("target", _receiverAddress);
			req.put("type", _assetType);
			req.put("count", _amount);

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
