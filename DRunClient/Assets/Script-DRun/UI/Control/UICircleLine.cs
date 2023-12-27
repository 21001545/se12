using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DRun.Client
{
	[RequireComponent(typeof(CanvasRenderer))]
	public class UICircleLine : MaskableGraphic
	{
		public enum FillDirection
		{
			ClockWise = 0,
			CounterClockWise = 1
		}

		public float thickness = 1;
		public int subdivision = 32;

		[Range(0,360)]
		public float beginAngle = 0;

		[Range(0, 1)]
		public float fillAmount = 1;

		public FillDirection fillDirection = FillDirection.CounterClockWise;
		public float fillScale = 1;

		private List<Vector2> _normalList = new List<Vector2>();
		private List<UIVertex> _vertexList = new List<UIVertex>();
		private List<int> _indexList = new List<int>();

        public UnityAction<float> onFillAmountChanged;

		public void setFillAmount(float fill, bool triggerBinding = false)
		{
			fillAmount = fill;

			if (triggerBinding) 
			    onFillAmountChanged?.Invoke(fill);

			SetAllDirty();
		}

		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			if( fillAmount == 0)
			{
				return;
			}

			buildNormalList();
			buildMesh();

			if( _vertexList.Count > 0)
			{
				vh.AddUIVertexStream(_vertexList, _indexList);
			}
		}

		private void buildNormalList()
		{
			_normalList.Clear();

			//Rect rect = rectTransform.rect;

			//Vector2 halfSize = rect.size / 2;
			//Vector2 center = rect.center;
			float scaledFillAmount = fillAmount * fillScale;

			float totalAngle = Mathf.PI * 2 * scaledFillAmount;
			
			
			int pointCount = (int)(subdivision * scaledFillAmount);
			if( pointCount < 3)
			{
				pointCount = 3;
			}
			pointCount += 1;

			for(int i = 0; i < pointCount; ++i)
			{
				float angle = beginAngle * Mathf.Deg2Rad;
				
				if( fillDirection == FillDirection.CounterClockWise)
				{
					angle += i * totalAngle / (pointCount - 1);
				}
				else
				{
					angle -= i * totalAngle / (pointCount - 1);
				}

				_normalList.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
			}
		}

		private void buildMesh()
		{
			_vertexList.Clear();
			_indexList.Clear();

			Rect rect = rectTransform.rect;
			Vector2 halfSize = rect.size / 2;
			Vector2 center = rect.center;
			float scaledFillAmount = fillAmount * fillScale;

			if (scaledFillAmount > 0 && scaledFillAmount < 1.0)
			{
				appendRound(_normalList[0], true);
			}

			for (int i = 0; i < _normalList.Count; ++i)
			{
				Vector2 normal = _normalList[i];

				int vertex_index = _vertexList.Count;

				addVertex(normal, true, halfSize, center);
				addVertex(normal, false, halfSize, center);

				if (i > 0)
				{
					addTriangle(vertex_index - 1, vertex_index - 2, vertex_index + 0);
					addTriangle(vertex_index - 1, vertex_index + 0, vertex_index + 1);
				}
			}

			if (scaledFillAmount > 0 && scaledFillAmount < 1.0)
			{
				appendRound(_normalList[_normalList.Count - 1], false);
			}
		}

		private void addVertex(Vector2 normal,bool dir,Vector2 circleSize,Vector3 center)
		{
			UIVertex vertex = UIVertex.simpleVert;

			float extrude = dir ? thickness : -thickness;

			vertex.position.x = normal.x * (circleSize.x + extrude) + center.x;
			vertex.position.y = normal.y * (circleSize.y + extrude) + center.y;
			vertex.position.z = 0;
			vertex.color = color;

			_vertexList.Add(vertex);
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
		
		private void appendRound(Vector2 normal,bool left)
		{
			float beginAngle = Mathf.Atan2(normal.y, normal.x);

			int vertex_index = _vertexList.Count;
			Rect rect = rectTransform.rect;

			Vector2 center = new Vector2(normal.x * rect.width / 2, normal.y * rect.height / 2);
			addVertex(center);

			int segment = 8;

			for (int i = 0; i < (segment + 1); ++i)
			{
				float angle = beginAngle;
				
				if( left)
				{
					angle += i * Mathf.PI / segment;
				}
				else
				{
					angle -= i * Mathf.PI / segment;
				}

				Vector2 position = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
				position.x *= thickness;
				position.y *= thickness;
				position += center;

				addVertex(position);

				if( i > 0)
				{
					if( left)
					{
						addTriangle(vertex_index + 0, vertex_index + (i + 1), vertex_index + i);
					}
					else
					{
						addTriangle(vertex_index + 0, vertex_index + i, vertex_index + (i + 1));
					}
				}
			}
		}
	}
}
