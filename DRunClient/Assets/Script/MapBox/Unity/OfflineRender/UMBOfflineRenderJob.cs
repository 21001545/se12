using DG.Tweening.Plugins.Core.PathCore;
using Festa.Client.Module;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client.MapBox
{
	public class UMBOfflineRenderJob : UMBAbstractOfflineRenderJob
	{
		private UMBOfflineRenderer _owner;
		private List<ClientTripPathData> _pathData;

		private MBTileCoordinateDouble _centerTilePos;
		private MBTileCoordinateDouble _minTilePos;
		private MBTileCoordinateDouble _maxTilePos;
		private MBLongLatCoordinate _startLocation;
		private MBLongLatCoordinate _endLocation;
		private UnityAction<Texture2D> _callback;
		private Vector2Int _size;

		public static UMBOfflineRenderJob create(UMBOfflineRenderer owner, List<ClientTripPathData> data, Vector2Int size, UnityAction<Texture2D> callback)
		{
			UMBOfflineRenderJob job = new UMBOfflineRenderJob();
			job.init(owner, data, size, callback);
			return job;
		}

		private void init(UMBOfflineRenderer owner, List<ClientTripPathData> data, Vector2Int size, UnityAction<Texture2D> callback)
		{
			_owner = owner;
			_pathData = data;
			_callback = callback;
			_size = size;
			//_path_list = new List<MBLongLatCoordinate>();

			calcCenterTilePos();
			//buildPath();
		}

		private void calcCenterTilePos()
		{
			MBLongLatCoordinate minPos = default(MBLongLatCoordinate);
			MBLongLatCoordinate maxPos = default(MBLongLatCoordinate);

			_startLocation = MBLongLatCoordinate.zero;
			_endLocation = MBLongLatCoordinate.zero;

			for (int i = 0; i < _pathData.Count; ++i)
			{
				ClientTripPathData data = _pathData[i];

				// 2022.08.03 log가 하나도 없을 경우 문제가 생긴다, 서버를 수정하였으나, 나중에도 혹시 몰라서
				if (data.path_list.Count == 0)
				{
					continue;
				}

				if (i == 0)
				{
					minPos = new MBLongLatCoordinate(data.min_lon, data.min_lat);
					maxPos = new MBLongLatCoordinate(data.min_lon + data.size_lon, data.min_lat + data.size_lat);
					_startLocation = data.getFirstLocation();
					_endLocation = data.getLastLocation();
				}
				else
				{
					minPos.pos.x = System.Math.Min(data.min_lon, minPos.pos.x);
					minPos.pos.y = System.Math.Min(data.min_lat, minPos.pos.y);
					maxPos.pos.x = System.Math.Max(data.min_lon + data.size_lon, maxPos.pos.x);
					maxPos.pos.y = System.Math.Max(data.min_lat + data.size_lat, maxPos.pos.y);

					_endLocation = data.getLastLocation();
				}
			}

			MBLongLatCoordinate centerPos = new MBLongLatCoordinate();
			centerPos.pos = (minPos.pos + maxPos.pos) / 2;

			//int zoom = MapBoxDefine.control_zoom_range.y;
			//for (; zoom >= 10; --zoom)
			//{
			//	MBTileCoordinateDouble minTilePos = MBTileCoordinateDouble.fromLonLat(minPos, zoom);
			//	MBTileCoordinateDouble maxTilePos = MBTileCoordinateDouble.fromLonLat(maxPos, zoom);

			//	DoubleVector2 diff = new DoubleVector2(maxTilePos.tile_x - minTilePos.tile_x, maxTilePos.tile_y - minTilePos.tile_y);

			//	//Debug.Log($"zoom:{zoom} diff:{diff.magnitude}");

			//	if (diff.magnitude <= 2.0f)
			//	{
			//		break;
			//	}
			//}

			int zoom = 15;

			_centerTilePos = MBTileCoordinateDouble.fromLonLat(centerPos, zoom);
			_minTilePos = MBTileCoordinateDouble.fromLonLat(minPos, zoom);
			_maxTilePos = MBTileCoordinateDouble.fromLonLat(maxPos, zoom);

			//Debug.Log($"centerTilePos:{_centerTilePos.ToString()}");
		}

		//private void buildPath()
		//{

		//	_path_list.Clear();

		//	for(int i = 0; i < _pathData.path_list.Count / 2; ++i)
		//	{
		//		double lon = _pathData.path_list[i * 2 + 0];
		//		double lat = _pathData.path_list[i * 2 + 1];

		//		lon = lon * _pathData.size_lon / 65535.0 + _pathData.min_lon;
		//		lat = lat * _pathData.size_lat / 65535.0 + _pathData.min_lat;

		//		_path_list.Add(new MBLongLatCoordinate(lon, lat));

		//	}

		//}

		private Vector2 calcScreenExtent()
		{
			Vector3 minWorldPos = _owner.mapBox.getControl().tilePosToWorldPosition(_minTilePos);
			Vector3 maxWorldPos = _owner.mapBox.getControl().tilePosToWorldPosition(_maxTilePos);

			Vector3 minSPos = _owner.targetCamera.WorldToScreenPoint(minWorldPos);
			Vector3 maxSPos = _owner.targetCamera.WorldToScreenPoint(maxWorldPos);

			Vector3 scExtent = maxSPos - minSPos;
			scExtent.x = Mathf.Abs(scExtent.x);
			scExtent.y = Mathf.Abs(scExtent.y);

			return scExtent;
		}

		private float calcBestZoom()
		{
			Vector2 extent = calcScreenExtent();
			Vector2 mapboxExtent = Vector2.zero;

			if (_size.x > _size.y)
			{
				mapboxExtent.x = _owner.targetCamera.pixelWidth;
				mapboxExtent.y = _owner.targetCamera.pixelHeight * _size.y / _size.x;
			}
			else
			{
				mapboxExtent.y = _owner.targetCamera.pixelHeight;
				mapboxExtent.x = _owner.targetCamera.pixelWidth * _size.x / _size.y;
			}

			return _owner.mapBox.getControl().calcFitZoom(extent, mapboxExtent, new Vector2(8, 16), 0.75f);

			//float from;
			//float to;
			//float screenCoverage = 0.75f;
			//float maxZoom = 17;
			//float minZoom = 8;

			//if( extent.x >= extent.y)
			//{
			//	from = extent.x;
			//	to = _owner.targetCamera.pixelWidth * screenCoverage;
			//}
			//else
			//{
			//	from = extent.y;
			//	to = _owner.targetCamera.pixelHeight * screenCoverage;
			//}

			//float diff_zoom = _owner.mapBox.getControl().calcZoomDeltaFromScale(from, to);

			//float targetZoom = Mathf.Clamp(_centerTilePos.zoom + diff_zoom, minZoom, maxZoom);

			//return targetZoom;
		}

		private void createTripPaths()
		{
			foreach (ClientTripPathData path in _pathData)
			{
				UMBActor_TripPath actor_path = _owner.createPath();
				actor_path.updatePath(path);
			}
		}

		private void createTripFlags()
		{
			UMBActor flag_source = _owner.mapBox.styleData.actorSourceContainer.getSource("flag").actor_source;

			if (_startLocation.isZero() || _endLocation.isZero())
			{
				return;
			}

			UMBActor_TripFlag start_flag = (UMBActor_TripFlag)_owner.mapBox.spawnActor(flag_source, _startLocation);
			start_flag.setup(0);

			UMBActor_TripFlag end_flag = (UMBActor_TripFlag)_owner.mapBox.spawnActor(flag_source, _endLocation);
			end_flag.setup(1);
		}

		public override IEnumerator run()
		{
			_owner.removeAllActors();

			// 일단 중점을 설정
			_owner.mapBox.getControl().moveTo(_centerTilePos);

			// 최적의 zoom값을 설정
			float bestZoom = calcBestZoom();

			Debug.Log($"min[{_minTilePos.toLongLat()}] max[{_maxTilePos.toLongLat()}] zoom[{bestZoom}]");

			_owner.mapBox.getControl().zoom(calcBestZoom(), true);

			createTripPaths();
			createTripFlags();

			//_owner.mapBox.getControl().moveTo(_centerTilePos);

			// 한 프레임 더 기다림
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			//yield return new WaitForSeconds(0.2f);

			while (true)
			{
				yield return new WaitForEndOfFrame();

				if( _owner.mapBox.getControl().checkAllVisibleTileLoaded())
				{
					break;
				}
			}

			// 한 프레임 더 기다림
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			_owner.targetCamera.Render();

			RenderTexture rt = _owner.targetCamera.targetTexture;

			RenderTexture prev = RenderTexture.active;

			RenderTexture.active = rt;

			Vector2Int textureSize;
			Rect sourceRect;

			calcTextureSize(rt, out textureSize, out sourceRect);

			Texture2D tex = new Texture2D(textureSize.x, textureSize.y, TextureFormat.RGB24, false, false);
			tex.ReadPixels( sourceRect, 0, 0);
			tex.Apply();
			RenderTexture.active = prev;

			_owner.endCurrentJob();
			_callback( tex);
		}

		public void calcTextureSize(RenderTexture rt,out Vector2Int textureSize,out Rect sourceRect)
		{
			textureSize = new Vector2Int();
			sourceRect = new Rect();

			// 가로로 길다
			if( _size.x >= _size.y)
			{
				textureSize.x = rt.width;
				textureSize.y = rt.height * _size.y / _size.x;

				sourceRect = new Rect(0, (rt.height - textureSize.y) / 2, textureSize.x, textureSize.y);
			}
			else
			{
				textureSize.y = rt.height;
				textureSize.x = rt.width * _size.x / _size.y;

				sourceRect = new Rect((rt.width - textureSize.x) / 2, 0, textureSize.x, textureSize.y);
			}
		}
	}
}
