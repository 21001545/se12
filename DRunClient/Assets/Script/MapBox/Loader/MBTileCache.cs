using System.Collections.Generic;

namespace Festa.Client.MapBox
{
	public class MBTileCache
	{
		private Dictionary<string, MBTile> _cache;

		public static MBTileCache create()
		{
			MBTileCache cache = new MBTileCache();
			cache.init();
			return cache;
		}

		private void init()
		{
			_cache = new Dictionary<string, MBTile>();
		}

		public MBTile get(string key)
		{
			lock(_cache)
			{
				MBTile tile;
				if (_cache.TryGetValue(key, out tile) == false)
				{
					return null;
				}

				return tile;
			}
		}

		public void put(string key,MBTile tile)
		{
			lock(_cache)
			{
				_cache.Add(key, tile);
			}
		}
	}
}
