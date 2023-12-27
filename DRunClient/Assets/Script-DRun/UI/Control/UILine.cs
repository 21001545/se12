using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UILine : MaskableGraphic
	{
		public float thickness = 1;
		public Vector2 begin = new Vector2(0,0);
		public Vector2 end = new Vector2(1,1);

		private List<UIVertex> _vertexList = new List<UIVertex>();
		private List<int> _indexList = new List<int>();


		public void setBegin(Vector2 begin)
		{
			this.begin = begin;
			SetAllDirty();
		}

		public void setEnd(Vector2 end)
		{
			this.end = end;
			SetAllDirty();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			buildMesh();

			if( _vertexList.Count > 0)
			{
				vh.AddUIVertexStream(_vertexList, _indexList);
			}
		}

		private void buildMesh()
		{
			_vertexList.Clear();
			_indexList.Clear();

			Vector2 dir = (end - begin).normalized;
			Vector2 extrude = Vector2.Perpendicular(dir);

			addVertex(begin + extrude * thickness);
			addVertex(begin - extrude * thickness);
			addVertex(end - extrude * thickness);
			addVertex(end + extrude * thickness);

			addTriangle(0, 1, 2);
			addTriangle(0, 2, 3);
		}

		private void addVertex(Vector2 position)
		{
			UIVertex vertex = UIVertex.simpleVert;

			vertex.position = position;
			vertex.color = color;

			_vertexList.Add(vertex);
		}

		private void addTriangle(int v0,int v1,int v2)
		{
			_indexList.Add(v0);
			_indexList.Add(v1);
			_indexList.Add(v2);
		}
	}
}
