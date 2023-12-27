using Festa.Client.Module;

namespace Festa.Client.RefData
{
	public class RefConfig : RefData
	{
		public int key;
		public object value;
	
		public int getInteger()
		{
			if( value == null || (value is string))
			{
				return 0;
			}

			if( value is int)
			{
				return (int)value;
			}
			else if( value is float)
			{
				return (int)(float)value;
			}
			else if( value is double)
			{
				return (int)(double)value;
			}
			else
			{
				return 0;
			}
		}

		public double getDouble()
		{
			if( value == null)
			{
				return 0;
			}

			if( value is int)
			{
				return (double)(int)value;
			}
			else if( value is long)
			{
				return (double)(long)value;
			}
			else if( value is float)
			{
				return (double)(float)value;
			}
			else if( value is double)
			{
				return (double)value;
			}

			return 0;
		}

		public string getString()
		{
			if (value == null || (value is string) == false)
			{
				return null;
			}

			return (string)value;
		}

		public class Key
		{
			public class Login
            {
				public static int verifyPhone_timeout = EncryptUtil.makeHashCode("login.phonenumberVerification.timeout");
				public static int verifyPhone_resendCode = EncryptUtil.makeHashCode("login.phonenumberVerification.resend");
			}

			public class Settings
            {
                public static int verifyEmail_timeout = EncryptUtil.makeHashCode("setting.emailVerification.timeout");
				public static int verifyEmail_resendCode = EncryptUtil.makeHashCode("setting.emailVerification.resend");
            }

			public class PushSettings
            {
                public static int doNotDisturb_defaultBegin = EncryptUtil.makeHashCode("defalutPushOffStart");
                public static int doNotDisturb_defaultEnd = EncryptUtil.makeHashCode("defalutPushOffFinish");
            }

			public class Walk
			{

			}

			public class FestaCoin
			{

			}

			public class Trip
			{

			}

			public class DailyBoost
			{
				public static int cool_time = EncryptUtil.makeHashCode("boost.cool_time");
				public static int daily_count = EncryptUtil.makeHashCode("boost.daily_count");
			}

			public class Map
			{
				public class Zoom
				{
					public static int init = EncryptUtil.makeHashCode("map.zoom.init");
					public static int trip_start = EncryptUtil.makeHashCode("map.zoom.trip_start");
					public static int hide_address = EncryptUtil.makeHashCode("map.zoom.hide_address");
				}
			}

			public class Chat
            {
                public static int dm_open_price = EncryptUtil.makeHashCode("chat.dm.openPrice");
            }

			public static class Tree
			{
				public static int init_tree_id = EncryptUtil.makeHashCode("tree.init_tree_id");
				public static int duration_unit_time = EncryptUtil.makeHashCode("tree.duration_unit_time");

				public static int status_flower = EncryptUtil.makeHashCode("tree.status.flower");
				public static int status_fruit = EncryptUtil.makeHashCode("tree.status.fruit");
				public static int status_bloom = EncryptUtil.makeHashCode("tree.status.bloom");
			}

			public static class Social
			{
				public static int social_score_season_first_time = EncryptUtil.makeHashCode("social_score.season.first_time");
				public static int social_score_season_period = EncryptUtil.makeHashCode("social_score.season.period");
				public static int social_score_season_reduction_rate = EncryptUtil.makeHashCode("social_score.season.reduction_rate");
			}

			public static class Health
			{
				public static int dailygoal_step_min = EncryptUtil.makeHashCode("healthdata.dailygoal.step.min");
				public static int dailygoal_step_max = EncryptUtil.makeHashCode("healthdata.dailygoal.step.max");
				public static int dailygoal_distance_min = EncryptUtil.makeHashCode("healthdata.dailygoal.distance.min");
				public static int dailygoal_distance_max = EncryptUtil.makeHashCode("healthdata.dailygoal.distance.max");
				public static int dailygoal_calories_min = EncryptUtil.makeHashCode("healthdata.dailygoal.calories.min");
				public static int dailygoal_calories_max = EncryptUtil.makeHashCode("healthdata.dailygoal.calories.max");
			}

			public static class DRun
			{
				public static int heart_ReduceRate = EncryptUtil.makeHashCode("heart_ReduceRate");
				public static int distance_ReduceUnit = EncryptUtil.makeHashCode("distance_ReduceUnit");
				public static int stamina_RecoveryCostRate = EncryptUtil.makeHashCode("stamina_recovery_costrate");
				
				public static int peb_to_drn = EncryptUtil.makeHashCode("peb_to_drn");
				public static int ui_stepcount_duration = EncryptUtil.makeHashCode("ui.home.todaystep.counting.duration");
				public static int running_pro_acquiredDRN = EncryptUtil.makeHashCode("running_pro_acquiredDRN");
				public static int running_pro_minDistanceForMining = EncryptUtil.makeHashCode("running_pro_minDistanceForMining");

				public static int running_firstmove_distance = EncryptUtil.makeHashCode("running.firstmove.distance");

				public static int running_path_color_min_speed = EncryptUtil.makeHashCode("running.path.color.min_speed");
				public static int running_path_color_max_speed = EncryptUtil.makeHashCode("running.path.color.max_speed");
				public static int running_path_color_mid_speed = EncryptUtil.makeHashCode("running.path.color.mid_speed");

				public static int daily_levelup_limit = EncryptUtil.makeHashCode("nft.daily_levelup_limit");

				public static int goal_distance_default = EncryptUtil.makeHashCode("goal_distance_default");
				public static int goal_time_default = EncryptUtil.makeHashCode("goal_time_default");

				public static int marathon_goal_5k = EncryptUtil.makeHashCode("marathon.goal.5k");
				public static int marathon_goal_10k = EncryptUtil.makeHashCode("marathon.goal.10k");
				public static int marathon_goal_20k = EncryptUtil.makeHashCode("marathon.goal.20k");
				public static int marathon_goal_40k = EncryptUtil.makeHashCode("marathon.goal.40k");

				public static int running_max_speed = EncryptUtil.makeHashCode("running_max_speed");
				public static int invitation_send_reward = EncryptUtil.makeHashCode("invitation_send_reward");
				public static int invitation_receive_reward = EncryptUtil.makeHashCode("invitation_receive_reward");

				public static int running_min_step_count = EncryptUtil.makeHashCode("running_min_step_count");
				public static int running_check_step_distance = EncryptUtil.makeHashCode("running_check_step_distance");
				public static int running_check_time = EncryptUtil.makeHashCode("running_check_time");

				public static int[] marathon_goals = new int[]
				{
					marathon_goal_5k,
					marathon_goal_10k,
					marathon_goal_20k,
					marathon_goal_40k,
				};
			}

			public static class SignIn
			{
				public static int email_verification_timeout = EncryptUtil.makeHashCode("siginin.emailVerification.timeout");
				public static int email_verification_resend = EncryptUtil.makeHashCode("siginin.emailVerification.resend");
			}

			public static class Wallet
			{
				public static int withraw_drnt_min = EncryptUtil.makeHashCode("wallet.withdraw.drnt.min");

			}

            public static class UI
            {
                public static readonly int gauge_fill_time = EncryptUtil.makeHashCode("gauge.fill.seconds");
            }
        }
	}
}
