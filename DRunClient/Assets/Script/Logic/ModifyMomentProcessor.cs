using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Logic
{
	public class ModifyMomentProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private MakeMomentViewModel _data;
		private JobProgressItemViewModel _jobProgress;
		private string _serverJobKey;

		private List<int> _uploadPhotoIndex;

		public static ModifyMomentProcessor create()
		{
			ModifyMomentProcessor processor = new ModifyMomentProcessor();
			processor.init();
			return processor;
		}

		protected override void init()
		{
			base.init();

			_data = MakeMomentViewModel.clone(ViewModel.MakeMoment);
			_jobProgress = JobProgressItemViewModel.create(JobProgressItemViewModel.JobType.modify_moment);
			ViewModel.JobProgress.ListMoment.add(_jobProgress);
		}

		protected override void buildSteps()
		{
			_stepList.Add(uploadPhotos);
			_stepList.Add(uploadToStorage);
			_stepList.Add(reqModifyMoment);
			_stepList.Add(watchJobProgress);
		}

		public override void run(Handler<AsyncResult<Module.Void>> handler)
		{
			_jobProgress.Status = ClientJobProgressData.Status.running;
			_jobProgress.Progress = 0;

			runSteps(0, _stepList, _logStepTime, result => {
				ViewModel.JobProgress.ListMoment.remove(_jobProgress);
                handler(Future.succeededFuture());
            });
		}

		private void uploadPhotos(Handler<AsyncResult<Module.Void>> handler)
		{
			_uploadPhotoIndex = new List<int>();
			List<string> photo_file_list = new List<string>();

			for(int i = 0; i < _uploadPhotoIndex.Count; ++i)
			{
				MakeMomentViewModel.PhotoItem item = _data.PhotoList[i];
				if( item.photoContext != null)
				{
					photo_file_list.Add(item.photoContext.path);
					_uploadPhotoIndex.Add(i);
				}
			}

			// 올릴 파일이 없당
			if( photo_file_list.Count == 0)
			{
				handler(Future.succeededFuture());
				return;
			}

			HttpFileUploader uploader = Network.createFileUploader(photo_file_list);
			uploader.setProgressCallback(uploadPhotoProgressCallback);
			uploader.run(ack => { 
				if( ack.getInteger("result") != ResultCode.ok)
				{
					handler(Future.failedFuture(new Exception("upload photo fail")));
				}
				else
				{
					_data.UploadID = ack.getInteger("id");
					_data.AgentID = ack.getInteger("agent_id");

					handler(Future.succeededFuture());
				}
			});
		}

		private void uploadPhotoProgressCallback(int progress)
		{
			_jobProgress.Progress = progress * 50 / 100;
		}

		private void uploadToStorage(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _uploadPhotoIndex.Count == 0)
			{
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.Moment.UploadPhotoToStorageReq);
			req.put("agent_upload_id", _data.UploadID);
			req.put("agent_id", _data.AgentID);
			req.put("moent_id", _data.MomentID);

			Network.call(req, ack =>		{
				if (ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<string> urlList = ack.getList<string>("file");
					for (int i = 0; i < urlList.Count; ++i)
					{
						_data.PhotoList[i].set(urlList[i]); // file path를 upload된 url path로 치환
					}

					handler(Future.succeededFuture());
				}
			});
		}

		private void reqModifyMoment(Handler<AsyncResult<Module.Void>> handler)
		{
			_jobProgress.Progress = 70;

			MapPacket req = Network.createReq(CSMessageID.Moment.ModifyMomentReq);
			_data.modifyReq(req);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					ClientJobProgressData progressData = (ClientJobProgressData)ack.get("data");
					_serverJobKey = progressData.key;

					handler(Future.succeededFuture());
				}
			});
		}


		private void watchJobProgress(Handler<AsyncResult<Module.Void>> handler)
		{
			ClientMain.instance.StartCoroutine(coroutineWatchJobProgress(handler));
		}

		private IEnumerator coroutineWatchJobProgress(Handler<AsyncResult<Module.Void>> handler)
		{
			while (_jobProgress.Status == ClientJobProgressData.Status.running)
			{
				yield return new WaitForSeconds(0.3f);

				bool wait_ack = true;

				MapPacket req = Network.createReq(CSMessageID.Account.GetJobProgressReq);
				req.put("id", _serverJobKey);

				Network.call(req, ack => {
					wait_ack = false;

					if (ack.getResult() != ResultCode.ok)
					{
						_jobProgress.Status = ClientJobProgressData.Status.complete;
						_jobProgress.ResultCode = ack.getResult();
					}
					else
					{
						ClientJobProgressData data = (ClientJobProgressData)ack.get("data");

						_jobProgress.Status = data.status;
						_jobProgress.Progress = 70 + data.progress * 30 / 100;
						_jobProgress.Param = data.param;
						_jobProgress.ResultCode = data.result_code;

						Debug.Log($"status[{data.status}] progress[{data.progress}] param[{data.param}] result_code[{data.result_code}]");

						// 2022.6.24 이강희
						if (data.status == ClientJobProgressData.Status.complete
							&& data.result_code == ResultCode.ok
							&& data.ack_data != null)
						{
							MapPacket ackPacket = data.packetFromAckData();
							if (ackPacket != null)
							{
								ViewModel.updateFromPacket(ackPacket);
							}
						}

					}
				});

				yield return new WaitWhile(() => { return wait_ack; });
			}

			handler(Future.succeededFuture());
		}

	}
}
