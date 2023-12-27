using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class UITripPath : MonoBehaviour
	{
		public UITripPathRenderer rendererSource;
		public Transform lineRendererRoot;
		public Transform startPoint;
		public Transform endPoint;

		private const double _pathPadding = 0.35;   // 25퍼센트 빈공간두고 path를 만든다
		private const double _lineLength = 2;	// 선 굵기

		private List<UITripPathRenderer> _rendererList;
		private List<UITripPathRenderer> _cacheList;

		private MBLongLatCoordinate _boundMin;
		private MBLongLatCoordinate _boundMax;
		private MBLongLatCoordinate _boundCenter;
		private MBLongLatCoordinate _startLocation;
		private MBLongLatCoordinate _endLocation;
		private double _pathScale;
		private double _localLineLength;

		private List<PolylinePoint> _pointList;

		public void init()
		{
			_cacheList = new List<UITripPathRenderer>();
			_rendererList = new List<UITripPathRenderer>();
			_pointList = new List<PolylinePoint>();
			rendererSource.gameObject.SetActive(false);
		}

		public void setup(List<ClientTripPathData> pathList)
		{
			removeAllRenderers();

			calcPathBound(pathList);
			calcPathScale();

			foreach (ClientTripPathData path in pathList)
			{
				createLine(path);
			}

			setupBeginEndPoints();
		}

		private void setupBeginEndPoints()
		{
			DoubleVector2 startPos = _startLocation.pos - _boundCenter.pos;
			DoubleVector2 endPos = _endLocation.pos - _boundCenter.pos;

			startPos *= _pathScale;
			endPos *= _pathScale;

			startPoint.localPosition = new Vector3((float)startPos.x, (float)startPos.y, 0);
			endPoint.localPosition = new Vector3((float)endPos.x, (float)endPos.y, 0);
		}

		private void calcPathBound(List<ClientTripPathData> pathList)
		{
			_boundMin = MBLongLatCoordinate.zero;
			_boundMax = MBLongLatCoordinate.zero;

			int valid_count = 0;
			foreach(ClientTripPathData path in pathList)
			{
				if( path.path_list.Count == 0)
				{
					continue;
				}

				if( valid_count == 0)
				{
					_boundMin = path.getMin();
					_boundMax = path.getMax();
					_startLocation = path.getFirstLocation();
					_endLocation = path.getLastLocation();
				}
				else
				{
					MBLongLatCoordinate path_min = path.getMin();
					MBLongLatCoordinate path_max = path.getMax();

					_boundMin.pos.x = System.Math.Min(_boundMin.pos.x, path_min.pos.x);
					_boundMin.pos.y = System.Math.Min(_boundMin.pos.y, path_min.pos.y);
					_boundMax.pos.x = System.Math.Max(_boundMax.pos.x, path_max.pos.x);
					_boundMax.pos.y = System.Math.Max(_boundMax.pos.y, path_max.pos.y);

					_endLocation = path.getLastLocation();
				}

				valid_count++;
			}

			_boundCenter = new MBLongLatCoordinate();
			_boundCenter.pos = (_boundMax.pos + _boundMin.pos) / 2.0;
		}

		private void calcPathScale()
		{
			RectTransform rt = transform as RectTransform;

			// 영역크기를 구한다
			Vector3[] corners = new Vector3[4];
			rt.GetLocalCorners(corners);

			double controlWidthHalf = (corners[2].x - corners[0].x) / 2;
			double controlHeightHalf = (corners[2].y - corners[0].y) / 2;

			double pathWidthHalf = (_boundMax.pos.x - _boundMin.pos.x) / 2;
			double pathHeightHalf = (_boundMax.pos.y - _boundMin.pos.y) / 2;

			double coverage = 1.0 - _pathPadding;

			double x_ratio = controlWidthHalf * coverage / pathWidthHalf;
			double y_ratio = controlHeightHalf * coverage / pathHeightHalf;

			if( x_ratio < y_ratio)
			{
				_pathScale = x_ratio;
			}
			else
			{
				_pathScale = y_ratio;
			}

			_localLineLength = _lineLength;
		}

		private void createLine(ClientTripPathData path)
		{
			createPointList(path);
			createRenderers(_pointList);
		}

		private void createRenderers(List<PolylinePoint> pointList)
		{
			int rendererCount = Mathf.CeilToInt( (float)_pointList.Count / (float)5000);

			int remainCount = pointList.Count;
			int beginIndex = 0;

			for(int i = 0; i < rendererCount; ++i)
			{
				int begin = beginIndex;
				int count = System.Math.Min(remainCount, 5000);

				PolylineMeshUI mesh = PolylineMeshUI.createPolylineMeshUI((float)_localLineLength);
				mesh.buildMesh(pointList, begin, count);

				UITripPathRenderer renderer = createRenderer();
				renderer.setup(mesh);

				beginIndex += count;
				remainCount -= count;
			}
		}

		private void createPointList(ClientTripPathData path)
		{
			List<PolylinePoint> tempPointList = new List<PolylinePoint>();
			int count = path.path_list.Count / 3;

			// 단색으로 할거니까
			for(int i = 0; i < count; ++i)
			{
				MBLongLatCoordinate current = new MBLongLatCoordinate(path.path_list[i * 3 + 0], path.path_list[i * 3 + 1]);

				DoubleVector2 pos = current.pos - _boundCenter.pos;
				pos *= _pathScale;
				pos.y = -pos.y;

				tempPointList.Add(new PolylinePoint(pos, Color.white));
			}

			float tolerance = 1.0f;
			_pointList = PolylineSimplify.Simplify(tempPointList, tolerance, false);
		}

		private void removeAllRenderers()
		{
			foreach(UITripPathRenderer renderer in _rendererList)
			{
				renderer.gameObject.SetActive(false);
				_cacheList.Add(renderer);
			}

			_rendererList.Clear();
		}

		private UITripPathRenderer createRenderer()
		{
			UITripPathRenderer renderer;
			if ( _cacheList.Count > 0)
			{
				renderer = _cacheList[0];
				_cacheList.RemoveAt(0);
			}
			else
			{
				GameObject go = Instantiate(rendererSource.gameObject, lineRendererRoot, false);
				renderer = go.GetComponent<UITripPathRenderer>();
			}

			renderer.gameObject.SetActive(true);
			_rendererList.Add(renderer);

			return renderer;
		}
	}
}
