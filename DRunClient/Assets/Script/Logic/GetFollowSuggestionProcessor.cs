using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Logic
{
	public class GetFollowSuggestionProcessor
	{
		private ClientNetwork _network;
		private ClientProfileCacheManager _profileCacheManager;
		private ClientViewModel _viewModel;

		public static GetFollowSuggestionProcessor create()
		{
			GetFollowSuggestionProcessor p = new GetFollowSuggestionProcessor();
			p.init();
			return p;
		}

		private void init()
		{
			_network = ClientMain.instance.getNetwork();
			_viewModel = ClientMain.instance.getViewModel();
			_profileCacheManager = ClientMain.instance.getProfileCache();
		}

		public void run(Action callback)
		{
			MapPacket req = _network.createReq(CSMessageID.Social.SuggestionReq);
			_network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					callback();
				}
				else
				{
					_viewModel.updateFromPacket(ack);
					prepareProfiles(callback);
				}
			});
		}

		private void prepareProfiles(Action callback)
		{
			List<ClientFollowSuggestion> list = _viewModel.Social.SuggestionList;

			prepareProfileIter(list, 0, callback);
		}

		private void prepareProfileIter(List<ClientFollowSuggestion> list,int id,Action callback)
		{
			if( id >= list.Count)
			{
				callback();
				return;
			}

			ClientFollowSuggestion suggestion = list[id];
			_profileCacheManager.getProfileCache( suggestion.suggestion_id, get_profile_result => { 
				if( get_profile_result.succeeded())
				{
					ClientProfileCache profile = get_profile_result.result();
					suggestion._profile = profile;
				}

				prepareProfileIter(list, id + 1, callback);
			});
		}
	}
}
