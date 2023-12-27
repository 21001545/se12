using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System.Collections.Generic;

namespace Festa.Client.Logic
{
	public class QueryMomentCommentProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManager;
		private ClientViewModel _viewModel;

		private ClientMoment _moment;
		private int _begin;
		private int _count;
		private int _slot;
		private List<ClientMomentComment> _commentList;

		public static QueryMomentCommentProcessor create(ClientMoment moment,int begin,int count)
		{
			QueryMomentCommentProcessor p = new QueryMomentCommentProcessor();
			p.init( moment, begin, count);
			return p;
		}

		public static QueryMomentCommentProcessor create(ClientMomentComment comment, int begin, int count)
		{
			QueryMomentCommentProcessor p = new QueryMomentCommentProcessor();
			p.init(comment._moment, begin, count, comment.slot_id);
			return p;
		}

		protected void init(ClientMoment moment,int begin,int count, int slot = 0)
		{
			base.init();

			_moment = moment;
			_begin = begin;
			_count = count;
			_slot = slot;
			_network = ClientMain.instance.getNetwork();
			_profileCacheManager = ClientMain.instance.getProfileCache();
			_viewModel = ClientMain.instance.getViewModel();
		}

		public List<ClientMomentComment> getCommentList()
		{
			return _commentList;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryComments);
			_stepList.Add(loadProfiles);
		}

		private void queryComments(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Moment.QueryCommentReq);
			req.put("moment_account_id", _moment.account_id);
			req.put("moment_id", _moment.id);
			req.put("slot_id", _slot);	// 2022.02.07 이강희 top level comment만 가져오기
			req.put("begin", _begin);
			req.put("count", _count);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_commentList = ack.getList<ClientMomentComment>("comment");
					List<bool> check_like_list = ack.getList<bool>("check_like");
					for(int i = 0; i < check_like_list.Count; ++i)
					{
						_commentList[i]._isLiked = check_like_list[i];
					}
					for (int i = 0; i < _commentList.Count; ++i)
					{
						_commentList[i]._moment = _moment;
					}

					handler(Future.succeededFuture());
				}
			});
		}

		private void loadProfiles(Handler<AsyncResult<Module.Void>> handler)
		{
			loadProfile_Iter(_commentList.GetEnumerator(), handler);
		}

		private void loadProfile_Iter(List<ClientMomentComment>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		{
			if (e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientMomentComment comment = e.Current;

			_profileCacheManager.getProfile(comment.comment_account_id, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					comment._profile = result.result();

					loadProfile_Iter(e, handler);
				}
			});
		}


	}
}
