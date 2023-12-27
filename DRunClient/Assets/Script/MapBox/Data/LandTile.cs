using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class LandTile : CustomSerializer
	{
		public List<LandTileFeature> featureList;
		public Dictionary<int, List<LandTileFeature>> featureGrid;

		[SerializeOption(SerializeOption.NONE)]
		public MBTileCoordinate _tilePos;

		[SerializeOption(SerializeOption.NONE)]
		public bool _meshBuilt = false;

		[SerializeOption(SerializeOption.NONE)]
		public Dictionary<int, List<LandTileEdge>> _edgeMap;

		// key: 이웃 타일의 feature
		// value: 내 타일의 features 
        [SerializeOption(SerializeOption.NONE)]
        public MultiDictionary<LandTileFeature, LandTileFeature>[] _edgeFacingFeatureCache; 

		public void pack(MessagePacker packer)
		{
			throw new NotImplementedException();
		}

		public void unpack(MessageUnpacker unpacker)
		{
			featureList = new List<LandTileFeature>();
			featureGrid = new Dictionary<int, List<LandTileFeature>>();

			int feature_count = unpacker.unpackArrayHeader();
			for(int i = 0; i < feature_count; ++i)
			{
				LandTileFeature feature = new LandTileFeature();
				feature.unpack(unpacker);
				featureList.Add(feature);
			}

			int grid_count = unpacker.unpackMapHeader();
			for(int i = 0; i < grid_count; ++i)
			{
				int key = unpacker.unpackInt();
				short[] value_array = LandTileFeature.unpackShortArray(unpacker);

				List<LandTileFeature> value_list = new List<LandTileFeature>();
				for(int j = 0; j < value_array.Length; ++j)
				{
					int index = value_array[j];
					value_list.Add(featureList[index]);
				}

				featureGrid.Add(key, value_list);
			}
		}

		public void postProcess()
		{
			_edgeMap = new Dictionary<int, List<LandTileEdge>>();
			_edgeFacingFeatureCache = new MultiDictionary<LandTileFeature, LandTileFeature>[4];

			for (int i = 0; i < featureList.Count; ++i)
			{
				LandTileFeature feature = featureList[i];
				feature._id = i;
				feature._tile = this;

				//buildFeatureGrid(feature);
				addTileEdge(feature);
			}

//#if UNITY_EDITOR
//			foreach (KeyValuePair<int, List<LandTileFeature>> item in _featureGrid)
//			{
//				int grid_x = item.Key / 100;
//				int grid_y = item.Key % 100;

//				Debug.Log($"grid_x[{grid_x}] grid_y[{grid_y}] feature[{item.Value.Count}]");
//			}
//#endif
		}

		private void addTileEdge(LandTileFeature feature)
		{
            foreach (LandTileEdge edge in feature._tileEdgeList)
            {
                List<LandTileEdge> edgeList;
                if(_edgeMap.TryGetValue( edge.getType(), out edgeList) == false)
                {
                    edgeList = new List<LandTileEdge>();
                    _edgeMap.Add(edge.getType(), edgeList);
                }

                edgeList.Add(edge);
            }
        }

        public List<LandTileEdge> getEdgeList(int edge_type)
        {
            List<LandTileEdge> edge_list;
            if( _edgeMap.TryGetValue(edge_type, out edge_list))
            {
                return edge_list;
            }

            return null;
        }

		public MultiDictionary<LandTileFeature,LandTileFeature> getEdgeFacingFeature(int edge_type,LandTile to)
		{
			MultiDictionary<LandTileFeature, LandTileFeature> map = _edgeFacingFeatureCache[edge_type];
			if( map == null)
			{
				map = buildEdgeFacingFeature(edge_type, to);
				_edgeFacingFeatureCache[edge_type] = map;
			}

			return map;
		}

        private MultiDictionary<LandTileFeature,LandTileFeature> buildEdgeFacingFeature(int edge_type,LandTile to)
        {
            MultiDictionary<LandTileFeature, LandTileFeature> map = new MultiDictionary<LandTileFeature, LandTileFeature>();

			int to_edge_type = LandTileEdge.EdgeType.adjaceny_type[edge_type];
			List<LandTileEdge> from_list = getEdgeList(edge_type);
			List<LandTileEdge> to_list = to.getEdgeList(to_edge_type);

			if( from_list == null || to_list == null)
			{
				return map;
			}

			foreach(LandTileEdge toEdge in to_list)
			{
				foreach(LandTileEdge fromEdge in from_list)
				{
					if( LandTileEdge.overlap( toEdge, fromEdge))
					{
						map.put(toEdge.getFeature(), fromEdge.getFeature());
					}
				}
			}

			return map;
		}

		//		private void buildFeatureGrid(LandTileFeature feature)
		//		{
		//			Vector2Int begin;
		//			Vector2Int end;

		//			begin = feature._bound.min / MapBoxDefine.landTileGridSize;
		//			end = feature._bound.max / MapBoxDefine.landTileGridSize;

		//			for(int x = begin.x; x <= end.x; ++x)
		//			{
		//				for(int y = begin.y; y <= end.y; ++y)
		//				{
		//					buildFeatureGrid(feature, x, y);
		//				}
		//			}
		//		}

		//		private void buildFeatureGrid(LandTileFeature feature,int grid_x,int grid_y)
		//		{
		//			List<Vector2> vertList = new List<Vector2>();

		//			PolygonShape gridShape = new PolygonShape();
		//			makeGridVertList(grid_x, grid_y, vertList);
		//			gridShape.setPolygon(vertList);

		//			PolygonShape triShape = new PolygonShape();

		//			//
		//			for(int i = 0; i < feature.indexList.Length / 3; ++i)
		//			{
		//				vertList.Clear();

		//				for(int p = 0; p < 3; ++p)
		//				{
		//					int v_index = feature.indexList[i * 3 + p];
		//					int x = feature.vertexList[v_index * 2 + 0];
		//					int y = feature.vertexList[v_index * 2 + 1];

		//					vertList.Add(new Vector2(x, y));
		//				}

		//				triShape.setPolygon(vertList);

		//				if( CollisionTest.polygonToPolygon(triShape, gridShape))
		//				{
		//					addFeatureToGrid(feature, grid_x, grid_y);
		//					break;
		//				}
		//			}
		//		}

		//		private void addFeatureToGrid(LandTileFeature feature,int grid_x,int grid_y)
		//		{
		//			List<LandTileFeature> list;
		//			int grid_key = MapBoxDefine.makeLandTileGridKey(grid_x, grid_y);
		//			if( _featureGrid.TryGetValue( grid_key, out list) == false)
		//			{
		//				list = new List<LandTileFeature> ();
		//				_featureGrid.Add( grid_key, list );
		//			}

		//#if UNITY_EDITOR
		//			if( list.Contains(feature))
		//			{
		//				Debug.Log($"feature already exists in grid: grid_x[{grid_x}] grid_y[{grid_y}]");
		//			}
		//#endif

		//			list.Add(feature);
		//		}

		//		private void makeGridVertList(int grid_x, int grid_y, List<Vector2> vertList)
		//		{
		//			vertList.Clear();

		//			Vector2 min = new Vector2(grid_x * MapBoxDefine.landTileGridSize, grid_y * MapBoxDefine.landTileGridSize);
		//			Vector2 max = new Vector2((grid_x + 1) * MapBoxDefine.landTileGridSize, (grid_y + 1) * MapBoxDefine.landTileGridSize);

		//			vertList.Add(min);
		//			vertList.Add(new Vector2(max.x, min.y));
		//			vertList.Add(max);
		//			vertList.Add(new Vector2(min.x, max.y));
		//		}
	}
}
