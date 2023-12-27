using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class StatureViewModel : AbstractViewModel
	{
		private ClientSocialTier _socialTier;
		private Dictionary<int, ClientAccountStat> _statMap;

		public Dictionary<int, ClientAccountStat> StatMap => _statMap;

		public ClientSocialTier SocialTier
		{
			get
			{
				return _socialTier;
			}
			set
			{
				Set(ref _socialTier, value);
				notifyPropetyChanged("Star");
			}
		}

		public int Star => _socialTier.star;

		public static StatureViewModel create()
		{
			StatureViewModel vm = new StatureViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_statMap = new Dictionary<int, ClientAccountStat>();
		}

		public ClientAccountStat getStat(int type)
		{
			ClientAccountStat stat;
			if(_statMap.TryGetValue(type, out stat))
			{
				return stat;
			}
			return null;
		}

		private void updateStats(List<ClientAccountStat> list)
		{
			foreach(ClientAccountStat stat in list)
			{
				if( _statMap.ContainsKey( stat.type))
				{
					_statMap.Remove(stat.type);
				}

				_statMap.Add(stat.type, stat);

				//Debug.Log($"LifeStat: id[{stat.type}] level[{stat.level}] exp[{stat.exp}]");
			}

			notifyPropetyChanged("StatMap");
		}

		public override void updateFromAck(MapPacket ack)
		{
			if (ack.contains(MapPacketKey.ClientAck.stat))
			{
				updateStats(ack.getList<ClientAccountStat>(MapPacketKey.ClientAck.stat));
			}

			if (ack.contains(MapPacketKey.ClientAck.social_tier))
			{
				SocialTier = (ClientSocialTier)ack.get(MapPacketKey.ClientAck.social_tier);
			}
		}
	}
}
