using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public static class LandTileLoadingSyncer
	{
		private static Dictionary<MBTileCoordinate, int> _loadingMap = new Dictionary<MBTileCoordinate, int>();

		public static bool tryLoading(MBTileCoordinate tilepos)
		{
			lock(_loadingMap)
			{
				if( _loadingMap.ContainsKey(tilepos) == false)
				{
					_loadingMap.Add(tilepos, 1);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static void endLoading(MBTileCoordinate tilepos)
		{
			lock(_loadingMap)
			{
				if( _loadingMap.ContainsKey(tilepos))
				{
					_loadingMap.Remove(tilepos);
				}
			}
		}
	}
}
