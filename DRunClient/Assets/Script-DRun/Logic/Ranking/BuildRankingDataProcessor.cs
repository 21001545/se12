using DRun.Client.NetData;
using DRun.Client.ViewModel;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Ranking
{
	// 최초 쿼리용 1 ~ 20등까지 긁어온다
	public class BuildRankingDataProcessor : BaseLogicStepProcessor
	{
		private int _mode_type;
		private int _mode_sub_type;
		private int _time_type;
		private long _time_id;
		private int _page_index;

		public const int pageCount = 20;

		private RankingCacheData _cacheData;

		private List<ClientRankingData> _rankingDataList;
		private ClientRankingData _myRankingData;

		public RankingCacheData getCacheData()
		{
			return _cacheData;
		}

		public static BuildRankingDataProcessor create(int mode_type,int mode_sub_type,int time_type,int page_index)
		{
			BuildRankingDataProcessor step = new BuildRankingDataProcessor();
			step.init(mode_type, mode_sub_type, time_type, page_index);
			return step;
		}

		private void init(int mode_type,int mode_sub_type,int time_type,int page_index)
		{
			base.init();
			_mode_type = mode_type;
			_mode_sub_type = mode_sub_type;
			_time_type = time_type;
			_time_id = calcCurrentTimeID(_time_type);
			_page_index = page_index;
		}

		private long calcCurrentTimeID(int time_type)
		{
			if( time_type == ClientRunningLogCumulation.TimeType.day)
			{
				return TimeUtil.todayDayCount();
			}
			else if( time_type == ClientRunningLogCumulation.TimeType.week)
			{
				return TimeUtil.thisWeekCount();
			}
			else if( time_type == ClientRunningLogCumulation.TimeType.month)
			{
				return TimeUtil.thisMonthCount();
			}
			else if( time_type == ClientRunningLogCumulation.TimeType.year)
			{
				return TimeUtil.thisYearCount();
			}
			else if( time_type == ClientRunningLogCumulation.TimeType.total)
			{
				return 0;
			}

			return -1;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryMyRanking);
			_stepList.Add(queryRanking);
			_stepList.Add(applyViewModel);
		}

		private void queryMyRanking(Handler<AsyncResult<Void>> handler)
		{
			QueryRankingSingleProcessor step = QueryRankingSingleProcessor.create(Network.getAccountID(), _mode_type, _mode_sub_type, _time_type, _time_id);
			step.run(result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_myRankingData = step.getRankingData();
					handler(Future.succeededFuture());
				}
			});
		}

		private void queryRanking(Handler<AsyncResult<Void>> handler)
		{
			QueryRankingProcessor step = QueryRankingProcessor.create(_mode_type, _mode_sub_type, _time_type, _time_id, _page_index * pageCount, pageCount);
			step.run(result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_rankingDataList = step.getRankingDataList();
					handler(Future.succeededFuture());
				}
			});
		}

		private void applyViewModel(Handler<AsyncResult<Void>> handler)
		{
			_cacheData = RankingCacheData.create(_rankingDataList, _myRankingData);

			// cache는 나중에 구현하자
			handler(Future.succeededFuture());
		}
	}
}
