using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class ClientMapRevealManager
	{
		private IntervalTimer _flushTimer;
		private HashSet<MBTileCoordinate> _updateTileList;

		private MapRevealViewModel ViewModel => ClientMain.instance.getViewModel().MapReveal;

		public static ClientMapRevealManager create()
		{
			ClientMapRevealManager manager = new ClientMapRevealManager();
			manager.init();
			return manager;
		}

		private void init()
		{
			_flushTimer = IntervalTimer.create(5.0f, false, true);
			_updateTileList = new HashSet<MBTileCoordinate>();
		}

		public void update()
		{
			if( _flushTimer.update())
			{
				
			}
		}

		public void reveal(List<ClientLocationLog> log_list)
		{
			foreach(ClientLocationLog log in log_list)
			{
				reveal(log);
			}

			if( _updateTileList.Count > 0)
			{
				ViewModel.notifyUpdate();
				_updateTileList.Clear();
			}
		}

		public void reveal(ClientLocationLog log)
		{
			// zoom[16]단위 tile position으로 변환
			
			MBTileCoordinateDouble tilePosDouble= MBTileCoordinateDouble.fromLonLat(log.longitude, log.latitude, MapBoxDefine.landTileGridZoom);
			MBTileCoordinate tilePos = tilePosDouble.toInteger();

			MapTileRevealData data = ViewModel.get(tilePos);
			if( data == null)
			{
				data = MapTileRevealData.create();
				ViewModel.put(tilePos, data);
			}

			//
			int grid_x = (int)System.Math.Floor((tilePosDouble.tile_x % 1.0) * MapBoxDefine.landTileGridCount);
			int grid_y = (int)System.Math.Floor((tilePosDouble.tile_y % 1.0) * MapBoxDefine.landTileGridCount);
			
			int key = MapBoxDefine.makeLandTileGridKey(grid_x, grid_y);

			if( data.set(key) && _updateTileList.Contains(tilePos) == false)
			{
				//Debug.Log($"reveal map: tile_x[{tilePos.tile_x}] tile_y[{tilePos.tile_y}] grid_x[{grid_x}] grid_y[{grid_y}]");

				_updateTileList.Add(tilePos);
			}
		}
	}
}
