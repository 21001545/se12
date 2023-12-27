using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Festa.Client.Module.Net
{
	public class MapPacket : AbstractPacket
	{
		protected Dictionary<int, object> _map;

		public Dictionary<int, object> getMap()
		{
			return _map;
		}

		public int getID()
		{
			return (int)get(MapPacketKey.msg_id);
		}

		public void setID(int id)
		{
			put(MapPacketKey.msg_id, id);
		}

		public object get(int id)
		{
			return _map[id];
		}

		public object getWithDefault(int id, object def)
		{
			object value;
			if (_map.TryGetValue(id, out value))
			{
				return value;
			}

			return def;
		}

		public long getLong(int id)
		{
			object value = get(id);
			if( value is long)
			{
				return (long)value;
			}
			else if (value is Int32 || value is int)
			{
				return (long)(int)(value);
			}
			else if( value is System.Double || value is double)
			{
				return (long)(double)value;
			}
			else if( value is System.Single)
			{
				return (long)(System.Single)value;
			}

			throw new System.Exception("getLong fail : unknown type -" + value.GetType().Name);
		}

		public void put(int id,object o)
		{
			_map.Add(id, o);
		}

		public bool contains(int id)
		{
			return _map.ContainsKey(id);
		}

		public object get(string str_id)
		{
			return get(EncryptUtil.makeHashCode(str_id));
		}

		public object getWithDefault(string str_id,object def)
		{
			return getWithDefault(EncryptUtil.makeHashCode(str_id), def);
		}

		public long getLong(string str_id)
		{
			return getLong(EncryptUtil.makeHashCode(str_id));
		}

		public List<V> getList<V>(int id)
		{
			object[] array = (object[])get(id);
			List<V> list = new List<V>();
			foreach(object o in array)
			{
				list.Add((V)o);
			}
			return list;
		}

		public List<V> getList<V>(string str_id)
		{
			return getList<V>(EncryptUtil.makeHashCode(str_id));
		}

		//
		public Dictionary<K,V> getDictionary<K,V>(int id)
		{
			Dictionary<object, object> dic_origin = (Dictionary<object,object>)get(id);
			Dictionary<K, V> dic_new = new Dictionary<K, V>();
			foreach(KeyValuePair<object,object> item in dic_origin)
			{
				dic_new.Add((K)item.Key, (V)item.Value);
			}
			return dic_new;
		}

		public Dictionary<K,V> getDictionary<K,V>(string str_id)
		{
			return getDictionary<K, V>(EncryptUtil.makeHashCode(str_id));
		}

		public void put(string str_id,object o)
		{
			put(EncryptUtil.makeHashCode(str_id), o);
		}

		public bool contains(string str_id)
		{
			return contains(EncryptUtil.makeHashCode(str_id));
		}

		public int getResult()
		{
			return (int)get(MapPacketKey.result);
		}

		public static MapPacket create()
		{
			MapPacket packet = new MapPacket();
			packet._map = new Dictionary<int, object>();
			return packet;
		}

		public static MapPacket createWithMsgID(int msg_id)
		{
			MapPacket packet = create();
			packet.setID(msg_id);
			return packet;
		}

		public System.Exception makeErrorException()
		{
			int error_code = getResult();
			string error_message = (string)getWithDefault(MapPacketKey.error_message, string.Empty);

			return new System.Exception($"result_code[{error_code}]\n{error_message}");
		}

		public System.Exception makeErrorException(int req_msg_id)
		{
			int error_code = getResult();
			string error_message = (string)getWithDefault(MapPacketKey.error_message, string.Empty);

			return new System.Exception($"req_msg_id[{req_msg_id}] result_code[{error_code}]\n{error_message}");
		}
	}


}

