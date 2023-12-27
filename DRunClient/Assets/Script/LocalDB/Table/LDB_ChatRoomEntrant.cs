using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.LocalDB
{
	public class LDB_ChatRoomEntrant
	{
		[Indexed]
		public long chatroom_id { get; set; }
		[Indexed]
		public int account_id { get; set; }
	}
}
