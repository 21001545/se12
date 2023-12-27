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
	public class ClientProfileCacheManager
	{
		private ClientNetwork _network;

		private Dictionary<int, ClientProfileCache> _profileMap;

		public static ClientProfileCacheManager create()
		{
			ClientProfileCacheManager manager = new ClientProfileCacheManager();
			manager.init();
			return manager;
		}

		private void init()
		{
			_network = ClientMain.instance.getNetwork();
			_profileMap = new Dictionary<int, ClientProfileCache>();
		}

		public void getProfile(int account_id,Handler<AsyncResult<ClientProfile>> callback)
		{
			getProfileCache(account_id, result => { 
				if( result.failed())
				{
					callback(Future.failedFuture<ClientProfile>(result.cause()));
				}
				else
				{
					ClientProfileCache cache = result.result();
					callback(Future.succeededFuture(cache.Profile));
				}
			});

		}

		public void getProfileCache(int account_id,Handler<AsyncResult<ClientProfileCache>> callback)
		{
			ClientProfileCache profile = getProfileFromCache(account_id);
		
			if( profile != null)
			{
				callback(Future.succeededFuture(profile));
				return;
			}

			getProfileFromServer(account_id, result => { 
				if( result.failed())
				{
					callback(Future.failedFuture<ClientProfileCache>(result.cause()));
				}
				else
				{
					ClientProfileCache profile = result.result();
					if( _profileMap.ContainsKey(account_id))
					{
						_profileMap.Remove(account_id);
					}
					_profileMap.Add(account_id, profile);

					callback(Future.succeededFuture(profile));
				}
			});
		}

		private ClientProfileCache getProfileFromCache(int account_id)
		{
			ClientProfileCache profile;
			if( _profileMap.TryGetValue( account_id, out profile) == false)
			{
				return null;
			}

			// 다른 먼가 좋은 방법이 있을까?
			if( account_id == _network.getAccountID())
			{
				// 프로필을 수정했네
				if( profile.Profile != ClientMain.instance.getViewModel().Profile.Profile)
				{
					return null;
				}
			}
			else
			{
				TimeSpan diff = DateTime.UtcNow - profile.CachedTime;
				if (diff.TotalHours > 1.0f)
				{
					return null;
				}
			}

			return profile;
		}

        private void getProfileFromServer(int account_id,Handler<AsyncResult<ClientProfileCache>> handler)
		{
			// 후후후 내껀 내가 챙겨
			// 내 프로필이 바뀌면 바로 반영되는것 처럼 보이도록 할려고
			if( _network.getAccountID() == account_id)
			{
				handler(Future.succeededFuture(ClientProfileCache.createMyAccount()));
				return;
			}

			MapPacket req = _network.createReq(CSMessageID.Account.GetProfileReq);
			req.put("id", account_id);

			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture<ClientProfileCache>(new Exception($"getProfile fail")));
				}
				else
				{
					//Debug.Log($"getProfileFromServer:account_id[{account_id}]");

					ClientProfileCache cache = ClientProfileCache.create(ack);

                    req = _network.createReq(CSMessageID.Social.CheckFollowReq);
                    req.put("id", account_id);

                    _network.call(req, ack =>
                    {
                        if (ack.getResult() == ResultCode.ok)
                        {
                            cache.Profile._isFollow = (bool)ack.get("follow");
							cache.Profile._isFollowBack = (bool)ack.get("follow_back");
							cache.Profile._socialScore = (int)ack.get("score");	// 2022.05.03 이강희
                        }
						cache._accountID = account_id;
                        handler(Future.succeededFuture(cache));
                    });
                }
			});
		}
	}
}
