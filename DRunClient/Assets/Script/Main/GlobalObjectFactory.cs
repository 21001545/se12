using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DRun.Client;
using DRun.Client.NetData;
using DRun.Client.RefData;
using DRun.Client.Running;
using DRun.Client.ViewModel;
using Festa.Client.MapBox;
using Festa.Client.Module.MsgPack;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.RefData;

namespace Festa.Client
{
	public class GlobalObjectFactory
	{
		private static ObjectFactory _instance = null;

		public static ObjectFactory getInstance()
		{
			if( _instance != null)
			{
				return _instance;
			}

			_instance = ObjectFactory.create();

			registerGenerics();
			registerObjects();

			return _instance;
		}

		private static void registerObjects()
		{
			_instance.register<MapPacket>();
			_instance.register<BlobData>();

			_instance.register<ClientProfile>();
			_instance.register<ClientHealthLogCumulation>();
			_instance.register<ClientHealthLogData>();
			_instance.register<ClientHealthLogDaily>();
			//_instance.register<ClientStepReward>();
			_instance.register<ClientLocationLog>();
			_instance.register<ClientTripConfig>();
			_instance.register<ClientTripLog>();
			_instance.register<ClientTripPathData>();
			_instance.register<ClientTripCheeringConfig>();
			_instance.register<ClientTripCheering>();
			_instance.register<ClientTripPhoto>();
			_instance.register<ClientTripCheerable>();
			_instance.register<ClientDailyBoost>();
			//_instance.register<ClientWalkLevel>();
			_instance.register<ClientAccountStat>();
			_instance.register<ClientSocialTier>();
			_instance.register<ClientMoment>();
			_instance.register<ClientMomentComment>();
			_instance.register<ClientWeather>();
			_instance.register<ClientFollowCumulation>();
			_instance.register<ClientFollow>();
			_instance.register<ClientFollowBack>();
			_instance.register<ClientFollowSuggestionConfig>();
			_instance.register<ClientFollowSuggestion>();
			_instance.register<ClientFeedConfig>();
			_instance.register<ClientFeed>();
			_instance.register<ClientAccountChatRoom>();
			_instance.register<ClientAccountChatRoomConfig>();
			_instance.register<ClientChatRoomEntrant>();
			_instance.register<ClientChatRoomLog>();
			_instance.register<ClientBody>();
			_instance.register<ClientSignup>();
			_instance.register<ClientBookmark>();
			_instance.register<ClientTree>();
			_instance.register<ClientTreeConfig>();
			_instance.register<ClientMapDeco>();
			_instance.register<ClientMapTileReveal>();
			_instance.register<ClientActivity>();
			_instance.register<ClientNewActivityCount>();
			_instance.register<ClientFriendConfig>();
			_instance.register<ClientRegisterEMail>();
			_instance.register<ClientPushNotificationConfig>();
			_instance.register<ClientDoNotDisturb>();
			_instance.register<ClientAccountSetting>();
			_instance.register<ClientJobProgressData>();
			_instance.register<ClientFriendHealthData>();
			_instance.register<ClientSticker>();
			_instance.register<ClientStickerBoard>();
			_instance.register<ClientSearchByDistance>();

			_instance.register<ClientBasicMode>();
			_instance.register<ClientBasicDailyStep>();
			_instance.register<ClientBasicDailyRewardSlot>();
			_instance.register<ClientBasicWeeklyReward>();
			_instance.register<ClientDRNBalance>();
			_instance.register<ClientDRNTransaction>();
			_instance.register<ClientProMode>();
			_instance.register<ClientRunningConfig>();
			_instance.register<ClientRunningLog>();
			_instance.register<ClientRunningLogCumulation>();
			_instance.register<ClientRankingData>();
			_instance.register<ClientWallet>();
			_instance.register<ClientWalletBalance>();
			_instance.register<ClientNFTItem>();
			_instance.register<ClientNFTMetadataConfig>();
			_instance.register<ClientInvitationCode>();
			_instance.register<ClientInvitationAccept>();
			_instance.register<ClientNFTBonus>();
			_instance.register<ClientNFTChangeLimit>();
			_instance.register<ClientNFTWithdrawExternal>();

			_instance.register<MBTile>();
			_instance.register<MBLayer>();
			_instance.register<MBFeature>();
			_instance.register<LandTile>();
			_instance.register<LandTileFeature>();

			//'
			_instance.register<RefConfig>();
			//_instance.register<RefWalkLevel>();
			_instance.register<RefString>();
			//_instance.register<RefLifeLog>();
			_instance.register<RefCaloriesStep>();
			_instance.register<RefMETs>();
			_instance.register<RefLanguage>();
			_instance.register<RefTree>();
			_instance.register<RefShopItem>();
			_instance.register<RefShopBoard>();
			_instance.register<RefFriendSlot>();
			_instance.register<RefMomentLifeLog>();
			_instance.register<RefLifeStat>();
			_instance.register<RefLifeStatLevel>();
			_instance.register<RefLifeStatGrowthAction>();

			_instance.register<RefSticker>();
			_instance.register<RefEntry>();
			_instance.register<RefEntryBox>();
			_instance.register<RefEntryLevel>();
			_instance.register<RefNFTGrade>();
			_instance.register<RefNFTLevel>();
			_instance.register<RefNFTStaminaEfficiency>();
			_instance.register<RefNFTPluralBonus>();
			//

			//
			_instance.register<LocalRunningStatusData>();
			//_instance.register<RunningStatData>();
			_instance.register<RunningPathData>();
			_instance.register<RunningPathStatData>();
			_instance.register<RunningPathMarathonData>();
		}

		private static void registerGenerics()
		{
			_instance.registerDictionary<int, object>();
			_instance.registerDictionary<int, ClientHealthLogCumulation>();
			_instance.registerDictionary<int, List<ClientHealthLogData>>();
			_instance.registerDictionary<int, int>();
			_instance.registerDictionary<int, List<LandTileFeature>>();

			_instance.registerList<int>();
			_instance.registerList<short[]>();
			_instance.registerList<object>();
			_instance.registerList<string>();
			_instance.registerList<double>();
			_instance.registerList<List<int>>();

			_instance.registerList<MBLayer>();
			_instance.registerList<MBFeature>();
			_instance.registerList<MBLayerRenderData>();
			_instance.registerList<MBAnchors>();
			_instance.registerList<LandTileFeature>();

			_instance.registerList<ClientHealthLogData>();
			//_instance.registerList<ClientWalkLevel>();
			_instance.registerList<ClientTripPathData>();
			_instance.registerList<ClientBookmark>();
			_instance.registerList<ClientTree>();
			_instance.registerList<ClientMapDeco>();
            _instance.registerList<ClientMomentComment>();
			_instance.registerList<ClientPushNotificationConfig>();
			_instance.registerList<ClientAccountSetting>();
			_instance.registerList<ClientSticker>();
			_instance.registerList<ClientBasicDailyStep>();
			_instance.registerList<ClientBasicDailyRewardSlot>();
			_instance.registerList<ClientBasicWeeklyReward>();
			_instance.registerList<ClientDRNTransaction>();
        }
	}
}
