using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Festa.Client.Module
{
	public class GameObjectCache : SingletonBehaviourT<GameObjectCache>
	{
		public class Cache
		{
			public int				type;
			public Transform		root;
			public List<ReusableMonoBehaviour>	free_list;
		}

		private Dictionary<int, Cache> _source_cache_map;
		private Dictionary<int, Cache> _instance_cache_map;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_source_cache_map = new Dictionary<int, Cache>();
			_instance_cache_map = new Dictionary<int, Cache>();

			gameObject.SetActive(false);
		}

		private Cache prepareCache(ReusableMonoBehaviour source,int type)
		{
			int source_id = source.GetInstanceID();
			Cache cache = null;
			if( _source_cache_map.TryGetValue( source_id, out cache) == false)
			{
				GameObject new_go = new GameObject(source.GetType().Name);
				new_go.SetActive(false);

				cache = new Cache();
				cache.type = type;
				cache.root = new_go.transform;
				cache.root.SetParent(transform, false);
				cache.free_list = new List<ReusableMonoBehaviour>();

				_source_cache_map.Add(source_id, cache);
			}

			return cache;
		}

		private ReusableMonoBehaviour makeBare(ReusableMonoBehaviour source,Transform parent,int type)
		{
			Cache cache = prepareCache(source, type);

			ReusableMonoBehaviour new_item = null;

			if( cache.free_list.Count <= 0)
			{
				GameObject new_one = (GameObject)Instantiate(source.gameObject,parent,false);

				new_item = new_one.GetComponent<ReusableMonoBehaviour>();
				new_one.SetActive(true);

				_instance_cache_map.Add(new_item.GetInstanceID(), cache);

				new_item.setInCache(false);

				new_item.onCreated(source);
			}
			else
			{
				int free_index = cache.free_list.Count - 1;
				new_item = cache.free_list[ free_index];

				new_item.transform.SetParent(parent, false);
				new_item.gameObject.SetActive(true);

				cache.free_list.RemoveAt(free_index);

				new_item.setInCache(false);

				new_item.onReused();
			}

			return new_item;
		}

		public T make<T>(T source,Transform parent, int type) where T : ReusableMonoBehaviour
		{
			return (T)makeBare(source, parent, type);
		}

		public void delete<T>(List<T> list) where T : ReusableMonoBehaviour
		{
			foreach(T item in list)
			{
				delete(item);
			}

			list.Clear();
		}

		public void delete(ReusableMonoBehaviour item)
		{
			// 삭제 되면 그럴 수 있음
			if(item == null)
			{
				return;
			}

			Cache cache = null;
			if( _instance_cache_map.TryGetValue(item.GetInstanceID(), out cache) == false)
			{
#if UNITY_EDITOR
				Debug.LogError("can't find cache", item.gameObject);	
#endif
				GameObject.Destroy(item.gameObject);
				return;
			}

			// active를 해제 해놓고 부모를 옮긴다 (최적화에 유리하단다)
			item.onDelete();

			item.gameObject.SetActive(false);
			item.transform.SetParent(cache.root, false);

			if( item.isInCache() == false)
			{
				item.setInCache(true);
				cache.free_list.Add(item);
			}
#if UNITY_EDITOR
			else
			{
				Debug.LogError("object already deleted", item.gameObject);
			}
#endif
		}

	}
}
