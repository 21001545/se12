using UnityEngine;
using System.Collections;

namespace Festa.Client.Module.Net
{
	public class TcpRecvBuffer
	{
		public byte[] buffer;
		public int capacity;
		public int offset;

		public TcpRecvBuffer()
		{
			buffer = new byte[1024 * 100];
			capacity = 1024 * 100;
			offset = 0;
		}
	}
}

