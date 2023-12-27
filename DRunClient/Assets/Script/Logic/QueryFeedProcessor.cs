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

namespace Festa.Client.Logic
{
	public class QueryFeedProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManager;
		private ClientViewModel _viewModel;

		private List<ClientFeed> _feedList;
		private List<ClientFeed> _loadedFeedList;

		private static bool _feedLoading;

		public static bool isFeedLoading()
		{
			return _feedLoading;
		}

		public static QueryFeedProcessor create()
		{
			QueryFeedProcessor p = new QueryFeedProcessor();
			p.init();
			return p;
		}

		protected override void buildSteps()
		{
			_stepList.Add(start);
			_stepList.Add(queryFeed);
            //_stepList.Add(checkBookmark);
			_stepList.Add(loadMoments);
			_stepList.Add(loadProfiles);
            _stepList.Add(queryRecentComment);
            _stepList.Add(loadCommentProfiles);
			_stepList.Add(rearrangeFeed);
            _stepList.Add(setupViewModel);
			_stepList.Add(end);
		}

		protected override void init()
		{
			base.init();

			_network = ClientMain.instance.getNetwork();
			_profileCacheManager = ClientMain.instance.getProfileCache(); 
			_viewModel = ClientMain.instance.getViewModel();
			_logStepTime = false;
		}

		private void start(Handler<AsyncResult<Module.Void>> handler)
		{
			_feedLoading = true;
			handler(Future.succeededFuture());
		}

		private void end(Handler<AsyncResult<Module.Void>> handler)
		{
			_feedLoading = false;
			handler(Future.succeededFuture());
		}

