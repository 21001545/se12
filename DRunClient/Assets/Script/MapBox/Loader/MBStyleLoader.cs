using Festa.Client.Module;
using Festa.Client.Module.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Festa.Client.MapBox
{
	public class MBStyleLoader : CoroutineStepProcessor
	{
		private MultiThreadWorker _multiThreadWorker;
		private MBStyleExpressionParser _expressionParser;

		private string _styleID;
		private string _localFilePathStyle;
		private string _localFileSpritePNG;
		private string _localFileSpriteJSON;
		private string _urlStyle;
		private string _urlSpritePNG;
		private string _urlSpriteJSON;

		private static string _userID;
		private static string _accessToken;

		private bool _loadFromFileCacheResult;

		private MBStyle _mbStyle;
		private Texture2D _textureSpritePNG;
		private JsonObject _jsonSprite;
		private Dictionary<string, Sprite> _dicSprite;
		private Dictionary<string, DateTime> _modifiedTimeMap;

		public MBStyle getMBStyle()
		{
			return _mbStyle;
		}

		public static MBStyleLoader create(string style_id,Dictionary<string,DateTime> modifiedTimeMap,MonoBehaviour behaviour)
		{
			MBStyleLoader loader = new MBStyleLoader();
			loader.init(style_id, modifiedTimeMap, behaviour);
			return loader;
		}

		private void init(string style_id, Dictionary<string, DateTime> modifiedTimeMap,MonoBehaviour behaviour)
		{
			base.init(behaviour);
			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();
			_expressionParser = MBStyleExpressionParser.create(MBStyleExpressionFactory.create());
			_modifiedTimeMap = modifiedTimeMap;

			_userID = MBAccess.userID;
			_accessToken = MBAccess.accessToken;
			_styleID = style_id;
			_localFilePathStyle = Application.temporaryCachePath + $"/mbstyle/{_styleID}.json";
			_localFileSpritePNG = Application.temporaryCachePath + $"/mbstyle/{_styleID}_Sprite.png";
			_localFileSpriteJSON = Application.temporaryCachePath + $"/mbstyle/{_styleID}_Sprite.json";

			_urlStyle = $"https://api.mapbox.com/styles/v1/{_userID}/{_styleID}?access_token={_accessToken}";
			_urlSpritePNG = $"https://api.mapbox.com/styles/v1/{_userID}/{_styleID}/sprite@2x.png?access_token={_accessToken}";
			_urlSpriteJSON = $"https://api.mapbox.com/styles/v1/{_userID}/{_styleID}/sprite@2x.json?access_token={_accessToken}";
		}

		public void run(Action<AsyncResult<Module.Void>> callback)
		{
			runSteps(0, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
				}

				callback(result);
			});
		}

		private IEnumerator validateDirectory(Action<AsyncResult<Module.Void>> callback)
		{
			string base_path = Application.temporaryCachePath + "/mbstyle";
			if(Directory.Exists(base_path) == false)
			{
				try
				{
					Directory.CreateDirectory(base_path);
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

			yield return null;
		}

		protected override void buildSteps()
		{
			_stepList.Add(validateDirectory);
			_stepList.Add(loadFromFileCache_Style);
			_stepList.Add(loadFromFileCache_SpritePNG);
			_stepList.Add(loadFromFileCache_SpriteJSON);

			_stepList.Add(downloadStyle);
			_stepList.Add(downloadSpritePNG);
			_stepList.Add(downloadSpriteJSON);

			_stepList.Add(generateSprite);
		}

		private IEnumerator loadFromFileCache_Style(Action<AsyncResult<Module.Void>> handler)
		{
			_loadFromFileCacheResult = true;

			// 앞단에서 실패해서 다음으로 넘어감
			if (_loadFromFileCacheResult == false)
			{
				handler(Future.succeededFuture());
				yield break;
			}

			_multiThreadWorker.execute<string>(promise => { 
				
				try
				{
					string text = File.ReadAllText(_localFilePathStyle);

					promise.complete(text);
				}
				catch(Exception e)
				{
					promise.fail(e);
				}
			
			}, result => { 
			
				if( result.failed())
				{
					_loadFromFileCacheResult = false;
					handler(Future.succeededFuture());
				}
				else
				{
					parseStyle(result.result(), parse_result => { 
						if( parse_result.failed())
						{
							_loadFromFileCacheResult = false;
						}
						else
						{
							MBStyle style = parse_result.result();

							bool isModified = true;

							DateTime modifiedTime;
							if(_modifiedTimeMap.TryGetValue( _styleID, out modifiedTime))
							{
								if (style.getModifiedTime() == modifiedTime)
								{
									isModified = false;

									Debug.Log($"style not modified: {_styleID}, {style.getModifiedTime()} == {modifiedTime}");
								}
								else
								{
									Debug.Log($"style modified : {_styleID}, {style.getModifiedTime()} <> {modifiedTime}");
								}
							}

							if( isModified)
							{
								_loadFromFileCacheResult = false;
							}
							else
							{
								_loadFromFileCacheResult = true;
								_mbStyle = style;
							}


							//Debug.Log($"loadFromFileCache success: {_localFilePathStyle}");
						}

						handler(Future.succeededFuture());
					});

				}
			});

			yield return null;
		}

		private IEnumerator loadFromFileCache_SpritePNG(Action<AsyncResult<Module.Void>> handler)
		{
			// 앞단에서 실패해서 다음으로 넘어감
			if( _loadFromFileCacheResult == false)
			{
				handler(Future.succeededFuture());
				yield break;
			}

			UnityWebRequest request = UnityWebRequestTexture.GetTexture("file://" + _localFileSpritePNG);
			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				_loadFromFileCacheResult = false;
				handler(Future.succeededFuture());
			}
			else
			{
				_loadFromFileCacheResult = true;
				_textureSpritePNG = DownloadHandlerTexture.GetContent(request);

				//Debug.Log($"loadFromFileCache success: {_localFileSpritePNG}");

				handler(Future.succeededFuture());
			}

			request.Dispose();
		}

		private IEnumerator loadFromFileCache_SpriteJSON(Action<AsyncResult<Module.Void>> handler)
		{
			if( _loadFromFileCacheResult == false)
			{
				handler(Future.succeededFuture());
				yield break;
			}

			_multiThreadWorker.execute<string>(promise => {

				try
				{
					string text = File.ReadAllText( _localFileSpriteJSON);

					promise.complete(text);
				}
				catch (Exception e)
				{
					promise.fail(e);
				}

			}, result => {

				if (result.failed())
				{
					_loadFromFileCacheResult = false;
					handler(Future.succeededFuture());
				}
				else
				{
					parseSpriteJSON(result.result(), parse_result => {
						if (parse_result.failed())
						{
							_loadFromFileCacheResult = false;
						}
						else
						{
							_loadFromFileCacheResult = true;
							_jsonSprite = parse_result.result();

							//Debug.Log($"loadFromFileCache success: {_localFileSpriteJSON}");
						}

						handler(Future.succeededFuture());
					});
				}
			});
		}

		private void parseStyle(string data,Action<AsyncResult<MBStyle>> handler)
		{
			_multiThreadWorker.execute<MBStyle>(promise => {

				try
				{
					JsonObject json = new JsonObject(data);
					MBStyle style = MBStyle.create(json, _expressionParser);

					//Debug.Log($"parseStyle:{_styleID}, version[{json.getInteger("version")}]");

					promise.complete(style);
				}
				catch (Exception e)
				{
					promise.fail(e);
				}

			},
			result => {

				if (result.failed())
				{
					handler(Future.failedFuture<MBStyle>(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture(result.result()));
				}

			});
		}

		private void parseSpriteJSON(string data,Action<AsyncResult<JsonObject>> handler)
		{
			_multiThreadWorker.execute<JsonObject>(promise => {
				try
				{
					JsonObject spriteJson = new JsonObject(data);
					promise.complete(spriteJson);
				}
				catch (Exception e)
				{
					promise.fail(e);
				}
			}, result => {
				if (result.failed())
				{
					handler(Future.failedFuture<JsonObject>(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture(result.result()));
				}
			});
		}

		private IEnumerator downloadStyle(Action<AsyncResult<Module.Void>> handler)
		{
			if (_loadFromFileCacheResult == true)
			{
				//Debug.Log($"loadStyle from cache: {_styleID}");

				handler(Future.succeededFuture());
				yield break;
			}

			UnityWebRequest request = new UnityWebRequest(_urlStyle);
			request.downloadHandler = new DownloadHandlerBuffer();

			yield return request.SendWebRequest();

			//Debug.Log(_urlStyle);

			if (request.result != UnityWebRequest.Result.Success)
			{
				handler(Future.failedFuture(new Exception($"{request.error}")));
			}
			else
			{
				string textStyle = request.downloadHandler.text;

				parseStyle(textStyle, parse_result => {
					if (parse_result.failed())
					{
						handler(Future.failedFuture(parse_result.cause()));
					}
					else
					{
						_mbStyle = parse_result.result();
						handler(Future.succeededFuture());

						Debug.Log($"download success: {_urlStyle}");
						File.WriteAllText(_localFilePathStyle, textStyle);
					}
				});
			}
		}

		private IEnumerator downloadSpritePNG(Action<AsyncResult<Module.Void>> handler)
		{
			if( _loadFromFileCacheResult == true)
			{
				handler(Future.succeededFuture());
				yield break;
			}

			UnityWebRequest request = UnityWebRequestTexture.GetTexture(_urlSpritePNG);
			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				handler(Future.failedFuture(new Exception($"{request.error}")));
			}
			else
			{
				_textureSpritePNG = DownloadHandlerTexture.GetContent(request);
				handler(Future.succeededFuture());

				File.WriteAllBytes(_localFileSpritePNG, request.downloadHandler.data);

				Debug.Log($"download success: {_urlSpritePNG}");

				Debug.Log($"style[{_styleID}] sptite:{_textureSpritePNG.width}x{_textureSpritePNG.height}");
			}

			request.Dispose();
		}

		private IEnumerator downloadSpriteJSON(Action<AsyncResult<Module.Void>> handler)
		{
			if( _loadFromFileCacheResult == true)
			{
				handler(Future.succeededFuture());
				yield break;
			}

			UnityWebRequest request = new UnityWebRequest(_urlSpriteJSON);
			request.downloadHandler = new DownloadHandlerBuffer();
			yield return request.SendWebRequest();
			
			if( request.result != UnityWebRequest.Result.Success)
			{
				handler(Future.failedFuture(new Exception($"{request.error}")));
			}
			else
			{
				string textSpriteJSON = request.downloadHandler.text;

				parseSpriteJSON(textSpriteJSON, parse_result =>
				{
					if (parse_result.failed())
					{
						handler(Future.failedFuture(parse_result.cause()));
					}
					else
					{
						_jsonSprite = parse_result.result();
						handler(Future.succeededFuture());

						Debug.Log($"download success: {_urlSpriteJSON}");

						File.WriteAllText(_localFileSpriteJSON, textSpriteJSON);
					}
				});
			}
		}

		private IEnumerator generateSprite(Action<AsyncResult<Module.Void>> handler)
		{
			_dicSprite = new Dictionary<string, Sprite>();
			foreach(KeyValuePair<string,object> item in _jsonSprite.getMap())
			{
				//yield return null;
				JsonObject jsonSprite = _jsonSprite.getJsonObject(item.Key);
				int x, y, width, height;
				x = jsonSprite.getInteger("x");
				y = jsonSprite.getInteger("y");
				width = jsonSprite.getInteger("width");
				height = jsonSprite.getInteger("height");

				Rect rect = new Rect(x, _textureSpritePNG.height - y - height, width, height);
				Vector2 pivot = new Vector2(0.5f, 0.5f);

				Sprite sprite = Sprite.Create(_textureSpritePNG, rect, pivot, 100,1,SpriteMeshType.FullRect);
				sprite.name = item.Key;

				_dicSprite.Add(item.Key, sprite);
			}

			_mbStyle.setSpriteDic(_dicSprite);
			handler(Future.succeededFuture());
			yield return null;
		}
	}
}
