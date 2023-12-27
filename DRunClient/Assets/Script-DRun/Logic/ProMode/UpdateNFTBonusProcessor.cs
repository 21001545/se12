using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.ProMode
{
	public class UpdateNFTBonusProcessor : BaseLogicStepProcessor
	{
		private bool _needUpdate;

		public static UpdateNFTBonusProcessor create()
		{
			UpdateNFTBonusProcessor step = new UpdateNFTBonusProcessor();
			step.init();
			return step;
		}

		protected override void buildSteps()
		{
			_stepList.Add(checkCondition);
			_stepList.Add(reqUpdate);
		}

		private void checkCondition(Handler<AsyncResult<Void>> handler)
		{
			_needUpdate = false;

			ClientNFTBonus bonus =  ViewModel.ProMode.NFTBonus;
			int nftCount = ViewModel.Wallet.getNFTItemList().Count;

			_needUpdate = bonus.nft_count != nftCount;

			handler(Future.succeededFuture());
		}

		private void reqUpdate(Handler<AsyncResult<Void>> handler)
		{
			if( _needUpdate == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.ProMode.UpdateNFTBonusReq);

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
