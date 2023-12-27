using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientFollowBack
	{
		public int follow_back_id;
		public int season_id;
		public int score;

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfileCache _profileCache;
	}
}
