using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBTriangle
	{
		private Vector2[] _points;

		//
		private float _A;
		private float _sign;

		private float _s0;
		private float _s1;
		private float _s2;

		private float _t0;
		private float _t1;
		private float _t2;

		public static MBTriangle create(int tri_id,short[] vertexList,short[] indexList)
		{
			MBTriangle tri = new MBTriangle();
			tri.init(tri_id, vertexList, indexList);
			return tri;
		}

		private void init(int tri_id,short[] vertexList,short[] indexList)
		{
			_points = new Vector2[3];

			for(int i = 0; i < 3; ++i)
			{
				int v_index = indexList[tri_id * 3 + i];
				_points[i] = new Vector2(vertexList[v_index * 2 + 0], vertexList[v_index * 2 + 1]);
			}

			_A = 0.5f * (-_points[1].y * _points[2].x +
							_points[0].y * (-_points[1].x + _points[2].x) +
							_points[0].x * (_points[1].y - _points[2].y) +
							_points[1].x * _points[2].y);
			_sign = _A < 0 ? -1 : 1;

			_s0 = (_points[0].y * _points[2].x - _points[0].x * _points[2].y) * _sign;
			_s1 = (_points[2].y - _points[0].y) * _sign;
			_s2 = (_points[0].x - _points[2].x) * _sign;

			_t0 = (_points[0].x * _points[1].y - _points[0].y * _points[1].x) * _sign;
			_t1 = (_points[0].y - _points[1].y) * _sign;
			_t2 = (_points[1].x - _points[0].x) * _sign;
		}

		public bool contains(Vector2 testPT)
		{
			float s = _s0 + _s1 * testPT.x + _s2 * testPT.y;
			float t = _t0 + _t1 * testPT.x + _t2 * testPT.y;

			return s > 0 && t > 0 && (s + t) < 2 * _A * _sign;
		}

		public static bool ptInTriangle(Vector2 testPT, Vector2 p0, Vector2 p1, Vector2 p2)
		{
			float A = 1 / 2 * (-p1.y * p2.x + p0.y * (-p1.x + p2.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y);
			float sign = A < 0 ? -1 : 1;
			float s = (p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * testPT.x + (p0.x - p2.x) * testPT.y) * sign;
			float t = (p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * testPT.x + (p1.x - p0.x) * testPT.y) * sign;

			return s > 0 && t > 0 && (s + t) < 2 * A * sign;
		}

	}
}
