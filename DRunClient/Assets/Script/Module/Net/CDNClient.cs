using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Festa.Client.Module.Net
{
	public class CDNClient : MonoBehaviour
	{
		public static CDNClient create(GameObject target)
		{
			CDNClient client = target.AddComponent<CDNClient>();
			client.init();
			return client;
		}

		private void init()
		{

		}

		public Coroutine readObject(string url,UnityAction<AsyncResult<byte[]>> callback)
		{
			return StartCoroutine(_readObject(url, callback));
		}

		public Coroutine readObjectToFile(string url,string file_path,UnityAction<AsyncResult<Void>> callback)
		{
			return StartCoroutine(_readObjectToFile(url, file_path, callback));
		}

		private IEnumerator _readObject(string url,UnityAction<AsyncResult<byte[]>> callback)
		{
			//Debug.Log(string.Format("readObject:{0}", url));

			UnityWebRequest request = new UnityWebRequest(url);
			request.downloadHandler = new DownloadHandlerBuffer();

			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				callback(Future.failedFuture<byte[]>(new HttpException(request)));
			}
			else
			{
				callback(Future.succeededFuture<byte[]>(request.downloadHandler.data));
			}

			request.Dispose();
		}

		private IEnumerator _readObjectToFile(string url,string file_path,UnityAction<AsyncResult<Void>> callback)
		{
			UnityWebRequest request = new UnityWebRequest(url);
			request.downloadHandler = new DownloadHandlerFile(file_path);

			yield return request.SendWebRequest();

			if( request.result != UnityWebRequest.Result.Success)
			{
				callback(Future.failedFuture(new HttpException(request)));
			}
			else
			{
				callback(Future.succeededFuture());
			}

			request.Dispose();
		}


	}
}
