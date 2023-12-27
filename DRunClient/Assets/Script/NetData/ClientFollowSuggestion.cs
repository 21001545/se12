using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientFollowSuggestion
	{
		public int slot_id;
		public int suggestion_id;

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfileCache _profile;
	}
}
