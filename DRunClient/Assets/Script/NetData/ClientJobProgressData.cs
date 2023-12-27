
using Festa.Client.Module.MsgPack;
using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.NetData
{
	public class ClientJobProgressData
	{
		public string key;
		public int status;
		public int progress;
		public int param;
		public int result_code;
		public string error_message;
		public BlobData ack_data;

		public static class Status
		{
			public const int create = 0;
			public const int running = 1;
			public const int complete = 2;
		}

		public MapPacket packetFromAckData()
		{
			if( ack_data == null)
			{
				return null;
			}

			try
			{
				using(MemoryStream inputStream = new MemoryStream(ack_data.getData()))
				{
					MessageUnpacker msgUnpacker = MessageUnpacker.create(inputStream);
					ObjectUnpacker objUnpacker = ObjectUnpacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);

					MapPacket packet = (MapPacket)objUnpacker.unpack(msgUnpacker);
					return packet;
				}
			}
			catch(Exception e)
			{
				Debug.LogException(e);
				return null;
			}
		}
	}
}
