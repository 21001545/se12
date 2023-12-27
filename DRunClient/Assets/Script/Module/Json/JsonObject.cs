using System.Collections;
using System.Collections.Generic;

namespace Festa.Client.Module
{

	// 하나 만들자..
	public class JsonObject
	{
		private IDictionary<string,object> _map;

		public JsonObject()
		{
			_map = new Dictionary<string,object>();
		}

		public JsonObject(string encoded)
		{
			//_map = Facebook.MiniJSON.Json.Deserialize( encoded) as IDictionary<string,object>;
			_map = MiniJson.JsonDecode(encoded) as IDictionary<string,object>;
		}

		public static JsonObject parse(string encoded)
		{
			if( string.IsNullOrEmpty( encoded))
			{
				return null;
			}

			return new JsonObject( encoded);
		}

		public JsonObject(IDictionary<string,object> map)
		{
			_map = map;
		}

		public IDictionary<string,object> getMap()
		{
			return _map;
		}

		public bool contains(string key)
		{
			return _map.ContainsKey( key);
		}

		public object getValue(string key)
		{
			object value;
			if( _map.TryGetValue(key, out value) == false)
			{
				throw new System.Exception("getValue fail : can't find key : " + key);
			}

			return value;
		}

		public int getInteger(string key)
		{
			if( _map.ContainsKey( key) == false)
			{
				throw new System.Exception("getInteger fail : can't find key : " + key);
			}

			object value = _map[ key];
			if( value is long)
			{
				return (int)(long)value;
			}
			else if( value is double)
			{
				return (int)(double)value;
			}

			throw new System.Exception( "getInteger fail : unknown type - " + value.GetType().Name);        
		}

		public long getLong(string key)
		{
			if( _map.ContainsKey( key) == false)
			{
				throw new System.Exception("getLong fail : can't find key : " + key);
			}

			object value = _map[key];
			if( value is long)
			{
				return (long)value;
			}
			else if( value is double)
			{
				return (long)(double)value;
			}

			throw new System.Exception( "getLong fail : unknwon type - " + value.GetType().Name);
		}

		public float getFloat(string key)
		{
			if( _map.ContainsKey( key) == false)
			{
				throw new System.Exception("getFloat fail : can't find key : " + key);
			}

			object value = _map[ key];
			if( value is long)
			{
				return (float)(long)value;
			}
			else if( value is double)
			{
				return (float)(double)value;
			}

			throw new System.Exception( "getFloat fail : unknown type - " + value.GetType().Name);        
		}

		public double getDouble(string key)
		{
			if( _map.ContainsKey( key) == false)
			{
				throw new System.Exception("getFloat fail : can't find key : " + key);
			}

			object value = _map[ key];
			if( value is long)
			{
				return (double)(long)value;
			}
			else if( value is double)
			{
				return (double)(double)value;
			}

			throw new System.Exception( "getFloat fail : unknown type - " + value.GetType().Name);        
		}

		public bool getBool(string key)
		{
			if( _map.ContainsKey( key) == false)
			{
				throw new System.Exception("getBool fail : can't find key : " + key);
			}

			return (bool)_map[key];
		}

		public string getString(string key)
		{
			if( _map.ContainsKey( key) == false)
			{
				return null;
			}

			return _map[key] as string;
		}

		public JsonObject getJsonObject(string key)
		{
			if( _map.ContainsKey( key) == false)
			{
				return null;
			}

			object obj = _map[ key];

			if( obj is IDictionary<string,object>)
			{
				return new JsonObject( obj as IDictionary<string,object>);
			}
			else
			{
				return null;
			}
		}

		public JsonArray getJsonArray(string key)
		{
			if (_map.ContainsKey( key) == false)
			{
				return null;
			}

			object obj = _map[ key];

			if( obj is IList<object>)
			{
				return new JsonArray( obj as IList<object>);
			}
			else
			{
				return null;
			}
		}

		public void put(string key,object value)
		{
			if( _map.ContainsKey( key))
			{
				_map.Remove( key);
			}

			if( value == null)
			{
				_map.Add( key, null);
			}
			else if( value is JsonArray)
			{
				_map.Add( key, ((JsonArray)value).getList());
			}
			else if( value is JsonObject)
			{
				_map.Add( key, ((JsonObject)value).getMap());
			}
			else if( value is float)
			{
				_map.Add( key, (double)(float)value);
			}
			else if( value is int)
			{
				_map.Add( key, (double)(int)value);
			}
			else if( value is long)
			{
				_map.Add(key, (double)(long)(value));
			}
			else if( value is double)
			{
				_map.Add( key, value);
			}
			else if( value is string)
			{
				_map.Add( key, value);
			}
			else if( value is bool)
			{
				_map.Add( key, value);
			}
			else
			{
				throw new System.Exception("not supported value : " + value.GetType().Name);
			}
		}

		public string encode()
		{
			//return Facebook.MiniJSON.Json.Serialize( _map);
			return MiniJson.JsonEncode(_map);
		}
	}
}