using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientActivity
	{
		public int slot_id;
		public DateTime event_time;
		public int event_type;
		public int agent_account_id;
		public int param1;
		public int param2;
		public string string_param;	// null일수 있다
		public int reward_type;
		public int reward_refid;
		public int reward_amount;
		public int claim_status;

		public static class Type
		{
			public const int moment_like = 1;
			public const int moment_comment = 2;

			public const int reward_moment_like = 100;
		}

		public static class ClaimStatus
		{
			public const int none = 0;
			public const int wait_claim = 1;
			public const int claimed = 2;
		}

		public int getMomentID()
		{
			if( event_type == Type.moment_like ||
				event_type == Type.moment_comment ||
				event_type == Type.reward_moment_like)
			{
				return param1;
			}
			else
			{
				return 0;
			}
		}

		public static ClientActivity createFromPushMetaData(JsonObject data)
		{
			ClientActivity clientActivity = new ClientActivity();
			clientActivity.slot_id = data.getInteger("slot_id");
			clientActivity.event_time = TimeUtil.dateTimeFromUnixTimestamp(data.getLong("event_time"));
			clientActivity.agent_account_id = data.getInteger("agent_account_id");
			clientActivity.param1 = data.getInteger("param1");
			clientActivity.param2 = data.getInteger("param2");
			clientActivity.string_param = data.getString("string_param");
			clientActivity.reward_type = data.getInteger("reward_type");
			clientActivity.reward_refid = data.getInteger("reward_refid");
			clientActivity.reward_amount = data.getInteger("reward_amount");
			clientActivity.claim_status = data.getInteger("claim_status");
			return clientActivity;
		}

		[SerializeOption(SerializeOption.NONE)]
		public ClientMoment _moment;

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfile _agentProfile;
	}
}
