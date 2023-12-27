using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ClipperLib;

namespace Festa.Client.MapBox
{
	public class LandTileFeature : CustomSerializer
	{
		public int area;
		public short[] vertexList;
		public short[] indexList;

		[SerializeOption(SerializeOption.NONE)]
		public int _id;

		[SerializeOption(SerializeOption.NONE)]
		public LandTile _tile;

		[SerializeOption(SerializeOption.NONE)]
		public MBMesh _mesh;

		[SerializeOption(SerializeOption.NONE)]
		public MBBound _bound;

		[SerializeOption(SerializeOption.NONE)]
		public List<List<IntPoint>> _outlinePaths;

		[SerializeOption(SerializeOption.NONE)]
		public List<MBTriangle> _triangleList;

		[SerializeOption(SerializeOption.NONE)]
		public List<LandTileEdge> _tileEdgeList;

        public void pack(MessagePacker packer)
		{
			throw new NotImplementedException();
		}

		public void unpack(MessageUnpacker unpacker)
		{
			area = unpacker.unpackInt();
			vertexList = unpackShortArray(unpacker);
			indexList = unpackShortArray(unpacker);

			buildOutlinePaths();
			buildBound();
			buildTriangleList();
			buildTileEdgeList();
		}

		public static short[] unpackShortArray(MessageUnpacker unpacker)
		{
			int length = unpacker.unpackBinaryHeader();
			byte[] bytes = unpacker.readPayload(length);

			short[] short_array = new short[length / 2];
			MBFeature.toShortArray(bytes, short_array, length / 2);
			return short_array;
		}
		
		public bool contains(Vector2 pos)
		{
			foreach(MBTriangle tri in _triangleList)
			{
				if( tri.contains(pos))
				{
					return true;
				}
			}
			return false;
		}

		private void buildBound()
		{
			Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
			Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

			for (int i = 0; i < vertexList.Length / 2; ++i)
			{
				int x = vertexList[i * 2 + 0];
				int y = vertexList[i * 2 + 1];

				min.x = Math.Min(min.x, x);
				min.y = Math.Min(min.y, y);
				max.x = Math.Max(max.x, x);
				max.y = Math.Max(max.y, y);
			}

			_bound = MBBound.fromMinMax(min, max);
		}

		private void buildTriangleList()
		{
			if( _outlinePaths == null)
			{
				return;
			}

			_triangleList = new List<MBTriangle>();
			int tri_count = indexList.Length / 3;
			for(int i = 0; i < tri_count; ++i)
			{
				_triangleList.Add(MBTriangle.create(i, vertexList, indexList));
			}
		}

		private void buildOutlinePaths()
		{
			_outlinePaths = null;

			try
			{
				List<List<IntPoint>> paths = new List<List<IntPoint>>();
				for (int i = 0; i < indexList.Length / 3; ++i)
				{
					List<IntPoint> path = new List<IntPoint>();

					for (int j = 0; j < 3; ++j)
					{
						int v_index = indexList[i * 3 + j];
						path.Add(new IntPoint(vertexList[v_index * 2 + 0], vertexList[v_index * 2 + 1]));
					}

					paths.Add(path);
				}

				Clipper clipperUnion = new Clipper();
				clipperUnion.AddPaths(paths, PolyType.ptSubject, true);
				paths.Clear();
				clipperUnion.Execute(ClipType.ctUnion, paths, PolyFillType.pftNonZero);

				ClipperOffset clipperOffset = new ClipperOffset();
				clipperOffset.AddPaths(paths, JoinType.jtSquare, EndType.etClosedPolygon);

				paths.Clear();
				clipperOffset.Execute(ref paths, -80);

//				_outlinePaths = paths;

				foreach (List<IntPoint> path in paths)
				{
					if (path.Count > 2)
					{
						_outlinePaths = paths;
						break;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning(e.ToString());	
			}
		}

		private void buildTileEdgeList()
		{
			_tileEdgeList = new List<LandTileEdge>();

            int tri_count = indexList.Length / 3;
            for (int i = 0; i < tri_count; ++i)
			{
				int i0 = indexList[i * 3 + 0];
				int i1 = indexList[i * 3 + 1];
				int i2 = indexList[i * 3 + 2];
				
				Vector2Int p0 = new Vector2Int(vertexList[i0 * 2 + 0], vertexList[i0 * 2 + 1]);
				Vector2Int p1 = new Vector2Int(vertexList[i1 * 2 + 0], vertexList[i1 * 2 + 1]);
				Vector2Int p2 = new Vector2Int(vertexList[i2 * 2 + 0], vertexList[i2 * 2 + 1]);

				addTileEdge(p0, p1);
				addTileEdge(p1, p2);
				addTileEdge(p2, p0);
			}
        }

		private void addTileEdge(Vector2Int p0,Vector2Int p1)
		{
			LandTileEdge edge = null;
			// check edge
			if( p0.x == 0 && p1.x == 0)
			{
				edge = LandTileEdge.create(this,LandTileEdge.EdgeType.left, p0.y, p1.y);
			}
			else if( p0.x == MapBoxDefine.tile_extent && p1.x == MapBoxDefine.tile_extent)
			{
				edge = LandTileEdge.create(this, LandTileEdge.EdgeType.right, p0.y, p1.y);
			}
			else if( p0.y == 0 && p1.y == 0)
			{
				edge = LandTileEdge.create(this, LandTileEdge.EdgeType.top, p0.x, p1.x);
			}
			else if( p0.y == MapBoxDefine.tile_extent && p1.y == MapBoxDefine.tile_extent)
			{
				edge = LandTileEdge.create(this, LandTileEdge.EdgeType.bottom, p0.x, p1.x);
			}

			if( edge != null)
			{
				_tileEdgeList.Add(edge);
			}
		}
    }
}
