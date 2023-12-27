using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.MsgPack;
using Festa.Client.Module.Net;
using Firebase.Crashlytics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class MBTileLoaderSingle : BaseStepProcessor
	{
		private string _baseURL;
		private CDNClient _cdnClient;
		private ClientNetwork _clientNetwork;
		private MultiThreadWorker _multiThreadWorker;

		private string _tileSetID;
		private MBTileCoordinate _tilePos;

		private string _local_file_path;
		private string _cdn_url;

		private byte[] _compressedTileData;

		private MBTile _tile;

		public MBTile getTile()
		{
			return _tile;
		}

		public static MBTileLoaderSingle create(string baseURL,string tileset_id,MBTileCoordinate tilePos)
		{
			MBTileLoaderSingle loader = new MBTileLoaderSingle();
			loader.init(baseURL, tileset_id, tilePos);
			return loader;
		}

		private void init(string baseURL,string tileset_id,MBTileCoordinate tilePos)
		{
			base.init();

			_baseURL = baseURL;
			_tileSetID = tileset_id;
			_tilePos = tilePos;

			_cdnClient = ClientMain.instance.getNetwork().getCDNClient();
			_clientNetwork = ClientMain.instance.getNetwork();
			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();

			_cdn_url = string.Format("{0}/mapbox/mbtile/{1}/{2}/{3}/{4}.mbtile", _baseURL, _tileSetID, _tilePos.zoom, _tilePos.tile_x, _tilePos.tile_y);
			_local_file_path = Application.temporaryCachePath + string.Format("/mbtile/{0}/{1}_{2}_{3}.mbtile", _tileSetID, _tilePos.zoom, _tilePos.tile_x, _tilePos.tile_y);

			Crashlytics.Log($"load mbtile:{_tileSetID}/{_tilePos.zoom}/{_tilePos.tile_x}/{_tilePos.tile_y}");
		}

		protected override void buildSteps()
		{
			_stepList = new List<StepProcessor>();
			_stepList.Add(validateDirectory);
			_stepList.Add(loadFromLocalFile);
		}

		private void validateDirectory(Handler<AsyncResult<Module.Void>> callback)
		{
			string base_path_0 = Application.temporaryCachePath + "/mbtile";
			string base_path_1 = base_path_0 + "/" + _tileSetID;

			if(Directory.Exists(base_path_1) == false)
			{
				try
				{
					if( Directory.Exists(base_path_0) == false)
					{
						Directory.CreateDirectory(base_path_0);
					}

					if( Directory.Exists(base_path_1) == false)
					{
						Directory.CreateDirectory(base_path_1);
					}

					callback(Future.succeededFuture());
				}
				catch(Exception e)
				{
					callback(Future.failedFuture(e));
				}
			}
			else
			{
				callback(Future.succeededFuture());
			}
		}

		private void loadFromLocalFile(Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<MBTile>(promise => {

				try
				{
					_compressedTileData = File.ReadAllBytes(_local_file_path);
					MBTile tile = parseFromBuffer();

					promise.complete(tile);
				}
				catch (System.Exception e)
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
					_tile = result.result();

					//Debug.Log(string.Format("load from local file : {0}", _cache_key));

					handler(Future.succeededFuture());
				}

			});
		}

		static ProfilerMarker markerParseFromBuffer = new ProfilerMarker("MBTileLoaderSingle.parseFromBuffer");

		private MBTile parseFromBuffer()
		{
			markerParseFromBuffer.Begin();

			//long begin = TimeUtil.unixTimestampUtcNow();

			byte[] data = _compressedTileData;
			using (MemoryStream outputStream = new MemoryStream(), inputStream = new MemoryStream(data))
			{
				using (GZipStream zipStream = new GZipStream(inputStream, CompressionMode.Decompress))
				{
					zipStream.CopyTo(outputStream);

					outputStream.Position = 0;

					MessageUnpacker msgUnpacker = MessageUnpacker.create(outputStream);
					ObjectUnpacker objUnpacker = ObjectUnpacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);

					MBTile tile = (MBTile)objUnpacker.unpack(msgUnpacker);
					tile._tilePos = _tilePos;
					tile.postProcess();

					//long end = TimeUtil.unixTimestampUtcNow();

					//Debug.Log(string.Format("unpack vector tile : {0}ms", end - begin));

					markerParseFromBuffer.End();

					return tile;
				}
			}
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

					_multiThreadWorker.execute<MBTile>(promise => {

						try
						{
							MBTile tile = parseFromBuffer();

							// 성공하면 저장도 여기서 하자, 에러나도 무시
							try
							{
								File.WriteAllBytes(_local_file_path, _compressedTileData);
							}
							catch (System.Exception e)
							{
								Debug.LogException(e);
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
							Debug.LogException(parse_result.cause());
							_stepList.Add(loadFromServer);

							handler(Future.succeededFuture());
						}
						else
						{
							_tile = parse_result.result();

							//Debug.Log(string.Format("load from CDN : {0}", _cache_key));

							handler(Future.succeededFuture());
						}
					});
				}
			});
		}

		private void loadFromServer(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = _clientNetwork.createReq(CSMessageID.Map.GetVectorTile);
			req.put("tileset_id", _tileSetID);
			req.put("zoom", _tilePos.zoom);
			req.put("tile_x", _tilePos.tile_x);
			req.put("tile_y", _tilePos.tile_y);
			req.put("option", true);

			_clientNetwork.callParallel(req, ack => {
				if (ack.getResult() != ResultCode.ok)
				{
					//Debug.LogError(ack.get(MapPacketKey.error_message));

					// 와 더이상 물러설대가 없다
					//handler(Future.failedFuture(new System.Exception("GetVectorTile fail")));
					
					
					handler(Future.succeededFuture());
				}
				else
				{
					string cache_key = (string)ack.get("cacheKey");
					BlobData blob = (BlobData)ack.get("vectortile_data");

					// 그럴 수 있다
					if( blob == null)
					{
						Debug.Log($"blank vector tile: {cache_key}");
						handler(Future.succeededFuture());
						return;
					}

					//Debug.Log(string.Format("laodFromServer:key[{0}] buffer[{1}]", cache_key, blob.getData().Length));

					_compressedTileData = blob.getData();

					_multiThreadWorker.execute<MBTile>(promise => {

						try
						{
							MBTile tile = parseFromBuffer();

							// 성공하면 저장도 여기서 하자, 에러나도 무시
							try
							{
								File.WriteAllBytes(_local_file_path, _compressedTileData);
							}
							catch (System.Exception e)
							{
								Debug.LogException(e);
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
							handler(Future.failedFuture(parse_result.cause()));
						}
						else
						{
							_tile = parse_result.result();

							//Debug.Log(string.Format("load from Server : {0}", _cache_key));

							handler(Future.succeededFuture());
						}
					});

				}
			});
		}
	}
}
