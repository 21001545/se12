using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Festa.Client.Module.Events;

namespace Festa.Client.Module.Net
{
	public class TcpPacketConsumer
	{
		private Dictionary<int,Handler<AbstractPacket>> _consumer_map;
		private List<AbstractPacket> _read_list;

		public static TcpPacketConsumer create()
		{
			TcpPacketConsumer consumer = new TcpPacketConsumer();
			consumer.init();
			return consumer;
		}

		private void init()
		{
			_consumer_map = new Dictionary<int,Handler<AbstractPacket>>();
			_read_list = new List<AbstractPacket>();
		}

		public void consume(List<AbstractPacket> packet_list)
		{
			for(int i = 0; i < packet_list.Count; ++i)
			{
				AbstractPacket packet = packet_list[ i];

				Handler<AbstractPacket> handler;
				if( _consumer_map.TryGetValue( packet.getID(), out handler))
				{
					handler( packet);
				}
				else
				{
					Debug.LogError( string.Format( "can't find packet consumer : id[{0}] type[{1}]", packet.getID(), packet.GetType().Name));
				}
			}
		}

		public void register(int id,Handler<AbstractPacket> consumer)
		{
			_consumer_map.Add( id, consumer);
		}

		public void pump(TcpSocket socket)
		{
			if( socket.hasReadList())
			{
				socket.dumpReadList(_read_list);
				consume(_read_list);
				_read_list.Clear();
			}
		}
	}
}