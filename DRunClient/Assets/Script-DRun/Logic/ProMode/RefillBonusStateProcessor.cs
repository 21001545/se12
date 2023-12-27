using DRun.Client.Logic;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace Assets.Script_DRun.Logic.ProMode
{
	public class RefillBonusStateProcessor : BaseLogicStepProcessor
	{
		public static RefillBonusStateProcessor create()
		{
			RefillBonusStateProcessor step = new RefillBonusStateProcessor();
			step.init();
			return step;
		}

		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.RefillNFTBonusReq);
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
