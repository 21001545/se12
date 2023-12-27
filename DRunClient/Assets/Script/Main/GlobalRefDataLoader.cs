using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using Festa.Client.Module.Net;
using Festa.Client.RefData;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client
{
	// TODO: 임시 파일이 기기내에서 계속 쌓일 수 있는지 확인이 필요하다

	public class GlobalRefDataLoader
	{
		private string			_baseURL;
		private MonoBehaviour	_behaviour;
		private CDNClient _cdnClient;
		private MultiThreadWorker _multiThreadWorker;

		private string _local_revision_path;
		private string _local_refdata_path;

		private int _local_revision_value;
		private int _remote_revision_value;

		private delegate IEnumerator StepPrcessor(UnityAction<AsyncResult<Void>> callback);
		private List<StepPrcessor> _stepList;

		public static GlobalRefDataLoader create(string baseURL)
		{
			GlobalRefDataLoader loader = new GlobalRefDataLoader();
			loader.init(baseURL);
			return loader;
		}

		private void init(string baseURL)
		{
			_behaviour = ClientMain.instance;
			_baseURL = baseURL;
			_cdnClient = ClientMain.instance.getNetwork().getCDNClient();
			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();

			// 2021.07.22 하나의 클라에서 여기저기 접속 할 수 있음으로
			int url_id = EncryptUtil.makeHashCodePositive(_baseURL);

			_local_revision_path = Application.temporaryCachePath + string.Format("/ref_data/revision_{0}.json", url_id);
			_local_refdata_path = Application.temporaryCachePath + string.Format("/ref_data/ref_datas_{0}", url_id);

			_stepList = new List<StepPrcessor>();
			_stepList.Add(validateDirectory);
			_stepList.Add(readLocalRevision);
			_stepList.Add(readRemoteRevision);
		}

		private string makeTempFilePath()
		{
			return Application.temporaryCachePath + "/" + System.Guid.NewGuid().ToString();
		}

		public void run(UnityAction<bool> callback)
		{
			runSteps(0, callback);
		}

		private void runSteps(int index,UnityAction<bool> callback)
		{
			if( index == _stepList.Count)
			{
				Debug.Log("RefDataLoader Complete");
				callback(true);
				return;
			}

			_behaviour.StartCoroutine(_stepList[index]( result=> { 

				if( result.failed())
				{
					Debug.LogException(result.cause());
					callback(false);
				}
				else
				{
					runSteps(index + 1, callback);
				}
			
			}));
		}

		private IEnumerator validateDirectory(UnityAction<AsyncResult<Void>> callback)
		{
			string base_path = Application.temporaryCachePath + "/ref_data";

			if( Directory.Exists(base_path) == false)
			{
				try
				{
					Directory.CreateDirectory(base_path);

					callback(Future.succeededFuture());
				}
				catch(System.Exception e)
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

		private IEnumerator readLocalRevision(UnityAction<AsyncResult<Void>> callback)
		{
			try
			{
				string text = File.ReadAllText(_local_revision_path);
				JsonObject json = new JsonObject(text);

				_local_revision_value = json.getInteger("value");

				callback(Future.succeededFuture());
			}
			catch(System.Exception e)
			{
				if( (e is System.IO.FileNotFoundException) == false)
				{
					Debug.LogException(e);
				}

				_local_revision_value = 0;
				callback(Future.succeededFuture());
			}

			yield return null;
		}
	
		private IEnumerator readRemoteRevision(UnityAction<AsyncResult<Void>> callback)
		{
			string remote_revision_url = string.Format("{0}/refdata/revision.json", _baseURL);

			yield return _cdnClient.readObject(remote_revision_url, result => { 
				if( result.failed())
				{
					callback(Future.failedFuture(result.cause()));
				}
				else
				{
					try
					{
						string str = System.Text.Encoding.UTF8.GetString(result.result());
						JsonObject json = new JsonObject(str);

						_remote_revision_value = json.getInteger("value");

						Debug.Log(string.Format("check refdata revision: local[{0}] remote[{1}]", _local_revision_value, _remote_revision_value));

						// 
						if( _local_revision_value == 0 || _local_revision_value > _remote_revision_value)
						{
							_stepList.Add(fullDownloadRefData);
						}
						else if( _local_revision_value < _remote_revision_value)
						{
							_stepList.Add(patchRefData);
						}
						else if(_local_revision_value == _remote_revision_value)
						{
							_stepList.Add(readLocalRefData);
						}


						callback(Future.succeededFuture());
					}
					catch (System.Exception e)
					{
						callback(Future.failedFuture(e));
					}
				}
			});
		}

		private IEnumerator fullDownloadRefData(UnityAction<AsyncResult<Void>> callback)
		{
			Debug.Log("start fullDownloadRefData");

			string remote_refdata_url = string.Format("{0}/refdata/{1}/ref_datas", _baseURL, _remote_revision_value);
			string temp_file_path = makeTempFilePath();

			bool wait = true;
			yield return _cdnClient.readObjectToFile(remote_refdata_url, temp_file_path, result=>{ 
				if( result.failed())
				{
					callback(Future.failedFuture(result.cause()));
				}
				else
				{
					// 읽어본다
					loadRefData(temp_file_path, read_result => { 
						if( read_result.failed())
						{
							wait = false;
							callback(Future.failedFuture(read_result.cause()));
						}
						else
						{
							copyFile(temp_file_path, _local_refdata_path, copy_result => {

								// Client는 용량이 부족하다
								safeDelete(temp_file_path);

								wait = false;
								if ( copy_result.failed())
								{
									callback(Future.failedFuture(copy_result.cause()));
								}
								else
								{
									_stepList.Add(writeLocalRevision); 
									callback(Future.succeededFuture());
								}
							});
						}
					});
				}
			});

			yield return new WaitUntil(() => { return wait; });
		}

		private IEnumerator patchRefData(UnityAction<AsyncResult<Void>> callback)
		{
			Debug.Log("start patchRefData");

			List<string> patch_file_list = new List<string>();
			for(int i = _local_revision_value + 1; i <= _remote_revision_value; ++i)
			{
				patch_file_list.Add(string.Format("{0}/patch", i));
			}

			string temp_file_path = makeTempFilePath();

			bool wait = true;

			// 일단 안전하게 파일을 복사해놓고
			copyFile(_local_refdata_path, temp_file_path, copy_result=>{ 
				if( copy_result.failed())
				{
					wait = false;

					Debug.LogException(copy_result.cause());

					// 실패하면 전체 다운로드 시도
					_stepList.Add(fullDownloadRefData);
					callback(Future.succeededFuture());
				}
				else
				{
					_behaviour.StartCoroutine(applyPatchIterate(0, patch_file_list, temp_file_path, patch_result=> {
						if (patch_result.failed())
						{
							wait = false;

							Debug.LogException(patch_result.cause());

							// 실패하면 전체 다운로드 시도
							_stepList.Add(fullDownloadRefData);
							callback(Future.succeededFuture());
						}
						else
						{
							string final_refdata_path = patch_result.result();
							// 읽어 본다
							loadRefData(final_refdata_path, load_result =>
							{
								if (load_result.failed())
								{
									wait = false;
									Debug.LogException(load_result.cause());

									// 실패하면 전체 다운로드 시도
									_stepList.Add(fullDownloadRefData);
									callback(Future.succeededFuture());
								}
								else
								{
									// 복사
									copyFile(final_refdata_path, _local_refdata_path, copy_result => {
										wait = false;
										if ( copy_result.failed())
										{
											Debug.LogException(copy_result.cause());

											// 실패하면 전체 다운로드 시도
											_stepList.Add(fullDownloadRefData);
											callback(Future.succeededFuture());
										}
										else
										{
											_stepList.Add(writeLocalRevision);
											callback(Future.succeededFuture());
										}
									});
								}
							});
						}
					}));
				}
			});

			yield return new WaitUntil(() => { return wait; });
		}

		private IEnumerator applyPatchIterate(int index,List<string> patch_file_list,string old_file_path,UnityAction<AsyncResult<string>> callback)
		{
			if( index == patch_file_list.Count)
			{
				callback(Future.succeededFuture(old_file_path));
				yield break;
			}

			string patch_file_path = makeTempFilePath();
			string new_file_path = makeTempFilePath();
			string url = string.Format("{0}/refdata/{1}", _baseURL, patch_file_list[index]);
			yield return _cdnClient.readObjectToFile( url, patch_file_path, download_result=>{ 
				if( download_result.failed())
				{
					callback(Future.failedFuture<string>(download_result.cause()));
				}
				else
				{
					applyPatch(old_file_path, patch_file_path, new_file_path, patch_result => {

						safeDelete(old_file_path);
						safeDelete(patch_file_path);

						if( patch_result.failed())
						{
							callback(Future.failedFuture<string>(patch_result.cause()));
						}
						else
						{
							Debug.Log(string.Format("patch applied : {0}", patch_file_list[index]));

							_behaviour.StartCoroutine(applyPatchIterate(index + 1, patch_file_list, new_file_path, callback));
						}
					});
				}
			});
		}

		private IEnumerator readLocalRefData(UnityAction<AsyncResult<Void>> callback)
		{
			bool wait = true;

			loadRefData(_local_refdata_path, result => {

				wait = false;

				if (result.failed())
				{
					callback(Future.failedFuture(result.cause()));
				}
				else
				{
					callback(Future.succeededFuture());
				}
			});

			yield return new WaitUntil(() => { return wait; });
		}

		private IEnumerator writeLocalRevision(UnityAction<AsyncResult<Void>> callback)
		{
			Debug.Log(string.Format("write local revision:{0}", _remote_revision_value));

			try
			{
				JsonObject json = new JsonObject();
				json.put("value", _remote_revision_value);
				File.WriteAllText( _local_revision_path, json.encode());

				callback(Future.succeededFuture());
			}
			catch(System.Exception e)
			{
				callback(Future.failedFuture(e));
			}

			yield return null;
		}

		private void loadRefData(string file_path,UnityAction<AsyncResult<RefDataContainer>> callback)
		{
			_multiThreadWorker.execute<RefDataContainer>(promise => {

				RefDataContainer refdata_container = RefDataContainer.create();
				byte[] buffer = new byte[4096];

				using (ZipArchive archive = ZipFile.OpenRead(file_path))
				{
					foreach(ZipArchiveEntry entry in archive.Entries)
					{
						ObjectSchema schema = GlobalObjectFactory.getInstance().getSchema(EncryptUtil.makeHashCode(entry.Name));
						if( schema == null)
						{
							Debug.LogWarning( string.Format("unknown refdata class:{0}", entry.Name));
							continue;
						}
						
						Stream zip_entry_input_stream = entry.Open();
						MemoryStream memory_stream = new MemoryStream();

						while(true)
						{
							int read_bytes = zip_entry_input_stream.Read(buffer, 0, buffer.Length);
							if (read_bytes == 0)
							{
								break;
							}

							memory_stream.Write(buffer, 0, read_bytes);
						}

						memory_stream.Position = 0;

						MessageUnpacker msgUnpacker = MessageUnpacker.create(memory_stream);
						ObjectUnpacker objUnpacker = ObjectUnpacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);

						Dictionary<object, object> ref_data = (Dictionary<object,object>)objUnpacker.unpack(msgUnpacker);
						refdata_container.putRefData(schema.getClassType(), ref_data);

						zip_entry_input_stream.Close();
					}
				}

				refdata_container.buildCustomCollections();

				promise.complete(refdata_container);

			}, result => { 

				if( result.failed())
				{
					callback(Future.failedFuture<RefDataContainer>(result.cause()));
				}
				else
				{
					RefDataContainer refdata_container = result.result();
					GlobalRefDataContainer.setInstance(refdata_container);

					callback(Future.succeededFuture<RefDataContainer>(result.result()));
				}
			});
		}

		private void copyFile(string source_file_path,string target_file_path,UnityAction<AsyncResult<Void>> callback)
		{
			_multiThreadWorker.execute<Void>(promise => {

				File.Copy(source_file_path, target_file_path, true);

				promise.complete();

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
		}

		private void applyPatch(string old_file_path,string patch_file_path,string new_file_path,UnityAction<AsyncResult<Void>> callback)
		{
			_multiThreadWorker.execute<Void>(promise => {

				bool result = NativeModule.BSPatch(old_file_path, patch_file_path, new_file_path);
				if( result == false)
				{
					promise.fail("patch fail");
				}
				else
				{
					promise.complete();
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
		}
		
		private void safeDelete(string file_path)
		{
			try
			{
				File.Delete(file_path);
			}
			catch(System.Exception e)
			{
				Debug.LogException(e);
			}
		}
	}
}
