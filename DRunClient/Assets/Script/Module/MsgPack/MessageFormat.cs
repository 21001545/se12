using UnityEngine;
using System.Collections;

namespace Festa.Client.Module.MsgPack
{

	public class MessageFormat
	{
		// INT7
		public static MessageFormat POSFIXINT = new MessageFormat(ValueType.INTEGER);
		// MAP4
		public static MessageFormat FIXMAP = new MessageFormat(ValueType.MAP);
		// ARRAY4
		public static MessageFormat FIXARRAY = new MessageFormat(ValueType.ARRAY);
		// STR5
		public static MessageFormat FIXSTR = new MessageFormat(ValueType.STRING);
		public static MessageFormat NIL = new MessageFormat(ValueType.NIL);
		public static MessageFormat NEVER_USED = new MessageFormat(null);
		public static MessageFormat BOOLEAN = new MessageFormat(ValueType.BOOLEAN);
		public static MessageFormat BIN8 = new MessageFormat(ValueType.BINARY);
		public static MessageFormat BIN16 = new MessageFormat(ValueType.BINARY);
		public static MessageFormat BIN32 = new MessageFormat(ValueType.BINARY);
		public static MessageFormat EXT8 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat EXT16 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat EXT32 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat FLOAT32 = new MessageFormat(ValueType.FLOAT);
		public static MessageFormat FLOAT64 = new MessageFormat(ValueType.FLOAT);
		public static MessageFormat UINT8 = new MessageFormat(ValueType.INTEGER);
		public static MessageFormat UINT16 = new MessageFormat(ValueType.INTEGER);
		public static MessageFormat UINT32 = new MessageFormat(ValueType.INTEGER);
		public static MessageFormat UINT64 = new MessageFormat(ValueType.INTEGER);

		public static MessageFormat INT8 = new MessageFormat(ValueType.INTEGER);
		public static MessageFormat INT16 = new MessageFormat(ValueType.INTEGER);
		public static MessageFormat INT32 = new MessageFormat(ValueType.INTEGER);
		public static MessageFormat INT64 = new MessageFormat(ValueType.INTEGER);
		public static MessageFormat FIXEXT1 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat FIXEXT2 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat FIXEXT4 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat FIXEXT8 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat FIXEXT16 = new MessageFormat(ValueType.EXTENSION);
		public static MessageFormat STR8 = new MessageFormat(ValueType.STRING);
		public static MessageFormat STR16 = new MessageFormat(ValueType.STRING);
		public static MessageFormat STR32 = new MessageFormat(ValueType.STRING);
		public static MessageFormat ARRAY16 = new MessageFormat(ValueType.ARRAY);
		public static MessageFormat ARRAY32 = new MessageFormat(ValueType.ARRAY);
		public static MessageFormat MAP16 = new MessageFormat(ValueType.MAP);
		public static MessageFormat MAP32 = new MessageFormat(ValueType.MAP);
		public static MessageFormat NEGFIXINT = new MessageFormat(ValueType.INTEGER);


		private static MessageFormat[] formatTable = new MessageFormat[256];
		public ValueType valueType;

		static MessageFormat()
		{
			for(int i = 0; i < 256; i ++)
			{
				formatTable[i] = toMessageFormat((byte)i);
			}
		}

		MessageFormat(ValueType v)
		{
			valueType = v;
		}

		public static MessageFormat valueof(byte b)
		{
			return formatTable[b & 0xFF];
		}

		static MessageFormat toMessageFormat(byte b)
		{
			if (Code.isPosFixInt(b))
			{
				return POSFIXINT;
			}
			if (Code.isNegFixInt(b))
			{
				return NEGFIXINT;
			}
			if (Code.isFixStr(b))
			{
				return FIXSTR;
			}
			if (Code.isFixedArray(b))
			{
				return FIXARRAY;
			}
			if (Code.isFixedMap(b))
			{
				return FIXMAP;
			}
			switch (b)
			{
				case Code.NIL:
					return NIL;
				case Code.FALSE:
				case Code.TRUE:
					return BOOLEAN;
				case Code.BIN8:
					return BIN8;
				case Code.BIN16:
					return BIN16;
				case Code.BIN32:
					return BIN32;
				case Code.EXT8:
					return EXT8;
				case Code.EXT16:
					return EXT16;
				case Code.EXT32:
					return EXT32;
				case Code.FLOAT32:
					return FLOAT32;
				case Code.FLOAT64:
					return FLOAT64;
				case Code.UINT8:
					return UINT8;
				case Code.UINT16:
					return UINT16;
				case Code.UINT32:
					return UINT32;
				case Code.UINT64:
					return UINT64;
				case Code.INT8:
					return INT8;
				case Code.INT16:
					return INT16;
				case Code.INT32:
					return INT32;
				case Code.INT64:
					return INT64;
				case Code.FIXEXT1:
					return FIXEXT1;
				case Code.FIXEXT2:
					return FIXEXT2;
				case Code.FIXEXT4:
					return FIXEXT4;
				case Code.FIXEXT8:
					return FIXEXT8;
				case Code.FIXEXT16:
					return FIXEXT16;
				case Code.STR8:
					return STR8;
				case Code.STR16:
					return STR16;
				case Code.STR32:
					return STR32;
				case Code.ARRAY16:
					return ARRAY16;
				case Code.ARRAY32:
					return ARRAY32;
				case Code.MAP16:
					return MAP16;
				case Code.MAP32:
					return MAP32;
				default:
					return NEVER_USED;
			}
		}

		

	}


}
