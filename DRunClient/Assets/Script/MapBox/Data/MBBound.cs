using UnityEngine;
using System;

namespace Festa.Client.MapBox
{
	public class MBBound
	{
		private Vector2Int _min;
		private Vector2Int _max;
		private Vector2Int _center;
		private Vector2Int _extent;

		public Vector2Int min => _min;
		public Vector2Int max => _max;
		public Vector2Int center => _center;
		public Vector2Int extent => _extent;

		public void setMinMax(Vector2Int min,Vector2Int max)
		{
			_min = min;
			_max = max;
			_center = (_max + _min) / 2;
			_extent = (_max - _min) / 2;
		}

		public void merge(MBBound bound)
		{
			_min.x = Math.Min(_min.x, bound.min.x);
			_min.y = Math.Min(_min.y, bound.min.y);
			_max.x = Math.Max(_max.x, bound.max.x);
			_max.y = Math.Max(_max.y, bound.max.y);
		}

		public static MBBound fromMinMax(Vector2Int min, Vector2Int max)
		{
			MBBound m = new MBBound();
			m.setMinMax(min,max);
			return m;
		}

		public static MBBound fromBound(MBBound bound)
		{
			return fromMinMax(bound.min, bound.max);
		}

		public static MBBound fromBoundWithOffset(MBBound bound,Vector2Int offset)
		{
			MBBound nb = new MBBound();
			nb._min = bound._min + offset;
			nb._max = bound._max + offset;
			nb._center = (nb._min + nb._max) / 2;
			nb._extent = (nb._max - nb._min) / 2;
			return nb;

		}

		public static bool checkOverlap(MBBound a,MBBound b)
		{
			Vector2Int diff_center = a._center - b._center;
			Vector2Int sum_extent = a._extent + b._extent;
			diff_center.x = System.Math.Abs(diff_center.x);
			diff_center.y = System.Math.Abs(diff_center.y);

			if( diff_center.x > sum_extent.x)
			{
				return false;
			}

			if( diff_center.y > sum_extent.y)
			{
				return false;
			}

			return true;
		}

		public static bool checkOverlapWithOffset(MBBound a,MBBound b,Vector2Int offset)
		{
			Vector2Int diff_center = a._center - (b._center + offset);
			Vector2Int sum_extent = a._extent + b._extent;
			diff_center.x = System.Math.Abs(diff_center.x);
			diff_center.y = System.Math.Abs(diff_center.y);

			if (diff_center.x > sum_extent.x)
			{
				return false;
			}

			if (diff_center.y > sum_extent.y)
			{
				return false;
			}

			return true;
		}
	}
}
