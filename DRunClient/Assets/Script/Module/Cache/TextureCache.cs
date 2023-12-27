using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client.Module
{
	public class TextureCache
	{
		private Dictionary<int, TextureCacheItem> _cacheMap;
		private Queue<TextureCacheItem> _deleteQueue;
		private List<TextureCacheItem> _tempList;
		private IntervalTimer _timer;

		public static TextureCache create()
		{
			TextureCache cache = new TextureCache();
			cache.init();
			return cache;
		}

		private void init()
		{
			_cacheMap = new Dictionary<int, TextureCacheItem>();
			_deleteQueue = new Queue<TextureCacheItem>();
			_tempList = new List<TextureCacheItem>();
			_timer = IntervalTimer.create(1.0f, true, false);
		}

		public void update()
		{
			if( _timer.update())
			{
				processExpire();
			}

			deleteExpired();
		}

		// cache에서 texture를 얻어낸다 (RefCount 증가)
		public TextureCacheItemUsage makeUsage(string key)
		{
			return makeUsage(EncryptUtil.makeHashCode(key));
		}

		public TextureCacheItemUsage makeUsage(int key)
		{
			TextureCacheItem item;
			if (_cacheMap.TryGetValue(key, out item))
			{
				item.incRefCount();
				return new TextureCacheItemUsage(item);
			}
			else
			{
				return null;
			}
		}

		public void deleteUsage(TextureCacheItemUsage usage)
		{
			TextureCacheItem item;
			if( _cacheMap.TryGetValue(usage.key, out item) == false)
			{
				Debug.LogWarning($"can't find texture cache: key:{usage.key}");
				return;
			}

			item.decRefCount();
		}

		public bool contains(int key)
		{
			return _cacheMap.ContainsKey(key);
		}

		public bool registerCache(int key,Texture texture)
		{
			if( _cacheMap.ContainsKey( key))
			{
				Debug.LogWarning($"already in cache: key:{key}");
				return false;
			}

			TextureCacheItem item = TextureCacheItem.create(key, texture);
			_cacheMap.Add(key, item);
			return true;
		}

		private void processExpire()
		{
			foreach(KeyValuePair<int,TextureCacheItem> item in _cacheMap)
			{
				TextureCacheItem value = item.Value;
				
				if( value.processExpire())
				{
					_tempList.Add(value);
				}
			}

			if (_tempList.Count > 0)
			{
				foreach (TextureCacheItem item in _tempList)
				{
					_cacheMap.Remove(item.getKey());
					_deleteQueue.Enqueue(item);
				}
				_tempList.Clear();
			}
		}

		private void deleteExpired()
		{
			if( _deleteQueue.Count > 0)
			{
				TextureCacheItem item = _deleteQueue.Dequeue();
				Debug.Log($"delete texture cache key:{item.getKey()} name:{item.getTexture().name}");
				item.delete();
			}
		}
	}
}
