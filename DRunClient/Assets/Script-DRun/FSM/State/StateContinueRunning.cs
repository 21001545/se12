using DRun.Client.Logic.Running;
using DRun.Client.NetData;
using DRun.Client.Running;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module.FSM;
using Festa.Client.NetData;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DRun.Client
{
	public class StateContinueRunning : ClientStateBehaviour
	{
		private bool _continue;

		public override int getType()
		{
			return ClientStateType.continue_running;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("check running...", 80);
			_continue = false;

			LoadLocalRunningData step = ClientMain.instance.getRunningRecorder().createLoadLocalData();
			step.run(result => {

				if (result.succeeded())
				{
					LocalRunningStatusData statusData = step.getStatusData();
					List<RunningPathData> pathList = step.getPathList();

					processLocalData(statusData, pathList);
				}
				else
				{
					changeToNextState();
				}
			});
		}

		public override void onExit(StateBehaviour<object> next_state)
		{
			// 이전 세션에 실행 중이 었을 수도 있다
			if( _continue == false)
			{
				ClientMain.instance.getGPSTracker().stop();
			}
		}

		private void processLocalData(LocalRunningStatusData statusData,List<RunningPathData> pathList)
		{
			if( statusData == null)
			{
				changeToNextState();
				return;
			}

			if( statusData.status != StateType.tracking &&
				statusData.status != StateType.paused)
			{
				Debug.Log($"discard save data: status[{statusData.status}]");
				changeToNextState();
				return;
			}

			if( statusData.running_id != _viewModel.Running.RunningConfig.next_running_id)
			{
				Debug.Log($"discard save data: running_id is different file[{statusData.running_id}] next_running_id[{_viewModel.Running.RunningConfig.next_running_id}]");
				changeToNextState();
			}

#if UNITY_EDITOR
			continueGPSForEditor(pathList);
#endif

			ClientMain.instance.getRunningRecorder().continueFromLocalData(statusData, pathList);
			changeToNextState();
		}

		private void continueGPSForEditor(List<RunningPathData> pathList)
		{
			GPSTracker_Editor tracker = GPSTracker_Editor.getInstance();

			GPSTilePosition pos = RunningPathData.getLastPosition(pathList);
			if( pos != null)
			{
				tracker.setLastLocation(pos);
			}
		}
	}
}
