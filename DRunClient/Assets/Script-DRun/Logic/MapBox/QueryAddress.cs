using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace DRun.Client.Logic.MapBox
{
	public class QueryAddress : BaseStepProcessor
	{
		private MBLongLatCoordinate _location;
		private string _url;
		private JsonObject _jsonResult;
		private string _addressString;

		public string getAddressString()
		{
			return _addressString;
		}

		public static QueryAddress create(MBLongLatCoordinate location)
		{
			QueryAddress step = new QueryAddress();
			step.init(location);
			return step;
		}

		private void init(MBLongLatCoordinate location)
		{
			base.init();
			_location = location;
			_url = string.Format("https://nominatim.openstreetmap.org/reverse.php?format=geojson&lat={0}&lon={1}&zoom=18", location.lat, location.lon);
		}

		protected override void buildSteps()
		{
			_stepList.Add(call);
			_stepList.Add(makeAddress);
		}

		private void call(Handler<AsyncResult<Void>> handler)
		{
			ClientMain.instance.StartCoroutine(_call(handler));
		}

		private IEnumerator _call(Handler<AsyncResult<Void>> handler)
		{
			UnityWebRequest request = UnityWebRequest.Get(_url);
			request.SetRequestHeader("Accept-Language", LanguageType.getAcceptLanguageValue(GlobalRefDataContainer.getStringCollection().getCurrentLangType()));

			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				handler(Future.failedFuture(new System.Exception(request.error)));
			}
			else
			{
				try
				{
					_jsonResult = new JsonObject(request.downloadHandler.text);
					handler(Future.succeededFuture());
				}
				catch(System.Exception e)
				{
					handler(Future.failedFuture(e));
				}
			}
		}

		private string[] addressKeyList =
		{
			"subdivision",
			"suburb",
			"borough",
			"district",
			"city-district",
			"village",
			"town",
			"city",
			"municipality"
		};

		private void makeAddress(Handler<AsyncResult<Void>> handler)
		{
			try
			{
				JsonArray feature_array = _jsonResult.getJsonArray("features");
				JsonObject feature = feature_array.getJsonObject(0);
				JsonObject properties = feature.getJsonObject("properties");
				JsonObject address = properties.getJsonObject("address");

				List<string> elements = new List<string>();
				foreach (string key in addressKeyList)
				{
					if (address.contains(key))
					{
						elements.Add(address.getString(key));
					}
				}

				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < 2 && i < elements.Count; ++i)
				{
					if (i > 0)
					{
						sb.Append(", ");
					}

					sb.Append(elements[i]);
				}

				_addressString = sb.ToString();
				handler(Future.succeededFuture());
			}
			catch (System.Exception e)
			{
				handler(Future.failedFuture(e));
			}
		}
	}
}
