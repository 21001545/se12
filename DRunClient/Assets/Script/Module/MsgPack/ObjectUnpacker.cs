using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Text;

namespace Festa.Client.Module.MsgPack
{

	public class ObjectUnpacker {

		private ObjectFactory _object_factory;
		private int _option_mask;

		public static ObjectUnpacker create(ObjectFactory of,int option_mask)
		{
			ObjectUnpacker unpacker = new ObjectUnpacker();
			unpacker.init(of, option_mask);
			return unpacker;
		}

		private void init(ObjectFactory of,int option_mask)
		{
			_object_factory = of;
			_option_mask = option_mask;
		}

		public object unpack(MessageUnpacker unpacker)
		{
			MessageFormat format = unpacker.getNextFormat();
			ValueType valueType = format.valueType;

			if( valueType == ValueType.NIL)
			{
				unpacker.unpackNil();
				return null;
			}
			else if( valueType == ValueType.INTEGER)
			{
				if( format == MessageFormat.INT64 || format == MessageFormat.UINT64)
				{
					return unpacker.unpackLong();
				}
				else if(format == MessageFormat.UINT32)
				{
					long value = unpacker.unpackLong();
					if( value < System.Int32.MinValue || value > System.Int32.MaxValue)
					{
						return value;
					}

					return (int)value;
				}
				else
				{
					return unpacker.unpackInt();
				}
			}
			else if( valueType == ValueType.FLOAT)
			{
				if( format == MessageFormat.FLOAT32)
				{
					return unpacker.unpackFloat();
				}
				else if( format == MessageFormat.FLOAT64)
				{
					return unpacker.unpackDouble();
				}
				else
				{
					throw new Exception(string.Format("unknown float value type"));
				}
			}
			else if( valueType == ValueType.BOOLEAN)
			{
				return unpacker.unpackBoolean();
			}
			else if( valueType == ValueType.STRING)
			{
				return unpacker.unpackString();
			}
			else if( valueType == ValueType.ARRAY)
			{
				int count = unpacker.unpackArrayHeader();
				//Array array = Array.CreateInstance(typeof(object), count);
				object[] array = new object[count];

				for (int i = 0; i < count; ++i)
				{
					//object item = unpack(unpacker);
					//array.SetValue(item, i);
					array[i] = unpack(unpacker);
				}

				return array;
			}
			else if( valueType == ValueType.MAP)
			{
				int count = unpacker.unpackMapHeader();
				Dictionary<object, object> map = new Dictionary<object, object>();

				for(int i = 0; i < count; ++i)
				{
					object key = unpack(unpacker);
					object value = unpack(unpacker);

					map.Add(key, value);
				}

				return map;
			}
			else if( valueType == ValueType.EXTENSION)
			{
				ExtensionTypeHeader header = unpacker.unpackExtensionTypeHeader();
				int type = header.type;
				if( type == ExtensionType.EXT_Class)
				{
					return unpackClass(unpacker, header.length);
				}
				else if( type == ExtensionType.EXT_ClassCustom)
				{
					return unpackCustomClass(unpacker, header.length);
				}
				else if( type == ExtensionType.EXT_Timestamp)
				{
					return unpackTimestamp(unpacker);
				}
				else if( type == ExtensionType.EXT_JsonObject)
				{
					return unpackJsonObject(unpacker, header.length);
				}
				else if( type == ExtensionType.EXT_JsonArray)
				{
					return unpackJsonArray(unpacker, header.length);
				}
				else
				{
					throw new Exception(string.Format("unknown extension type : {0}", type));
				}
			}
			else
			{
				throw new Exception( string.Format("unknown value type : {0}", valueType.name));
			}
		}

		private object unpackJsonObject(MessageUnpacker unpacker,int length)
		{
			byte[] payload = unpacker.readPayload(length);
			return new JsonObject(Encoding.UTF8.GetString(payload));
		}

		private object unpackJsonArray(MessageUnpacker unpacker,int length)
		{
			byte[] payload = unpacker.readPayload(length);
			return new JsonArray(Encoding.UTF8.GetString(payload));
		}

		private object unpackTimestamp(MessageUnpacker unpacker)
		{
			byte[] temp = unpacker.readPayload(8);

			if (BitConverter.IsLittleEndian)
			{
				MessageUnpacker.reverseArray(ref temp, 0, 8);
			}
			long epoc = BitConverter.ToInt64(temp, 0);

			return TimeUtil.dateTimeFromUnixTimestamp(epoc);
		}

		private object unpackCustomClass(MessageUnpacker unpacker,int payload_length)
		{
			int class_key = unpacker.unpackInt();

