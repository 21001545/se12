using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module.MsgPack
{
	public struct ExtensionTypeHeader
	{
		public byte type;
		public int length;

		public ExtensionTypeHeader(byte type,int length)
		{
			this.type = type;
			this.length = length;
		}
	}
}
