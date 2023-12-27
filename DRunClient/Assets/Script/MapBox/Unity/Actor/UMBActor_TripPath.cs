using Festa.Client.Module;
using Festa.Client.NetData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Festa.Client.MapBox
{
    public class UMBActor_TripPath : UMBActor
    {
		private const int _pointZoom = 18;
		private const int _maxSingleMeshPointCount = 5000;

		private ClientTripPathData _tripPathData;
		private List<PolylinePoint> _pointList;
		private MBTileCoordinateDouble _pointPivotTilePos;
		private PolylineMesh _polylineMesh;

		private List<PolylineRenderer> _outlineRendererList;
		private List<PolylineRenderer> _rendererList;
		
		public override void onCreated(ReusableMonoBehaviour source)
		{
			base.onCreated(source);
			_pointList = new List<PolylinePoint>();
			_polylineMesh = PolylineMesh.createPolylineMesh();
			_rendererList = new List<PolylineRenderer>();
			_outlineRendererList = new List<PolylineRenderer>();
			_canPick = false;
			//_pathList = new List<MBLongLatCoordinate>();
			//_positionList = new List<Vector3>();
			//_timeList = new List<long>();
			//_distanceList = new List<double>();
			//_currentRevision = _targetRevision = 0;
			//_extraLineRendererList = new List<UMBTripPathRenderer>();
			//lineRenderer.setOwner(this);
			//_thickness = 32;
		}

		public override void onReused()
		{
			base.onReused();

			//_pathList.Clear();
			//_currentRevision = _targetRevision = 0;
			//foreach(UMBTripPathRenderer renderer in _extraLineRendererList)
			//{
			//	renderer.gameObject.SetActive(false);
			//}
			//_lr.positionCount = 0;
		}

		public override void onDelete()
		{
			GameObjectCache.getInstance().delete(_rendererList);
			GameObjectCache.getInstance().delete(_outlineRendererList);
		}

		public void updatePath(ClientTripPathData path)
		{
			_tripPathData = path;
			//Debug.Log($"count[{path.path_list.Count}]");
			buildPointList(path);
			changePosition(MBLongLatCoordinate.fromTilePos(_pointPivotTilePos));

			// 필요하면 multi thread로 변경
			List<PolylinePoint> simplifiedList = PolylineSimplify.Simplify(_pointList, 200, false);
			//Debug.Log($"simplified: [{_pointList.Count}] -> [{simplifiedList.Count}]");
			buildPolylineMesh(simplifiedList);
		}

		private static Color32 fromHTMLColor(string code)
		{
			Color color;
			ColorUtility.TryParseHtmlString(code, out color);
			return color;
		}

		private static string[] colorCodes = new string[]
		{
			//"#5EB1FF",
			//"#A0F14A",
			//"#FFE452",
			//"#FFAC5E"

			"#EB5C56",	// red
            "#FFC163",	// yellow
            "#51C8CF",	// green
            "#3B6CCD"	// blue
        };

		private static Color32[] speedColors = null;

		private static Color32 getSpeedColor(double speed)
		{
			if( speed < 1.11)
			{
				return speedColors[0];
			}
			else if (speed < 2.78)
			{
				return speedColors[1];
			}
			else if( speed < 4.17)
			{
				return speedColors[2];
			}

			return speedColors[3];
		}

		private void prepareSpeedColors()
		{
			speedColors = new Color32[colorCodes.Length];
			for(int i = 0; i < colorCodes.Length; ++i)
			{
				speedColors[i] = fromHTMLColor(colorCodes[ i]);
			}
		}

		private static Color32 _pausedLineColor = new Color(0.8f, 0.8f, 0.8f, 1);

		private void buildPointList(ClientTripPathData path)
		{
			prepareSpeedColors();

			_pointList.Clear();
			int count = path.path_list.Count / 3;
			for(int i = 0; i < count - 1; ++i)
			{
				MBLongLatCoordinate current = new MBLongLatCoordinate(path.path_list[i * 3 + 0], path.path_list[i * 3 + 1]);
				MBLongLatCoordinate next = new MBLongLatCoordinate(path.path_list[(i + 1) * 3 + 0], path.path_list[(i + 1) * 3 + 1]);

				double distance = current.distanceFrom(next) * 1000.0;
				double duration = (path.path_time_list[i + 1] - path.path_time_list[i]) / 1000.0;

				double speed = duration == 0 ? 0 : distance / duration;

				Color32 color;
				
				if( path.trip_type == ClientTripConfig.TripType.none)
				{
					color = _pausedLineColor;
				}
				else
				{
					color = getSpeedColor(speed);
				}

				MBTileCoordinateDouble tilePos = MBTileCoordinateDouble.fromLonLat(current, _pointZoom);
				if( i == 0)
				{
					_pointPivotTilePos = tilePos;
				}

				Vector2Int pos = (tilePos.tile_pos - _pointPivotTilePos.tile_pos) * MapBoxDefine.tile_extent;
				_pointList.Add( new PolylinePoint(pos,color));	

				if( i == count - 2)
				{
					tilePos = MBTileCoordinateDouble.fromLonLat(next, _pointZoom);
					pos = (tilePos.tile_pos - _pointPivotTilePos.tile_pos) * MapBoxDefine.tile_extent;
					_pointList.Add(new PolylinePoint(pos, color));
				}
			}
		}

		//public override void update()
		//{
		//	base.update();
		//}

		public override void updateTransformPosition()
		{
			base.updateTransformPosition();

			float scale = (float)_control.calcPivotScale(_control.getZoomDamper().getCurrent(), _pointZoom);
			_rt.localScale = Vector3.one * (MapBoxDefine.scale_pivot * scale * MapBoxDefine.tile_extent);
		}

		private void buildPolylineMesh(List<PolylinePoint> pointList)
		{
			int rendererCount = Mathf.CeilToInt((float)_pointList.Count / (float)_maxSingleMeshPointCount);

			int remainCount = pointList.Count;
			int beginIndex = 0;

			Color32 colorOutline = Color.black;
			Color32 colorLine = Color.white;

			for (int i = 0; i < rendererCount; ++i)
			{
				int begin = beginIndex;
				int count = System.Math.Min(remainCount, _maxSingleMeshPointCount);

				_polylineMesh.buildMesh(pointList, begin, count);

				setupRenderer(i, _outlineRendererList, 4, colorOutline, 1000);
				setupRenderer(i, _rendererList, 3, colorLine, 1001);

				beginIndex += count;
				remainCount -= count;
			}

			hideRemainRenderers(_rendererList, rendererCount);
			hideRemainRenderers(_outlineRendererList, rendererCount);
		}

		private void setupRenderer(int index,List<PolylineRenderer> cacheList, float width, Color color,int sortingOrder)
		{
			PolylineRenderer renderer = allocRenderer(index, cacheList);
			renderer.setup(_polylineMesh, _mapBox.getUMBStyle().getPolylineMaterial(), width, color,sortingOrder);
		}

		private PolylineRenderer allocRenderer(int index,List<PolylineRenderer> cacheList)
		{
			PolylineRenderer renderer = null;
			if (index >= cacheList.Count)
			{
				renderer = _mapBox.polyline_renderer_source.make<PolylineRenderer>(transform, GameObjectCacheType.actor);
				cacheList.Add(renderer);
			}
			else
			{
				renderer = cacheList[index];
				renderer.gameObject.SetActive(true);
			}

			return renderer;
		}

		private void hideRemainRenderers(List<PolylineRenderer> list,int used_count)
		{
			if (used_count < list.Count)
			{
				for (int i = used_count; i < list.Count; ++i)
				{
					list[i].gameObject.SetActive(false);
				}
			}
		}

		//private void OnDrawGizmos()
		//{
		//	if( _pointList == null)
		//	{
		//		return;
		//	}

		//	for(int i = 0; i < _pointList.Count - 1; ++i)
		//	{
		//		PolylinePoint begin = _pointList[i];
		//		PolylinePoint end = _pointList[i + 1];

		//		Vector3 beginPos = transform.TransformPoint(new Vector3( begin.position.x, -begin.position.y, 0));
		//		Vector3 endPos = transform.TransformPoint(new Vector3(end.position.x, -end.position.y, 0));

		//		Gizmos.color = Color.Lerp( begin.color, end.color, 0.5f);
		//		Gizmos.DrawLine(beginPos, endPos);
		//	}

		//	for(int i = 0; i < _pointList.Count; ++i)
		//	{
		//		PolylinePoint point = _pointList[i];

		//		Vector3 wPos = transform.TransformPoint(new Vector3( point.position.x, -point.position.y, 0));
		//		Gizmos.color = point.color;
		//		Gizmos.DrawCube(wPos, Vector3.one / 50.0f);
		//	}
		//}
	}
}
