using DRun.Client.ViewModel;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client
{
	public class ClientViewModel
	{
		private ProfileViewModel _profile;
		private HealthViewModel _health;
		private LocationViewModel _location;
		//private StepRewardViewModel _stepReward;
		private WalletViewModel _wallet;
		private TripViewModel _trip;
		private StatureViewModel _stature;
		private MakeMomentViewModel _makeMoment;
		private FeedViewModel _feed;
		private MomentViewModel _moment;
		private WeatherViewModel _weather;
		private SocialViewModel _social;
		private ChatViewModel _chat;
		private BookmarkViewModel _bookmark;
		private DailyBoostViewModel _dailyBoost;
		private TreeViewModel _tree;
		private MapRevealViewModel _mapReveal;
		private ActivityViewModel _activity;
		private JobProgressViewModel _jobProgress;
		private StickerViewModel _sticker;

		// drun
		private SignUpViewModel _signup;
		private SignInViewModel _signin;
		private PlayModeViewModel _playMode;
		private BasicModeViewModel _basicMode;
		private RunningViewModel _running;
		private ProModeViewModel _proMode;
		private RecordViewModel _record;
		private RankingViewModel _ranking;
		private WalletWithdrawViewModel _withdraw;
		private EventViewModel _event;

		public ProfileViewModel Profile => _profile;
		public HealthViewModel Health => _health;
		public LocationViewModel Location => _location;
		//public StepRewardViewModel StepReward => _stepReward;
		public WalletViewModel Wallet => _wallet;
		public TripViewModel Trip => _trip;
		public StatureViewModel Stature => _stature;
		public MakeMomentViewModel MakeMoment => _makeMoment;
		public FeedViewModel Feed => _feed;
		public MomentViewModel Moment => _moment;
		public WeatherViewModel Weather => _weather;
		public SocialViewModel Social => _social;
		public ChatViewModel Chat => _chat;
		public BookmarkViewModel Bookmark => _bookmark;
		public DailyBoostViewModel DailyBoost => _dailyBoost;
		public TreeViewModel Tree => _tree;
		public MapRevealViewModel MapReveal => _mapReveal;
		public ActivityViewModel Activity => _activity;
		public JobProgressViewModel JobProgress => _jobProgress;
		public StickerViewModel Sticker => _sticker;
		public SignUpViewModel SignUp => _signup;
		public SignInViewModel SignIn => _signin;
		public BasicModeViewModel BasicMode => _basicMode;
		public PlayModeViewModel PlayMode => _playMode;
		public RunningViewModel Running => _running;
		public ProModeViewModel ProMode => _proMode;
		public RecordViewModel Record => _record;
		public RankingViewModel Ranking => _ranking;
		public WalletWithdrawViewModel Withdraw => _withdraw;
		public EventViewModel Event => _event;

		private List<AbstractViewModel> _vmList;

		public static ClientViewModel create()
		{
			ClientViewModel vm = new ClientViewModel();
			vm.init();
			return vm;
		}

		private void init()
		{
			_vmList = new List<AbstractViewModel>();

			_profile = addViewModel(ProfileViewModel.create());
			_health = addViewModel(HealthViewModel.create());
			_location = addViewModel(LocationViewModel.create());
			_wallet = addViewModel(WalletViewModel.create());
			_trip = addViewModel(TripViewModel.create());
			_stature = addViewModel(StatureViewModel.create());
			_makeMoment = addViewModel(MakeMomentViewModel.create());
			_feed = addViewModel(FeedViewModel.create());
			_moment = addViewModel(MomentViewModel.create());
			_weather = addViewModel(WeatherViewModel.create());
			_social = addViewModel(SocialViewModel.create());
			_chat = addViewModel(ChatViewModel.create());
			_bookmark = addViewModel(BookmarkViewModel.create());
			_dailyBoost = addViewModel(DailyBoostViewModel.create());
			_tree = addViewModel(TreeViewModel.create());
			_mapReveal = addViewModel(MapRevealViewModel.create());
			_activity = addViewModel(ActivityViewModel.create());
			_jobProgress = addViewModel(JobProgressViewModel.create());
			_sticker = addViewModel(StickerViewModel.create());

			//drun
			_signup = addViewModel(SignUpViewModel.create());
			_signin = addViewModel(SignInViewModel.create());
			_basicMode = addViewModel(BasicModeViewModel.create());
			_playMode = addViewModel(PlayModeViewModel.create());
			_running = addViewModel(RunningViewModel.create());
			_proMode = addViewModel(ProModeViewModel.create());
			_record = addViewModel(RecordViewModel.create());
			_ranking = addViewModel(RankingViewModel.create());
			_withdraw = addViewModel(WalletWithdrawViewModel.create());
			_event = addViewModel(EventViewModel.create());
		}

		private T addViewModel<T>(T vm) where T : AbstractViewModel
		{
			_vmList.Add(vm);
			return vm;
		}

		private void reset()
		{
			foreach(AbstractViewModel vm in _vmList)
			{
				vm.reset();
			}
		}

		public void setupFromLogin(int account_id,MapPacket ack)
		{
			reset();

			_profile.updateFromAck(ack);

			Debug.Log($"singup step:{_profile.Signup.step}");

			//

			Dictionary<int, ClientHealthLogCumulation> dic_health_cumulation = ack.getDictionary<int, ClientHealthLogCumulation>(MapPacketKey.ClientAck.health_log_cumulation);
			_health.updateCumulation(dic_health_cumulation);

			_dailyBoost.DailyBoost = (ClientDailyBoost)ack.get(MapPacketKey.ClientAck.daily_boost);

			//
			_wallet.updateFromAck(ack);

			//
			_trip.updateFromAck(ack);
			_health.Body = (ClientBody)ack.get(MapPacketKey.ClientAck.body);

			_stature.updateFromAck(ack);

			_feed.FeedConfig = (ClientFeedConfig)ack.get(MapPacketKey.ClientAck.feed_config);

			_social.updateFromAck(ack);

			_tree.updateFromAck(ack);
			_activity.updateFromAck(ack);
			_chat.updateFromAck(ack);
			_sticker.updateFromAck(ack);
			_basicMode.updateFromAck(ack);
			_playMode.updateFromAck(ack);
			_running.updateFromAck(ack);
			_proMode.updateFromAck(ack);
			_record.updateFromAck(ack);
			_event.updateFromAck(ack);

			updateEquipedNFTItem();

			// 버그 수정
			_wallet.clearAccumulatedDRNChange();
		}

		public void updateFromPacket(MapPacket ack)
		{
			_profile.updateFromAck(ack);
			_wallet.updateFromAck(ack);
			_trip.updateFromAck(ack);

			if( ack.contains(MapPacketKey.ClientAck.daily_boost))
			{
				_dailyBoost.DailyBoost = (ClientDailyBoost)ack.get(MapPacketKey.ClientAck.daily_boost);
			}

			if (ack.contains(MapPacketKey.ClientAck.health_log_cumulation))
			{
				_health.updateCumulation(ack.getDictionary<int, ClientHealthLogCumulation>(MapPacketKey.ClientAck.health_log_cumulation));
			}

			if( ack.contains(MapPacketKey.ClientAck.weather))
			{
				_weather.Data = (ClientWeather)ack.get(MapPacketKey.ClientAck.weather);
			}

			if( ack.contains(MapPacketKey.ClientAck.feed_config))
			{
				_feed.FeedConfig = (ClientFeedConfig)ack.get(MapPacketKey.ClientAck.feed_config);
			}

			if( ack.contains(MapPacketKey.ClientAck.body))
			{
				_health.Body = (ClientBody)ack.get(MapPacketKey.ClientAck.body);
			}

			_stature.updateFromAck(ack);
			_social.updateFromAck(ack);
			_tree.updateFromAck(ack);
			_activity.updateFromAck(ack);
			_chat.updateFromAck(ack);
			_sticker.updateFromAck(ack);
			_basicMode.updateFromAck(ack);
			_playMode.updateFromAck(ack);
			_running.updateFromAck(ack);
			_proMode.updateFromAck(ack);
			_record.updateFromAck(ack);
			_event.updateFromAck(ack);

			updateEquipedNFTItem();
		}
		
		private void updateEquipedNFTItem()
		{
			if( _proMode.Data.nft_token_id != 0)
			{
				_proMode.EquipedNFTItem = _wallet.getNFTItem(_proMode.Data.nft_token_id);
			}
			else
			{
				_proMode.EquipedNFTItem = null;
			}
		}
	}
}
