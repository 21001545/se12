using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Text;

namespace Festa.Client.Module.MsgPack
{
	public class MessagePacker
	{
		private Stream _stream;
		private int _total_written_bytes;
		
		private byte[] _temp = new byte[10];
		private Encoding _encoding = Encoding.UTF8;

		public static MessagePacker create(Stream stream)
		{
			MessagePacker packer = new MessagePacker();
			packer.init(stream);
			return packer;
		}

		private void init(Stream stream)
		{
			_stream = stream;
			_total_written_bytes = 0;
		}

		public void resetStream()
		{
			_stream.Position = 0;
			_total_written_bytes = 0;
		}

		public int getTotalWrittenBytes()
		{
			return _total_written_bytes;
		}

		public void packNil()
		{
			writeByte(Code.NIL);
		}

		public void packBoolean(bool b)
		{
			writeByte((byte)(b ? Code.TRUE : Code.FALSE));
		}

		public void packByte(byte b)
		{
			if (b < 128) {
				writeByte(b);
			}
			else
			{
				writeByteAndByte(Code.UINT8, b);
			}
		}

		public void packShort(short v)
		{
			if( v < -(1 << 5))
			{
				if( v < -(1 << 7))
				{
					writeByteAndShort(Code.INT16, v);
				}
				else
				{
					writeByteAndByte(Code.INT8, (byte)v);
				}
			}
			else if( v < (1 << 7))
			{
				writeByte((byte)v);
			}
			else
			{
				if( v < (1 << 8))
				{
					writeByteAndByte(Code.UINT8, (byte)v);
				}
				else
				{
					writeByteAndShort(Code.UINT16, v);
				}
			}
		}

		public void packInt(int v)
		{
			if( v < -(1 << 5))
			{
				if( v < -(1 << 15))
				{
					writeByteAndInt(Code.INT32, v);
				}
				else if( v < -(1 << 7))
				{
					writeByteAndShort(Code.INT16, (short)v);
				}
				else
				{
					writeByteAndByte(Code.INT8, (byte)v);
				}
			}
			else if( v < (1 << 7))
			{
				writeByte((byte)v);
			}
			else
			{
				if( v < (1 << 8))
				{
					writeByteAndByte(Code.UINT8, (byte)v);
				}
				else if( v < (1 << 16))
				{
					writeByteAndShort(Code.UINT16, (short)v);
				}
				else
				{
					writeByteAndInt(Code.UINT32, v);
				}
			}
		}

		public void packLong(long v)
		{
			if (v < -(1L << 5))
			{
				if (v < -(1L << 15))
				{
					if (v < -(1L << 31))
					{
						writeByteAndLong(Code.INT64, v);
					}
					else
					{
						writeByteAndInt(Code.INT32, (int)v);
					}
				}
				else
				{
					if (v < -(1 << 7))
					{
						writeByteAndShort(Code.INT16, (short)v);
					}
					else
					{
						writeByteAndByte(Code.INT8, (byte)v);
					}
				}
			}
			else if (v < (1 << 7))
			{
				// fixnum
				writeByte((byte)v);
			}
			else
			{
				if (v < (1L << 16))
				{
					if (v < (1 << 8))
					{
						writeByteAndByte(Code.UINT8, (byte)v);
					}
					else
					{
						writeByteAndShort(Code.UINT16, (short)v);
					}
				}
				else
				{
					if (v < (1L << 32))
					{
						writeByteAndInt(Code.UINT32, (int)v);
					}
					else
					{
						writeByteAndLong(Code.UINT64, v);
					}
				}
			}
		}

		public void packFloat(float v)
		{
			writeByteAndFloat(Code.FLOAT32, v);
		}

		public void packDouble(double v)
		{
			writeByteAndDouble(Code.FLOAT64, v);
		}
		
		public void packString(string s)
		{
			if( string.IsNullOrEmpty(s))
			{
				packRawStringHeader(0);
			}
			else
			{
				byte[] bytes = _encoding.GetBytes(s);
				packRawStringHeader(bytes.Length);
				addPayload(bytes, 0, bytes.Length);
			}
		}

		public void packArrayHeader(int arraySize)
		{
			if( arraySize < 0)
			{
				throw new Exception("array size must be >= 0");
			}

			if( arraySize < (1 << 4))
			{
				writeByte((byte)(Code.FIXARRAY_PREFIX | arraySize));
			}
			else if( arraySize < (1 << 16))
			{
				writeByteAndShort(Code.ARRAY16, (short)arraySize);
			}
			else
			{
				writeByteAndInt(Code.ARRAY32, arraySize);
			}
		}

		public void packBinaryHeader(int len)
		{
			if (len < (1 << 8))
			{
				writeByteAndByte(Code.BIN8, (byte)len);
			}
			else if (len < (1 << 16))
			{
				writeByteAndShort(Code.BIN16, (short)len);
			}
			else
			{
				writeByteAndInt(Code.BIN32, len);
			}
		}

		public void packMapHeader(int mapSize)
		{
			if( mapSize < 0)
			{
				throw new Exception("map size must be >= 0");
			}
			
			if( mapSize < (1 << 4))
			{
				writeByte((byte)(Code.FIXMAP_PREFIX | mapSize));
			}
			else if( mapSize < (1 << 16))
			{
				writeByteAndShort(Code.MAP16, (short)mapSize);
			}
			else
			{
				writeByteAndInt(Code.MAP32, mapSize);
			}
		}

