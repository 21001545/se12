using System.Collections;
using UnityEngine;
using UnityEditor;
using Festa.Client.Module.MsgPack;

namespace Festa.Client.Module.Net
{
	public class CustomSerializablePacket : AbstractPacket,CustomSerializer
	{
		public virtual int getID()
		{
			throw new System.Exception("not implemented");
		}

		public virtual void pack(MessagePacker packer)
		{
			throw new System.Exception("not implemented");
		}

		public virtual void unpack(MessageUnpacker unpacker)
		{
			throw new System.Exception("not implemented");
		}
	}
	

}

