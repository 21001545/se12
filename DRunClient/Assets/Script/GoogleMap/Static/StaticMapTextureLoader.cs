using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Festa.GoogleMap.Static
{
	public class StaticMapTextureLoader
	{
		public static IEnumerator Load(MapQueryConfig config,UnityAction<Texture> callback)
		{
			string url = config.makeURL();

			//Debug.Log(url);

			UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
			yield return request.SendWebRequest();
			if( request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(request.error);
				callback(null);
			}
			else
			{
				Texture tex = DownloadHandlerTexture.GetContent(request);
				callback(tex);
			}
		}
	}
}
