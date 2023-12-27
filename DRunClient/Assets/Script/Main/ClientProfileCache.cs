using Festa.Client.Module.MsgPack;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;

namespace Festa.Client
{
	public class ClientProfileCache
    {
        [SerializeOption(SerializeOption.NONE)]
        public int _accountID;

        private ClientProfile _profile;
		private WalletViewModel _wallet;
		private SocialViewModel _social;
		private MomentViewModel _moment;
		private BookmarkViewModel _bookmark;
		private StickerViewModel _sticker;
		private StatureViewModel _stature;

		private DateTime _cachedTime;

		public ClientProfile Profile => _profile;
		public WalletViewModel Wallet => _wallet;
		public SocialViewModel Social => _social;
		public MomentViewModel Moment => _moment;
		public BookmarkViewModel Bookmark => _bookmark;
		public StickerViewModel Sticker => _sticker;
		public StatureViewModel Stature => _stature;
		public DateTime CachedTime => _cachedTime;

		public static ClientProfileCache create(MapPacket ack)
		{
			ClientProfileCache cache = new ClientProfileCache();
			cache.init(ack);
			return cache;
		}

		public static ClientProfileCache createMyAccount()
		{
			ClientProfileCache cache = new ClientProfileCache();
			cache.initMyAccount();
			return cache;
		}

		private void initMyAccount()
		{
			ClientViewModel vm = ClientMain.instance.getViewModel();

			_accountID = ClientMain.instance.getNetwork().getAccountID();
			_wallet = vm.Wallet;
			_social = vm.Social;
			_moment = vm.Moment;
			_bookmark = vm.Bookmark;
			_profile = vm.Profile.Profile;
			_sticker = vm.Sticker;
			_stature = vm.Stature;

			_cachedTime = DateTime.Now;
		}

		private void init(MapPacket ack)
		{
			_wallet = WalletViewModel.create();
			_social = SocialViewModel.create();
			_moment = MomentViewModel.create();
			_bookmark = BookmarkViewModel.create();
			_sticker = StickerViewModel.create();
			_stature = StatureViewModel.create();

			_cachedTime = DateTime.UtcNow;

			_profile = (ClientProfile)ack.get(MapPacketKey.ClientAck.profile);
			_wallet.FestaCoin = (int)ack.get(MapPacketKey.ClientAck.festa_coin);
			_social.FollowCumulation = (ClientFollowCumulation)ack.get(MapPacketKey.ClientAck.follow_cumulation);
			_social.FriendConfig = (ClientFriendConfig)ack.get(MapPacketKey.ClientAck.friend_config);
			_sticker.StickerBoard = (ClientStickerBoard)ack.get(MapPacketKey.ClientAck.sticker_board);
			_stature.updateFromAck(ack);
		}

	}
}
