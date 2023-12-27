using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Festa.Client.Module.Net
{
	public class CDNImageDownloader : CoroutineStepProcessor
	{
		private CDNClient _cdn_client;
		private string _url;
		private int _call_id;
		private string _file_path;
		private int _cacheKey;
		private bool _isHEIC;
		private MultiThreadWorker _thread_worker;
		private TextureCache _textureCache;

		private Texture _texture;
		private TextureCacheItemUsage _textureUsage;

		private static Dictionary<string, int> _loadingMap = new Dictionary<string, int>();

		public static bool tryLoading(string url)
		{
			lock(_loadingMap)
			{
				if( _loadingMap.ContainsKey( url) == false)
				{
					_loadingMap.Add(url, 1);
					return true;
				}
				else
				{
					return false;
				}
			}
		}

		public static void endLoading(string url)
		{
			lock(_loadingMap)
			{
				if( _loadingMap.ContainsKey(url))
				{
					_loadingMap.Remove(url);
				}
			}
		}

		public static CDNImageDownloader create(CDNClient cdn_client,MultiThreadWorker thread_worker,TextureCache textureCache,string url,int call_id)
		{
			CDNImageDownloader loader = new CDNImageDownloader();
			loader.init(cdn_client, thread_worker, textureCache, url, call_id);
			return loader;
		}

		public static bool validateCacheDirectory()
		{
			string path = Application.temporaryCachePath + "/image_cache";
			if (Directory.Exists(path) == false)
			{
				try
				{
					Directory.CreateDirectory(path);
					return true;
				}
				catch(Exception e)
				{
					Debug.LogException(e);
					return false;
				}
			}

			return true;
		}

		private void init(CDNClient cdn_client,MultiThreadWorker thread_worker,TextureCache textureCache,string url,int call_id)
		{
			base.init(cdn_client);

			_cdn_client = cdn_client;
			_url = url;
			_call_id = call_id;
			_thread_worker = thread_worker;
			_textureCache = textureCache;
			_isHEIC = _url.EndsWith("HEIC");

			_file_path = Application.temporaryCachePath + "/image_cache/" + EncryptUtil.makeHashCodePositive(url) + Path.GetExtension(url);
			_cacheKey = EncryptUtil.makeHashCode(_url);
		}

		protected override void buildSteps()
		{
			_stepList.Add(loadFromCache);
		}

		public void run(UnityAction<int, TextureCacheItemUsage> callback)
		{
			if( tryLoading( _url) == false)
			{
				//Debug.Log(string.Format( string.Format("image already loading: {0}", _url)));

				MainThreadDispatcher.dispatchFixedUpdate(() => {
					run(callback);
				});
			}
			else
			{
				//Debug.Log($"start loading : call_id[{_call_id}] url[{_url}]");

				runSteps(0, result => {
					//Debug.Log($"end loading : call_id[{_call_id}] url[{_url}] result[{result.succeeded()}]");

					endLoading(_url);

					if (result.failed())
					{
						Debug.LogException(result.cause());
					}

					callback(_call_id, _textureUsage);   // 실패했으면 texture == null이겠지
					_texture = null;
				});
			}
		}

		private IEnumerator loadFromCache(Action<AsyncResult<Void>> handler)
		{
			_textureUsage = _textureCache.makeUsage(_cacheKey);
			if( _textureUsage != null)
			{
				_texture = _textureUsage.texture;
				handler(Future.succeededFuture());
				yield break;
			}

			bool wait = true;
			loadTextureFromFile(_file_path, _isHEIC, result => {
				wait = false;
				if (result.failed())
				{
					_stepList.Add(loadFromCDN);
					handler(Future.succeededFuture());
				}
				else
				{
					_texture = result.result();
					_textureCache.registerCache(_cacheKey, _texture);
					_textureUsage = _textureCache.makeUsage(_cacheKey);

					handler(Future.succeededFuture());
				}

			});

			yield return new WaitWhile(() => { return wait; });
		}

		private IEnumerator loadFromCDN(Action<AsyncResult<Void>> handler)
		{
			bool wait = true;

			_cdn_client.readObjectToFile(_url, _file_path, result => { 
				if( result.failed())
				{
					wait = false;
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					loadTextureFromFile(_file_path, _isHEIC, load_result => {
						wait = false;

						if ( load_result.failed())
						{
							// 지우자
							File.Delete(_file_path);
							handler(Future.failedFuture(load_result.cause()));
						}
						else
						{
							_texture = load_result.result();
							_textureCache.registerCache(_cacheKey, _texture);
							_textureUsage = _textureCache.makeUsage(_cacheKey);
							handler(Future.succeededFuture());
						}
					});
				}
			});


			yield return new WaitWhile(() => { return wait; });
		}

		private void prepareHEIC(string file_path,Action<string> callback)
		{
			if( _isHEIC)
			{
				callback(NativeGallery.GetImageMiniThumbnailPathFromImagePath(file_path, 400, 400));
			}
			else
			{
				callback(file_path);
			}
		}

		private void loadTextureFromFile(string file_path,bool isHEIC,Action<AsyncResult<Texture2D>> handler)
		{
			prepareHEIC(file_path, new_file_path => {
				/*
								_thread_worker.execute<byte[]>(promise => {

									try
									{
										//if (isHEIC)
										//{
										//	file_path = NativeGallery.GetImageMiniThumbnailPathFromImagePath(file_path, 400, 400);
										//}

										byte[] buffer = File.ReadAllBytes(new_file_path);
										promise.complete(buffer);
									}
									catch (Exception e)
									{
										promise.fail(e);
									}

								}, result => {
									if (result.failed())
									{
										handler(Future.failedFuture<Texture2D>(result.cause()));
									}
									else
									{
										byte[] buffer = result.result();

										Texture2D tex = new Texture2D(2, 2);
										if (tex.LoadImage(buffer) == false)
										{
											UnityEngine.Object.DestroyImmediate(tex);
											handler(Future.failedFuture<Texture2D>(new Exception("loadImage fail")));
										}
										else
										{
											handler(Future.succeededFuture<Texture2D>(tex));
										}
									}
								});
				*/

				_behaviour.StartCoroutine(loadTextureFromFileCoroutine(new_file_path, handler));
			});
		}

		private IEnumerator loadTextureFromFileCoroutine(string file_path,Action<AsyncResult<Texture2D>> handler)
		{
			//Debug.Log($"load texture from file: {file_path}");

			string url;
			url = "file://" + file_path;
			UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				handler( Future.failedFuture<Texture2D>( new Exception($"{request.error},{file_path}")));
			}
			else
			{
				Texture2D texture = DownloadHandlerTexture.GetContent(request);
				texture.name = $"{file_path}_{texture.width}_{texture.height}_{texture.format}";
				handler(Future.succeededFuture(texture));
			}
			request.Dispose();
		}
	}
}
