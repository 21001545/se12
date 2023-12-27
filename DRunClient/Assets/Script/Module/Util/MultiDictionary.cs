using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module
{
	public class MultiDictionary<K,V>
	{
		private Dictionary<K, List<V>> _dic = new Dictionary<K,List<V>>();

		// 나중에 중복 체크도 넣어보자
		public void put(K key,V value)
		{
			List<V> list;
			if(_dic.TryGetValue( key, out list) == false)
			{
				list = new List<V>();
				_dic.Add( key, list );
			}

			list.Add(value);
		}

		public void putReplace(K key,V value)
		{
			List<V> list;
			if (_dic.TryGetValue(key, out list) == false)
			{
				list = new List<V>();
				_dic.Add(key, list);
			}

			if( list.Contains(value))
			{
				list.Remove(value);
			}

			list.Add(value);
		}

		public void remove(K key,V value)
		{
			List<V> list;
			if (_dic.TryGetValue(key, out list) == false)
			{
				return;
			}

			list.Remove(value);
		}

		public void remove(K key)
		{
			_dic.Remove(key);
		}

		public List<V> get(K key)
		{
			List<V> list;
			if( _dic.TryGetValue(key, out list) == false)
			{
				return null;
			}

			return list;
		}
	}
}
