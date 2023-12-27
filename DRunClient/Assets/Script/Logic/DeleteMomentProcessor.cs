using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System.Collections;
using UnityEngine;

namespace Festa.Client.Logic
{
	// 작업이 끝나면 feed갱신, bookmark갱신, moment_list(profile창) 갱신 

	public class DeleteMomentProcessor : BaseStepProcessor
	{
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private int _moment_id;
		private JobProgressItemViewModel _jobProgress;
		private string _serverJobKey;

		public static DeleteMomentProcessor create(int moment_id)
		{
			DeleteMomentProcessor processor = new DeleteMomentProcessor();
			processor.init(moment_id);
			return processor;
		}

		private void init(int moment_id)
		{
			base.init();

			_moment_id = moment_id;
			_jobProgress = JobProgressItemViewModel.create(JobProgressItemViewModel.JobType.delete_moment);
			ViewModel.JobProgress.ListMoment.add(_jobProgress);
		}

		protected override void buildSteps()
		{
			_stepList.Add(reqDeleteMoment);
			_stepList.Add(watchJobProgress);
			_stepList.Add(removeFromViewModel);
		}

		public override void run(Handler<AsyncResult<Module.Void>> handler)
		{
			_jobProgress.Status = ClientJobProgressData.Status.running;
			_jobProgress.Progress = 0;

			runSteps(0, _stepList, _logStepTime, result => {
				ViewModel.JobProgress.ListMoment.remove(_jobProgress);
			});
		}

		private void reqDeleteMoment(Handler<AsyncResult<Module.Void>> handler)
		{
			MapPacket req = Network.createReq(CSMessageID.Moment.DeleteMomentReq);
			req.put("id", _moment_id);

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
						_jobProgress.Progress = data.progress;
						_jobProgress.Param = data.param;
						_jobProgress.ResultCode = data.result_code;

						Debug.Log($"status[{data.status}] progress[{data.progress}] param[{data.param}] result_code[{data.result_code}]");
					}
				});

				yield return new WaitWhile(() => { return wait_ack; });
			}

			handler(Future.succeededFuture());
		}

		private void removeFromViewModel(Handler<AsyncResult<Module.Void>> handler)
		{
			if( _jobProgress.ResultCode == ResultCode.ok )
			{
				ViewModel.Moment.delete(_moment_id);
			}

			handler(Future.succeededFuture());
		}
	}


}
