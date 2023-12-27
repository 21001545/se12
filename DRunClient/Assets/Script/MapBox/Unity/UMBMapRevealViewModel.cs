using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.MapBox
{
	public class UMBMapRevealViewModel : AbstractViewModel
	{
		protected Dictionary<MBTileCoordinate, MapTileRevealData> _revealData;

		public Dictionary<MBTileCoordinate, MapTileRevealData> RevealData => _revealData;
		public string RevealData_Adjacency => "";	// 

		protected override void init()
		{
			base.init();
			_revealData = new Dictionary<MBTileCoordinate, MapTileRevealData>();
		}

		public MapTileRevealData get(MBTileCoordinate tilePos)
		{
			MapTileRevealData data;
			if (_revealData.TryGetValue(tilePos, out data) == false)
			{
				return null;
			}

			return data;
		}

		public void put(MBTileCoordinate tilePos, MapTileRevealData data)
		{
			if (_revealData.ContainsKey(tilePos) == true)
			{
				throw new Exception($"revealdata already exsits: tile_x[{tilePos.tile_x}] tile_y[{tilePos.tile_y}]");
			}

			_revealData.Add(tilePos, data);
		}

		public void notifyUpdate()
		{
			notifyPropetyChanged("RevealData");
			notifyPropetyChanged("RevealData_Adjacency");
		}
	}
}
