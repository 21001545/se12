using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Festa.Client.Module.MsgPack;

namespace Festa.Client.Module.Net
{
	public class TcpClient
	{
		private ObjectFactory _object_factory;
		private TcpSocket	_socket = null;
		private List<AbstractPacket> _read_packet_list;
		private TcpPacketConsumer	_packet_consumer;

		public static TcpClient create(ObjectFactory of)
		{
			TcpClient client = new TcpClient();
			client.init(of);
			return client;
		}

		private void init(ObjectFactory of)
		{
			_object_factory = of;
			_read_packet_list = new List<AbstractPacket>();
			_packet_consumer = TcpPacketConsumer.create();
		}

		private TcpSocket createSocket()
		{
			return TcpSocket.create(_object_factory, SerializeOption.ALL, 4096);
		}

		public void update()
		{
			if( _socket != null)
			{
				if( _socket.hasReadList())
				{
					_read_packet_list.Clear();
					_socket.dumpReadList(_read_packet_list);

					_packet_consumer.consume( _read_packet_list);
				}
			}
		}

		private void onConnected(TcpSocket connected_socket,bool result)
		{
		}

		private void onDisconnected(TcpSocket disconencted_socket)
		{
		}
	}
}