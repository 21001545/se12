using System.Collections.Generic;

namespace Festa.Client.Module
{
	public class MJsonObject
	{
		private IDictionary<int, object> _map;

		public MJsonObject()
		{
			_map = new Dictionary<int, object>();
		}

		public MJsonObject(IDictionary<int,object> map)
		{
			_map = map;
		}

		public IDictionary<int,object> getMap()
		{
			return _map;
		}

		public bool contains(int key)
		{
			return _map.ContainsKey(key);
		}

		public MJsonObject getObject(int key)
		{
			object obj;
			if( _map.TryGetValue( key, out obj) == false)
			{
				return null;
			}

			if( obj is IDictionary<int,object>)
			{
				return new MJsonObject(obj as IDictionary<int, object>);
			}
			else
			{
				return null;
			}
		}

		public MJsonArray getArray(int key)
		{
			object obj;
			if( _map.TryGetValue( key, out obj) == false)
			{
				return null;
			}

			if( obj is IList<object>)
			{
				return new MJsonArray(obj as IList<object>);
			}
			else
			{
				return null;
			}
		}

		public long getNumber(int key)
		{
			object obj;
			if (_map.TryGetValue(key, out obj) == false)
			{
				throw new System.Exception("doesn't contains key: " + key);
			}

			if( obj is long)
			{
				return (long)obj;
			}
			else if( obj is int)
			{
				return (long)((int)obj);
			}

			throw new System.Exception("getNumber fail : unknown type - " + obj.GetType().Name);
		}

		public string getString(int key)
		{
			object obj;
			if (_map.TryGetValue(key, out obj) == false)
			{
				throw new System.Exception("doesn't contains key: " + key);
			}

			if (obj is string)
			{
				return (string)obj;
			}

			throw new System.Exception("getNumber fail : unknown type - " + obj.GetType().Name);
		}
	}
}