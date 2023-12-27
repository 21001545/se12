using System.Collections.Generic;

namespace Festa.Client.RefData
{
	public class RefDataContainer
	{
		private Dictionary<System.Type, Dictionary<int, RefData>> _map;
		private RefStringCollection _stringCollection;

		public static RefDataContainer create()
		{
			RefDataContainer container = new RefDataContainer();
			container.init();
			return container;
		}

		private void init()
		{
			_map = new Dictionary<System.Type, Dictionary<int, RefData>>();
		}

		public RefStringCollection getStringCollection()
		{
			return _stringCollection;
		}

		public void buildCustomCollections()
		{
			_stringCollection = RefStringCollection.create(this);
		}

		public T get<T>(int key) where T : RefData
		{
			Dictionary<int, RefData> data_map;

			if( _map.TryGetValue( typeof(T), out data_map) == false)
			{
				return null;
			}

			RefData value;
			if( data_map.TryGetValue(key,out value) == false)
			{
				return null;
			}

			return (T)value;
		}

		public Dictionary<int,RefData> getMap<T>() where T : RefData
		{
			Dictionary<int, RefData> data_map;
			if( _map.TryGetValue( typeof(T), out data_map) == false)
			{
				return null;
			}

			return data_map;
		}

		public void putRefData(System.Type type,Dictionary<object,object> dic)
		{
			if (_map.ContainsKey(type))
			{
				_map.Remove(type);
			}

			Dictionary<int, RefData> ref_data = new Dictionary<int, RefData>();
			foreach(KeyValuePair<object,object> item in dic)
			{
				ref_data.Add((int)item.Key, (RefData)item.Value);
			}

			_map.Add(type, ref_data);
		}
	}
}
