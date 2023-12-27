using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class PolylineMesh : MBMesh
	{
		public static PolylineMesh createPolylineMesh()
		{
			PolylineMesh mesh = new PolylineMesh();
			mesh.init();
			return mesh;
		}

		public void buildMesh(List<PolylinePoint> pointList,int begin,int count)
		{
			reset();

			//// 전체 길이를 구한다
			//float total_length = 0;
			//for(int i = 0; i < count - 1; ++i)
			//{
			//	Vector2Int p0 = pointList[begin + i].position;
			//	Vector2Int p1 = pointList[begin + i + 1].position;

			//	total_length += (p1 - p0).magnitude;
			//}

			float uv_x = 0;
			//float uv_scale = 1.0f / total_length;

			for(int i = 0; i < count - 1; ++i)
			{
				Color32 c0 = pointList[begin + i].color;
				Color32 c1 = pointList[begin + i + 1].color;

				Vector2Int p0 = pointList[begin + i].position;
				Vector2Int p1 = pointList[begin + i + 1].position;

				Vector2 diff = p1 - p0;
				Vector2 dir = diff.normalized;
				Vector2 extrude = Vector2.Perpendicular(dir);

				float uv_x0 = uv_x;
				float uv_x1 = uv_x + (p1 - p0).magnitude / (float)MapBoxDefine.tile_extent;

				Vector2 uv0_up = new Vector2(uv_x0, 1);
				Vector2 uv0_down = new Vector2(uv_x0, 0);

				Vector2 uv1_up = new Vector2(uv_x1, 1);
				Vector2 uv1_down = new Vector2(uv_x1, 0);

				uv_x = uv_x1;

				int v_index;

				// 시작점 round cap
				if( i == 0)
				{
					v_index = getVertexCount();

					addVertex(p0, (extrude - dir).normalized, c0, uv0_down);
					addVertex(p0, (-extrude - dir).normalized, c0, uv0_up);

					addTriangle(v_index + 1, v_index + 0, v_index + 3);
					addTriangle(v_index + 1, v_index + 3, v_index + 2);
				}

				v_index = getVertexCount();

				addVertex(p0, -extrude, c0, uv0_up);
				addVertex(p0, extrude, c0, uv0_down);

				addVertex(p1, extrude, c1, uv1_down);
				addVertex(p1, -extrude, c1, uv1_up);

				if ( i > 0 )
				{
					addTriangle(v_index - 1, v_index + 1, v_index + 0);
					addTriangle(v_index - 2, v_index + 1, v_index - 1);
				}

				addTriangle(v_index + 0, v_index + 1, v_index + 2);
				addTriangle(v_index + 0, v_index + 2, v_index + 3);

				// 종료지점 round cap
				if( i == count - 2)
				{
					v_index = getVertexCount();

					addVertex(p1, (-extrude + dir).normalized, c0, uv1_up);
					addVertex(p1, (extrude + dir).normalized, c0, uv1_down);

					addTriangle(v_index - 1, v_index - 2, v_index + 1);
					addTriangle(v_index - 1, v_index + 1, v_index + 0);
				}
			}
		}

		protected virtual void addVertex(Vector2Int p, Vector2 e, Color32 color, Vector2 uv)
		{
			Vector3 pos = new Vector3(p.x, -p.y, 0);
			pos.x /= 4096.0f;
			pos.y /= 4096.0f;

			Vector3 normal;
			//normal.x = e.x * 2 + (round ? 1 : 0);
			//normal.y = e.y * 2 + (up ? 1 : 0);
			normal.x = e.x;
			normal.y = -e.y;
			normal.z = 0;

			//Vector2 uv;
			//uv.x = (((dir == 0 ? 0 : (dir < 0 ? -1 : 1)) + 1) | ((linesofar & 0x3F) << 2));
			//uv.y = (linesofar >> 6);

			_vertexList.Add(pos);
			_normalList.Add(normal);
			_uvList.Add(uv);
			_colorList.Add(color);
		}

		//protected virtual void addVertex(Vector2Int p,Vector2 e,Color32 color,bool round,bool up,int dir,int linesofar)
		//{
		//	Vector3 pos = new Vector3(p.x, -p.y, 0);
		//	pos.x /= 4096.0f;
		//	pos.y /= 4096.0f;

		//	Vector3 normal;
		//	//normal.x = e.x * 2 + (round ? 1 : 0);
		//	//normal.y = e.y * 2 + (up ? 1 : 0);
		//	normal.x = e.x;
		//	normal.y = -e.y;
		//	normal.z = 0;

		//	Vector2 uv;
		//	uv.x = (((dir == 0 ? 0 : (dir < 0 ? -1 : 1)) + 1) | ((linesofar & 0x3F) << 2));
		//	uv.y = (linesofar >> 6);

		//	_vertexList.Add(pos);
		//	_normalList.Add(normal);
		//	_uvList.Add(uv);
		//	_colorList.Add(color);
		//}

		protected virtual void addTriangle(int p0, int p1, int p2)
		{
			_indexList.Add((ushort)p0);
			_indexList.Add((ushort)p1);
			_indexList.Add((ushort)p2);
		}
	}
}
