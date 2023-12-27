using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientMapTileReveal
	{
		public int tile_x;
		public int tile_y;
		public DateTime update_time;
		public BlobData reveal_data;
	}
}
