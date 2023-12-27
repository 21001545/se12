using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class QueryBookmarkProcessor : BaseStepProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManager;
		private ClientViewModel _viewModel;

		private List<ClientBookmark> _loadedBookmarkList;
		private List<ClientBookmark> _deletedMomentBookmarkList;

		private int _target_account_id;
		private int _begin;
		private int _count;

		public List<ClientBookmark> getBookmarkList()
		{
			return _loadedBookmarkList;
		}

		public static QueryBookmarkProcessor create(int target_account_id,int begin,int count)
		{
			QueryBookmarkProcessor p = new QueryBookmarkProcessor();
			p.init(target_account_id, begin, count);
			return p;
		}

		protected override void buildSteps()
		{
			_stepList.Add(queryBookmark);
			_stepList.Add(loadMoments);
			_stepList.Add(loadProfiles);
			_stepList.Add(filterInvalidBookmark);
			_stepList.Add(reqRemoveInvalidBookmark);
			//_stepList.Add(setupViewModel);
		}

		protected void init(int target_account_id,int begin,int count)
		{
			base.init();

			_network = ClientMain.instance.getNetwork();
			_profileCacheManager = ClientMain.instance.getProfileCache();
			_viewModel = ClientMain.instance.getViewModel();

			_target_account_id = target_account_id;
			_begin = begin;
			_count = count;
		}

		private void queryBookmark(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Feed.QueryBookmarkReq);
			req.put("id", _target_account_id);
			req.put("begin", _begin);
			req.put("count", _count);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_loadedBookmarkList = ack.getList<ClientBookmark>(MapPacketKey.ClientAck.bookmark);
					handler(Future.succeededFuture());
				}
			});
		}

		private void loadMoments(Handler<AsyncResult<Module.Void>> handler)
		{
			Dictionary<int, List<ClientBookmark>> datasource_map = new Dictionary<int, List<ClientBookmark>>();

			foreach(ClientBookmark bookmark in _loadedBookmarkList)
			{
				List<ClientBookmark> list;
				if( datasource_map.TryGetValue( bookmark.object_datasource_id, out list) == false)
				{
					list = new List<ClientBookmark>();
					datasource_map.Add(bookmark.object_datasource_id, list);
				}

				list.Add(bookmark);
			}

			loadMomentsIter(datasource_map.GetEnumerator(), handler);
		}
		private void loadMomentsIter(Dictionary<int,List<ClientBookmark>>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			int datasource_id = e.Current.Key;
			List<ClientBookmark> list = e.Current.Value;

			List<int> account_list = new List<int>();
			List<int> id_list = new List<int>();
			
			foreach(ClientBookmark bookmark in list)
			{
				account_list.Add(bookmark.object_owner_account_id);
				id_list.Add(bookmark.object_id);
			}

			MapPacket req = _network.createReq(CSMessageID.Moment.QueryRemoteListReq);
			req.put("datasource_id", datasource_id);
			req.put("account", account_list);
			req.put("id", id_list);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<ClientMoment> moment_list = ack.getList<ClientMoment>("data");
					List<bool> check_like_list = ack.getList<bool>("check_like");

					//
					for(int i = 0; i < moment_list.Count; ++i)
					{
						ClientMoment moment = moment_list[i];
						moment._isLiked = check_like_list[i];

						foreach(ClientBookmark bookmark in list)
						{
							if( bookmark.object_owner_account_id == moment.account_id &&
								bookmark.object_id == moment.id)
							{
								bookmark._moment = moment;
								break;
							}
						}
					}

					loadMomentsIter(e, handler);
				}
			});
		}

		private void loadProfiles(Handler<AsyncResult<Module.Void>> handler)
		{
			loadProfileIter(_loadedBookmarkList.GetEnumerator(), handler);
		}

		private void loadProfileIter(List<ClientBookmark>.Enumerator e,Handler<AsyncResult<Module.Void>> handler)
		{
			if( e.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientBookmark bookmark = e.Current;
			if( bookmark._moment == null)
			{
				loadProfileIter(e, handler);
				return;
			}


			_profileCacheManager.getProfile(bookmark._moment.account_id, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					bookmark._moment._profile = result.result();

					loadProfileIter(e, handler);
				}
			});
		}

		private void filterInvalidBookmark(Handler<AsyncResult<Module.Void>> handler)
		{
			// 삭제된 모먼트 북마크 제거
			_deletedMomentBookmarkList = new List<ClientBookmark>();
			List<ClientBookmark> newList = new List<ClientBookmark>();
			foreach(ClientBookmark bookmark in _loadedBookmarkList)
			{
				if( bookmark._moment == null)
				{
					_deletedMomentBookmarkList.Add(bookmark);
					continue;
				}

				newList.Add(bookmark);
			}

			_loadedBookmarkList = newList;

			handler(Future.succeededFuture());
		}

		// 삭제된 모먼트가 있다면 북마크에서 제거 시킴
		private void reqRemoveInvalidBookmark(Handler<AsyncResult<Module.Void>> handler)
		{
			reqRemoveBookmarkIter(_deletedMomentBookmarkList.GetEnumerator(),handler);
		}

		private void reqRemoveBookmarkIter(IEnumerator<ClientBookmark> it,Handler<AsyncResult<Module.Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			ClientBookmark bookmark = it.Current;

			MapPacket req = _network.createReq(CSMessageID.Feed.RemoveBookmarkReq);

			req.put("object_datasource_id", bookmark.object_datasource_id);
			req.put("object_owner_account_id", bookmark.object_owner_account_id);
			req.put("object_type", bookmark.object_type);
			req.put("object_id", bookmark.object_id);

			_network.call(req, ack => {
				reqRemoveBookmarkIter(it, handler);
			});
		}
	}
}
