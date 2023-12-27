using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class QueryActivityProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManger;
		private ClientViewModel _viewModel;

		private int _begin_slot_id;
		private int _begin_index;
		private int _count;

		private List<ClientActivity> _activityList;

		public static QueryActivityProcessor create(int begin_slot_id,int begin_index,int count)
		{
			QueryActivityProcessor processor = new QueryActivityProcessor();
			processor.init(begin_slot_id, begin_index, count);
			return processor;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryActivity);
			_stepList.Add(queryMoment);
			_stepList.Add(queryAgentProfiles);
			_stepList.Add(setupViewModel);
		}

		private void init(int begin_slot_id,int begin_index,int count)
		{
			base.init();

			_network = ClientMain.instance.getNetwork();
			_profileCacheManger = ClientMain.instance.getProfileCache();
			_viewModel = ClientMain.instance.getViewModel();

			_begin_slot_id = begin_slot_id;
			_begin_index = begin_index;
			_count = count;
		}

		private void queryActivity(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Social.QueryActivityReq);
			req.put("slot_id", _begin_slot_id);
			req.put("begin", _begin_index);
			req.put("count", _count);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					var list = ack.getList<ClientActivity>("data");

					// 2022.06.08 읽은 slot-id가 꼬이는 이슈가 있다
					int max_slot_id = 0;
					
					// 220215, swpark 이미 받은 좋아요 보상 받은건 노출시키지 않는다.
					_activityList = new List<ClientActivity>();
					foreach (var data in list)
					{
						if( data.slot_id > max_slot_id)
						{
							max_slot_id = data.slot_id;
						}

						if (data.event_type == ClientActivity.Type.reward_moment_like && data.claim_status == ClientActivity.ClaimStatus.claimed)
						{
							continue;
						}

						_activityList.Add(data);
					}

					_viewModel.Activity.ReadSlotID = max_slot_id;
					Debug.Log($"read slot id : {_viewModel.Activity.ReadSlotID}");

					handler(Future.succeededFuture());
				}
			});
		}

		private void queryMoment(Handler<AsyncResult<Module.Void>> handler)
		{
			List<int> id_list = new List<int>();
			foreach (ClientActivity activity in _activityList)
			{
				int moment_id = activity.getMomentID();
				if( moment_id != 0)
				{
					if( id_list.Contains(moment_id))
					{
						continue;
					}

					id_list.Add(moment_id);
				}
			}

			MapPacket req = _network.createReq(CSMessageID.Moment.QueryListByIDReq);
			req.put("id", id_list);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<ClientMoment> moment_list = ack.getList<ClientMoment>("data");

					Dictionary<int,ClientMoment> momentDic = new Dictionary<int, ClientMoment>();
					foreach(ClientMoment moment in moment_list)
					{
						momentDic.Add(moment.id, moment);
					}

					foreach(ClientActivity activity in _activityList)
					{
						int moment_id = activity.getMomentID();
						if( moment_id != 0)
						{
							momentDic.TryGetValue(moment_id, out activity._moment);
						}
					}

					handler(Future.succeededFuture());
				}
			});
		}

		private void queryAgentProfiles(Handler<AsyncResult<Module.Void>> handler)
		{
			queryProfileIterator(_activityList.GetEnumerator(), handler);
		}

		private void queryProfileIterator(List<ClientActivity>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientActivity activity = e.Current;

			_profileCacheManger.getProfile(activity.agent_account_id, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					activity._agentProfile = result.result();

					queryProfileIterator(e, handler);
				}
			});
		}

		private void setupViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			_viewModel.Activity.appendActivity(_activityList);

			handler(Future.succeededFuture());
		}
	}
}
