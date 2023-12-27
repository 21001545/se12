using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.MsgPack;
using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Unity.Profiling;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class LandTileLoader : BaseStepProcessor
	{
		private string			_baseURL;
		private MonoBehaviour	_behaviour;
		private CDNClient		_cdnClient;
		private ClientNetwork	_network;
		private MultiThreadWorker _multiThreadWorker;

		private MBTileCoordinate _tilePos;
		private string _cache_key;

		private string _local_file_path;
		private string _cdn_url;

		private byte[] _compressedTileData;

		private LandTileCache _cache;
		private LandTile _tile;
		private static string _streetTileSetID = "mapbox.mapbox-streets-v8";

		public LandTile getTile()
		{
			return _tile;
		}

		public static LandTileLoader create(string baseURL,MBTileCoordinate tilePos)
		{
			LandTileLoader loader = new LandTileLoader();
			loader.init(baseURL, tilePos);
			return loader;
		}

		protected void init(string baseURL,MBTileCoordinate tilePos)
		{
			base.init();

			_behaviour = ClientMain.instance;
			_baseURL = baseURL;
			_tilePos = tilePos.getValidByWrap();
			_cdnClient = ClientMain.instance.getNetwork().getCDNClient();
			_network = ClientMain.instance.getNetwork();
			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();
			_cache = ClientMain.instance.getLandTileCache();

			_cache_key = $"{_tilePos.zoom}/{_tilePos.tile_x}/{_tilePos.tile_y}";
			_cdn_url = $"{_baseURL}/mapbox/landtile/{_streetTileSetID}/{_tilePos.zoom}/{_tilePos.tile_x}/{_tilePos.tile_y}.landtile";
			_local_file_path = Application.temporaryCachePath + $"/landtile/{_streetTileSetID}/{_tilePos.zoom}_{_tilePos.tile_x}_{_tilePos.tile_y}.landtile";
		}

		protected override void buildSteps()
		{
			_stepList = new List<StepProcessor>();
			_stepList.Add(checkFromMemory);
		}

		public void run(int call_id,Handler<int,bool> callback)
		{
			if( LandTileLoadingSyncer.tryLoading(_tilePos) == false)
			{
				Debug.Log($"land tile[{_tilePos}] already loading. : wait for end");

				MainThreadDispatcher.dispatchFixedUpdate(() => {
					run(call_id, callback);
				});
			}
			else
			{
				base.run(result => {
					LandTileLoadingSyncer.endLoading(_tilePos);
					callback(call_id, result.succeeded());
				});
			}
		}

		private void checkFromMemory(Handler<AsyncResult<Module.Void>> handler)
		{
			_tile = _cache.get(_cache_key);
			if( _tile == null)
			{
				_stepList.Add(validateDirectory);
				_stepList.Add(loadFromLocalFile);
			}

			handler(Future.succeededFuture());
		}

		private void validateDirectory(Handler<AsyncResult<Module.Void>> handler)
		{
			string base_path_0 = Application.temporaryCachePath + "/landtile";
			string base_path_1 = base_path_0 + "/" + _streetTileSetID;

			if (Directory.Exists(base_path_1) == false)
			{
				try
				{
					if (Directory.Exists(base_path_0) == false)
					{
						Directory.CreateDirectory(base_path_0);
					}

					if (Directory.Exists(base_path_1) == false)
					{
						Directory.CreateDirectory(base_path_1);
					}

					handler(Future.succeededFuture());
				}
				catch (Exception e)
				{
					handler(Future.failedFuture(e));
				}
			}
			else
			{
				handler(Future.succeededFuture());
			}
		}

		static ProfilerMarker markerParseFromBuffer = new ProfilerMarker("LandTileLoader.parseFromBuffer");

		private void parseFromBuffer(bool write_to_file,Handler<AsyncResult<LandTile>> handler)
		{
			_multiThreadWorker.execute<LandTile>(promise => {
				try
				{
					LandTile tile = parseFromBuffer();

					if (write_to_file)
					{
						try
						{
							File.WriteAllBytes(_local_file_path, _compressedTileData);
						}
						catch (Exception e)
						{
							Debug.LogException(e);
						}
					}
					promise.complete(tile);
				}
				catch (System.Exception e)
				{
					promise.fail(e);
				}
			}, parse_result => {
				if (parse_result.failed())
				{
					handler(Future.failedFuture<LandTile>(parse_result.cause()));
				}
				else
				{
					_tile = parse_result.result();
					_cache.put(_cache_key, _tile);

					handler(Future.succeededFuture(parse_result.result()));
				}
			});
		}

		private LandTile parseFromBuffer()
		{
			markerParseFromBuffer.Begin();

			byte[] data = _compressedTileData;
			using (MemoryStream outputStream = new MemoryStream(), inputStream = new MemoryStream(data))
			{
				using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
				{
					zipStream.CopyTo(outputStream);
					outputStream.Position = 0;

					MessageUnpacker msgUnpacker = MessageUnpacker.create(outputStream);
					ObjectUnpacker objUnpacker = ObjectUnpacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);

					LandTile tile = (LandTile)objUnpacker.unpack(msgUnpacker);
					tile._tilePos = _tilePos;

					tile.postProcess();

					markerParseFromBuffer.End();

					return tile;
				}
			}	
		}

		private void loadFromLocalFile(Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<byte[]>(promise => {
				try
				{
					byte[] buffer = File.ReadAllBytes(_local_file_path);
					promise.complete(buffer);
				}
				catch (Exception e)
				{
					promise.fail(e);
				}

			}, result => {

				if (result.failed())
				{
					if (!(result.cause() is FileNotFoundException))
					{
						Debug.LogException(result.cause());
					}

					_stepList.Add(loadFromCDN);
					handler(Future.succeededFuture());
				}
				else
				{
					_compressedTileData = result.result();

					parseFromBuffer(false, parse_result => {
						if (parse_result.failed())
						{
							_stepList.Add(loadFromCDN);
							handler(Future.succeededFuture());
						}
						else
						{
							_stepList.Add(buildTileMesh);
							handler(Future.succeededFuture());
						}
					});
				}

			});
		}

		private void loadFromCDN(Handler<AsyncResult<Module.Void>> handler)
		{
			_cdnClient.readObject(_cdn_url, result => {
				if (result.failed())
				{
					System.Exception ex = result.cause();
					if (ex is HttpException)
					{
						HttpException http_ex = (HttpException)ex;
						if (http_ex.getResponseCode() != 404)
						{
							Debug.LogException(result.cause());
						}
					}
					else
					{
						Debug.LogException(result.cause());
					}

					_stepList.Add(loadFromServer);

					handler(Future.succeededFuture());
				}
				else
				{
					_compressedTileData = result.result();

					parseFromBuffer(true, parse_result => { 
					
						if( parse_result.failed())
						{
							Debug.LogException(parse_result.cause());
							_stepList.Add(loadFromServer);
							handler(Future.succeededFuture());
						}
						else
						{
							_stepList.Add(buildTileMesh);
							handler(Future.succeededFuture());
						}
					});
				}
			});
		}

		private void loadFromServer(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _network.createReq(CSMessageID.Map.GetLandTile);
			req.put("zoom", _tilePos.zoom);
			req.put("tile_x", _tilePos.tile_x);
			req.put("tile_y", _tilePos.tile_y);

			_network.callParallel(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					string cache_key = (string)ack.get("cacheKey");
					BlobData blob = (BlobData)ack.get("landtile_data");

					_compressedTileData = blob.getData();

					parseFromBuffer(true, parse_result => { 
						if( parse_result.failed())
						{
							handler(Future.failedFuture(parse_result.cause()));
						}
						else
						{
							_stepList.Add(buildTileMesh);
							handler(Future.succeededFuture());
						}
					});
				}
			});
		}

		private void buildTileMesh(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _tile._meshBuilt == true)
			{
				handler(Future.succeededFuture());
				return;
			}

			LandTileMeshBuilder builder = LandTileMeshBuilder.create(_tile);
			builder.run(result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_tile._meshBuilt = true;
					handler(Future.succeededFuture());
				}
			});
		}
	}
}
