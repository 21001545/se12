using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class TextureCacheItem
	{
		private int _key;
		private Texture _texture;
		private float _remainTime;
		private int _refCount;

		public int getKey()
		{
			return _key;
		}

		public Texture getTexture()
		{
			return _texture;
		}

		public bool processExpire()
		{
			if( _refCount > 0)
			{
				return false;
			}

			_remainTime -= 1.0f;
			return _remainTime <= 0.0f;
		}

		public void delete()
		{
			UnityEngine.Object.Destroy(_texture);
		}

		public void incRefCount()
		{
			_refCount++;

			//Debug.Log($"incRefCount: key:{_key} ref_count:{_refCount}");
		}

		public void decRefCount()
		{
			if( _refCount < 0)
			{
				Debug.LogWarning("refcount is zero");
			}
			else
			{
				_refCount--;
				if( _refCount == 0)
				{
					_remainTime = 30.0f;
				}

				//Debug.Log($"decRefCount: key:{_key} ref_count:{_refCount}");
			}
		}

		public static TextureCacheItem create(int key,Texture texture)
		{
			TextureCacheItem item = new TextureCacheItem();
			item._key = key;
			item._texture = texture;

			return item;
		}
	}
}
