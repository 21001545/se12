using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class QueryFollowBackProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManager;
		private SocialViewModel _viewModel;

		private int _target_account_id;
		private Vector2Int _range;
		private List<ClientFollowBack> _followBackList;

		public static QueryFollowBackProcessor create(int target_account_id,Vector2Int range,SocialViewModel targetViewModel)
		{
			QueryFollowBackProcessor p = new QueryFollowBackProcessor();
			p.init(target_account_id,range,targetViewModel);
			return p;
		}

		private void init(int target_account_id,Vector2Int range,SocialViewModel targetViewModel)
		{
			base.init();

			_network = ClientMain.instance.getNetwork();
			_profileCacheManager = ClientMain.instance.getProfileCache();
			_viewModel = targetViewModel;

			_target_account_id = target_account_id;
			_range = range;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryFollowBack);
			_stepList.Add(queryProfile);
			_stepList.Add(addToViewModel);
		}

		private void queryFollowBack(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Social.QueryFollowBackReq);
			req.put("id", _target_account_id);
			req.put("begin", _range.x);
			req.put("count", _range.y);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture( ack.makeErrorException()));
				}
				else
				{
					_followBackList = ack.getList<ClientFollowBack>(MapPacketKey.ClientAck.follow_back);
					handler(Future.succeededFuture());
				}
			});
		}

		private void queryProfile(Handler<AsyncResult<Void>> handler)
		{
			prepareProfileIter(_followBackList, 0, () => {
				handler(Future.succeededFuture());
			});
		}
		
		private void prepareProfileIter(List<ClientFollowBack> list,int id,System.Action callback)
		{
			if( id >= list.Count)
			{
				callback();
				return;
			}

			ClientFollowBack follow_back = list[id];
			_profileCacheManager.getProfileCache(follow_back.follow_back_id, get_profile_result => { 
				if( get_profile_result.succeeded())
				{
					ClientProfileCache profile = get_profile_result.result();
					follow_back._profileCache = profile;
				}

				prepareProfileIter(list, id + 1, callback);
			});
		}

		private void addToViewModel(Handler<AsyncResult<Void>> handler)
		{
			_viewModel.appendFollowBackList(_followBackList);
			handler(Future.succeededFuture());
		}
	}
}
