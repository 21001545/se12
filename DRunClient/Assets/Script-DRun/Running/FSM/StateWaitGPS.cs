using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;

namespace DRun.Client.Running
{
	public class StateWaitGPS : RecorderStateBehaviour
	{
		private IntervalTimer _timer;
		private long _waitStartTime;
		private int _nextState;

		public override int getType()
		{
			return StateType.wait_gps;
		}

		protected override void init()
		{
			base.init();
			_timer = IntervalTimer.create(0.3f, false, false);
			_timer.stop();
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			// GPS실행
			GPSTracker.start();
			_timer.setNext();

			_waitStartTime = TimeUtil.unixTimestampUtcNow();
			if ( param != null)
			{
				_nextState = (int)param;
			}
			else
			{
				_nextState = StateType.wait_first_move;
			}

			if( _nextState == StateType.wait_first_move)
			{
				ViewModel.resetForStart(ViewModel.RunningType,ViewModel.RunningSubType,ViewModel.Goal, ProVM.Data, ProVM.EquipedNFTItem, ProVM.NFTBonus,
												HealthVM.Body.getWeightWithKG());
			}
		}

		public override void update()
		{
			if( _timer.update())
			{
				GPSTracker.getLocationFrom(_waitStartTime + 1, _queryLocationList);
				ViewModel.GPSSignalStatus = GPSTracker.getStatusInfo().status;

				// 신호가 잡혔다
				if (_queryLocationList.Count > 0)
				{
					if (_nextState == StateType.wait_first_move)
					{
						ViewModel.FirstMoveCheckLocation = _queryLocationList[_queryLocationList.Count - 1];
						ViewModel.GPSAvailableTime = DateTime.UtcNow;
						ViewModel.GPSChecked = true;

						MBLongLatCoordinate startLocation = _queryLocationList[0].toMBLocation();
						MBLongLatCoordinate endLocation = _queryLocationList[_queryLocationList.Count - 1].toMBLocation();

						ViewModel.StartLocation = startLocation;
						ViewModel.CurrentLocation = endLocation;
					}

					_owner.changeState(_nextState);
				}
				else
				{
					_timer.setNext();
				}
			}
		}

	}
}
