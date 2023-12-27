using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientFollow
	{
		public int follow_id;
		public int season_id;
		public int score;

		[SerializeOption(SerializeOption.NONE)]
		public bool _isFollowBack;

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfileCache _profileCache;
	}
}
