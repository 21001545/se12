using Festa.Client.Module;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Festa.Client.Module.Net
{
	public class HttpFileUploader
	{
		private MonoBehaviour _behaviour;
		private MultiThreadWorker _threadWorker;
		private string _url;
		private List<string> _file_list;

		private JsonObject _ack;

		private delegate IEnumerator StepProcessor(Action<AsyncResult<Void>> callback);
		private List<StepProcessor> _stepList;

		private WWWForm _form;

		private Action<int> _progressCallback;
		
		public void setProgressCallback(Action<int> callback)
		{
			_progressCallback = callback;
		}

		public static HttpFileUploader create(MonoBehaviour behaviour,MultiThreadWorker threadWorker,string url,List<string> file_list)
		{
			HttpFileUploader uploader = new HttpFileUploader();
			uploader.init(behaviour, threadWorker, url, file_list);
			return uploader;
		}

		private void init(MonoBehaviour behaviour,MultiThreadWorker threadWorker,string url,List<string> file_list)
		{
			_behaviour = behaviour;
			_threadWorker = threadWorker;
			_url = url;
			_file_list = file_list;
			_ack = new JsonObject();
			_ack.put("result", false);

			_stepList = new List<StepProcessor>();
			_stepList.Add(buildWWWForm);
			_stepList.Add(upload);
		}

		public void run(Action<JsonObject> callback)
		{
			runSteps(0, callback);
		}

		private void runSteps(int index,Action<JsonObject> callback)
		{
			if( index == _stepList.Count)
			{
				callback(_ack);
				return;
			}

			_behaviour.StartCoroutine(_stepList[index](result => { 
			
				if( result.failed())
				{
					Debug.LogException(result.cause());
					callback(_ack);
				}
				else
				{
					runSteps(index + 1, callback);
				}
			}));
		}

		private IEnumerator buildWWWForm(Action<AsyncResult<Void>> callback)
		{
			bool wait = true;

			_form = new WWWForm();
			_threadWorker.execute<Void>(promise => { 
			
				try
				{
					for(int i = 0; i < _file_list.Count; ++i)
					{
						int progress = i * 20 / _file_list.Count;
						MainThreadDispatcher.dispatch(() => {
							callProgressCallback( progress);
						});

						string path = _file_list[i];

						string filename = Path.GetFileName(path);
						byte[] data = File.ReadAllBytes(path);

						_form.AddBinaryData("file", data, filename, "application/octet-stream");

						Debug.Log(string.Format("filename[{0}] size[{1}]", filename, data.Length));
					}

					promise.complete();
				}
				catch(Exception e)
				{
					promise.fail(e);
				}
			}, result => { 
			
				if( result.failed())
				{
					callback(Future.failedFuture(result.cause()));
				}
				else
				{
					callback(Future.succeededFuture());
				}

			});


			yield return new WaitUntil( ()=> { return wait; });
		}

		private IEnumerator upload(Action<AsyncResult<Void>> callback)
		{
			UnityWebRequest req = UnityWebRequest.Post(_url, _form);
			UnityWebRequestAsyncOperation async = req.SendWebRequest();

			while(async.isDone == false)
			{
				yield return new WaitForSeconds(0.1f);

				float progress = async.progress;
				callProgressCallback(20 + (int)(progress * 80));
			}

			if (req.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError(req.error);
				callback(Future.failedFuture(new Exception(req.error)));
			}
			else
			{
				try
				{
					_ack = new JsonObject(req.downloadHandler.text);
					
					callback(Future.succeededFuture());
				}
				catch (Exception e)
				{
					callback(Future.failedFuture(e));
				}
			}

			req.Dispose();
		}

		private void callProgressCallback(int progress)
		{
			if( _progressCallback != null)
			{
				_progressCallback(progress);
			}
		}
	}
}
