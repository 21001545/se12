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
	/*
	 * Step1. 사진이 있을 경우 서버에 사진을 먼저 전송한다
	 * Step2. 사진 전송이 끝나면 Moment 생성을 요청한다
	 * Step3. Moment 생성 및 Feed 갱신이 끝날때 까지 기다린다 (progress감시)
	*/

	public class MakeMomentProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private MakeMomentViewModel _data;
		private JobProgressItemViewModel _jobProgress;

		private string _serverJobKey;

		public static MakeMomentProcessor create()
		{
			MakeMomentProcessor processor = new MakeMomentProcessor();
			processor.init();
			return processor;
		}

		protected override void init()
		{
			base.init();

			_data = MakeMomentViewModel.clone(ViewModel.MakeMoment);
			_jobProgress = JobProgressItemViewModel.create(JobProgressItemViewModel.JobType.make_moment);

			if( _data.PhotoList.Count > 0)
			{
				_jobProgress.Photo = _data.PhotoList[0].photoContext;
			}

			ViewModel.JobProgress.ListMoment.add(_jobProgress);
		}

		protected override void buildSteps()
		{
			_stepList.Add(uploadPhotos);
			_stepList.Add(reqMakeMoment);
			_stepList.Add(watchJobProgress);
		}

		public override void run(Handler<AsyncResult<Module.Void>> handler)
		{
			_jobProgress.Status = ClientJobProgressData.Status.running;
			_jobProgress.Progress = 0;

			runSteps(0, _stepList, _logStepTime, result => {
				ViewModel.JobProgress.ListMoment.remove(_jobProgress);
			});
		}

		private void uploadPhotos(Handler<AsyncResult<Module.Void>> handler)
		{
			List<string> photo_file_list = new List<string>();
			foreach(MakeMomentViewModel.PhotoItem item in _data.PhotoList)
			{
				photo_file_list.Add( item.photoContext.path);
			}

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

		private void reqMakeMoment(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Moment.MakeReq);
			_data.makeReq(req);

			Network.call(req, ack => {
				if (ack.getResult() != ResultCode.ok)
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
			while( _jobProgress.Status == ClientJobProgressData.Status.running)
			{
				yield return new WaitForSeconds(0.3f);

				bool wait_ack = true;

				MapPacket req = Network.createReq(CSMessageID.Account.GetJobProgressReq);
				req.put("id", _serverJobKey);

				Network.call(req, ack => {
					wait_ack = false;

					if ( ack.getResult() != ResultCode.ok)
					{
						_jobProgress.Status = ClientJobProgressData.Status.complete;
						_jobProgress.ResultCode = ack.getResult();
					}
					else
					{
						ClientJobProgressData data = (ClientJobProgressData)ack.get("data");

						_jobProgress.Status = data.status;
						_jobProgress.Progress = 50 + data.progress * 50 / 100;
						_jobProgress.Param = data.param;
						_jobProgress.ResultCode = data.result_code;

						Debug.Log($"status[{data.status}] progress[{data.progress}] param[{data.param}] result_code[{data.result_code}]");
					
						// 이강희
						if( data.status == ClientJobProgressData.Status.complete 
							&& data.result_code == ResultCode.ok
							&& data.ack_data != null)
						{
							MapPacket ackPacket = data.packetFromAckData();
							if( ackPacket != null)
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
