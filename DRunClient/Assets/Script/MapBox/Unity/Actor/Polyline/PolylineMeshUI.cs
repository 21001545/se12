using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class PolylineMeshUI : PolylineMesh
	{
		private List<UIVertex> _uiVertexList;
		private List<int> _uiIndexList;

		private float _lineLength;

		public List<UIVertex> getUIVertexList()
		{
			return _uiVertexList;
		}

		public List<int> getUIIndexList()
		{
			return _uiIndexList;
		}

		public override int getVertexCount()
		{
			return _uiVertexList.Count;
		}

		public static PolylineMeshUI createPolylineMeshUI(float lineLength)
		{
			PolylineMeshUI mesh = new PolylineMeshUI();
			mesh.init(lineLength);
			return mesh;
		}

		protected void init(float lineLength)
		{
			_lineLength = lineLength;
			_uiIndexList = new List<int>();
			_uiVertexList = new List<UIVertex>();
		}

		public override void reset()
		{
			_uiIndexList.Clear();
			_uiVertexList.Clear();
		}

		protected override void addVertex(Vector2Int p, Vector2 e, Color32 color, Vector2 uv)
		{
			Vector3 extrude = new Vector3(e.x, -e.y, 0) * _lineLength;

			UIVertex vertex = UIVertex.simpleVert;
			vertex.position = new Vector3(p.x, -p.y, 0) + extrude;
			vertex.color = color;
			_uiVertexList.Add(vertex);

			//Vector3 pos = new Vector3(p.x, -p.y, 0);
			//pos.x /= 4096.0f;
			//pos.y /= 4096.0f;

			//Vector3 normal;
			////normal.x = e.x * 2 + (round ? 1 : 0);
			////normal.y = e.y * 2 + (up ? 1 : 0);
			//normal.x = e.x;
			//normal.y = -e.y;
			//normal.z = 0;

			//Vector2 uv;
			//uv.x = (((dir == 0 ? 0 : (dir < 0 ? -1 : 1)) + 1) | ((linesofar & 0x3F) << 2));
			//uv.y = (linesofar >> 6);

			//_vertexList.Add(pos);
			//_normalList.Add(normal);
			//_uvList.Add(uv);
			//_colorList.Add(color);
		}

		protected override void addTriangle(int p0, int p1, int p2)
		{
			_uiIndexList.Add(p0);
			_uiIndexList.Add(p1);
			_uiIndexList.Add(p2);
		}

	}
}
