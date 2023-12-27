using UnityEngine;
using System.Collections;

namespace Festa.Client.Module.MsgPack
{

	public interface CustomSerializer
	{
		void pack(MessagePacker packer);
		void unpack(MessageUnpacker unpacker);
	}

}
