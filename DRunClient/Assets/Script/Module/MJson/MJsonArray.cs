using System.Collections.Generic;

namespace Festa.Client.Module
{
	public class MJsonArray
	{
		private IList<object> _list;

		public MJsonArray()
		{
			_list = new List<object>();
		}

		public MJsonArray(IList<object> list)
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

		public MJsonArray getArray(int index)
		{
			if (index < 0 || index >= _list.Count)
			{
				throw new System.Exception("index out of range");
			}

			object obj = _list[index];

			if( obj is IList<object>)
			{
				return new MJsonArray(obj as IList<object>);
			}
			else
			{
				return null;
			}
		}

		public MJsonObject getObject(int index)
		{
			if (index < 0 || index >= _list.Count)
			{
				throw new System.Exception("index out of range");
			}

			object obj = _list[index];
			if( obj is IDictionary<int,object>)
			{
				return new MJsonObject(obj as IDictionary<int, object>);
			}
			else
			{
				return null;
			}
		}

		public long getNumber(int index)
		{
			if (index < 0 || index >= _list.Count)
			{
				throw new System.Exception("index out of range");
			}

			object value = _list[index];
			if( value is long)
			{
				return (long)value;
			}
			else if( value is int)
			{
				return (long)((int)value);
			}

			throw new System.Exception("getNumber fail : unknown type - " + value.GetType().Name);
		}

		public string getString(int index)
		{
			if (index < 0 || index >= _list.Count)
			{
				throw new System.Exception("index out of range");
			}

			return (string)_list[index];
		}
	}
}