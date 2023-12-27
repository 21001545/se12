using System;

namespace DRun.Client.NetData
{
	public class ClientInvitationCode
	{
		public string code;
		public DateTime create_time;

		public int accept_account_id;
		public DateTime accept_time;

		public int accepted_count;
		public DateTime last_accepted_time;
	}
}
