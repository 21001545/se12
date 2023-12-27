using DRun.Client.NetData;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.Module;
using Festa.Client;
using static Festa.Client.CSMessageID;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Ranking
{
	public class QueryRankingSingleProcessor : BaseLogicStepProcessor
	{
		private int _target_account_id;
		private int _mode_type;
		private int _mode_sub_type;
		private int _time_type;
		private long _time_id;

		private ClientRankingData _rankingData;

		public ClientRankingData getRankingData()
		{
			return _rankingData;
		}

		public static QueryRankingSingleProcessor create(int target_account_id,int mode_type, int mode_sub_type, int time_type, long time_id)
		{
			QueryRankingSingleProcessor step = new QueryRankingSingleProcessor();
			step.init(target_account_id,mode_type, mode_sub_type, time_type, time_id);
			return step;
		}

		private void init(int target_account_id,int mode_type, int mode_sub_type, int time_type, long time_id)
		{
			base.init();
			_target_account_id = target_account_id;
			_mode_type = mode_type;
			_mode_sub_type = mode_sub_type;
			_time_type = time_type;
			_time_id = time_id;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryRanking);
			_stepList.Add(queryProfile);
		}


		private void queryRanking(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Ranking.ReadRankingSingleReq);
			req.put("mode_type", _mode_type);
			req.put("mode_sub_type", _mode_sub_type);
			req.put("time_type", _time_type);
			req.put("time_id", _time_id);
			req.put("id", _target_account_id);
			req.put("delta", _time_type == ClientRunningLogCumulation.TimeType.total ? false : true);

			Network.call(req, ack => {
				if (ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					// _rankingData.score == 0 (랭킹 데이터가 없는 경우)
					_rankingData = (ClientRankingData)ack.get("data");
					handler(Future.succeededFuture());
				}
			});
		}

		private void queryProfile(Handler<AsyncResult<Void>> handler)
		{
			ClientMain.instance.getProfileCache().getProfileCache(_rankingData.account_id, result => {
				if( result.succeeded())
				{
					_rankingData._profileCache = result.result();
				}

				handler(Future.succeededFuture());
			});
		}
	}
}
