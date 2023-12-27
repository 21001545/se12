using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class GetStepTodayTotalRankProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private HealthTodayTotalRankViewModel ViewModel => ClientMain.instance.getViewModel().Health.TodayStepTotalRank;

		public static GetStepTodayTotalRankProcessor create()
		{
			GetStepTodayTotalRankProcessor processor = new GetStepTodayTotalRankProcessor();
			processor.init();
			return processor;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryTotalRank);
		}

		private void queryTotalRank(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.HealthData.GetDailyTotalRankReq);
			req.put("type", HealthDataType.step);
			req.put("id", TimeUtil.todayDayCount());

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					int rank = (int)ack.get("rank");
					int count = (int)ack.get("count");
					int average = (int)ack.get("average");

					ViewModel.Rank = rank;
					ViewModel.Count = count;
					ViewModel.Average = average;
					ViewModel.updateRankRatio();

					handler(Future.succeededFuture());
				}
			});

		}
	}
}
