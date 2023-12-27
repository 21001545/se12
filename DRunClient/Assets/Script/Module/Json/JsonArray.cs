using System.Collections;
using System.Collections.Generic;

namespace Festa.Client.Module
{
	public class JsonArray
	{
		private IList<object> _list;

		public JsonArray()
		{
			_list = new List<object>();
		}

		public JsonArray(string encoded)
		{
			//_list = Facebook.MiniJSON.Json.Deserialize( encoded) as IList<object>;
			_list = MiniJson.JsonDecode(encoded) as IList<object>;
		}

		public JsonArray(IList<object> list)
		{
			_list = list;
		}

		public IList<object> getList()
		{
			return _list;
		}

		public int size()
		{
			return _list.Count;
		}

		public int getInteger(int pos)
		{
			if( pos < 0 || pos >= _list.Count)
			{
				throw new System.Exception( "getInteger fail : index out of range");
			}

			object value = _list[ pos];
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

		public float getFloat(int pos)
		{
			if( pos < 0 || pos >= _list.Count)
			{
				throw new System.Exception( "getFloat fail : index out of range");
			}

			object value = _list[ pos];
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

		public double getDouble(int pos)
		{
			if( pos < 0 || pos >= _list.Count)
			{
				throw new System.Exception( "getDouble fail : index out of range");
			}

			object value = _list[ pos];
			if( value is long)
			{
				return (double)(long)value;
			}
			else if( value is double)
			{
				return (double)(double)value;
			}

			throw new System.Exception( "getDouble fail : unknown type - " + value.GetType().Name);
		}

		public string getString(int pos)
		{
			if( pos < 0 || pos >= _list.Count)
			{
				throw new System.Exception( "getDouble fail : index out of range");
			}

			return (string)_list[ pos];
		}

		public JsonObject getJsonObject(int pos)
		{
			if( pos < 0 || pos >= _list.Count)
			{
				throw new System.Exception( "getJsonObject fail : index out of range");
			}

			object obj = _list[ pos];

			if( obj is IDictionary<string,object>)
			{
				return new JsonObject( obj as IDictionary<string,object>);
			}
			else
			{
				return null;
			}
		}

		public JsonArray getJsonArray(int pos)
		{
			if( pos < 0 || pos >= _list.Count)
			{
				throw new System.Exception( "getJsonArray fail : index out of range");
			}

			object obj = _list[ pos];

			if( obj is IList<object>)
			{
				return new JsonArray( obj as IList<object>);
			}
			else
			{
				return null;
			}
		}

		public object getValue(int pos)
		{
			if (pos < 0 || pos >= _list.Count)
			{
				throw new System.Exception("getJsonArray fail : index out of range");
			}

			return _list[pos];
		}

		public void add(object value)
		{
			if( value == null)
			{
				_list.Add(null);
			}
			else if( value is JsonArray)
			{
				_list.Add( ((JsonArray)value).getList());
			}
			else if( value is JsonObject)
			{
				_list.Add( ((JsonObject)value).getMap());
			}
			else if( value is float)
			{
				_list.Add( (double)(float)value);
			}
			else if( value is int)
			{
				_list.Add( (double)(int)value);
			}
			else if( value is double)
			{
				_list.Add( value);
			}
			else if( value is string)
			{
				_list.Add( value);
			}
			else
			{
				throw new System.Exception("not supported value : " + value.GetType().Name);
			}        
		}

		public string encode()
		{
			//return Facebook.MiniJSON.Json.Serialize( _list);
			return MiniJson.JsonEncode(_list);
		}
	}

	
}
