using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public class MBStyleCache
	{
		private Dictionary<string, MBStyle> _cache;

		public static MBStyleCache create()
		{
			MBStyleCache cache = new MBStyleCache();
			cache.init();
			return cache;
		}

		private void init()
		{
			_cache = new Dictionary<string,MBStyle>();
		}

		public MBStyle get(string style_id)
		{
			lock(_cache)
			{
				MBStyle style;
				if(_cache.TryGetValue(style_id, out style) == false)
				{
					return null;
				}

				return style;
			}
		}

		public void put(string style_id,MBStyle style)
		{
			lock(_cache)
			{
				_cache.Add(style_id, style);
			}
		}
	}
}
