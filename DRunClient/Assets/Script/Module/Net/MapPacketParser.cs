using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module.Net
{
	public class MapPacketParser
	{
		public delegate void ParseHandler(object obj, MapPacket packet);

		private Dictionary<int, ParseHandler> _handlers;

		public static MapPacketParser create()
		{
			MapPacketParser parser = new MapPacketParser();
			parser.init();
			return parser;
		}

		private void init()
		{
			_handlers = new Dictionary<int, ParseHandler>();
		}

		public void bind(int key, ParseHandler handler)
		{
			_handlers.Add(key, handler);
		}

		public void parse(MapPacket ack)
		{
			foreach(KeyValuePair<int,object> item in ack.getMap())
			{
				ParseHandler handler;
				if( _handlers.TryGetValue( item.Key, out handler))
				{
					handler(item.Value, ack);
				}
			}
		}
	}
}
