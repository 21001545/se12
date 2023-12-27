//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.UI.Extensions;
//using AwesomeCharts;
//using Festa.Client.MapBox;

//[RequireComponent(typeof(CanvasRenderer))]
//public class UMBTripPathRenderer : LineSegmentsRenderer
//{
//    private UMBActor_TripPath _owner;

//    private int _begin = 0;
//    private int _count = 0;

//    private static Color[] speedColor = null;

//    private List<long> TimeList => _owner.getTimeList();
//    private List<double> DistanceList => _owner.getDistanceList();
//    private List<Vector3> PositionList => _owner.getPositionList();

//    protected override void Start()
//    {
//        //GetComponent<Renderer>().sortingOrder = 2000;
//    }

//    public void setOwner(UMBActor_TripPath owner)
//	{
//        _owner = owner;
//	}

//    public void updatePosition(int begin,int count)
//	{
//        _begin = begin;
//        _count = count;
//        SetAllDirty();
//	}

//    //private static Color[] speedColor =
//    //{
//    //    new Color(94/255.0f,177/255.0f,255/255.0f),
//    //    new Color(160/255.0f,241/255.0f,74/255.0f),
//    //    new Color(255/255.0f,228/255.0f,82/255.0f),
//    //    new Color(255/255.0f,172/255.0f,94/255.0f)
//    //};

//    private static string[] speedHtmlColors = new string[]
//    {
//        "#5EB1FF",
//        "#A0F14A",
//        "#FFE452",
//        "#FFAC5E",
//    };

//    private void buildSpeedColor()
//	{
//        speedColor = new Color[speedHtmlColors.Length];

//        for(int i = 0; i < speedHtmlColors.Length; ++i)
//		{
//            Color color;
//            ColorUtility.TryParseHtmlString(speedHtmlColors[i], out color);

//            speedColor[i] = color;
//        }
//	}

//    private int getSpeedColorIndex(int positionIndex)
//	{
//        if( positionIndex == _begin || positionIndex >= TimeList.Count) // 그럴 수 있다
//		{
//            return 0;
//		}
//        else
//		{
//			float time = (TimeList[positionIndex] - TimeList[positionIndex - 1]) * 0.001f;
//            double speed = 0;
//            if (time > 0)
//            {
//                speed = DistanceList[positionIndex] / time;
//            }

//            int colorIndex;
            
//            if (speed < 1.11)
//            {
//                colorIndex = 0;
//            }
//            else if (speed < 2.78)
//            {
//                colorIndex = 1;
//            }
//            else if (speed < 4.17)
//            {
//                colorIndex = 2;
//            }
//            else
//            {
//                colorIndex = 3;
//            }

//            return colorIndex;
//		}
//	}

//    protected override void OnPopulateMesh(VertexHelper vh)
//    {
//        vh.Clear();
//        if (_owner == null || _count <= 1)
//            return;

//        // 그럴 수 있다
//        if( (_begin + _count) > PositionList.Count)
//		{
//            return;
//		}

//        if( speedColor == null)
//		{
//            buildSpeedColor();
//		}

//		UIVertex vertex = UIVertex.simpleVert;
//        float _thickness = _owner.getThickness();

//		//for (int i = 0; i < _positions.Length - 1; ++i)
//        for(int i = 0; i < _count - 1; ++i)
//		{
//			Vector3 position = PositionList[_begin + i];
//			Vector3 diff = (PositionList[_begin + i + 1] - PositionList[_begin + i]);
//			Vector3 dir = diff.normalized;

//			Vector3 up = Vector3.Cross(Vector3.forward, dir);

//			Vector3 v0 = position - up * _thickness * 0.5f;
//			Vector3 v1 = position + up * _thickness * 0.5f;
//			Vector3 v2 = PositionList[_begin + i + 1] + up * _thickness * 0.5f;
//			Vector3 v3 = PositionList[_begin + i + 1] - up * _thickness * 0.5f;

//			//vertex.color = speedColor[colorIndex];
//			vertex.color = speedColor[getSpeedColorIndex(_begin + i)];

//			vertex.position = v0;
//			vh.AddVert(vertex);
//			vertex.position = v1;
//			vh.AddVert(vertex);

//			vertex.color = speedColor[getSpeedColorIndex(_begin + i+1)];

//			vertex.position = v2;
//			vh.AddVert(vertex);
//			vertex.position = v3;
//			vh.AddVert(vertex);

//            int v_index = i * 4;

//            if (i > 0)
//			{
//				vh.AddTriangle(v_index - 1, v_index + 1, v_index + 0);
//				vh.AddTriangle(v_index - 2, v_index + 1, v_index - 1);
//			}
//			vh.AddTriangle(v_index + 0, v_index + 1, v_index + 2);
//			vh.AddTriangle(v_index + 0, v_index + 2, v_index + 3);
//		}
//	}
//}
