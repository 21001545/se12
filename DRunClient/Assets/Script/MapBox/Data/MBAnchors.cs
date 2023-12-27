using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBAnchors
	{
		private MBFeature _feature;
		private List<MBAnchor> _list;
		private float _length;
		private MBTileCoordinate _tilePos;
		private bool _isContinueLine;

		public List<MBAnchor> getAnchors()
		{
			return _list;
		}

		public float getLength()
		{
			return _length;
		}

		public MBFeature getFeature()
		{
			return _feature;
		}

		public bool isContinueLine()
		{
			return _isContinueLine;
		}

		public static MBAnchors create(MBFeature feature,List<Vector2Int> line)
		{
			MBAnchors anchors = new MBAnchors();
			anchors.init(feature, line);
			return anchors;
		}

		public static MBAnchors create(List<ClipperLib.IntPoint> path)
		{
			MBAnchors anchors = new MBAnchors();
			anchors.init(path);
			return anchors;
		}

		private void init(MBFeature feature,List<Vector2Int> line)
		{
			_feature = feature;
			_tilePos = _feature._tile._tilePos;

			_length = 0;
			_list = new List<MBAnchor>();

			int count = line.Count;
			
			for(int i = 0; i < line.Count - 1; ++i)
			{
				Vector2Int pt = line[i];
				Vector2Int diff = line[i + 1] - pt;

				float length = diff.magnitude;
				float angle = Mathf.Atan2(diff.y, diff.x);

				MBAnchor anchor = MBAnchor.create(pt, new Vector2( diff.x / length, diff.y / length), angle, length, _length);
				_list.Add(anchor);
				_length += length;
			}

			makeLinks();

			_isContinueLine = isOutsideBound(line[0].x, line[0].y) || isOutsideBound(line[count-1].x, line[count-1].y);
		}

		private static bool isOutsideBound(int x,int y)
		{
			return x <= 0 || y <= 0 || x >= MapBoxDefine.tile_extent || y >= MapBoxDefine.tile_extent;
		}

		// closed polygon이라서 마지막 원점도 포함시킨다 (linestring하고 다름)
		private void init(List<ClipperLib.IntPoint> path)
		{
			_feature = null;
			_tilePos = MBTileCoordinate.fromLonLat(0, 0, 0);

			_length = 0;
			_list = new List<MBAnchor>();

			int count = path.Count;
			for (int i = 0; i < count; ++i)
			{
				int bx, by, ex, ey;

				if( i < count - 1)
				{
					bx = (int)path[i].X;
					by = (int)path[i].Y;
					ex = (int)path[i + 1].X;
					ey = (int)path[i + 1].Y;
				}
				else
				{
					bx = (int)path[i].X;
					by = (int)path[i].Y;
					ex = (int)path[0].X;
					ey = (int)path[0].Y;
				}

				int dx = ex - bx;
				int dy = ey - by;

				float length = (float)System.Math.Sqrt(dx * dx + dy * dy);
				float angle = (float)System.Math.Atan2(dy, dx);

				MBAnchor anchor = MBAnchor.create(new Vector2Int(bx, by), (new Vector2(dx, dy)).normalized, angle, length, _length);
				_list.Add(anchor);
				_length += length;
			}

			makeLinks();
		}

		private void makeLinks()
		{
			for (int i = 0; i < _list.Count; ++i)
			{
				MBAnchor anchor = _list[i];
				MBAnchor prev = null;
				MBAnchor next = null;

				if (i > 0)
				{
					prev = _list[i - 1];
				}

				if (i < _list.Count - 1)
				{
					next = _list[i + 1];
				}

				anchor.setLink(prev, next);
			}
		}

		public MBAnchorCursor getCursor(float offset)
		{
			//그럴 수 있다
			if( offset < 0)
			{
				MBAnchor first_anchor = _list[0];

				Vector2 position = first_anchor.getPosition();
				float angle = first_anchor.getAngle();

				position += first_anchor.getDirection() * offset;

				return MBAnchorCursor.create(this, first_anchor, offset, position, angle);
			}
			// 그럴 수 있다
			else if ( offset >= _length)
			{
				MBAnchor end_anchor = _list[_list.Count - 1];

				Vector2 position = end_anchor.getPosition();
				float angle = end_anchor.getAngle();

				position += end_anchor.getDirection() * offset;

				return MBAnchorCursor.create(this, end_anchor, offset, position, angle);
			}
			else
			{
				float remain_offset = offset;

				for(int i = 0; i < _list.Count; ++i)
				{
					MBAnchor anchor = _list[i];
					if( remain_offset > anchor.getLength())
					{
						remain_offset -= anchor.getLength();
					}
					else
					{
						Vector2 position = anchor.getPosition() + anchor.getDirection() * remain_offset;
						float angle = anchor.getAngle();

						return MBAnchorCursor.create(this, anchor, offset, position, angle);
					}
				}
			}

			return null;
		}
	}
}
