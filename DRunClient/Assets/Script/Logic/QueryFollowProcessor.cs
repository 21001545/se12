using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class QueryFollowProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManager;
		private SocialViewModel _viewModel;

		private int _target_account_id;
		private Vector2Int _range;
		private List<ClientFollow> _followList;

		public static QueryFollowProcessor create(int target_account_id,Vector2Int range,SocialViewModel targetViewModel)
		{
			QueryFollowProcessor p = new QueryFollowProcessor();
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
			_stepList.Add(queryFollow);
			_stepList.Add(checkFollowBack);
			_stepList.Add(queryProfile);
			_stepList.Add(addToViewModel);
		}

		private void queryFollow(Handler<AsyncResult<Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Social.QueryFollowReq);
			req.put("id", _target_account_id);
			req.put("begin", _range.x);
			req.put("count", _range.y);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_followList = ack.getList<ClientFollow>(MapPacketKey.ClientAck.follow);
					handler(Future.succeededFuture());
				}
			});
		}

		private void checkFollowBack(Handler<AsyncResult<Void>> handler)
		{
			checkFollowBackIter(_followList.GetEnumerator(), handler);
		}

		private void checkFollowBackIter(List<ClientFollow>.Enumerator e,Handler<AsyncResult<Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientFollow follow = e.Current;
			MapPacket req = _network.createReq(CSMessageID.Social.CheckFollowBackReq);
			req.put("id", follow.follow_id);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					follow._isFollowBack = (bool)ack.get("check_result");
					checkFollowBackIter(e, handler);
				}
			});
		}

		private void queryProfile(Handler<AsyncResult<Void>> handler)
		{
			prepareProfileIter(_followList, 0, () => {
				handler(Future.succeededFuture());
			});
		}

		private void prepareProfileIter(List<ClientFollow> list,int id,System.Action callback)
		{
			if( id >= list.Count)
			{
				callback();
				return;
			}

			ClientFollow follow = list[id];
			_profileCacheManager.getProfileCache(follow.follow_id, get_profile_result=> { 
				if( get_profile_result.succeeded())
				{
					ClientProfileCache profile = get_profile_result.result();
					follow._profileCache = profile;
				}

				prepareProfileIter(list, id + 1, callback);
			});
		}

		private void addToViewModel(Handler<AsyncResult<Void>> handler)
		{
			_viewModel.appendFollowList(_followList);
			handler(Future.succeededFuture());
		}
	}
}
