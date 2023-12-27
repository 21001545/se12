using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

namespace Festa.Client.Module.MsgPack
{
	public class MessageUnpacker
	{
		private Stream _stream;
		private int _total_read_bytes;
		private byte[] _temp = new byte[8];
		private Encoding _encoding = Encoding.UTF8;
		private byte[] _string_buf = new byte[64];

		public static MessageUnpacker create(Stream stream)
		{
			MessageUnpacker unpacker = new MessageUnpacker();
			unpacker.init(stream);
			return unpacker;
		}

		private void init(Stream stream)
		{
			_stream = stream;
			_total_read_bytes = 0;
		}

		public void resetStream()
		{
			_stream.Position = 0;
			_total_read_bytes = 0;
		}

		public void reset(Stream stream)
		{
			_stream = stream;
			_total_read_bytes = 0;
		}

		public Stream getStream()
		{
			return _stream;
		}

		public int getTotalReadBytes()
		{
			return _total_read_bytes;
		}

		public MessageFormat getNextFormat()
		{
			byte b = peekByte();
			return MessageFormat.valueof(b);
		}

		public int unpackInt()
		{
			byte b = readByte();
			if( Code.isFixInt(b))
			{
				return unchecked((sbyte)b);
			}

			switch(b)
			{
				case Code.UINT8:
					byte u8 = readByte();
					return u8 & 0xff;
				case Code.UINT16:
					short u16 = readShort();
					return u16 & 0xffff;
				case Code.UINT32:
					int u32 = readInt();
					if( u32 < 0)
					{
						throw new Exception("overflowU32");
					}
					return u32;
				case Code.UINT64:
					long u64 = readLong();
					if( u64 < 0 || u64 > (long)System.Int32.MaxValue)
					{
						throw new Exception("overflowU64");
					}
					return (int)u64;
				case Code.INT8:
					sbyte i8 = unchecked((sbyte)readByte());
					return i8;
				case Code.INT16:
					short i16 = readShort();
					return i16;
				case Code.INT32:
					int i32 = readInt();
					return i32;
				case Code.INT64:
					long i64 = readLong();
					if( i64 < (long)System.Int32.MinValue || i64 > (long)System.Int32.MaxValue)
					{
						throw new Exception("overflowi64");
					}
					return (int)i64;
			}

			throw new Exception( string.Format( "unexpected[integer] : {0}", b));
		}

		public long unpackLong()
		{
			byte b = readByte();
			if( Code.isFixInt(b))
			{
				return (long)b;
			}
			switch(b)
			{
				case Code.UINT8:
					byte u8 = readByte();
					return (long)(u8 & 0xff);
				case Code.UINT16:
					short u16 = readShort();
					return (long)(u16 & 0xffff);
				case Code.UINT32:
					int u32 = readInt();
					if( u32 < 0)
					{
						return (long)(u32 & 0x7fffffff) + 0x80000000L;
					}
					else
					{
						return (long)u32;
					}
				case Code.UINT64:
					long u64 = readLong();
					if(u64 < 0)
					{
						throw new Exception("overflowU64");
					}
					return u64;
				case Code.INT8:
					byte i8 = readByte();
					return (long)i8;
				case Code.INT16:
					short i16 = readShort();
					return (long)i16;
				case Code.INT32:
					int i32 = readInt();
					return (long)i32;
				case Code.INT64:
					long i64 = readLong();
					return i64;
			}

			throw new Exception(string.Format("unexpected[long] : {0}", b));
		}

		public float unpackFloat()
		{
			byte b = readByte();
			switch(b)
			{
				case Code.FLOAT32:
					float fv = readFloat();
					return fv;
				case Code.FLOAT64:
					double dv = readDouble();
					return (float)dv;
			}

			throw new Exception(string.Format("unexepcted[float] : {0}", b));
		}

		public double unpackDouble()
		{
			byte b = readByte();
			switch(b)
			{
				case Code.FLOAT32:
					float fv = readFloat();
					return (double)fv;
				case Code.FLOAT64:
					double dv = readDouble();
					return dv;
			}
			throw new Exception(string.Format("unexpected[double] : {0}", b));
		}

		public string unpackString()
		{
			int len = unpackRawStringHeader();
			if( len == 0)
			{
				return string.Empty;
			}

			if(_string_buf.Length < len)
			{
				Array.Resize<byte>(ref _string_buf, len);
			}

			if( _stream.Read(_string_buf, 0, len) != len)
			{
				throw new Exception(string.Format("unpack string fail : EOF"));
			}

			_total_read_bytes += len;

			return _encoding.GetString(_string_buf, 0, len);
		}

		public int unpackRawStringHeader()
		{
			byte b = readByte();
			if( Code.isFixedRaw(b))
			{
				return b & 0x1f;
			}

			int len = tryReadStringHeader(b);
			if( len >= 0)
			{
				return len;
			}

			throw new Exception(string.Format("unexpected[string] : {0}", b));
		}

		public int unpackArrayHeader()
		{
			byte b = readByte();
			if(Code.isFixedArray(b))
			{
				return b & 0x0f;
			}

			switch(b)
			{
				case Code.ARRAY16:
					return readNextLength16();
				case Code.ARRAY32:
					return readNextLength32();
			}

			throw new Exception(string.Format("unexpected[Array] : {0}",b));
		}

		public int unpackBinaryHeader()
		{
			byte b = readByte();
			if(Code.isFixedRaw(b))
			{
				return b & 0xff;
			}

			switch(b)
			{
				case Code.BIN8:
					return readNextLength8();
				case Code.BIN16:
					return readNextLength16();
				case Code.BIN32:
					return readNextLength32();
			}

			throw new Exception(string.Format("unexpected[Binary] : {0}", b));
		}

