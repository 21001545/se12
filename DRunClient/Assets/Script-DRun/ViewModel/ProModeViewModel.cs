using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Net;

namespace DRun.Client.ViewModel
{
	public class ProModeViewModel : AbstractViewModel
	{
		private ClientProMode _proMode;
		private ClientNFTItem _equipedNFTItem;
		private ClientNFTBonus _nftBonus;
		private ClientNFTChangeLimit _nftChangeLimit;

		public ClientProMode Data
		{
			get
			{
				return _proMode;
			}
			set
			{
				Set(ref _proMode, value);
			}
		}

		public ClientNFTItem EquipedNFTItem
		{
			get
			{
				return _equipedNFTItem;
			}
			set
			{
				Set(ref _equipedNFTItem, value);
			}
		}

		public ClientNFTBonus NFTBonus
		{
			get
			{
				return _nftBonus;
			}
			set
			{
				Set(ref _nftBonus, value);
			}
		}

		public ClientNFTChangeLimit NFTChangeLimit
		{
			get
			{
				return _nftChangeLimit;
			}
			set
			{
				Set(ref _nftChangeLimit, value);
			}
		}

		public float ExpRatio
		{
			get
			{
				if( _equipedNFTItem == null)
				{
					return 0;
				}
				return _equipedNFTItem.getEXPRatio();
			}
		}

		public float DistanceRatio
		{
			get
			{
				if( _equipedNFTItem == null)
				{
					return 0;
				}
				return _equipedNFTItem.getDistanceRatio();
			}
		}

		public float StaminaRatio
		{
			get
			{
				if (_equipedNFTItem == null)
				{
					return 0;
				}
				return _equipedNFTItem.getStaminaRatio();
			}
		}

		public float HeartRatio
		{
			get
			{
				if(_equipedNFTItem == null)
				{
					return 0;
				}

				return _equipedNFTItem.getHeartRatio();
			}
		}

		public static ProModeViewModel create()
		{
			ProModeViewModel vm = new ProModeViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();
		}

		protected override void bindPacketParser()
		{
			bind(MapPacketKey.ClientAck.pro_mode, updateProMode);
			bind(MapPacketKey.ClientAck.nft_bonus, updateNFTBonus);
			bind(MapPacketKey.ClientAck.nft_change_limit, updateNFTChangeLimit);
		}

		private void updateProMode(object obj,MapPacket ack)
		{
			Data = (ClientProMode)obj;
		}

		private void updateNFTBonus(object obj,MapPacket ack)
		{
			NFTBonus = (ClientNFTBonus)obj;
		}

		private void updateNFTChangeLimit(object obj,MapPacket ack)
		{
			NFTChangeLimit= (ClientNFTChangeLimit)obj;
		}
	}
}
