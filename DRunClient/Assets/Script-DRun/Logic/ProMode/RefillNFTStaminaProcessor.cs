using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.ProMode
{
	public class RefillNFTStaminaProcessor : BaseLogicStepProcessor
	{
		private int _tokenID;
		private int _refillAmount;

		public static RefillNFTStaminaProcessor create(int tokenID,int refillAmount)
		{
			RefillNFTStaminaProcessor step = new RefillNFTStaminaProcessor();
			step.init(tokenID,refillAmount);
			return step;
		}

		private void init(int tokenID,int refillAmount)
		{
			base.init();
			_tokenID = tokenID;
			_refillAmount = refillAmount;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.RefillStaminaReq);
			req.put("id", _tokenID);
			req.put("delta", _refillAmount);

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
