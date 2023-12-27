using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.BasicMode
{
	public class ClaimDailyRewardProcessor : BaseStepProcessor
	{
		private int _slot_id;

		private long _boxReward;
		private long _bonusReward;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public long getBoxReward()
		{
			return _boxReward;
		}

		public long getBonusReward()
		{
			return _bonusReward;
		}

		public static ClaimDailyRewardProcessor create(int slot_id)
		{
			ClaimDailyRewardProcessor p = new ClaimDailyRewardProcessor();
			p.init(slot_id);
			return p;
		}

		private void init(int slot_id)
		{
			base.init();
			_slot_id = slot_id;
		}
		
		protected override void buildSteps()
		{
			_stepList.Add(req);
		}

		private void req(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.BasicMode.ClaimDailyRewardReq);
			req.put("slot_id", _slot_id);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);

					_boxReward = ack.getLong("delta");
					_bonusReward = ack.getLong("bonus");

					handler(Future.succeededFuture());
				}
			});
		}
	}
}