		public void packRawStringHeader(int len)
		{
			if (len < (1 << 5))
			{
				writeByte((byte)(Code.FIXSTR_PREFIX | len));
			}
			else if (len < (1 << 8))
			{
				writeByteAndByte(Code.STR8, (byte)len);
			}
			else if( len < (1 << 16))
			{
				writeByteAndShort(Code.STR16, (short)len);
			}
			else
			{
				writeByteAndInt(Code.STR32, len);
			}
		}

		public void packExtentionTypeHeader(byte extType,int payloadLen)
		{
			if (payloadLen < (1 << 8))
			{
				if (payloadLen > 0 && (payloadLen & (payloadLen - 1)) == 0)
				{ // check whether dataLen == 2^x
					if (payloadLen == 1)
					{
						writeByteAndByte(Code.FIXEXT1, extType);
					}
					else if (payloadLen == 2)
					{
						writeByteAndByte(Code.FIXEXT2, extType);
					}
					else if (payloadLen == 4)
					{
						writeByteAndByte(Code.FIXEXT4, extType);
					}
					else if (payloadLen == 8)
					{
						writeByteAndByte(Code.FIXEXT8, extType);
					}
					else if (payloadLen == 16)
					{
						writeByteAndByte(Code.FIXEXT16, extType);
					}
					else
					{
						writeByteAndByte(Code.EXT8, (byte)payloadLen);
						writeByte(extType);
					}
				}
				else
				{
					writeByteAndByte(Code.EXT8, (byte)payloadLen);
					writeByte(extType);
				}
			}
			else if (payloadLen < (1 << 16))
			{
				writeByteAndShort(Code.EXT16, (short)payloadLen);
				writeByte(extType);
			}
			else
			{
				writeByteAndInt(Code.EXT32, payloadLen);
				writeByte(extType);

				// TODO support dataLen > 2^31 - 1
			}
		}

		public void addPayload(byte[] src,int off,int len)
		{
			_stream.Write(src, off, len);
			_total_written_bytes += len;
		}

		private void writeByte(byte b)
		{
			_stream.WriteByte(b);
			_total_written_bytes++;
		}

		private void writeByteAndByte(byte b,byte v)
		{
			_stream.WriteByte(b);
			_stream.WriteByte(v);
			_total_written_bytes += 2;
		}

		private void writeByteAndShort(byte b,short v)
		{
			_stream.WriteByte(b);

			_temp[0] = (byte)(v >> 8);
			_temp[1] = (byte)v;
			_stream.Write(_temp, 0, 2);

			_total_written_bytes += 3;
		}

		private void writeByteAndInt(byte b,int v)
		{
			_stream.WriteByte(b);

			_temp[0] = (byte)(v >> 24);
			_temp[1] = (byte)(v >> 16);
			_temp[2] = (byte)(v >> 8);
			_temp[3] = (byte)v;

			_stream.Write(_temp, 0, 4);
			_total_written_bytes += 5;
		}

		private void writeByteAndFloat(byte b,float v)
		{
			byte[] raw = BitConverter.GetBytes(v);

			_stream.WriteByte(b);
			if (BitConverter.IsLittleEndian)
			{
				_temp[0] = raw[3];
				_temp[1] = raw[2];
				_temp[2] = raw[1];
				_temp[3] = raw[0];
			}
			else
			{
				_temp[0] = raw[0];
				_temp[1] = raw[1];
				_temp[2] = raw[2];
				_temp[3] = raw[3];
			}

			_stream.Write(_temp, 0, 4);
			_total_written_bytes += 5;
		}

		private void writeByteAndDouble(byte b,double v)
		{
			byte[] raw = BitConverter.GetBytes(v);

			_stream.WriteByte(b);
			if( BitConverter.IsLittleEndian)
			{
				_temp[0] = raw[7];
				_temp[1] = raw[6];
				_temp[2] = raw[5];
				_temp[3] = raw[4];
				_temp[4] = raw[3];
				_temp[5] = raw[2];
				_temp[6] = raw[1];
				_temp[7] = raw[0];
			}
			else
			{
				_temp[0] = raw[0];
				_temp[1] = raw[1];
				_temp[2] = raw[2];
				_temp[3] = raw[3];
				_temp[4] = raw[4];
				_temp[5] = raw[5];
				_temp[6] = raw[6];
				_temp[7] = raw[7];
			}

			_stream.Write(_temp, 0, 8);
			_total_written_bytes += 9;
		}

		private void writeByteAndLong(byte b,long v)
		{
			_stream.WriteByte(b);
			_temp[0] = (byte)(v >> 56);
			_temp[1] = (byte)(v >> 48);
			_temp[2] = (byte)(v >> 40);
			_temp[3] = (byte)(v >> 32);
			_temp[4] = (byte)(v >> 24);
			_temp[5] = (byte)(v >> 16);
			_temp[6] = (byte)(v >> 8);
			_temp[7] = (byte)(v);

			_stream.Write(_temp, 0, 8);
			_total_written_bytes += 9;
		}
	}


}
