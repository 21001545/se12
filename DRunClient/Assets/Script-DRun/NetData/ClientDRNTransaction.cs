using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.NetData
{
	public class ClientDRNTransaction
	{
		public int transaction_id;
		public int type;
		public int step;
		public string senderAddress;
		public string receiverAddress;
		public string uuid;
		public string api_status;
		public long delta;
		public long prev_balance;
		public long next_balance;
		public DateTime create_time;
		public DateTime update_time;

		public static class Type
		{
			public static int claim_daily_reward = 1;
			public static int claim_weekly_reward = 2;
			public static int pro_mode_running = 3;
			public static int refill_stamina = 4;

			public static int start_withdraw = 5;       // 지갑으로 보내기 위해 drn을 먼저 차감
			public static int wait_withdraw = 6;        // 자식 지갑에 송금 되는걸 기다림
			public static int rollback_withdraw = 7;    // 실패 할 경우 drn을 다시 돌려줌

			public static int wait_gather = 8;          // 대표 지갑에 송금 되는걸 기다림
			public static int complete_gather = 9;      // drn을 실제로 증가 시킴

			public static int nft_boost_exp = 10;   // 경험치 부스트 구매
			public static int nft_level_up = 11;    // nft 레벨업

			public static int wait_claim_invitation_send = 12;  // 초대코드 제공 보상
			public static int invitation_receive = 13; // 초대코드 가입 보상
			public static int invitation_send = 14; // 초대코드 제공 보상 완료
		}

		public static class Step
		{
			public static int create = 1;
			public static int wait_api_transaction = 2;

			public static int done = 100;
		}
	}
}
