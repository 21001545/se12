using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientBookmark
	{
		public int object_datasource_id;
		public int object_owner_account_id;
		public int object_type;
		public int object_id;
		public DateTime create_time;

		[SerializeOption(SerializeOption.NONE)]
		public ClientMoment _moment;
	}
}
