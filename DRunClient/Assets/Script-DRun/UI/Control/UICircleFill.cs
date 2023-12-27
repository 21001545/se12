using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UICircleFill : MaskableGraphic
	{
		[Range(8,256)]
		public int subdivision = 32;

		protected List<UIVertex> _vertexList = new List<UIVertex>();
		protected List<int> _indexList = new List<int>();

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			buildMesh();

			vh.AddUIVertexStream(_vertexList, _indexList);
		}
		
		protected virtual void buildMesh()
		{
			_vertexList.Clear();
			_indexList.Clear();

			Rect rect = rectTransform.rect;
			Vector2 halfSize = rect.size / 2.0f;
			Vector2 center = rect.center;

			addVertex(center);

			int pointCount = subdivision + 1;
			for (int i = 0; i < pointCount; ++i)
			{
				float angle = Mathf.PI * 2.0f * i / (subdivision);

				Vector2 position = center;
				position.x += Mathf.Cos(angle) * halfSize.x;
				position.y += Mathf.Sin(angle) * halfSize.y;

				addVertex(position);

				if (i > 0)
				{
					addTriangle(0, (i + 1), i);
				}
			}
		}

		protected virtual void addVertex(Vector2 position)
		{
			UIVertex vertex = UIVertex.simpleVert;
			vertex.position = position;
			vertex.color = color;

			_vertexList.Add(vertex);
		}

		protected virtual void addTriangle(int v0, int v1, int v2)
		{
			_indexList.Add(v0);
			_indexList.Add(v1);
			_indexList.Add(v2);
		}
	}
}
