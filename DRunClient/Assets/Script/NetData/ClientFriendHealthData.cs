using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientFriendHealthData
	{
		public int account_id;
		public int value;

		[SerializeOption(SerializeOption.NONE)]
		public int _rank;

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfileCache _profile;
	}
}
