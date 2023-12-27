using DRun.Client.Running;
using DRun.Client.ViewModel;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using Festa.Client.NetData;
using UnityEngine.Events;

namespace DRun.Client.Run
{
	public class StatePaused : RecorderStateBehaviour
	{
		private IntervalTimer _timer;
		private IntervalTimer _stepCountTimer;

		public override int getType()
		{
			return StateType.paused;
		}

		protected override void init()
		{
			base.init();

			_timer = IntervalTimer.create(1.0f, false, false);
			_stepCountTimer = IntervalTimer.create(2.0f, false, false);
			_timer.stop();
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_timer.setNext();
			_stepCountTimer.setNext();

			// 앱을 다시 실행해서 온 경우도 있다
			if( prev_state != null && prev_state.getType() == StateType.tracking)
			{
				ViewModel.splitPath(true);
				Recorder.saveLocalData(true);
			}
		}

		public override void update()
		{
			if( _timer.update())
			{
				recordNewLocations(() => {
					_timer.setNext();
				});
			}

			//if (_stepCountTimer.update())
			//{
			//	updateAllPathStepCount(() => {
			//		_stepCountTimer.setNext();
			//	});
			//}
		}

		public override void onExit(StateBehaviour<object> next_state)
		{
			//recordNewLocations(() => { });
			if( next_state.getType() == StateType.tracking)
			{
				ViewModel.splitPath(false);
				Recorder.saveLocalData(true);
			}
		}

		private void recordNewLocations(UnityAction callback)
		{
			if( ViewModel.GPSChecked == false ||
				ViewModel.FirstMoveChecked == false)
			{
				callback();
				return;
			}

			GPSTracker.getLocationFrom(ViewModel.LastGPSQueryTime + 1, _queryLocationList);
			ViewModel.GPSSignalStatus = GPSTracker.getStatusInfo().status;

			if (_queryLocationList.Count > 0)
			{
				ClientLocationLog lastLog = _queryLocationList[_queryLocationList.Count - 1];

				long lastRecordTime = TimeUtil.unixTimestampFromDateTime(lastLog.event_time);
				ViewModel.LastGPSQueryTime = lastRecordTime + 1;
				ViewModel.CurrentLocation = lastLog.toMBLocation();

				// 마라톤 모드에서는 pause가 멈춤 대기 상태라서 아무것도 하지 않는다				
				if (ViewModel.isProMode())
				{
					ViewModel.appendCurrentPathLog(_queryLocationList);

					RunningPathData currentPath = ViewModel.CurrentPathData;
					currentPath.processAfterUpdateStepCount();
					if(currentPath.hasGPSPosition())
					{
						ViewModel.CurrentLocation = currentPath.getLastPosition();
					}

					ViewModel.writePathEvent(RunningPathEvent.EventType.append_log, ViewModel.CurrentPathData);
				}

				Recorder.saveLocalData(true);
				callback();
			}
			else
			{
				ViewModel.calcStatus();
				callback();
			}
		}

	}
}
