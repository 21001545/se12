using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class LandTileCache
	{
		private Dictionary<string, LandTile> _cache;

		public static LandTileCache create()
		{
			LandTileCache cache = new LandTileCache();
			cache.init();
			return cache;
		}

		private void init()
		{
			_cache = new Dictionary<string, LandTile>();
		}

		public LandTile get(string key)
		{
			lock(_cache)
			{
				LandTile tile;
				if( _cache.TryGetValue(key, out tile) == false)
				{
					return null;
				}

				return tile;
			}
		}

		public void put(string key,LandTile tile)
		{
			lock(_cache)
			{
				if( _cache.ContainsKey(key))
				{
					Debug.LogWarning($"landtile cache already exists : {key}");
					return;
				}

				_cache.Add(key, tile);
			}
		}
	}
}
