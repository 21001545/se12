namespace Festa.Client
{
	public static class ClientStateType
	{
		public static int sleep = 0;
		public static int firebase_login = 1;
		public static int server_login = 2;

		public static int startup = 3;
		public static int check_permission_android = 4;
		public static int check_permission_ios = 5;
		public static int init_health_device = 6;
		public static int load_refdata = 7;
		public static int init_location_device = 8;
		public static int select_server = 9;
		//public static int process_walklevel = 10;
		public static int process_health_log = 11;
		public static int end_loading = 12;
		public static int register_push = 13;
		public static int start_local_chatdata = 14;
		public static int firebase_push = 15;
		public static int firebase_app = 16;
        public static int init_account = 17;
		public static int become_active = 18;
		public static int init_mapbox = 19;
		public static int query_chatroom_list = 20;
		public static int pause_trip = 21;

        public static int running = 100;

		// drun
		public static int check_signin = 1001;
		public static int signin = 1002;
		public static int signup = 1003;
		public static int resetpassword = 1004;
		public static int login = 1005;
		public static int run = 1006;
		public static int expire_daily_step_reward = 1007;
		public static int claim_weekly_reward = 1008;
		public static int continue_running = 1009;
		public static int process_nft = 1010;
		public static int read_today_marathon_record = 1011;
		public static int read_remote_config = 1012;
		public static int check_force_update = 1013;
	}
}
