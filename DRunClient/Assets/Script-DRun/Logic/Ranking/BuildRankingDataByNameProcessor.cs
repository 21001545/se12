using DRun.Client.Logic;
using DRun.Client.Logic.Ranking;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace Assets.Script_DRun.Logic.Ranking
{
	public class BuildRankingDataByNameProcessor : BaseLogicStepProcessor
	{
		private int _mode_type;
		private int _mode_sub_type;
		private int _time_type;
		private long _time_id;
		private string _search_name;

		private RankingCacheData _cacheData;

		private List<int> _accountList;
		private List<ClientRankingData> _rankingDataList;
		private ClientRankingData _myRankingData;

		public RankingCacheData getCacheData()
		{
			return _cacheData;
		}

		public static BuildRankingDataByNameProcessor create(int mode_type,int mode_sub_type,int time_type,string name)
		{
			BuildRankingDataByNameProcessor step = new BuildRankingDataByNameProcessor();
			step.init(mode_type, mode_sub_type, time_type, name);
			return step;
		}

		private void init(int mode_type,int mode_sub_type,int time_type,string name)
		{
			base.init();
			_mode_type = mode_type;
			_mode_sub_type = mode_sub_type;
			_time_type = time_type;
			_time_id = calcCurrentTimeID(_time_type);
			_search_name = name;
		}

		private long calcCurrentTimeID(int time_type)
		{
			if (time_type == ClientRunningLogCumulation.TimeType.day)
			{
				return TimeUtil.todayDayCount();
			}
			else if (time_type == ClientRunningLogCumulation.TimeType.week)
			{
				return TimeUtil.thisWeekCount();
			}
			else if (time_type == ClientRunningLogCumulation.TimeType.month)
			{
				return TimeUtil.thisMonthCount();
			}
			else if (time_type == ClientRunningLogCumulation.TimeType.year)
			{
				return TimeUtil.thisYearCount();
			}
			else if (time_type == ClientRunningLogCumulation.TimeType.total)
			{
				return 0;
			}

			return -1;
		}

		protected override void buildSteps()
		{
			_stepList.Add(searchByName);
			_stepList.Add(loadRanking);
			_stepList.Add(loadMyRanking);
			_stepList.Add(applyViewModel);
		}

		private void searchByName(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Account.SearchByNameReq);
			req.put("name", _search_name);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_accountList = ack.getList<int>("account_id");
					Debug.Log($"search by name: count[{_accountList.Count}]");

					//if( _accountList.Contains( Network.getAccountID()) == false)
					//{
					//	_accountList.Add(Network.getAccountID());
					//}
					handler(Future.succeededFuture());
				}
			});
		}

		private void loadRanking(Handler<AsyncResult<Void>> handler)
		{
			_rankingDataList = new List<ClientRankingData>();
			loadRankingIter(_accountList.GetEnumerator(), handler);
		}

		private void loadRankingIter(IEnumerator<int> it,Handler<AsyncResult<Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			int account_id = it.Current;
			QueryRankingSingleProcessor step = QueryRankingSingleProcessor.create(account_id, _mode_type, _mode_sub_type, _time_type, _time_id);
			step.run(result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					ClientRankingData data = step.getRankingData();
					if( data.score != 0)
					{
						_rankingDataList.Add(data);
					}

					//if( data.account_id == Network.getAccountID())
					//{
					//	_myRankingData = data;
					//}

					loadRankingIter(it, handler);
				}
			});
		}

		private void loadMyRanking(Handler<AsyncResult<Void>> handler)
		{
			int account_id = Network.getAccountID();
			QueryRankingSingleProcessor step = QueryRankingSingleProcessor.create( account_id, _mode_type, _mode_sub_type, _time_type, _time_id);
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

		private void applyViewModel(Handler<AsyncResult<Void>> handler)
		{
			_cacheData = RankingCacheData.create(_rankingDataList, _myRankingData);

			handler(Future.succeededFuture());
		}
	}
}
