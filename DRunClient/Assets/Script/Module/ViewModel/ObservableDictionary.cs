using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module
{
	public class ObservableDictionary<K,V> : IViewModel where V : class
	{
		private Dictionary<K,V> _dic = new Dictionary<K,V>();
		private List<Binding> _bindingList = new List<Binding>();

		public int size()
		{
			return _dic.Count;
		}

		public V get(K key)
		{
			V value;
			if( _dic.TryGetValue(key, out value) == false)
			{
				return null;
			}

			return value;
		}

		public void put(K key,V value)
		{
			if( _dic.ContainsKey(key))
			{
				_dic.Remove(key);
				_dic.Add(key, value);

				updateBinding(CollectionEventType.update,new KeyValuePair<K,V>(key,value));
			}
			else
			{
				_dic.Add(key, value);
				updateBinding(CollectionEventType.add, new KeyValuePair<K, V>(key, value));
			}
		}

		public void remove(K key)
		{
			updateBinding(CollectionEventType.remove, key);
		}

		public void clear()
		{
			updateBinding(CollectionEventType.clear, null);
		}

		public Dictionary<K,V> getDic()
		{
			return _dic;
		}

		private void updateBinding(int event_type,object item)
		{
			foreach (Binding binding in _bindingList)
			{
				binding.updateCollection(event_type, item);
			}
		}

		public void registerBinding(Binding binding)
		{
			_bindingList.Add(binding);
		}

		public void unregisterBinding(Binding binding)
		{
			_bindingList.Remove(binding);
		}

	}
}
