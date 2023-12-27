using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.ProMode
{
	public class EquipNFTProcessor : BaseLogicStepProcessor
	{
		private int _tokenID;

		public static EquipNFTProcessor create(int tokenID)
		{
			EquipNFTProcessor step = new EquipNFTProcessor();
			step.init(tokenID);
			return step;
		}

		private void init(int tokenID)
		{
			base.init();
			_tokenID = tokenID;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqToServer);
		}

		private void reqToServer(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.ProMode.EquipNFTItemReq);
			req.put("id", _tokenID);
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
