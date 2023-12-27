using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client
{
    public static class CSMessageID
    {
		public static int ReqError = 10000;

		public static class Account
		{
			public static int LoginReq = 100;
			public static int LoginAck = 101;
			public static int BecomeActiveReq = 102;
			public static int BecomeActiveAck = 103;
			public static int ResignActiveReq = 104;
			public static int ResignActiveAck = 105;
			public static int RegisterPushTokenReq = 106;
			public static int RegisterPushTokenAck = 107;
			public static int ModifyProfileReq = 108;
			public static int ModifyProfileAck = 109;
			public static int GetProfileReq = 110;
			public static int GetProfileAck = 111;
			public static int GetCountryByIPReq = 112;
			public static int GetCountryByIPAck = 113;
			public static int ChangeBodyReq = 114;
			public static int ChangeBodyAck = 115;
			public static int CheckNameReq = 116;
			public static int CheckNameAck = 117;
			public static int WithdrawReq = 118;
			public static int WithdrawAck = 119;
			public static int UpdateSignupStepReq = 120;
			public static int UpdateSignupStepAck = 121;
			public static int SearchByNameReq = 122;
			public static int SearchByNameAck = 123;
			public static int SearchByPhoneNumberReq = 124;
			public static int SearchByPhoneNumberAck = 125;
			public static int CheckSessionReq = 126;
			public static int CheckSessionAck = 127;
			public static int RegisterEMailReq = 128;
			public static int RegisterEMailAck = 129;
			public static int ConfirmEMailReq = 130;
			public static int ConfirmEMailAck = 131;
			public static int ChangeDoNotDisturbReq = 132;
			public static int ChangeDoNotDisturbAck = 133;
			public static int ChangePushConfigReq = 134;
			public static int ChangePushConfigAck = 135;
			public static int ChangeAccountSettingReq = 136;
			public static int ChangeAccountSettingAck = 137;
			public static int GetJobProgressReq = 138;
			public static int GetJobProgressAck = 139;
			public static int SaveStickerBoardReq = 140;
			public static int SaveStickerBoardAck = 141;
			public static int GetStickerBoardReq = 142;
			public static int GetStickerBoardAck = 143;
			public static int ChangeNameReq = 144;
			public static int ChangeNameAck = 145;
			public static int WriteLogReq = 146;
			public static int WriteLogAck = 147;
			public static int AcceptInvitationCodeReq = 148;
			public static int AcceptInvitationCodeAck = 149;
			public static int QueryInvitationAcceptorListReq = 150;
			public static int QueryInvitationAcceptorListAck = 150;
		}

		public static class HealthData
		{
			public static int RecordHealthDataReq = 200;
			public static int RecordHealthDataAck = 201;
			public static int GetDailyTotalRankReq = 202;
			public static int GetDailyTotalRankAck = 203;
			public static int GetDailyFriendRankReq = 204;
			public static int GetDailyFriendRankAck = 205;
			public static int ChangeDailyGoalReq = 206;
			public static int ChangeDailyGoalAck = 207;
			public static int QueryDailyLogReq = 208;
			public static int QueryDailyLogAck = 209;
			public static int QueryStatisticsReq = 210;
			public static int QueryStatisticsAck = 211;
			public static int QueryStatMonthlyLogReq = 212;
			public static int QueryStatMonthlyLogAck = 213;
		}

		public static class Map
		{
			public static int GetVectorTile = 300;
			public static int GetLandTile = 301;

			public static int GetTileRevealReq = 302;
			public static int GetTileRevealAck = 303;
			public static int UpdateTileRevealReq = 304;
			public static int UpdateTileRevealAck = 305;

			public static int SearchByDistanceReq = 306;
			public static int SearchByDistanceAck = 307;
		}

		public static class Location
		{
			public static int RecordLocationLogReq = 400;
			public static int RecordLocationLogAck = 401;
		}

		public static class Trip
		{
			public static int BeginTripReq = 500;
			public static int BeginTripAck = 501;
			public static int EndTripReq = 502;
			public static int EndTripAck = 503;
			public static int CommitTripReq = 504;
			public static int CommitTripAck = 505;
			public static int QueryTripListReq = 506;
			public static int QueryTripListAck = 507;
			public static int ChangeTripTypeReq = 508;
			public static int ChangeTripTypeAck = 509;
			public static int PauseTripReq = 510;
			public static int PauseTripACk = 511;
			public static int ResumeTripReq = 512;
			public static int ResumeTripAck = 513;
			public static int GetTripConfigReq = 514;
			public static int GetTripConfigAck = 515;
			public static int CheckCheerableReq = 516;
			public static int CheckCheerableAck = 517;
			public static int CheerTripReq = 518;
			public static int CheerTripAck = 519;
			public static int AddPhotoReq = 520;
			public static int AddPhotoAck = 521;
			public static int RemovePhotoReq = 522;
			public static int RemovePhotoAck = 523;
			public static int UploadTripPhotoToStorageReq = 524;
			public static int UploadTripPhotoToStorageAck = 525;
			public static int GetTripCheeringListReq = 526;
			public static int GetTripCheeringListAck = 527;
			public static int GetTripPhotoListReq = 528;
			public static int GetTripPhotoListAck = 529;
			public static int ChangeTripNameReq = 530;
			public static int ChangeTripNameAck = 531;
		}

		public static class Moment
		{
			public static int MakeReq = 600;
			public static int MakeAck = 601;
			public static int QueryListReq = 602;
			public static int QueryListAck = 603;
			//public static int QueryFeedReq = 604;
			//public static int QueryFeedAck = 605;
			public static int AddCommentReq = 606;
			public static int AddCommentAck = 607;
			public static int DeleteCommandReq = 608;
			public static int DeleteCommandAck = 609;
			public static int LikeReq = 610;
			public static int LikeAck = 611;
			public static int UnlikeReq = 612;
			public static int UnLikeAck = 613;
			public static int CommentLikeReq = 614;
			public static int CommentLikeAck = 615;
			public static int CommentUnlikeReq = 616;
			public static int CommentUnlikeAck = 617;
			public static int QueryCommentReq = 618;
			public static int QueryCommentAck = 619;
			public static int CheckLikeReq = 620;
			public static int CheckLikeAck = 621;
			public static int QueryRemoteListReq = 622;
			public static int QueryRemoteListAck = 623;
			public static int ModifyCommentReq = 624;
			public static int ModifyCommentAck = 625;
			public static int CheckCommentLikeReq = 626;
			public static int CheckCommentLikeAck = 627;
			public static int QueryListByIDReq = 628;
			public static int QueryListByIDAck = 629;
			public static int UploadPhotoToStorageReq = 630;
			public static int UploadPhotoToStorageAck = 631;
			public static int ModifyMomentReq = 632;
			public static int ModifyMomentAck = 633;
			public static int DeleteMomentReq = 634;
			public static int DeleteMomentAck = 635;
		}

		public static class Weather
		{
			public static int QueryWeatherReq = 700;
			public static int QueryWeatherAck = 701;
		}

		public static class Social
		{
			public static int FollowReq = 800;
			public static int FollowAck = 801;
			public static int UnfollowReq = 802;
			public static int UnfollowAck = 803;
			public static int SuggestionReq = 804;
			public static int SuggestionAck = 805;
			public static int QueryFollowReq = 806;
			public static int QureyFollowAck = 807;
			public static int QueryFollowBackReq = 808;
			public static int QueryFollowBackAck = 809;
			public static int CheckFollowBackReq = 810;
			public static int CheckFollowBackAck = 811;
			public static int CheckFollowReq = 812;
			public static int CheckFollowAck = 813;
			public static int QueryActivityReq = 814;
			public static int QueryActivityAck = 815;
			public static int GetNewActivityCountReq = 816;
			public static int GetNewActivityCountAck = 817;
			public static int ClaimActivityRewardReq = 818;
			public static int ClaimActivityRewardAck = 819;
			public static int SaveActivityReadSlotIDReq = 820;
			public static int SaveActivityReadSlotIDAck = 821;
			public static int QueryFollowInTripReq = 822;
			public static int QueryFollowInTripAck = 823;
		}

		public static class Feed
		{
			public static int QueryReq = 900;
			public static int QueryAck = 901;
			public static int AddBookmarkReq = 902;
			public static int AddBookmarkAck = 903;
			public static int RemoveBookmarkReq = 904;
			public static int RemoveBookmarkAck = 905;
			public static int QueryBookmarkReq = 906;
			public static int QueryBookmarkAck = 907;
			public static int CheckBookmarkReq = 908;
			public static int CheckBookmarkAck = 909;
		}

		public static class Chat
		{
			public static int OpenDMChatRoomReq = 1000;
			public static int OpenDMChatRoomAck = 1001;
			public static int SendMessageReq = 1002;
			public static int SendMessageAck = 1003;
			public static int QueryAccountChatRoomReq = 1004;
			public static int QueryAccountChatRoomAck = 1005;
			public static int GetChatRoomLatestLogIDReq = 1006;
			public static int GetChatRoomLatestLogIDAck = 1007;
			public static int ChangeChatRoomPushConfigReq = 1008;
			public static int ChangeChatRoomPushConfigAck = 1009;
			public static int QueryChatRoomLogReq = 1010;
			public static int QueryChatRoomLogAck = 1011;
			public static int GetChatRoomConfigReq = 1012;
			public static int GetChatRoomConfigAck = 1013;
			public static int LeaveChatRoomReq = 1014;
			public static int LeaveChatRoomAck = 1015;
			public static int SendFileReq = 1016;
			public static int SendFileAck = 1017;
		}

		public static class Tree
		{
			public static int ClaimTreeRewardReq = 1100;
			public static int ClaimTreeRewardAck = 1101;
			public static int ChangeTreeReq = 1102;
			public static int ChangeTreeAck = 1103;
		}

		public static class Shop
		{
			public static int PurchaseShopItemReq = 1200;
			public static int PurchaseShopItemACk = 1201;
		}

		public static class Auth
		{
			public static int SendEMailVerifyCodeReq = 1300;
			public static int SendEMailVerifyCodeAck = 1301;
			public static int VerifyEMailReq = 1302;
			public static int VerifyEMailAck = 1303;
			public static int SignInReq = 1304;
			public static int SignInAck = 1305;
			public static int SignUpReq = 1306;
			public static int SignUpAck = 1307;
			public static int ResetPasswordReq = 1308;
			public static int ResetPasswordAck = 1309;
			public static int CheckSignUpReq = 1310;
			public static int CheckSignUpAck = 1311;
		}

		public static class BasicMode
		{
			public static int ExpireDailyRewardReq = 1400;
			public static int ExpireDailyRewardAck = 1401;
			public static int ClaimDailyRewardReq = 1402;
			public static int ClaimDailyRewardAck = 1403;
			public static int LevelUpReq = 1404;
			public static int LevelUpAck = 1405;
			public static int ClaimWeeklyRewardReq = 1406;
			public static int ClaimWeeklyRewardAck = 1407;
		}

		public static class ProMode
		{
			public static int RefillStatReq = 1500;
			public static int RefillStatAck = 1501;
			public static int WriteRunningLogReq = 1502;
			public static int WriteRunningLogAck = 1503;
			public static int QueryRunningLogReq = 1504;
			public static int QueryRunningLogAck = 1505;
			public static int RefillStaminaReq = 1506;
			public static int RefillStaminaAck = 1507; 
			public static int LevelUpReq = 1508;
			public static int LevelUpAck = 1509;
			public static int QueryRunningLogCumulationReq = 1510;
			public static int QueryRunningLogCumulationAck = 1511;
			public static int EquipNFTItemReq = 1512;
			public static int EquipNFTItemAck = 1513;
			public static int BoostExpReq = 1514;
			public static int BoostExpPAck = 1515;
			public static int CheckLevelUpLimitReq = 1516;
			public static int CheckLevelUpLimitAck = 1517;
			public static int UpdateNFTBonusReq = 1518;
			public static int UpdateNFTBonusAck = 1519;
			public static int RefillNFTBonusReq = 1520;
			public static int RefillNFTBonusAck = 1521;
			public static int UnequipNFTItemReq = 1522;
			public static int UnequipNFTItemAck = 1533;
		}

		public static class Ranking
		{
			public static int ReadRankingReq = 1600;
			public static int ReadRankingAck = 1601;
			public static int ReadRankingSingleReq = 1602;
			public static int ReadRankingSingleAck = 1603;
		}

		public static class Wallet
		{
			public static int CreateWalletReq = 1700;
			public static int CreateWalletAck = 1701;
			public static int GetBalanceReq = 1702;
			public static int GetBalanceAck = 1703;
			public static int CheckPinHashReq = 1704;
			public static int CheckPinHashAck = 1705;
			public static int GetFeeReq = 1706;
			public static int GetFeeAck = 1707;
			public static int ProcessIncompleteTransactionReq = 1708;
			public static int ProcessIncompleteTransactionAck = 1709;
			public static int QueryIncompleteTransactionReq = 1710;
			public static int QueryIncompleteTransactionAck = 1711;
			public static int QueryTransactionReq = 1712;
			public static int QueryTransactionAck = 1713;
			public static int SpendingToWalletReq = 1714;
			public static int SpendingToWalletAck = 1715;
			public static int WalletToSpendingReq = 1716;
			public static int WalletToSpendingAck = 1717;
			public static int WalletToExternalReq = 1718;
			public static int WalletToExternalAck = 1719;
			public static int QueryNFTReq = 1720;
			public static int QueryNFTAck = 1721;
			public static int NFTItemToExternalReq = 1722;
			public static int NFTItemToExternalAck = 1723;
		}

	}

}

