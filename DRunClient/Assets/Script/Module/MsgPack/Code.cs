using UnityEngine;
using System.Collections;

namespace Festa.Client.Module.MsgPack
{
	public static class Code
	{
        public const byte POSFIXINT_MASK = (byte) 0x80;

        public const byte FIXMAP_PREFIX = (byte) 0x80;
        public const byte FIXARRAY_PREFIX = (byte) 0x90;
        public const byte FIXSTR_PREFIX = (byte) 0xa0;

        public const byte NIL = (byte) 0xc0;
        public const byte NEVER_USED = (byte) 0xc1;
        public const byte FALSE = (byte) 0xc2;
        public const byte TRUE = (byte) 0xc3;
        public const byte BIN8 = (byte) 0xc4;
        public const byte BIN16 = (byte) 0xc5;
        public const byte BIN32 = (byte) 0xc6;
        public const byte EXT8 = (byte) 0xc7;
        public const byte EXT16 = (byte) 0xc8;
        public const byte EXT32 = (byte) 0xc9;
        public const byte FLOAT32 = (byte) 0xca;
        public const byte FLOAT64 = (byte) 0xcb;
        public const byte UINT8 = (byte) 0xcc;
        public const byte UINT16 = (byte) 0xcd;
        public const byte UINT32 = (byte) 0xce;
        public const byte UINT64 = (byte) 0xcf;

        public const byte INT8 = (byte) 0xd0;
        public const byte INT16 = (byte) 0xd1;
        public const byte INT32 = (byte) 0xd2;
        public const byte INT64 = (byte) 0xd3;

        public const byte FIXEXT1 = (byte) 0xd4;
        public const byte FIXEXT2 = (byte) 0xd5;
        public const byte FIXEXT4 = (byte) 0xd6;
        public const byte FIXEXT8 = (byte) 0xd7;
        public const byte FIXEXT16 = (byte) 0xd8;

        public const byte STR8 = (byte) 0xd9;
        public const byte STR16 = (byte) 0xda;
        public const byte STR32 = (byte) 0xdb;

        public const byte ARRAY16 = (byte) 0xdc;
        public const byte ARRAY32 = (byte) 0xdd;

        public const byte MAP16 = (byte) 0xde;
        public const byte MAP32 = (byte) 0xdf;

		public const byte NEGFIXINT_PREFIX = (byte)0xe0;

		//
		public static bool isFixInt(byte b)
		{
			int v = b & 0xff;
			return v <= 0x7f || v >= 0xe0;
		}

		public static bool isPosFixInt(byte b)
		{
			return (b & POSFIXINT_MASK) == 0;
		}
		
		public static bool isNegFixInt(byte b)
		{
			return (b & NEGFIXINT_PREFIX) == NEGFIXINT_PREFIX;
		}

		public static bool isFixStr(byte b)
		{
			return (b & (byte)0xe0) == Code.FIXSTR_PREFIX;
		}

		public static bool isFixedArray(byte b)
		{
			return (b & (byte)0xf0) == Code.FIXARRAY_PREFIX;
		}

		public static bool isFixedMap(byte b)
		{
			return (b & (byte)0xf0) == Code.FIXMAP_PREFIX;
		}

		public static bool isFixedRaw(byte b)
		{
			return (b & (byte)0xe0) == Code.FIXSTR_PREFIX;
		}
	}

}

