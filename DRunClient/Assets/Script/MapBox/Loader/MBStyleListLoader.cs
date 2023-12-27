using Festa.Client.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace Festa.Client.MapBox
{
	public class MBStyleListLoader : CoroutineStepProcessor
	{
		private string _url;
		private JsonArray _styleList;
		private Dictionary<string, DateTime> _styleModifiedTime;

		public static MBStyleListLoader create(MonoBehaviour behavour)
		{
			MBStyleListLoader loader = new MBStyleListLoader();
			loader.init(behavour);
			return loader;
		}

		private static string _userID = "ke7789";
		private static string _accessToken = "sk.eyJ1Ijoia2U3Nzg5IiwiYSI6ImNsM2lkZXR1aTA2NHkzY283djV6ZzZwa3MifQ.SZrwLXaMgS3MJJ0p1Ej0wg";

		//sk.eyJ1Ijoia2U3Nzg5IiwiYSI6ImNsM2lkZXR1aTA2NHkzY283djV6ZzZwa3MifQ.SZrwLXaMgS3MJJ0p1Ej0wg

		public JsonArray getStyleList()
		{
			return _styleList;
		}

		public Dictionary<string,DateTime> getStyleModifiedTime()
		{
			return _styleModifiedTime;
		}

		protected override void init(MonoBehaviour behaviour)
		{
			base.init(behaviour);

			_url = $"https://api.mapbox.com/styles/v1/{_userID}?access_token={_accessToken}";
		}


		public void run(Action<AsyncResult<Module.Void>> callback)
		{
			runSteps(0, result => {
				if (result.failed())
				{
					Debug.LogException(result.cause());
				}

				callback(result);
			});
		}

		protected override void buildSteps()
		{
			_stepList.Add(downloadStyleList);
		}

		private IEnumerator downloadStyleList(Action<AsyncResult<Module.Void>> handler)
		{
			UnityWebRequest request = UnityWebRequest.Get(_url);
			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				handler(Future.failedFuture(new Exception(request.error)));
			}
			else
			{
				try
				{
					string text = request.downloadHandler.text;
					//Debug.Log(text);
					_styleList = new JsonArray(text);
					_styleModifiedTime = new Dictionary<string,DateTime>();

					for(int i = 0; i < _styleList.size(); ++i)
					{
						JsonObject json = _styleList.getJsonObject(i);
						string id = json.getString("id");
						string modified = json.getString("modified");

						DateTime modifiedDate;
						if( TimeUtil.tryParseISO8601(modified, out modifiedDate))
						{
							_styleModifiedTime.Add(id, modifiedDate);

							//Debug.Log($"{id}:{modifiedDate}");
						}
					}

					handler(Future.succeededFuture());
				}
				catch(Exception e)
				{
					handler(Future.failedFuture(e));
				}
			}
		}
	}
}
