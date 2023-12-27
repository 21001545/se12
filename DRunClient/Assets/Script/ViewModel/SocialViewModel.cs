using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class SocialViewModel : AbstractViewModel
	{
		private ClientFollowCumulation _followCumulation;
		private ClientFollowSuggestionConfig _suggestionConfig;
		private List<ClientFollowSuggestion> _suggestionList;

		private ClientFriendConfig _friendConfig;

		private List<ClientFollow> _followList;
		private List<ClientFollowBack> _followBackList;

		public const int follow_page_count = 10;
		public const int follow_back_page_count = 10;

		public ClientFollowCumulation FollowCumulation
		{
			get
			{
				return _followCumulation;
			}
			set
			{
				Set(ref _followCumulation, value);
			}
		}

		public ClientFollowSuggestionConfig SuggestionConfig
		{
			get
			{
				return _suggestionConfig;
			}
			set
			{
				Set(ref _suggestionConfig, value);
			}
		}

		public List<ClientFollowSuggestion> SuggestionList
		{
			get
			{
				return _suggestionList;
			}
			set
			{
				Set(ref _suggestionList, value);
			}
		}

		public List<ClientFollow> FollowList
		{
			get
			{
				return _followList;
			}
			set
			{
				Set(ref _followList, value);
			}
		}

		public List<ClientFollowBack> FollowBackList
		{
			get
			{
				return _followBackList;
			}
			set
			{
				Set(ref _followBackList, value);
			}
		}

		public ClientFriendConfig FriendConfig
		{
			get
			{
				return _friendConfig;
			}
			set
			{
				Set(ref _friendConfig, value);
			}
		}

		public static SocialViewModel create()
		{
			SocialViewModel vm = new SocialViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();
			_suggestionList = new List<ClientFollowSuggestion>();
			_followList = new List<ClientFollow>();
			_followBackList = new List<ClientFollowBack>();
		}

		public Vector2Int getFollowNextPage()
		{
			Vector2Int range = new Vector2Int();
			range.x = _followList.Count;
			range.y = follow_page_count;
			return range;
		}

		public Vector2Int getFollowBackNextPage()
		{
			Vector2Int range = new Vector2Int();
			range.x = _followBackList.Count;
			range.y = follow_back_page_count;
			return range;
		}

		public void appendFollowBackList(List<ClientFollowBack> list)
		{
			_followBackList.AddRange(list);
			notifyPropetyChanged(nameof(FollowBackList));
		}

		public void appendFollowList(List<ClientFollow> list)
		{
			_followList.AddRange(list);
			notifyPropetyChanged(nameof(FollowList));
		}
	
		public override void updateFromAck(MapPacket ack)
		{
			if( ack.contains(MapPacketKey.ClientAck.follow_cumulation))
			{
				FollowCumulation = (ClientFollowCumulation)ack.get(MapPacketKey.ClientAck.follow_cumulation);
			}

			if( ack.contains(MapPacketKey.ClientAck.follow_suggestion_config))
			{
				SuggestionConfig = (ClientFollowSuggestionConfig)ack.get(MapPacketKey.ClientAck.follow_suggestion_config);
			}

			if(ack.contains(MapPacketKey.ClientAck.follow_suggestion))
			{
				SuggestionList = ack.getList<ClientFollowSuggestion>(MapPacketKey.ClientAck.follow_suggestion);
			}

			if( ack.contains(MapPacketKey.ClientAck.friend_config))
			{
				FriendConfig = (ClientFriendConfig)ack.get(MapPacketKey.ClientAck.friend_config);
			}
		}

		public int getMaxFriendCount()
		{
			RefFriendSlot refSlot = GlobalRefDataContainer.getInstance().get<RefFriendSlot>(_friendConfig.slot_level);
			if( refSlot == null)
			{
				return 0;
			}
			return refSlot.max_friends;
		}
	}
}
