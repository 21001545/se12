using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class GetStepTodayFriendRankProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private HealthTodayFriendRankViewModel ViewModel => ClientMain.instance.getViewModel().Health.TodayStepFriendRank;

		private List<ClientFriendHealthData> _list;

		public static GetStepTodayFriendRankProcessor create()
		{
			GetStepTodayFriendRankProcessor processor = new GetStepTodayFriendRankProcessor();
			processor.init();
			return processor;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryFriendRank);
			_stepList.Add(calcRank);
			_stepList.Add(loadProfiles);
			_stepList.Add(applyViewModel);
		}

		private void queryFriendRank(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.HealthData.GetDailyFriendRankReq);
			req.put("type", HealthDataType.step);
			req.put("id", TimeUtil.todayDayCount());

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_list = ack.getList<ClientFriendHealthData>("data");

					handler(Future.succeededFuture());
				}
			});
		}

		private void calcRank(Handler<AsyncResult<Module.Void>> handler)
		{
			int my_today = ClientMain.instance.getViewModel().Health.TodayStepCount;

			ClientFriendHealthData myData = new ClientFriendHealthData();
			myData.account_id = Network.getAccountID();
			myData.value = my_today;

			_list.Add(myData);

			_list.Sort((a,b)=>{ 
				if( a.value > b.value)
				{
					return -1;
				}
				else if( a.value < b.value)
				{
					return 1;
				}

				return 0;
			});

			// 랭킹 번호 매기기
			int last_value = int.MaxValue;
			int last_rank = 0;
			foreach(ClientFriendHealthData data in _list)
			{
				if( data.value < last_value)
				{
					last_rank++;
				}

				data._rank = last_rank;
				last_value = data.value;
			}

			handler(Future.succeededFuture());
		}

		private void loadProfiles(Handler<AsyncResult<Module.Void>> handler)
		{
			loadProfileIter(_list.GetEnumerator(), handler);
		}

		private void loadProfileIter(IEnumerator<ClientFriendHealthData> it,Handler<AsyncResult<Module.Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientFriendHealthData data = it.Current;
			ClientMain.instance.getProfileCache().getProfileCache(data.account_id, result => { 
				if( result.succeeded())
				{
					data._profile = result.result();
				}

				loadProfileIter(it, handler);
			});
		}

		private void applyViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.FriendList = _list;

			int max_value = -1;
			for(int i = 0; i < _list.Count; ++i)
			{
				if( _list[i].account_id == Network.getAccountID() )
				{
					ViewModel.MyIndex = i;
				}

				max_value = System.Math.Max(max_value,_list[i].value);
			}

			ViewModel.MaxValue = max_value;

			handler(Future.succeededFuture());
		}
	}
}
