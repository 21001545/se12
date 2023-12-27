using DRun.Client.Logic.ProMode;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.Wallet
{
	public class NFTItemToExternalProcessor : BaseLogicStepProcessor
	{
		private string _receiverAddress;
		private int _tokenID;

		public static NFTItemToExternalProcessor create(string receiverAddress, int tokenID)
		{
			NFTItemToExternalProcessor processor = new NFTItemToExternalProcessor();
			processor.init(receiverAddress, tokenID);
			return processor;
		}

		private void init(string receiverAddress, int tokenID)
		{
			base.init();
			_receiverAddress = receiverAddress;
			_tokenID = tokenID;
		}

		protected override void buildSteps()
		{
			_stepList.Add(unequipNFT);
			_stepList.Add(withdraw);
		}

		private void unequipNFT(Handler<AsyncResult<Void>> handler)
		{
			// MainDZ가 아니면 skip
			if( ViewModel.ProMode.EquipedNFTItem == null ||
				ViewModel.ProMode.EquipedNFTItem.token_id != _tokenID)
			{
				handler(Future.succeededFuture());
				return;
			}

			// 일단 장착을 해제하고 진행해야 한다
			UnequipNFTProcessor step = UnequipNFTProcessor.create();
			step.run(handler);
		}

		private void withdraw(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Wallet.NFTItemToExternalReq);
			req.put("id", _tokenID);
			req.put("target", _receiverAddress);

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
