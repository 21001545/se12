//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace Festa.Client.MapBox
//{
//	public class MBPathLineMeshBuilder : MBLineStringMeshBuilder
//	{
//		private List<short> _pathList;
//		protected override void init()
//		{
//			base.init();
//			_pathList = new List<short>();
//		}

//		protected override void reset()
//		{
//			base.reset();
//			_pathList.Clear();
//		}

//		public override void build(MBFeature feature, Color color,int offset_x = 0,int offset_y = 0)
//		{
//			throw new NotImplementedException("not implemented");
//		}

//		public void build(List<MBLongLatCoordinate> pathList,int zoom,Color color)
//		{
//			reset();

//			// 기준점
//			MBTileCoordinateDouble center_tilepos = MBTileCoordinateDouble.fromLonLat(pathList[0], zoom);

//			for(int i = 0; i < pathList.Count; ++i)
//			{
//				MBTileCoordinateDouble tile_pos = MBTileCoordinateDouble.fromLonLat(pathList[i], zoom);

//				double offset_x = tile_pos.tile_x - center_tilepos.tile_x;
//				double offset_y = tile_pos.tile_y - center_tilepos.tile_y;

//				offset_x *= 4096;
//				offset_y *= 4096;

//				_pathList.Add((short)offset_x);
//				_pathList.Add((short)offset_y);
//			}

//			buildLineMesh(_pathList.ToArray(), 10.0f, true, false);

//			makeColors(color);
//		}

//	}
//}
