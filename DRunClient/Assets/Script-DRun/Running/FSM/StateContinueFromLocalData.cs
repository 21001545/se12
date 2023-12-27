using Festa.Client.Module.FSM;
using Festa.Client.NetData;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.Running
{
	public class StateContinueFromLocalData : RecorderStateBehaviour
	{
		public class Param
		{
			public LocalRunningStatusData statusData;
			public List<RunningPathData> pathList;
		}

		public override int getType()
		{
			return StateType.continue_from_localdata;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			Param data = param as Param;

			// view 모델 셋업
			ViewModel.resetForContinue(data.statusData,data.pathList);

			if( data.statusData.status == StateType.paused || data.statusData.status == StateType.tracking)
			{
				_owner.changeState(StateType.wait_gps, data.statusData.status);
			}
			else
			{
				Debug.LogError($"could continue running: invalid status: [{data.statusData.status}]");
			}
		}
	}
}
