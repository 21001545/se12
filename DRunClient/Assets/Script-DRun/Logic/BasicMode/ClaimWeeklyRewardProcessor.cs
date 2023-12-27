using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;

namespace DRun.Client.Logic.BasicMode
{
	public class ClaimWeeklyRewardProcessor : BaseStepProcessor
	{
		private int _week_id;

		private long _delta;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public int getWeekID()
		{
			return _week_id;
		}

		public long getReward()
		{
			return _delta;
		}

		public static ClaimWeeklyRewardProcessor create(int week_id)
		{
			ClaimWeeklyRewardProcessor p = new ClaimWeeklyRewardProcessor();
			p.init(week_id);
			return p;
		}

		private void init(int week_id)
		{
			base.init();
			_week_id = week_id;
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqClaim);
			_stepList.Add(applyVM);
		}

		private void reqClaim(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.BasicMode.ClaimWeeklyRewardReq);
			req.put("week_id", _week_id);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ViewModel.updateFromPacket(ack);
					_delta = ack.getLong("delta");

					handler(Future.succeededFuture());
				}
			});
		}

		private void applyVM(Handler<AsyncResult<Void>> handler)
		{
			if( _delta > 0)
			{
				ViewModel.BasicMode.ClaimedWeeklyRewardData = ClaimedWeeklyRewardData.create(_week_id, _delta);
			}

			handler(Future.succeededFuture());
		}

	}
}
