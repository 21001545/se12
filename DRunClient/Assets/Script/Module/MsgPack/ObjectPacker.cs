using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;

namespace Festa.Client.Module.MsgPack
{

	public class ObjectPacker {
		private ObjectFactory _object_factory;
		private int _option_mask;

		public static ObjectPacker create(ObjectFactory of,int option_mask)
		{
			ObjectPacker packer = new ObjectPacker();
			packer.init(of, option_mask);
			return packer;
		}

		private void init(ObjectFactory of,int option_mask)
		{
			_object_factory = of;
			_option_mask = option_mask;

		}

		public void pack(MessagePacker packer, object o)
		{
			if (o == null)
			{
				packer.packNil();
				return;
			}

			Type t = o.GetType();

			if (t.IsPrimitive)
			{
				if (t.Equals(typeof(int)))
				{
					packer.packInt((int)o);
				}
				else if (t.Equals(typeof(long)))
				{
					packer.packLong((long)o);
				}
				else if (t.Equals(typeof(float)))
				{
					packer.packFloat((float)o);
				}
				else if (t.Equals(typeof(double)))
				{
					packer.packDouble((double)o);
				}
				else if (t.Equals(typeof(bool)))
				{
					packer.packBoolean((bool)o);
				}
				else if (t.Equals(typeof(byte)))
				{
					packer.packByte((byte)o);
				}
				else if (t.Equals(typeof(short)))
				{
					packer.packShort((short)o);
				}
				else if (t.Equals(typeof(char)))
				{
					packer.packByte((byte)o);
				}
				else
				{
					throw new Exception("unsupported primitive type : " + t.Name);
				}
			}
			else if (t.Equals(typeof(string)))
			{
				packer.packString((string)o);
			}
			else if(t.Equals(typeof(DateTime)))
			{
				long long_time = TimeUtil.unixTimestampFromDateTime((DateTime)o);

				byte[] payload = new byte[8];
				payload[0] = (byte)((long_time >> 56) & 0xff);
				payload[1] = (byte)((long_time >> 48) & 0xff);
				payload[2] = (byte)((long_time >> 40) & 0xff);
				payload[3] = (byte)((long_time >> 32) & 0xff);
				payload[4] = (byte)((long_time >> 24) & 0xff);
				payload[5] = (byte)((long_time >> 16) & 0xff);
				payload[6] = (byte)((long_time >> 8) & 0xff);
				payload[7] = (byte)((long_time) & 0xff);

				packer.packExtentionTypeHeader(ExtensionType.EXT_Timestamp, 8);
				packer.addPayload(payload, 0, 8);
			}
			else if(t.Equals(typeof(JsonObject)))
			{
				byte[] payload = Encoding.UTF8.GetBytes(((JsonObject)o).encode());
				packer.packExtentionTypeHeader(ExtensionType.EXT_JsonObject, payload.Length);
				packer.addPayload(payload, 0, payload.Length);
			}
			else if( t.Equals(typeof(JsonArray)))
			{
				byte[] payload = Encoding.UTF8.GetBytes(((JsonArray)o).encode());
				packer.packExtentionTypeHeader(ExtensionType.EXT_JsonArray, payload.Length);
				packer.addPayload(payload, 0, payload.Length);
			}
			// else if( t.Equals(UnixTimestamp))
			else if ( o is IList)
			{
				IList array = o as IList;
				var array_enumerator = array.GetEnumerator();

				packer.packArrayHeader(array.Count);
				while(array_enumerator.MoveNext())
				{
					pack(packer, array_enumerator.Current);
				}
			}
			else if( o is IDictionary)
			{
				IDictionary dic = o as IDictionary;
				var dic_enumerator = dic.GetEnumerator();

				packer.packMapHeader(dic.Count);
				while(dic_enumerator.MoveNext())
				{
					pack(packer, dic_enumerator.Key);
					pack(packer, dic_enumerator.Value);
				}
			}
			else
			{
				ObjectSchema schema = _object_factory.getSchema(t);
				if( schema == null)
				{
					throw new Exception(string.Format("unknown type : {0}", t.Name));
				}

				if( schema.isCustomSerializer())
				{
					CustomSerializer custom_serializer = (CustomSerializer)o;

					packer.packExtentionTypeHeader(ExtensionType.EXT_ClassCustom, 1);
					packer.packInt(schema.getHashID());

					custom_serializer.pack(packer);
				}
				else
				{
					packer.packExtentionTypeHeader(ExtensionType.EXT_Class, 1);
					packer.packInt(schema.getHashID());

					List<ObjectField> field_list = schema.getFields(_option_mask);
					packer.packArrayHeader(field_list.Count);

					for (int i = 0; i < field_list.Count; ++i)
					{
						ObjectField field = field_list[i];
						object field_value = field._field.GetValue(o);
						pack(packer, field_value);
					}
				}
			}
		}
	}

}
