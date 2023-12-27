using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.ViewModel
{
	public class EventViewModel : AbstractViewModel
	{
		private ClientInvitationCode _invitationCode;

		public ClientInvitationCode InvitationCode
		{
			get
			{
				return _invitationCode;
			}
			set
			{
				Set(ref _invitationCode, value);
			}
		}
		
		public static EventViewModel create()
		{
			EventViewModel vm = new EventViewModel();
			vm.init();
			return vm;
		}

		protected override void bindPacketParser()
		{
			bind(MapPacketKey.ClientAck.invitation_code, updateInvitationCode);
		}

		private void updateInvitationCode(object obj,MapPacket ack)
		{
			InvitationCode = (ClientInvitationCode)obj;
		}
	}
}
