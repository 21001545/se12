using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.ProMode
{
	public class BoostExpNFTProcessor : BaseLogicStepProcessor
	{
		public int _tokenID;
		public int _boostAmount;

		public static BoostExpNFTProcessor create(int tokenID,int boostAmount)
		{
			BoostExpNFTProcessor step = new BoostExpNFTProcessor();
			step.init(tokenID, boostAmount);
			return step;
		}

		private void init(int tokenID,int boostAmount)
		{
			base.init();
			_tokenID = tokenID;
			_boostAmount = boostAmount;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.BoostExpReq);
			req.put("id", _tokenID);
			req.put("delta", _boostAmount);

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