		private void queryFeed(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Feed.QueryReq);
			_network.call(req, ack => {
				if (ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_viewModel.updateFromPacket(ack);

					_feedList = ack.getList<ClientFeed>("feed");
					_loadedFeedList = new List<ClientFeed>();
					for(int i = 0; i < _feedList.Count && i < 10; ++i)
					{
						_loadedFeedList.Add(_feedList[i]);
					}

					List<int> bookmarkList = ack.getList<int>("bookmark");
					foreach(int object_id in bookmarkList)
					{
						ClientFeed feed = null;
						for(int i =0; i < _loadedFeedList.Count; ++i)
						{
							if(_loadedFeedList[ i].object_id == object_id)
							{
								feed = _loadedFeedList[i];
								break;
							}
						}

						if( feed != null)
						{
							feed._checkBookmark = true;
						}
					}
					

					Debug.Log($"loaded feed count: {_loadedFeedList.Count}");

					handler(Future.succeededFuture());
				}
			});
		}

		//private void checkBookmark(Handler<AsyncResult<Module.Void>> handler)
		//{
		//	checkBookmarkIter(_loadedFeedList.GetEnumerator(), handler);	
		//}

		//private void checkBookmarkIter(List<ClientFeed>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		//{
		//	if( e.MoveNext() == false)
		//	{
		//		handler(Future.succeededFuture());
		//		return;
		//	}

		//	ClientFeed feed = e.Current;

		//	MapPacket req = _network.createReq(CSMessageID.Feed.CheckBookmarkReq);
		//	req.put("object_datasource_id", feed.object_datasource_id);
		//	req.put("object_owner_account_id", feed.object_owner_account_id);
		//	req.put("object_type", feed.object_type);
		//	req.put("object_id", feed.object_id);

		//	_network.call(req, ack => {
		//		if( ack.getResult() == ResultCode.ok)
		//		{
		//			feed._checkBookmark = (bool)ack.get("check_result");
		//		}

		//		checkBookmarkIter(e, handler);
		//	});
		//}

		private void loadMoments(Handler<AsyncResult<Module.Void>> handler)
		{
			Dictionary<int, List<ClientFeed>> datasource_map = new Dictionary<int, List<ClientFeed>>();

			foreach(ClientFeed feed in _loadedFeedList)
			{
				List<ClientFeed> list;
				if( datasource_map.TryGetValue( feed.object_datasource_id, out list) == false)
				{
					list = new List<ClientFeed>();
					datasource_map.Add(feed.object_datasource_id, list);
				}

				list.Add(feed);
			}

			loadMoments_Iter(datasource_map.GetEnumerator(), result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					// 로딩에 실패한 것들은 걸러내보자
					List<ClientFeed> temp_list = new List<ClientFeed>();
					foreach(ClientFeed feed in _loadedFeedList)
					{
						if( feed._moment != null)
						{
							temp_list.Add(feed);
						}
					}

					_loadedFeedList = temp_list;

					handler(Future.succeededFuture());
				}
			});
		}

		private void loadMoments_Iter(Dictionary<int,List<ClientFeed>>.Enumerator e, Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			int datasource_id = e.Current.Key;
			List<ClientFeed> list = e.Current.Value;

			List<int> account_list = new List<int>();
			List<int> id_list = new List<int>();

			foreach(ClientFeed feed in list)
			{
				account_list.Add(feed.object_owner_account_id);
				id_list.Add(feed.object_id);
			}

			MapPacket req = _network.createReq(CSMessageID.Moment.QueryRemoteListReq);
			req.put("datasource_id", datasource_id);
			req.put("account", account_list);
			req.put("id", id_list);

			_network.call(req, ack => {
				if (ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<ClientMoment> moment_list = ack.getList<ClientMoment>("data");
					List<bool> check_like_list = ack.getList<bool>("check_like");

					// assign moment
					for(int i = 0; i < moment_list.Count; ++i)
					{
						ClientMoment moment = moment_list[i];
						moment._isLiked = check_like_list[i];

						foreach (ClientFeed feed in list)
						{
							if (feed.object_owner_account_id == moment.account_id &&
								feed.object_id == moment.id)
							{
								feed._moment = moment;
								break;
							}
						}
					}

					loadMoments_Iter(e, handler);
				}
			});
		}

		private void loadProfiles(Handler<AsyncResult<Module.Void>> handler)
		{
			loadProfileIter(_loadedFeedList.GetEnumerator(), handler);
		}

		private void loadProfileIter(List<ClientFeed>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientFeed feed = e.Current;

			_profileCacheManager.getProfile(feed._moment.account_id, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					feed._moment._profile = result.result();

					loadProfileIter(e, handler);
				}
			});
		}

        private void queryRecentComment(Handler<AsyncResult<Module.Void>> handler)
        {
            queryRecentCommentIter(_loadedFeedList.GetEnumerator(), handler);
        }

        private void queryRecentCommentIter(List<ClientFeed>.Enumerator e, Handler<AsyncResult<Module.Void>> handler)
        {
            if (e.MoveNext() == false)
            {
                handler(Future.succeededFuture());
                return;
            }

            ClientFeed feed = e.Current;
            feed._moment.recent_comment = new List<ClientMomentComment>();
            // 최근 코멘트 2개만 가져오자.
            MapPacket req = _network.createReq(CSMessageID.Moment.QueryCommentReq);
            req.put("moment_account_id", feed._moment.account_id);
            req.put("moment_id", feed._moment.id);
			req.put("slot_id", 0);	// 2022.02.07 이강희 top level comment만 얻어오기
            req.put("begin", 0);
            req.put("count", 2);

            _network.call(req, ack =>
            {
                if (ack.getResult() != ResultCode.ok)
                {
                }
                else
                {
                    var commentList = ack.getList<ClientMomentComment>("comment");
                    List<bool> check_like_list = ack.getList<bool>("check_like");
                    for (int i = 0; i < check_like_list.Count; ++i)
                    {
                        commentList[i]._isLiked = check_like_list[i];
                    }

                    feed._moment.recent_comment = commentList;
                }

                queryRecentCommentIter(e, handler);
            });
        }

		private void loadCommentProfiles(Handler<AsyncResult<Module.Void>> handler)
        {
			List<ClientMomentComment> clientMomentComments = new List<ClientMomentComment>();

			foreach( var feed in _loadedFeedList)
            {
				if ( feed._moment.recent_comment.Count > 0 )
					clientMomentComments.AddRange(feed._moment.recent_comment);
            }

			loadCommentProfilesIter(clientMomentComments.GetEnumerator(), handler);
        }

        private void loadCommentProfilesIter(List<ClientMomentComment>.Enumerator e, Handler<AsyncResult<Module.Void>> handler)
        {
            if (e.MoveNext() == false)
            {
                handler(Future.succeededFuture());
                return;
            }

            ClientMomentComment comment = e.Current;
            _profileCacheManager.getProfile(comment.comment_account_id, result =>
            {
                if (result.failed() == false )
                {
                    comment._profile = result.result();
                }

                loadCommentProfilesIter(e, handler);
            });
        }

		// 2022.06.03 이강희
		// 방금 내가 올린 게시물은 어떤 조건과 상관 없이 최상단으로 가야됨
		// 올린 시간이 특정 간격 미만일 경우 순서가 바뀌는 문제 도 수정
		private void rearrangeFeed(Handler<AsyncResult<Module.Void>> handler)
		{
			for(int i = 0; i < _loadedFeedList.Count; i++)
			{
				ClientFeed feed = _loadedFeedList[i];
				feed._serverOrder = i;
				feed._justUploadedMyMoment = checkJustUploadedMyMoment(feed._moment);
			}

			_loadedFeedList.Sort((a, b) => {

				// 방금 내가 올린건 최상단
				if( a._justUploadedMyMoment == true && b._justUploadedMyMoment == false)
				{
					return -1;
				}
				else if( a._justUploadedMyMoment == false && b._justUploadedMyMoment == true)
				{
					return 1;
				}

				// 둘다 방금 올렸으면 시간 순으로 정렬
				if( a._justUploadedMyMoment == true && b._justUploadedMyMoment == true)
				{
					if( a._moment.create_time > b._moment.create_time)
					{
						return -1;
					}
					else if( a._moment.create_time < b._moment.create_time)
					{
						return 1;
					}
				}

				if( a._serverOrder < b._serverOrder)
				{
					return -1;
				}
				else if( a._serverOrder > b._serverOrder)
				{
					return 1;
				}

				return 0;
			});

			//
			//for(int i = 0; i < _loadedFeedList.Count; ++i)
			//{
			//	ClientFeed feed = _loadedFeedList[i];
			//	if( feed._serverOrder != i)
			//	{
			//		Debug.Log($"feed order changed: {feed._serverOrder} -> {i}");
			//	}
			//}

			handler(Future.succeededFuture());
		}

		private bool checkJustUploadedMyMoment(ClientMoment moment)
		{
			if( moment.account_id != _network.getAccountID())
			{
				return false;
			}

			// 기분 2분이내
			TimeSpan diff = DateTime.UtcNow - moment.create_time;
			if( diff.TotalMinutes > 3.0f)
			{
				return false;
			}

			return true;
		}

		private void setupViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			_viewModel.Feed.FeedList = _feedList;
			_viewModel.Feed.LoadedFeedList = _loadedFeedList;
			_viewModel.Feed.LastFeedQueryTime = DateTime.UtcNow;

			handler(Future.succeededFuture());
		}
	}
}
