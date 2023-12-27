using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.Running
{
	public class UMBActor_RunningPath : UMBActor
	{
		private const int _pointZoom = 18;
		private const int _maxSingleMeshPointCount = 5000;

		public Gradient speedGradient;
		public Color pausedColor;
		public Material matSolidLine;
		public Material matDotLine;
		public float thickness = 5.0f;

		private List<PolylineRenderer> _allocatedRendererList;
		private double _colorSpeedMin = 3;
		private double _colorSpeedMid = 15;
		private double _colorSpeedMax = 36;
		private PolylineMesh _polylineMesh;

		private Dictionary<int, List<PolylineRenderer>> _rendererMap;

		MBTileCoordinateDouble _pointPivotTilePos;

		public override bool CanPick => false;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			base.onCreated(source);

			_allocatedRendererList = new List<PolylineRenderer>();
			_rendererMap = new Dictionary<int, List<PolylineRenderer>>();
			_polylineMesh = PolylineMesh.createPolylineMesh();
		}

		public override void onReused()
		{
			base.onReused();
		}

		public override void onDelete()
		{
			clear();
		}

		public void clear()
		{
			GameObjectCache.getInstance().delete(_allocatedRendererList);
			_rendererMap.Clear();
		}

		//public void setup(List<ClientTripPathData> pathList)
		//{
		//	for(int i = 0; i < pathList.Count; ++i)
		//	{
		//		ClientTripPathData path = pathList[i];

		//		if( i == 0)
		//		{
		//			_pointPivotTilePos = MBTileCoordinateDouble.fromLonLat(path.getFirstLocation(), _pointZoom);
		//			changePosition(MBLongLatCoordinate.fromTilePos(_pointPivotTilePos));
		//		}

		//		updatePath(path);
		//	}
		//}

		public void updatePath(int running_type,ClientTripPathData path)
		{
			if( _rendererMap.Count == 0)
			{
				_pointPivotTilePos = MBTileCoordinateDouble.fromLonLat(path.getFirstLocation(), _pointZoom);
				changePosition(MBLongLatCoordinate.fromTilePos(_pointPivotTilePos));
			}

			Material mat = selectMaterial(running_type,path);

			List<PolylineRenderer> cacheList = getCachedRendererList(path.path_id);
			List<PolylinePoint> pointList = buildPointList(path);
			int usedRendererCount = buildPolylineMesh(0, mat, pointList, cacheList);
			hideRemainRenderers(cacheList, usedRendererCount);
		}

		public void updatePath(RunningPathData pathData)
		{
			if( pathData.isValidPath() == false)
			{
				return;
			}

			if( _rendererMap.Count == 0)
			{
				_pointPivotTilePos = MBTileCoordinateDouble.fromLonLat(pathData.getFirstPosition(), _pointZoom);
				changePosition(MBLongLatCoordinate.fromTilePos(_pointPivotTilePos));
			}

			List<PolylineRenderer> cacheList = getCachedRendererList(pathData.getPathID());

			int usedRendererCount = 0;
			foreach(RunningPathData.SubPath subPath in pathData.getSubPathList())
			{
				if( subPath.visible == false)
				{
					continue;
				}

				Material mat = selectMaterial(pathData, subPath);
				
				List<PolylinePoint> pointList = buildPointList(pathData, subPath);
				int count = buildPolylineMesh(usedRendererCount, mat, pointList, cacheList);
				usedRendererCount += count;
			}

			hideRemainRenderers(cacheList, usedRendererCount);

		}

		public Material selectMaterial(RunningPathData pathData,RunningPathData.SubPath subPath)
		{
			if( pathData.isMarathonMode())
			{
				return matSolidLine;
			}
			else
			{
				if( pathData.isPaused())
				{
					return matSolidLine;
				}
				else
				{
					if( subPath.minable)
					{
						return matSolidLine;
					}
					else
					{
						return matDotLine;
					}
				}
			}
		}

		public Material selectMaterial(int running_type,ClientTripPathData pathData)
		{
			if(running_type == ClientRunningLogCumulation.RunningType.promode)
			{
				if (pathData.trip_type == ClientTripConfig.TripType.none)
				{
					return matSolidLine;
				}
				else
				{
					if (pathData.mined == 1)
					{
						return matSolidLine;
					}
					else
					{
						return matDotLine;
					}
				}
			}
			else
			{
				return matSolidLine;
			}
		}


		private List<PolylineRenderer> getCachedRendererList(int path_id)
		{
			List<PolylineRenderer> cacheList;
			if(_rendererMap.TryGetValue(path_id, out cacheList) == false)
			{
				cacheList = new List<PolylineRenderer>();
				_rendererMap.Add(path_id, cacheList);
			}

			return cacheList;
		}

		public void initColorSpeed()
		{
			_colorSpeedMin = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_path_color_min_speed, 3);
			_colorSpeedMid = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_path_color_mid_speed, 16);
			_colorSpeedMax = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_path_color_max_speed, 36);
		}

		private Color evaluateSpeedColor(double speed)
		{
			float ratio;

			if( speed < _colorSpeedMid)
			{
				ratio = (float)((speed - _colorSpeedMin) / (_colorSpeedMid - _colorSpeedMin));
			}
			else
			{
				ratio = (float)((speed - _colorSpeedMid) / (_colorSpeedMax - _colorSpeedMid)) + 0.5f;
			}

			ratio = Mathf.Clamp(ratio, 0, 1);
			return speedGradient.Evaluate(ratio);
		}

		private List<PolylinePoint> buildPointList(RunningPathData pathData,RunningPathData.SubPath path)
		{
			List<PolylinePoint> pointList = new List<PolylinePoint>();
			List<GPSTilePosition> posList = pathData.getPosList();
			
			for(int i = path.range.x; i <= path.range.y; ++i)
			{
				GPSTilePosition pos = posList[i];

				double speed = (i == 1) ? posList[0].deltaSpeedKMH : pos.deltaSpeedKMH;

				Color32 color;
				if( pathData.isPaused())
				{
					color = pausedColor;
				}
				else
				{
					color = evaluateSpeedColor(speed);
				}

				Vector2Int v = (pos.tile_pos - _pointPivotTilePos.tile_pos) * MapBoxDefine.tile_extent;
				pointList.Add(new PolylinePoint(v, color));
			}

			return pointList;
		}

		private List<PolylinePoint> buildPointList(ClientTripPathData path)
		{
			List<PolylinePoint> pointList = new List<PolylinePoint>();
			int count = path.path_list.Count / 3;
			for(int i = 0; i < count - 1; ++i)
			{
				MBLongLatCoordinate current = new MBLongLatCoordinate(path.path_list[i * 3 + 0], path.path_list[i * 3 + 1]);
				MBLongLatCoordinate next = new MBLongLatCoordinate(path.path_list[(i + 1) * 3 + 0], path.path_list[(i + 1) * 3 + 1]);

				double distanceKM = current.distanceFrom(next);
				double timeSecond = (path.path_time_list[i + 1] - path.path_time_list[i]) / 1000.0;

				double speed = timeSecond == 0 ? 0 : distanceKM * 3600.0 / timeSecond;

				Color32 color;
				if( path.trip_type == ClientTripConfig.TripType.none)
				{
					color = pausedColor;
				}
				else
				{
					// 그럴수 있다, 이전 값이 있으면 그걸 쓰도록 하자
					if( speed == 0 && pointList.Count > 0)
					{
						color = pointList[pointList.Count - 1].color;
					}
					else
					{
						color = evaluateSpeedColor(speed);
					}
				}

				MBTileCoordinateDouble tilePos = MBTileCoordinateDouble.fromLonLat(current, _pointZoom);

				Vector2Int pos = (tilePos.tile_pos - _pointPivotTilePos.tile_pos) * MapBoxDefine.tile_extent;
				pointList.Add(new PolylinePoint(pos, color));

				if( i == count - 2)
				{
					tilePos = MBTileCoordinateDouble.fromLonLat(next, _pointZoom);
					pos = (tilePos.tile_pos - _pointPivotTilePos.tile_pos) * MapBoxDefine.tile_extent;
					pointList.Add(new PolylinePoint(pos, color));
				}
			}

			return PolylineSimplify.Simplify(pointList, 200, false);
		}

		private int buildPolylineMesh(int renderer_offset,Material mat, List<PolylinePoint> pointList,List<PolylineRenderer> cacheList)
		{
			int rendererCount = Mathf.CeilToInt((float)pointList.Count / (float)_maxSingleMeshPointCount);

			int remainCount = pointList.Count;
			int beginIndex = 0;

			for(int i = 0; i < rendererCount; ++i)
			{
				int begin = beginIndex;
				int count = System.Math.Min(remainCount, _maxSingleMeshPointCount);

				_polylineMesh.buildMesh(pointList, begin, count);

				setupRenderer(renderer_offset + i, cacheList, thickness, mat, Color.white, 1001);

				beginIndex += count;
				remainCount -= count;
			}

			return rendererCount;
		}

		private PolylineRenderer allocRenderer(int index,List<PolylineRenderer> cacheList)
		{
			PolylineRenderer renderer;
			if (index >= cacheList.Count)
			{
				renderer = _mapBox.polyline_renderer_source.make<PolylineRenderer>(transform, GameObjectCacheType.actor);
				cacheList.Add(renderer);
				_allocatedRendererList.Add(renderer);
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

		private void setupRenderer(int index,List<PolylineRenderer> cacheList,float width,Material mat,Color color,int sortingOrder)
		{
			PolylineRenderer renderer = allocRenderer(index, cacheList);
			renderer.setup(_polylineMesh, mat, width, color, sortingOrder);
		}

		public override void updateTransformPosition()
		{
			base.updateTransformPosition();

			float scale = (float)_control.calcPivotScale(_control.getZoomDamper().getCurrent(), _pointZoom);
			_rt.localScale = Vector3.one * (MapBoxDefine.scale_pivot * scale * MapBoxDefine.tile_extent);
		}
	}
}