			ObjectSchema schema = _object_factory.getSchema(class_key);
			if( schema == null)
			{
				//throw new Exception(string.Format("can't find object schema from key : {0}", class_key));
				Logger.logWarning(string.Format("unknown object schema : key[{0}] length[{1}]", class_key, payload_length));
				unpacker.readPayload(payload_length);
				return null;
			}

			object o = schema.createInstance();

			CustomSerializer custom_serializer = (CustomSerializer)o;
			custom_serializer.unpack(unpacker);

			return o;
		}

		private object unpackClass(MessageUnpacker unpacker,int payload_length)
		{
			int class_key = unpacker.unpackInt();

			long read_class_position = unpacker.getStream().Position;
			
			ObjectSchema schema = _object_factory.getSchema(class_key);
			if( schema == null)
			{
				Logger.logWarning(string.Format("unknown object schema : key[{0}] length[{1}]", class_key, payload_length));
				//unpacker.readPayload(payload_length);
				unpacker.getStream().Position = read_class_position + payload_length;
				return null;

				//throw new Exception(string.Format("can't find object schema from key : {0}", class_key));
			}

			int count = unpacker.unpackArrayHeader();

			List<ObjectField> field_list = schema.getFields(_option_mask);

			if( field_list.Count != count)
			{
				Logger.logWarning(string.Format("class[{0}] field count is different : in class[{1}] <> in data[{2}]", schema.getClassType().Name, field_list.Count, count));
				//unpacker.readPayload(payload_length);
				unpacker.getStream().Position = read_class_position + payload_length;
				return null;

				// throw new Exception(string.Format("class[{0}] field count is different : in class[{1}] <> in data[{2}]", schema.getClassType().Name,field_list.Count, count));
			}

			try
			{
				object instance = Activator.CreateInstance(schema.getClassType());
				for (int i = 0; i < field_list.Count; ++i)
				{
					object field_item = unpack(unpacker);

					setField(instance, field_list[i], field_item);
				}
				return instance;
			}
			catch(Exception e)
			{
				throw new Exception( string.Format("unpackClass fail:{0}", schema.getClassType().Name), e);
			}
		}

		private void setField(object target,ObjectField obj_field,object item)
		{
			FieldInfo field_info = obj_field._field;

			if( item == null)
			{
				field_info.SetValue(target, null);
				return;
			}

			if( obj_field._type == ObjectField.Type_Array)
			{
				// 무조건 object[]임
				if( item.GetType().IsArray)
				{
					if( field_info.FieldType.GetElementType().Equals( typeof(object)) == true)
					{
						field_info.SetValue(target, item);
					}
					else
					{
						object[] src_array = (object[])item;
						Array target_array = Array.CreateInstance(field_info.FieldType.GetElementType(), src_array.Length);

						for(int i = 0; i < src_array.Length; ++i)
						{
							target_array.SetValue(src_array[i], i);
						}

						field_info.SetValue(target, target_array);
					}
				}
				else
				{
					throw new Exception(string.Format("field item set fail : class[{0}] name[{1}] type[{2}] <> data[{3}]", target.GetType().Name, field_info.Name, field_info.FieldType.Name, item.GetType().Name));
				}
			}
			else if( obj_field._type == ObjectField.Type_List)
			{
				field_info.SetValue(target, obj_field.validateForGeneric(0, item));
/*
				IList target_list = obj_field.createGenericList();

				if( item.GetType().IsArray)
				{
					object[] src_array = (object[])item;
					for(int i = 0; i < src_array.Length; ++i)
					{
						object src_item = src_array[i];

						if( src_item.GetType().Is)

						target_list.Add(src_array[i]);
					}

					field_info.SetValue(target, target_list);
				}
				else
				{
					throw new Exception(string.Format("field item set fail : class[{0}] name[{1}] type[{2}] <> data[{3}]", target.GetType().Name, field_info.Name, field_info.FieldType.Name, item.GetType().Name));
				}
*/
			}
			else if( obj_field._type == ObjectField.Type_Dictionary)
			{
				field_info.SetValue(target, obj_field.validateForGeneric(0, item));
/*
				IDictionary target_dic = obj_field.createGenericDictionary();

				if( typeof(IDictionary).IsAssignableFrom( item.GetType()))
				{
					IDictionary src_dic = (IDictionary)item;
					IDictionaryEnumerator dic_enum = src_dic.GetEnumerator();
					while(dic_enum.MoveNext())
					{
						target_dic.Add(dic_enum.Key, dic_enum.Value);
					}

					field_info.SetValue(target, target_dic);
				}
				else
				{
					throw new Exception(string.Format("field item set fail : class[{0}] name[{1}] type[{2}] <> data[{3}]", target.GetType().Name, field_info.Name, field_info.FieldType.Name, item.GetType().Name));
				}
*/
			}
			else
			{
				field_info.SetValue(target, item);
			}
			
		}
	}

}

