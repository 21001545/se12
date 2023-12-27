using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.MsgPack;
using Festa.Client.Module.Net;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Unity.Profiling;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBTileLoader : BaseStepProcessor
	{
		private MultiThreadWorker _multiThreadWorker;
		private MBTileCache _cache;

		private string			 _baseURL;
		private MBTileCoordinate _tilePos;
		private MBStyle			 _style;

		private string _cacheKey;

		private MBTile _tile;
		private MBTile _streetTile;
		private MBTile _terrainTile;

		private static string _streetTileSetID = "mapbox.mapbox-streets-v8";
		private static string _terrainTileSetID = "mapbox.mapbox-terrain-v2";

		private static int _loadingCount = 0;

		public static int getLoadingCount()
		{
			return _loadingCount;
		}

		public MBTile getTile()
		{
			return _tile;
		}
	
		public static MBTileLoader create(string baseURL,MBTileCoordinate tilePos, MBStyle style)
		{
			MBTileLoader loader = new MBTileLoader();
			loader.init(baseURL, tilePos, style);
			return loader;
		}

		private void init(string baseURL,MBTileCoordinate tilePos,MBStyle style)
		{
			base.init();

			_baseURL = baseURL;
			_tilePos = tilePos.getValidByWrap();
			_style = style;

			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();

			_tile = null;
			_streetTile = null;
			_terrainTile = null;

			_cache = ClientMain.instance.getMBTileCache();
			_cacheKey = string.Format("{0}/{1}/{2}", _tilePos.zoom, _tilePos.tile_x, _tilePos.tile_y);
			//_logStepTime = true;
		}

		protected override void buildSteps()
		{
			_stepList = new List<StepProcessor>();
			//_stepList.Add(delayForTest);
			_stepList.Add(checkFromMemory);
		}

		private void delayForTest(Handler<AsyncResult<Void>> handler)
		{
			// thread 점유하는게 실제 테스트 상황이랑 좀 다른것 같아서

			ClientMain.instance.StartCoroutine(_delayForTest(handler));


			//_multiThreadWorker.execute<Module.Void>(promise => {

			//	System.Threading.Thread.Sleep(500);
			//	promise.complete();
			//}, result => {
			//	handler(Future.succeededFuture());
			//});
		}

		private IEnumerator _delayForTest(Handler<AsyncResult<Void>> handler)
		{
			yield return new WaitForSeconds(0.3f);

			handler(Future.succeededFuture());
		}

		private void checkFromMemory(Handler<AsyncResult<Module.Void>> handler)
		{
			_tile = _cache.get(_cacheKey);
			if (_tile == null)
			{
				_stepList.Add(loadStreetTile);
				_stepList.Add(loadTerrainTile);
				_stepList.Add(mergeTile);
				_stepList.Add(buildMesh);
				_stepList.Add(saveCache);
			}
			else if (_tile._dicStyleRenderData.ContainsKey(_style) == false)
			{
				_stepList.Add(buildMesh);
			}

			handler(Future.succeededFuture());
		}

		public void run(int call_id,Handler<int,bool> callback)
		{
			// 다른 곳에서 이미 로딩중이다
			if ( MBTileLoadingSyncer.tryLoading(_tilePos) == false)
			{
				//Debug.Log(string.Format("tile[{0}] already loading. : wait for end", _tilePos));

				// fixed update time만큼 기다려 본다
				MainThreadDispatcher.dispatchFixedUpdate(() => {
					run(call_id,callback);
				});
			}
			else
			{
				_loadingCount++;

				runSteps(0, _stepList, _logStepTime, result => {
					MBTileLoadingSyncer.endLoading(_tilePos);

					_loadingCount--;

					if ( result.failed())
					{
						callback(call_id, false);
					}
					else
					{
						callback(call_id, true);
					}
				});
			}
		}

		private void loadStreetTile(Handler<AsyncResult<Module.Void>> handler)
		{
			MBTileLoaderSingle loader = MBTileLoaderSingle.create(_baseURL, _streetTileSetID, _tilePos);
			loader.run(result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_streetTile = loader.getTile();
					if( _streetTile == null)
					{
						handler(Future.failedFuture(new System.Exception("loadStreeTile fail")));
					}
					else
					{
						handler(Future.succeededFuture());
					}
				}
			});
		}

		private void loadTerrainTile(Handler<AsyncResult<Module.Void>> handler)
		{
			MBTileLoaderSingle loader = MBTileLoaderSingle.create(_baseURL, _terrainTileSetID, _tilePos);
			loader.run(result => {
				if (result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_terrainTile = loader.getTile();	// null일수 있다
					handler(Future.succeededFuture());
				}
			});
		}

		private void mergeTile(Handler<AsyncResult<Module.Void>> handler)
		{
			// 2022.7.15 이강희 바다의 경우 terrainTile이 없을 수 있다
			if (_terrainTile == null)
			{
				_tile = _streetTile;
				handler(Future.succeededFuture());
				return;
			}

			_multiThreadWorker.execute<Module.Void>(promise => { 
				try
				{

					_streetTile.mergeFrom(_terrainTile);

					promise.complete();
				}
				catch(System.Exception e)
				{
					promise.fail(e);
				}
			}, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_tile = _streetTile;
					handler(Future.succeededFuture());
				}
			});
		}

		private void buildMesh(Handler<AsyncResult<Module.Void>> handler)
		{
			MBTileMeshBuilder meshBuilder = MBTileMeshBuilder.create(_tile, _style);
			meshBuilder.run(build_result => {

				//Debug.Log(string.Format("end load : {0} - load({1}) build_result({2})", _tilePos.ToString(), Time.realtimeSinceStartup - beginTime, build_result));

				if (build_result == false)
				{
					handler(Future.failedFuture( new System.Exception(string.Format("failed to build tile mesh:{0}/{1}/{2}",_tilePos.zoom, _tilePos.tile_x, _tilePos.tile_y))));
				}
				else
				{
					handler(Future.succeededFuture());
				}

			});
		}

		private void saveCache(Handler<AsyncResult<Module.Void>> handler)
		{
			_cache.put(_cacheKey, _tile);
			handler(Future.succeededFuture());
		}
	}
}
