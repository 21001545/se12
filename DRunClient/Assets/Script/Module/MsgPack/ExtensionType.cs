using UnityEngine;
using UnityEditor;

namespace Festa.Client.Module.MsgPack
{
	public static class ExtensionType
	{
		public static byte EXT_Class = 1;
		public static byte EXT_Timestamp = 2;
		public static byte EXT_ClassCustom = 3;
		public static byte EXT_JsonObject = 4;
		public static byte EXT_JsonArray = 5;
	}
}