using UnityEngine;
using System.Collections;
using Festa.Client.Module;

namespace Festa.Client
{
	public static class MapPacketKey
	{
		public static int msg_id = EncryptUtil.makeHashCode("msg_id");
		public static int result = EncryptUtil.makeHashCode("result");
		public static int source_msg_id = EncryptUtil.makeHashCode("source_msg_id");
		//public static int user_id = EncryptUtil.makeHashCode("user_id");
		//public static int socket_id = EncryptUtil.makeHashCode("socket_id");
		//public static int socket_hash_code = EncryptUtil.makeHashCode("socket_hash_code");
		public static int error_message = EncryptUtil.makeHashCode("error_message");
		public static int is_newaccount = EncryptUtil.makeHashCode("is_newaccount");
		//public static class Login
		//{
		//	public static int username				= EncryptUtil.makeHashCode("login.username");
		//	public static int password				= EncryptUtil.makeHashCode("login.password");
		//	public static int password_enc			= EncryptUtil.makeHashCode("login.password_enc");
		//}

		//// Map
		//public static class Map
		//{
		//	public static int map_id				= EncryptUtil.makeHashCode("map.map_id");
		//	public static int actor_id			= EncryptUtil.makeHashCode("map.actor_id");
		//	public static int actor_type			= EncryptUtil.makeHashCode("map.actor_type");
		//	public static int position			= EncryptUtil.makeHashCode("map.position");
		//	public static int frame = EncryptUtil.makeHashCode("map.frame");

		//	public static int command_ack	= EncryptUtil.makeHashCode("map.cmd_ack");
		//	public static int exist_actors_snap_data_ack = EncryptUtil.makeHashCode("map.exist_actors_snap_data_ack");
		//	public static int new_actors_snap_data_ack = EncryptUtil.makeHashCode("map.new_actors_data_ack");
		//}

		public static class ClientAck
		{
			public static int profile = EncryptUtil.makeHashCode("profile");
			public static int health_log_cumulation = EncryptUtil.makeHashCode("health_log_cumulation");
			//public static int step_reward = EncryptUtil.makeHashCode("step_reward");
			//public static int today_step_count = EncryptUtil.makeHashCode("today_step_count");
			public static int festa_coin = EncryptUtil.makeHashCode("festa_coin");
			public static int festa_coin_delta = EncryptUtil.makeHashCode("festa_coin_delta");
			public static int trip_config = EncryptUtil.makeHashCode("trip_config");
			public static int trip_cheering_config = EncryptUtil.makeHashCode("trip_cheering_config");
			public static int trip_last_path = EncryptUtil.makeHashCode("trip_last_path");
			public static int trip_last_photo = EncryptUtil.makeHashCode("trip_last_photo");
			public static int trip_log = EncryptUtil.makeHashCode("trip_log");
			public static int trip_cheering = EncryptUtil.makeHashCode("trip_cherring");
			public static int trip_photo = EncryptUtil.makeHashCode("trip_photo");
			public static int daily_boost = EncryptUtil.makeHashCode("daily_boost");
			//public static int walk_level = EncryptUtil.makeHashCode("walk_level");
			public static int stat = EncryptUtil.makeHashCode("stat");
			public static int social_tier = EncryptUtil.makeHashCode("social_tier");
			public static int weather = EncryptUtil.makeHashCode("weather");
			public static int follow_cumulation = EncryptUtil.makeHashCode("follow_cumulation");
			public static int follow = EncryptUtil.makeHashCode("follow");
			public static int follow_back = EncryptUtil.makeHashCode("follow_back");
			public static int follow_suggestion = EncryptUtil.makeHashCode("follow_suggestion");
			public static int follow_suggestion_config = EncryptUtil.makeHashCode("follow_suggestion_config");
			public static int feed_config = EncryptUtil.makeHashCode("feed_config");
			public static int chatroom_list = EncryptUtil.makeHashCode("chatroom_list");
			public static int body = EncryptUtil.makeHashCode("body_measurement_info");
			public static int signup = EncryptUtil.makeHashCode("signup");
			public static int bookmark = EncryptUtil.makeHashCode("bookmark");
			public static int tree_config = EncryptUtil.makeHashCode("tree_config");
			public static int tree_list = EncryptUtil.makeHashCode("tree_list");
			public static int tree_harvest_coinamount = EncryptUtil.makeHashCode("tree_harvest_coinamount");
			public static int activity_read_slot_id = EncryptUtil.makeHashCode("activity_read_slot_id");
			public static int friend_config = EncryptUtil.makeHashCode("friend_config");
			public static int register_email = EncryptUtil.makeHashCode("register_email");
			public static int push_config = EncryptUtil.makeHashCode("push_config");
			public static int push_donot_disturb = EncryptUtil.makeHashCode("push_donot_disturb");
			public static int account_setting = EncryptUtil.makeHashCode("account_setting");
			public static int chatroom_config = EncryptUtil.makeHashCode("chatroom_config");
			public static int sticker = EncryptUtil.makeHashCode("sticker");
			public static int sticker_board = EncryptUtil.makeHashCode("sticker_board");
			public static int basic_mode = EncryptUtil.makeHashCode("basic_mode");
			public static int basic_daily_step = EncryptUtil.makeHashCode("basic_daily_step");
			public static int basic_daily_reward_slot = EncryptUtil.makeHashCode("basic_daily_reward_slot");
			public static int basic_weekly_reward = EncryptUtil.makeHashCode("basic_weekly_reward");
			public static int drn_balance = EncryptUtil.makeHashCode("drn_balance");
			public static int drn_transaction = EncryptUtil.makeHashCode("drn_transaction");
			public static int pro_mode = EncryptUtil.makeHashCode("pro_mode");
			public static int running_config = EncryptUtil.makeHashCode("running_config");
			public static int running_log_cumulation = EncryptUtil.makeHashCode("running_log_cumulaton");
			public static int wallet = EncryptUtil.makeHashCode("wallet");
			public static int wallet_balance = EncryptUtil.makeHashCode("wallet_balance");
			public static int nft_item = EncryptUtil.makeHashCode("nft_item");
			public static int nft_metadata_config = EncryptUtil.makeHashCode("nft_metadata_config");
			public static int invitation_code = EncryptUtil.makeHashCode("invitation_code");
			public static int nft_bonus = EncryptUtil.makeHashCode("nft_bonus");
			public static int nft_change_limit = EncryptUtil.makeHashCode("nft_change_limit");
			public static int nft_withdraw_external = EncryptUtil.makeHashCode("nft_withdraw_external");
		}
	}

}
