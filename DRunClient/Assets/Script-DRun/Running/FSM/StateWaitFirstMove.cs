using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.Running
{
	public class StateWaitFirstMove : RecorderStateBehaviour
	{
		private IntervalTimer _timer;
		private long _queryBeginTime;

		private double _checkDistance;

		protected override void init()
		{
			base.init();

			_timer = IntervalTimer.create(1.0f, false, false);
			_timer.stop();
		}

		public override int getType()
		{
			return StateType.wait_first_move;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_timer.setNext();
			_queryBeginTime = TimeUtil.unixTimestampFromDateTime(ViewModel.GPSAvailableTime);
			_checkDistance = (double)GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_firstmove_distance, 5) / 1000.0;
		}

		public override void update()
		{
			if( _timer.update())
			{
				GPSTracker.getLocationFrom(_queryBeginTime + 1, _queryLocationList);
				ViewModel.GPSSignalStatus = GPSTracker.getStatusInfo().status;

				if ( _queryLocationList.Count == 0)
				{
					_timer.setNext();
					return;
				}

				double distance = calcTotalDistance(_queryLocationList);

				Debug.Log($"check first move : {(distance * 1000).ToString("N")} m");

				if( distance >= _checkDistance)
				{
					DateTime lastEventTime = _queryLocationList[_queryLocationList.Count - 1].event_time;
					ViewModel.FirstMoveChecked = true;
					ViewModel.TrackingStartTime = lastEventTime;
					ViewModel.LastGPSQueryTime = TimeUtil.unixTimestampFromDateTime(ViewModel.TrackingStartTime);

					ViewModel.appendCurrentPathLog(_queryLocationList);

					ViewModel.StartLocation = ViewModel.CurrentPathData.getFirstPosition();
					ViewModel.CurrentLocation = ViewModel.CurrentPathData.getLastPosition();

					_owner.changeState(StateType.tracking);
				}
				else
				{
					MBLongLatCoordinate startLocation = _queryLocationList[0].toMBLocation();
					MBLongLatCoordinate endLocation = _queryLocationList[_queryLocationList.Count - 1].toMBLocation();

					ViewModel.StartLocation = startLocation;
					ViewModel.CurrentLocation = endLocation;

					_timer.setNext();
				}
			}
		}

		private double calcTotalDistance(List<ClientLocationLog> logList)
		{
			if( logList.Count < 2)
			{
				return 0;
			}

			List<GPSTilePosition> posList = new List<GPSTilePosition>();
			for(int i = 0; i < logList.Count; ++i)
			{
				GPSTilePosition pos = GPSTilePosition.create(posList.Count, logList[i]);
				posList.Add(pos);
			}

			List<GPSTilePosition> list = GPSPathSimplify.Simplify(posList, GPSPathSimplify.defaultThreshold, false);
			if( list.Count < 2)
			{
				return 0;
			}
			
			double distance = 0;

			GPSTilePosition lastPos = list[0];
			for(int i = 1; i < list.Count; ++i)
			{
				GPSTilePosition pos = list[i];
				distance += MapBoxUtil.distance(lastPos.gps_pos, pos.gps_pos);

				lastPos = pos;
			}

			return distance;
		}
	}
}
