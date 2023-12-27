using Festa.Client.MapBox;
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
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering;

namespace Festa.Client.Logic
{
	public class QueryTripCheerableListProcessor : BaseStepProcessor
	{
		private MBLongLatCoordinate _searchByDistanceCenterLocation;
		private double _searchByDistanceRadius;
		private int _searchByDistanceCount;

		private List<ClientSearchByDistance> _searchByDistanceList;

		private Dictionary<int,ClientTripCheerable> _searchByDistanceTripUserList;
		private List<ClientTripCheerable> _followInTripList;

		private List<ClientTripCheerable> _cheerableListInSearchByDistance;
		private List<ClientTripCheerable> _cheerableListInFollow;

		protected ClientNetwork Network => ClientMain.instance.getNetwork();
		protected ClientViewModel ViewModel => ClientMain.instance.getViewModel();
	
		public List<ClientTripCheerable> getCheearableInSearchByDistance()
		{
			return _cheerableListInSearchByDistance;
		}

		public List<ClientTripCheerable> getCheerableInFollow()
		{
			return _cheerableListInFollow;
		}

		public static QueryTripCheerableListProcessor create()
		{
			QueryTripCheerableListProcessor processor = new QueryTripCheerableListProcessor();
			processor.init();
			return processor;
		}

		protected override void init()
		{
			base.init();

			//_searchByDistanceCenterLocation = ClientMain.instance.getLocation().getDevice().getLastLocation();
			_searchByDistanceRadius = 30.0; // 반경 30km
			_searchByDistanceCount = 30;	// 너무 많은가?
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqSearchByDistance);					// 주변 유저 목록 얻어오기
			_stepList.Add(filterTripUserSearchByDistance);      // 얻어온 유저중 탐험중인 유저 검사
			_stepList.Add(reqFollowInTrip);                     // 탐험중인 친구 목록 얻어 오기
			_stepList.Add(removeSearchByDistanceInFollow);      // 주변 유저중 친구는 빼주자
			_stepList.Add(makeCheerableSearchByDistance);   // 실제응원 가능한 목록 만들기 (중목 응원 제외)
			_stepList.Add(makeCheerableFollow);             // 실제응원 가능한 목록 만들기 (중목 응원 제외)
			_stepList.Add(applyViewModel);
		}

		private void reqSearchByDistance(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Map.SearchByDistanceReq);
			req.put("longitude", _searchByDistanceCenterLocation.lon);
			req.put("latitude", _searchByDistanceCenterLocation.lat);
			req.put("radius", _searchByDistanceRadius);
			req.put("count", _searchByDistanceCount);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_searchByDistanceList = ack.getList<ClientSearchByDistance>("data");
					handler(Future.succeededFuture());
				}
			});
		}

		private void filterTripUserSearchByDistance(Handler<AsyncResult<Module.Void>> handler)
		{
			_searchByDistanceTripUserList = new Dictionary<int,ClientTripCheerable>();

			filterTripUserSearchByDistanceIter(_searchByDistanceList.GetEnumerator(), handler);
		}

		private void filterTripUserSearchByDistanceIter(IEnumerator<ClientSearchByDistance> it,Handler<AsyncResult<Module.Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientSearchByDistance user = it.Current;

			// 나도 있으니 빼주자
			if( user.account_id == Network.getAccountID())
			{
				filterTripUserSearchByDistanceIter(it, handler);
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.Trip.GetTripConfigReq);
			req.put("id", user.account_id);

			Network.call(req, ack => { 
				if( ack.getResult() == ResultCode.ok)
				{
					ClientTripConfig tripConfig = (ClientTripConfig)ack.get("data");
					if( tripConfig.status != ClientTripConfig.StatusType.none)
					{
						_searchByDistanceTripUserList.Add(user.account_id,ClientTripCheerable.create(user.account_id, tripConfig.next_trip_id));
					}
				}

				filterTripUserSearchByDistanceIter(it, handler);
			});
		}

		private void reqFollowInTrip(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Social.QueryFollowInTripReq);
			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_followInTripList = ack.getList<ClientTripCheerable>("data");
					foreach(ClientTripCheerable trip in _followInTripList)
					{
						trip._isFollow = true;
					}
					handler(Future.succeededFuture());
				}
			});
		}

		private void removeSearchByDistanceInFollow(Handler<AsyncResult<Module.Void>> handler)
		{
			List<int> delList = new List<int>();
			foreach(ClientTripCheerable followCheearable in _followInTripList)
			{
				if( _searchByDistanceTripUserList.ContainsKey(followCheearable.account_id))
				{
					delList.Add(followCheearable.account_id);
				}
			}

			foreach(int follow_id in delList)
			{
				_searchByDistanceTripUserList.Remove(follow_id);
			}

			handler(Future.succeededFuture());
		}

		private void makeCheerableIter(IEnumerator<ClientTripCheerable> it,List<ClientTripCheerable> resultList,Handler<AsyncResult<Module.Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientTripCheerable tripCheerable = it.Current;
			MapPacket req = Network.createReq(CSMessageID.Trip.CheckCheerableReq);
			req.put("id", tripCheerable.account_id);
			req.put("sub_id", tripCheerable.trip_id);

			Network.call(req, ack => { 
				if( ack.getResult() == ResultCode.ok)
				{
					tripCheerable._isAlreadyCheered = (bool)ack.get("check_result") == false;

					resultList.Add(tripCheerable);
				}

				makeCheerableIter(it, resultList, handler);
			});
		}

		private void makeCheerableSearchByDistance(Handler<AsyncResult<Module.Void>> handler)
		{
			List<ClientTripCheerable> checkList = _searchByDistanceTripUserList.Values.ToList();
			_cheerableListInSearchByDistance = new List<ClientTripCheerable>();

			makeCheerableIter(checkList.GetEnumerator(), _cheerableListInSearchByDistance, handler);
		}

		private void makeCheerableFollow(Handler<AsyncResult<Module.Void>> handler)
		{
			_cheerableListInFollow = new List<ClientTripCheerable>();

			makeCheerableIter(_followInTripList.GetEnumerator(), _cheerableListInFollow, handler);
		}

		private void applyViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			ViewModel.Trip.CheerableListByDistance = _cheerableListInSearchByDistance;
			ViewModel.Trip.CheerableListByFollow = _cheerableListInFollow;
			ViewModel.Trip.LastQueryCheerableTime = TimeUtil.unixTimestampUtcNow();
			ViewModel.Trip.TotalCheerableCount = _cheerableListInSearchByDistance.Count + _cheerableListInFollow.Count;

			if( ViewModel.Trip.TotalCheerableCount > 0)
			{
				Debug.Log($"query trip cheerable: by-distance[{_cheerableListInSearchByDistance.Count}] by-follow[{_cheerableListInFollow.Count}] total[{ViewModel.Trip.TotalCheerableCount}]");
			}

			handler(Future.succeededFuture());
		}
	}
}
