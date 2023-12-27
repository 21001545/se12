using Festa.Client;
using Festa.Client.Module.MsgPack;
using System;

namespace DRun.Client
{
	public class ClientInvitationAccept
	{
		public int acceptor_account_id;
		public DateTime accept_time;

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfileCache _profileCache;

	}
}
