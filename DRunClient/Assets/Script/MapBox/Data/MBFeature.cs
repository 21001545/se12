using Festa.Client.Module.MsgPack;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBFeatureType
	{
		public const int point = 1;
		public const int linestring = 2;
		public const int polygon = 3;

	}

	public class MBFeature : CustomSerializer
	{
		public long id;
		public int type;
		//public List<int> tagList;
		//public List<List<int>> pathList;
		public short[] tagList;
		public List<short[]> pathList;
		
		//public object[] tagList;
		//public object[][] pathList;

		public void pack(MessagePacker packer)
		{
			throw new NotImplementedException();
		}

		public void unpack(MessageUnpacker unpacker)
		{
			id = unpacker.unpackLong();
			type = unpacker.unpackInt();

			int length;

			length = unpacker.unpackBinaryHeader();
			byte[] bytes_tag = unpacker.readPayload(length);

			tagList = new short[length / 2];
			toShortArray(bytes_tag, tagList, length / 2);
			//Buffer.BlockCopy(bytes_tag, 0, tagList, 0, length);

			int path_count = unpacker.unpackArrayHeader();
			pathList = new List<short[]>();

			for(int i = 0; i < path_count; ++i)
			{
				length = unpacker.unpackBinaryHeader();
				byte[] bytes_path = unpacker.readPayload(length);

				short[] path = new short[length / 2];
				//Buffer.BlockCopy(bytes_path, 0, path, 0, length);
				toShortArray(bytes_path, path, length / 2);

				pathList.Add(path);
			}
		}

		public static void toShortArray(byte[] src,short[] dst,int length)
		{
			for(int i = 0; i < length; ++i)
			{
				dst[i] = (short)((src[i * 2 + 0] << 8) + (src[i * 2 + 1]));
			}
		}

		[SerializeOption(SerializeOption.NONE)]
		public MBLayer _layer;

		[SerializeOption(SerializeOption.NONE)]
		public MBTile _tile;

		[SerializeOption(SerializeOption.NONE)]
		private Dictionary<int, object> _properties;

		[SerializeOption(SerializeOption.NONE)]
		private Dictionary<int, int> _properties_as_hash;

		[SerializeOption(SerializeOption.NONE)]
		private MBMesh _mesh;

		[SerializeOption(SerializeOption.NONE)]
		private MBBound _bound;

		//[SerializeOption(SerializeOption.NONE)]
		//private bool _isOutlinePolygon;

		[SerializeOption(SerializeOption.NONE)]
		private List<int> _meshBuildSlot;

		[SerializeOption(SerializeOption.NONE)]
		private List<MBAnchors> _anchors;

		public Dictionary<int,object> getProperties()
		{
			return _properties;
		}

		//public bool isOutlinePolygon()
		//{
		//	return _isOutlinePolygon;
		//}

		public MBBound getBound()
		{
			return _bound;
		}

		public List<MBAnchors> getAnchors()
		{
			return _anchors;
		}

		public void setMesh(MBMesh mesh)
		{
			_mesh = mesh;
		}

		public MBMesh getMesh()
		{
			return _mesh;
		}

		public int getMeshBuildSlot(int path_id)
		{
			if( _meshBuildSlot == null)
			{
				return 0;
			}

			return _meshBuildSlot[path_id];
		}


		public void buildProperties(MBLayer layer)
		{
			_properties = new Dictionary<int, object>();
			_properties_as_hash = new Dictionary<int, int>();

			for (int i = 0; i < tagList.Length / 2; ++i)
			{
				int key_index = (int)tagList[i * 2 + 0];
				int value_index = (int)tagList[i * 2 + 1];

				int key = layer.keyList[key_index];
				object value = layer.valueList[value_index];
				int hashed_value = layer.hashedValueList[value_index];

				if( _properties.ContainsKey(key) == false)
				{
					_properties.Add(key, value);
				}
				if( _properties_as_hash.ContainsKey(key) == false)
				{
					_properties_as_hash.Add(key, hashed_value);
				}
			}
		}

		//bool MapBoxMeshBuilder::isClockwise(DSClipperLib::Path& path)
		//{
		//	int sum = 0;
		//	for (int i = 0; i < path.size(); ++i)
		//	{
		//		DSClipperLib::IntPoint & pt1 = path[i];
		//		DSClipperLib::IntPoint & pt2 = path[(i + 1) % path.size()];

		//		sum += (pt2.X - pt1.X) * (pt2.Y + pt1.Y);
		//	}

		//	return sum > 0;
		//}

		/*
		int ccw(Point A, Point B, Point C)
		{
			return (B.x - A.x) * (C.y - A.y) - (C.x - A.x) * (B.y - A.y);
		}
		*/

//		static double signedArea(const GeometryCoordinates& ring) {
//		   double sum = 0;

//		for (std::size_t i = 0, len = ring.size(), j = len - 1; i<len; j = i++) {
//        const GeometryCoordinate& p1 = ring[i];
//        const GeometryCoordinate& p2 = ring[j];
//        sum += (p2.x - p1.x) * (p1.y + p2.y);
//    }

//    return sum;
//}
		double signedArea(short[] path)
		{
			int count = path.Length / 2;

			int i = 0;
			int j = count - 1;

			double sum = 0;

			for(i = 0, j = count - 1; i < count; j = i++)
			{
				Vector2 p1 = new Vector2(path[i * 2 + 0], path[i * 2 + 1]);
				Vector2 p2 = new Vector2(path[j * 2 + 0], path[j * 2 + 1]);

				sum += (p2.x - p1.x) * (p1.y + p2.y);
			}

			return sum;
		}

		//bool isCCW(short[] path)
		//{
		//	if( path.Length < 6)
		//	{
		//		return false;
		//	}

		//	Vector2Int a = new Vector2Int(path[0], path[1]);
		//	Vector2Int b = new Vector2Int(path[2], path[3]);
		//	Vector2Int c = new Vector2Int(path[4], path[5]);

		//	return ((b.x - a.x) * (c.y - a.y) - (c.x - a.x) * (b.y - a.y)) > 0;
		//}

		private static List<List<Vector2Int>> _clippedLines = new List<List<Vector2Int>>();

		public List<MBAnchors> buildAnchors()
		{
			_anchors = new List<MBAnchors>();
			foreach(short[] path in pathList)
			{
				MapBoxUtil.clipLines(path, _clippedLines);

				foreach(List<Vector2Int> line in _clippedLines)
				{
					if( line.Count < 2)
					{
						continue;
					}

					MBAnchors anchors = MBAnchors.create(this, line);
					if (anchors.getAnchors().Count == 0)
					{
						continue;
					}
					_anchors.Add(anchors);
				}
			}

			return _anchors;
		}

		public void buildBound()
		{
			if( type == MBFeatureType.polygon || type == MBFeatureType.linestring)
			{
				_meshBuildSlot = new List<int>();

				Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
				Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

				foreach(short[] path in pathList)
				{
					for (int i = 0; i < path.Length / 2; ++i)
					{
						int x = (int)path[i * 2 + 0];
						int y = (int)path[i * 2 + 1];

						min.x = Math.Min(min.x, x);
						min.y = Math.Min(min.y, y);
						max.x = Math.Max(max.x, x);
						max.y = Math.Max(max.y, y);
					}

					double area = signedArea(path);

					//bool ccw = isCCW( path);
					bool ccw = area >= 0;
					if (ccw)
					{
						_meshBuildSlot.Add(0);
					}
					else
					{
						_meshBuildSlot.Add(1);
					}
				}

				_bound = MBBound.fromMinMax(min, max);

				// checkOutline
				//_isOutlinePolygon = checkOutlinePolygon();
				//if( _isOutlinePolygon)
				//{
				//	Debug.Log(string.Format("outline polygon:{0},{1}", _boundMin, _boundMax));
				//}

				//if( _isOutlinePolygon)
				//{
				//	if( _layer._outlinePolygonList == null)
				//	{
				//		_layer._outlinePolygonList = new List<MBFeature>();
				//	}

				//	_layer._outlinePolygonList.Add(this);
				//}
			}
		}

		private static int outlineMarginMin = 1;
		private static int outlineMarginMax = 100 - outlineMarginMin;
		private static Vector2Int outlineBoundMin = new Vector2Int(4096 * outlineMarginMin / 100, 4096 * outlineMarginMin / 100);
		private static Vector2Int outlineBoundMax = new Vector2Int(4096 * outlineMarginMax / 100, 4096 * outlineMarginMax / 100);
		private static MBBound outlineBound = MBBound.fromMinMax(outlineBoundMin, outlineBoundMax);

		private bool checkOutlinePolygon()
		{
			if (_bound.min.x < outlineBound.min.x)
				return true;
			if (_bound.min.y < outlineBound.min.y)
				return true;

			if (_bound.max.x > outlineBound.max.x)
				return true;
			if (_bound.max.y > outlineBound.max.y)
				return true;

			return false;
		}

		public bool checkOutsideTile()
		{
			if(_bound == null)
			{
				buildBound();
			}

			if (_bound.max.x < 0 || _bound.min.x > MapBoxDefine.tile_extent)
				return true;
			if (_bound.max.y < 0 || _bound.min.y > MapBoxDefine.tile_extent)
				return true;

			return false;
		}

		public bool has(int key)
		{
			return _properties.ContainsKey(key);
		}

		public object get(int key)
		{
			return _properties[key];
		}

		public object get(int key,object def)
		{
			object value;
			if( _properties.TryGetValue(key, out value) == false)
			{
				value = def;
			}

			return value;
		}

		public int getHashed(int key)
		{
			int value;
			if( _properties_as_hash.TryGetValue(key,out value) == false)
			{
				return 0;
			}

			return value;
		}

	}

}
