using UnityEngine;
using UnityEditor;

namespace Festa.Client.Module.MsgPack
{
	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class SerializeOption : System.Attribute
	{
		public const int NONE = 0;
		public const int ALL = 0xFF;
		public const int CSNet = 0xFE;

		public int value;

		public SerializeOption(int v = ALL)
		{
			value = v;
		}
	}

}
