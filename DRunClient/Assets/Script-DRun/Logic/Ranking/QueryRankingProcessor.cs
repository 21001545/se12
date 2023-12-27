using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Ranking
{
	public class QueryRankingProcessor : BaseLogicStepProcessor
	{
		private int _mode_type;
		private int _mode_sub_type;
		private int _time_type;
		private long _time_id;
		private int _begin;
		private int _count;

		private List<ClientRankingData> _rankingDataList;

		public List<ClientRankingData> getRankingDataList()
		{
			return _rankingDataList;
		}

		public static QueryRankingProcessor create(int mode_type,int mode_sub_type,int time_type,long time_id,int begin,int count)
		{
			QueryRankingProcessor step = new QueryRankingProcessor();
			step.init(mode_type, mode_sub_type, time_type, time_id, begin, count);
			return step;
		}

		private void init(int mode_type, int mode_sub_type, int time_type, long time_id, int begin, int count)
		{
			base.init();
			_mode_type = mode_type;
			_mode_sub_type = mode_sub_type;
			_time_type = time_type;
			_time_id = time_id;
			_begin = begin;
			_count = count;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryRanking);
			_stepList.Add(queryProfile);
		}

		private void queryRanking(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Ranking.ReadRankingReq);
			req.put("mode_type", _mode_type);
			req.put("mode_sub_type", _mode_sub_type);
			req.put("time_type", _time_type);
			req.put("time_id", _time_id);
			req.put("begin", _begin);
			req.put("count", _count);
			req.put("delta", _time_type == ClientRunningLogCumulation.TimeType.total ? false : true);

			Network.call(req, ack => {
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_rankingDataList = ack.getList<ClientRankingData>("data");
					handler(Future.succeededFuture());
				}
			});
		}

		private void queryProfile(Handler<AsyncResult<Void>> handler)
		{
			queryProfileIter(_rankingDataList.GetEnumerator(), handler);
		}

		private void queryProfileIter(IEnumerator<ClientRankingData> it,Handler<AsyncResult<Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientRankingData data = it.Current;

			ClientMain.instance.getProfileCache().getProfileCache(data.account_id, result => { 

				// 실패해도 넘어감
				if( result.succeeded())
				{
					data._profileCache = result.result();
				}

				queryProfileIter(it, handler);
			});
		}

	}
}