		public int unpackMapHeader()
		{
			byte b = readByte();
			if(Code.isFixedMap(b))
			{
				return b & 0x0f;
			}
			switch(b)
			{
				case Code.MAP16:
					return readNextLength16();
				case Code.MAP32:
					return readNextLength32();
			}

			throw new Exception(string.Format("unexpected[Map] : {0}", b));
		}

		public ExtensionTypeHeader unpackExtensionTypeHeader()
		{
			byte b = readByte();
			switch(b)
			{
				case Code.FIXEXT1:
					{
						byte type = readByte();
						return new ExtensionTypeHeader(type, 1);
					}
				case Code.FIXEXT2:
					{
						byte type = readByte();
						return new ExtensionTypeHeader(type, 2);
					}
				case Code.FIXEXT4:
					{
						byte type = readByte();
						return new ExtensionTypeHeader(type, 4);
					}
				case Code.FIXEXT8:
					{
						byte type = readByte();
						return new ExtensionTypeHeader(type, 8);
					}
				case Code.FIXEXT16:
					{
						byte type = readByte();
						return new ExtensionTypeHeader(type, 16);
					}
				case Code.EXT8:
					{
						byte length = readByte();
						byte type = readByte();
						return new ExtensionTypeHeader(type, length);
					}
				case Code.EXT16:
					{
						ushort length = (ushort)readShort();
						byte type = readByte();
						return new ExtensionTypeHeader(type, length);
					}
				case Code.EXT32:
					{
						int length = readInt();
						byte type = readByte();
						return new ExtensionTypeHeader(type, length);
					}
			}

			throw new Exception(string.Format("unexpected[ExtensionTypeHeader] : {0}", b));
		}

		public bool unpackBoolean()
		{
			byte b = readByte();
			if( b == Code.FALSE)
			{
				return false;
			}
			else if( b == Code.TRUE)
			{
				return true;
			}

			throw new Exception(string.Format("unexpected[Boolean] : {0}", b));
		}
		
		public void unpackNil()
		{
			byte b = readByte();
			if( b == Code.NIL)
			{
				return;
			}

			throw new Exception(string.Format("not nil : {0}", b));
		}

		public byte[] readPayload(int length)
		{
			if( _string_buf.Length < length)
			{
				Array.Resize<byte>(ref _string_buf, length);
			}

			if( _stream.Read(_string_buf, 0, length) != length)
			{
				throw new Exception("readaPayload fail : EOF");
			}

			_total_read_bytes += length;
			return _string_buf;
		}

		private int tryReadStringHeader(byte b)
		{
			switch(b)
			{
				case Code.STR8:
					return readNextLength8();
				case Code.STR16:
					return readNextLength16();
				case Code.STR32:
					return readNextLength32();
				default:
					return -1;
			}
		}

		private int readNextLength8()
		{
			byte u8 = readByte();
			return u8 & 0xff;
		}

		private int readNextLength16()
		{
			short u16 = readShort();
			return u16 & 0xffff;
		}

		private int readNextLength32()
		{
			int u32 = readInt();
			if(u32 < 0)
			{
				throw new Exception("overflowU32 Length");
			}
			return u32;
		}

		private byte peekByte()
		{
			long prev_pos = _stream.Position;
			int r = _stream.ReadByte();
			if (r == -1)
			{
				throw new Exception("read stream fail : length - 1");
			}

			_stream.Position = prev_pos;
			return (byte)r;
		}

		private byte readByte()
		{
			int r = _stream.ReadByte();
			if( r == -1)
			{
				throw new Exception("read stream fail : length - 1");
			}
			_total_read_bytes++;

			return (byte)r;
		}

		public static void reverseArray(ref byte[] b,int start,int length)
		{
			byte tmp;
			for (int i = 0; i < length / 2; ++i)
			{
				tmp = b[i + start];
				b[start + i] = b[start + (length - i - 1)];
				b[start + (length - i - 1)] = tmp;
			}
		}

		private short readShort()
		{
			if (_stream.Read(_temp, 0, 2) != 2)
			{
				throw new Exception("read stream fail : length - 2");
			}
			_total_read_bytes += 2;

			if (BitConverter.IsLittleEndian)
			{
				reverseArray(ref _temp, 0, 2);
			}
			return BitConverter.ToInt16(_temp, 0);
		}

		private int readInt()
		{
			if (_stream.Read(_temp, 0, 4) != 4)
			{
				throw new Exception("read stream fail : length - 4");
			}
			_total_read_bytes += 4;

			if (BitConverter.IsLittleEndian)
			{
				reverseArray(ref _temp, 0, 4);
			}
			return BitConverter.ToInt32(_temp, 0);
		}

		private long readLong()
		{
			if (_stream.Read(_temp, 0, 8) != 8)
			{
				throw new Exception("read stream fail : length - 8");
			}
			_total_read_bytes += 8;

			if (BitConverter.IsLittleEndian)
			{
				reverseArray(ref _temp, 0, 8);
			}
			return BitConverter.ToInt64(_temp, 0);
		}

		private float readFloat()
		{
			if (_stream.Read(_temp, 0, 4) != 4)
			{
				throw new Exception("read stream fail : length - 4");
			}
			_total_read_bytes += 4;

			if (BitConverter.IsLittleEndian)
			{
				reverseArray(ref _temp, 0, 4);
			}
			return BitConverter.ToSingle(_temp, 0);
		}

		private double readDouble()
		{
			if (_stream.Read(_temp, 0, 8) != 8)
			{
				throw new Exception("read stream fail : length - 8");
			}
			_total_read_bytes += 8;

			if (BitConverter.IsLittleEndian)
			{
				reverseArray(ref _temp, 0, 8);
			}
			return BitConverter.ToDouble(_temp, 0);
		}
	}
}
