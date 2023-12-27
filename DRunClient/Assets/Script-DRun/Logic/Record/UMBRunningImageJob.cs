using DRun.Client.NetData;
using DRun.Client.Running;
using Festa.Client.MapBox;
using Festa.Client.NetData;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client.Logic.Record
{
	public class UMBRunningImageJob : UMBAbstractOfflineRenderJob
	{
		private UMBOfflineRenderer _owner;
		private ClientRunningLog _log;
		private Vector2Int _size;
		private UnityAction<Texture2D> _callback;

		private Vector2Int _textureSize;
		private Rect _sourceRect;

		private MBTileCoordinateDouble _centerTilePos;
		private MBTileCoordinateDouble _minTilePos;
		private MBTileCoordinateDouble _maxTilePos;
		private MBLongLatCoordinate _startLocation;
		private MBLongLatCoordinate _endLocation;

		public static UMBRunningImageJob create(UMBOfflineRenderer owner,ClientRunningLog log,Vector2Int size,UnityAction<Texture2D> callback)
		{
			UMBRunningImageJob job = new UMBRunningImageJob();
			job.init(owner, log, size, callback);
			return job;
		}

		private void init(UMBOfflineRenderer owner, ClientRunningLog log, Vector2Int size, UnityAction<Texture2D> callback)
		{
			_owner = owner;
			_log = log;
			_size = size;
			_callback = callback;

			calcCenterTilePos();
		}

		private void calcCenterTilePos()
		{
			MBLongLatCoordinate minPos = MBLongLatCoordinate.zero;
			MBLongLatCoordinate maxPos = MBLongLatCoordinate.zero;

			_startLocation = MBLongLatCoordinate.zero;
			_endLocation = MBLongLatCoordinate.zero;

			for(int i = 0; i < _log.pathList.Count; ++i)
			{
				ClientTripPathData data = _log.pathList[i];

				// 2022.08.03 log가 하나도 없을 경우 문제가 생긴다, 서버를 수정하였으나, 나중에도 혹시 몰라서
				if (data.path_list.Count == 0)
				{
					continue;
				}

				if (data.size_lon == 0 && data.size_lat == 0)
				{
					data.recalcBound();
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

			int zoom = 15;

			_centerTilePos = MBTileCoordinateDouble.fromLonLat(centerPos, zoom);
			_minTilePos = MBTileCoordinateDouble.fromLonLat(minPos, zoom);
			_maxTilePos = MBTileCoordinateDouble.fromLonLat(maxPos, zoom);
		}

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

			return UMBControl.calcFitZoom(_centerTilePos.zoom, extent, mapboxExtent, new Vector2(8, 18), 0.75f);
		}

		private void createPath()
		{
			UMBActor_RunningPath path_actor = (UMBActor_RunningPath)_owner.mapBox.spawnActor("drun.path", _startLocation);

			path_actor.thickness = 2.5f * _textureSize.x / (float)_size.x;
			path_actor.clear();
			
			foreach(ClientTripPathData path in _log.pathList )
			{
				path_actor.updatePath(_log.running_type,path);
			}
		}

		private void createPoints()
		{
			_owner.mapBox.spawnActor("drun.log.start", _startLocation);
			_owner.mapBox.spawnActor("drun.log.end", _endLocation);
		}

		public override IEnumerator run()
		{
			_owner.removeAllActors();
			_owner.removeAllTiles();

			// 최초 값 세팅 (zoom값 계산을 위해)
			_owner.mapBox.getControl().initCurrentTilePos(_centerTilePos);

			// 텍스쳐 사이즈 설정
			calcTextureSize();

			// 최적의 zoom값을 설정
			float bestZoom = calcBestZoom();
			int targetTileZoom = MapBoxDefine.clampControlZoom((int)bestZoom);

			double tileScale = _owner.mapBox.getControl().calcTileScale(targetTileZoom, _centerTilePos.zoom);
			_centerTilePos.tile_pos.x *= tileScale;
			_centerTilePos.tile_pos.y *= tileScale;
			_centerTilePos.zoom = targetTileZoom;

			//Debug.Log($"centerTilePos[{_centerTilePos}] min[{_minTilePos.toLongLat()}] max[{_maxTilePos.toLongLat()}] zoom[{bestZoom}]");

			// 위치 이동
			_owner.mapBox.getControl().moveTo(_centerTilePos, bestZoom);

			createPath();
			createPoints();

			// 한 프레임 더 기다림
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			while (true)
			{
				yield return new WaitForEndOfFrame();

				if (_owner.mapBox.getControl().checkAllVisibleTileLoaded())
				{
					break;
				}
			}

			//Debug.Log($"tile loading complete");

			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			_owner.targetCamera.Render();

			//Debug.Log($"render offline");

			RenderTexture rt = _owner.targetCamera.targetTexture;

			RenderTexture prev = RenderTexture.active;

			RenderTexture.active = rt;


			Texture2D tex = new Texture2D(_textureSize.x, _textureSize.y, TextureFormat.RGB24, false, false);
			tex.ReadPixels(_sourceRect, 0, 0);
			tex.Apply();
			RenderTexture.active = prev;

			_owner.endCurrentJob();
			_callback(tex);

			yield break;
		}

		public void calcTextureSize()
		{
			RenderTexture rt = _owner.targetCamera.targetTexture;
			_textureSize = new Vector2Int();
			//sourceRect = new Rect();

			// 가로로 길다
			if (_size.x >= _size.y)
			{
				_textureSize.x = rt.width;
				_textureSize.y = rt.height * _size.y / _size.x;

				_sourceRect = new Rect(0, (rt.height - _textureSize.y) / 2, _textureSize.x, _textureSize.y);
			}
			else
			{
				_textureSize.y = rt.height;
				_textureSize.x = rt.width * _size.x / _size.y;

				_sourceRect = new Rect((rt.width - _textureSize.x) / 2, 0, _textureSize.x, _textureSize.y);
			}

			//Debug.Log($"textureSize[{textureSize}] sourceRect[{sourceRect}] targetSize[{_size}] targetRatio[{(float)_size.x / textureSize.x}, {(float)_size.y / textureSize.y}]");
		}
	}
}
