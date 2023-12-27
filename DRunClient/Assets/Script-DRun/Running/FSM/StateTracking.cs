using DRun.Client.ViewModel;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using Festa.Client.NetData;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client.Running
{
	public class StateTracking : RecorderStateBehaviour
	{
		private IntervalTimer _timer;
		//private IntervalTimer _stepCountTimer;

		public class ProcessedPathData
		{
			public int mined;
			//public int step_count;
			public List<ClientLocationLog> log_list;
			public long start_time;
			public long end_time;

			public static ProcessedPathData create(int mined)
			{
				ProcessedPathData data = new ProcessedPathData();
				data.mined = mined;
				data.log_list = new List<ClientLocationLog>();
				return data;
			}
		}

		public override int getType()
		{
			return StateType.tracking;
		}

		protected override void init()
		{
			base.init();

			_timer = IntervalTimer.create(1.0f, false, false);
			//_stepCountTimer = IntervalTimer.create(1.0f, false, false);
			_timer.stop();
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_timer.setNext(1.0f);
			//_stepCountTimer.setNext();
			ViewModel.calcStatus();

			// 몽땅 저장
			if( prev_state != null && prev_state.getType() == StateType.wait_first_move)
			{
				Recorder.saveLocalData(false);
			}
		}

		public override void update()
		{
			if( _timer.update())
			{
				updateNow(() => {
					_timer.setNext();
				});
			}

			//if(_stepCountTimer.update())
			//{
			//	updateAllPathStepCount(() => {
			//		_stepCountTimer.setNext();
			//	});
			//}
		}

		public void updateNow(UnityAction callback)
		{
			recordNewLocations(() => {
				updatePathStepCount(ViewModel.CurrentPathData, () => {
					if (ViewModel.CurrentPathData.hasGPSPosition())
					{
						ViewModel.CurrentLocation = ViewModel.CurrentPathData.getLastPosition();
					}

					ViewModel.calcStatus();
					ViewModel.writePathEvent(RunningPathEvent.EventType.append_log, ViewModel.CurrentPathData);
					Recorder.saveLocalData(true);

					callback();
				});
			});
		}

		public override void onExit(StateBehaviour<object> next_state)
		{
			// 빼자
			//recordNewLocations(() => { });
		}

		private void recordNewLocations(UnityAction callback)
		{
			GPSTracker.getLocationFrom(ViewModel.LastGPSQueryTime + 1, _queryLocationList);
			ViewModel.GPSSignalStatus = GPSTracker.getStatusInfo().status;

			if (_queryLocationList.Count > 0)
			{
				processLocationLog(_queryLocationList);

				callback();
			}
			else
			{
				ViewModel.calcStatus();
				callback();
			}
		}

		public void processLocationLog(List<ClientLocationLog> logList)
		{
			ViewModel.LastGPSQueryTime = TimeUtil.unixTimestampFromDateTime(logList[logList.Count - 1].event_time);

			ViewModel.appendCurrentPathLog(logList);
			
			//Recorder.saveLocalData(true);

			//if( ViewModel.isProMode())
			//{
			//	ViewModel.notifyRunningStatUpdate();
			//}

			// 달성하면
			if (ViewModel.isMarathonMode() && ViewModel.GoalRatio >= 1.0f)
			{
				Debug.Log("complete marathon !!");
				_owner.changeState(StateType.paused);
			}

			//if ( ViewModel.isProMode())
			//{
			//	List<ProcessedPathData> pathList = processLocationLogProMode(ViewModel.StatData, ViewModel.CurrentPathData, _queryLocationList);
			//	ViewModel.LastGPSQueryTime = pathList[pathList.Count - 1].end_time + 1;
			//	ViewModel.CurrentLocation = _queryLocationList[_queryLocationList.Count - 1].toMBLocation();

			//	for (int i = 0; i < pathList.Count; ++i)
			//	{
			//		ProcessedPathData pathData = pathList[i];

			//		if (i == 0)
			//		{
			//			ViewModel.appendCurrentPathLog(pathData.log_list);
			//			ViewModel.CurrentLocation = ViewModel.CurrentPathData.getLastLocation();
			//			Recorder.saveLocalData(true);
			//		}
			//		else
			//		{
			//			ViewModel.appendNewPathData(ClientTripConfig.TripType.running, pathData.mined, pathData.log_list);
			//			ViewModel.CurrentLocation = ViewModel.CurrentPathData.getLastLocation();
			//			Recorder.saveLocalData(true);
			//		}
			//	}

			//	ViewModel.notifyRunningStatUpdate();
			//}
			//else if( ViewModel.isMarathonMode())
			//{
			//	List<ClientLocationLog> validLogList = processLocationLogMarathonMode(logList);
			//	if( validLogList.Count > 0)
			//	{
			//		ViewModel.appendCurrentPathLog(validLogList);
			//	}

			//	ClientLocationLog lastLog = logList[logList.Count - 1];
			//	ViewModel.LastGPSQueryTime = TimeUtil.unixTimestampFromDateTime(lastLog.event_time);
			//	ViewModel.CurrentLocation = ViewModel.CurrentPathData.getLastPosition();

			//	Recorder.saveLocalData(true);

			//	// 달성하면
			//	if( ViewModel.GoalRatio >= 1.0f)
			//	{
			//		Debug.Log("complete marathon !!");
			//		_owner.changeState(StateType.paused);
			//	}
			//}
		}

		//public List<ClientLocationLog> processLocationLogMarathonMode(List<ClientLocationLog> logList)
		//{
		//	List<ClientLocationLog> validList = new List<ClientLocationLog>();

		//	int goal = ViewModel.Goal;
		//	int lastGoalResult = ViewModel.getCurrentGoalResult();

		//	bool isTimeGoal = ViewModel.RunningSubType == ClientRunningLogCumulation.MarathonType._free_time;

		//	if( isTimeGoal)
		//	{
		//		goal *= 60;	// 초단위로 바꿈
		//	}

		//	ClientLocationLog lastLog = ViewModel.CurrentPathData.getLastLocationLog();

		//	double sum_distance = 0;
		//	double sum_time = 0;

		//	for(int i = 0; i < logList.Count; ++i)
		//	{
		//		ClientLocationLog log = logList[i];

		//		sum_distance += ClientLocationLog.calcDistance(lastLog, log);
		//		sum_time += ClientLocationLog.calcDeltaTime(lastLog, log);

		//		int check_result;
		//		if( isTimeGoal)
		//		{
		//			check_result = lastGoalResult + (int)sum_time;
		//		}
		//		else
		//		{
		//			check_result = lastGoalResult + (int)(sum_distance * 1000);
		//		}

		//		validList.Add(log);
		//		lastLog = log;

		//		// 아직 목표에 도달하지 못했다
		//		if ( check_result >= goal)
		//		{
		//			break;
		//		}
		//	}

		//	return validList;
		//}

		//public List<ProcessedPathData> processLocationLogProMode(RunningStatData statData,ClientTripPathData currentPathData,List<ClientLocationLog> logList)
		//{
		//	ClientNFTItem nftItem = statData._nftItem;

		//	//RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(nftItem.grade);
		//	int heartReduceRate = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.heart_ReduceRate, 1);
		//	int distanceReduceRate = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.distance_ReduceUnit, 100);
		//	double maxSpeed = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.running_max_speed, 30);

		//	//
		//	List<ProcessedPathData> processedList = new List<ProcessedPathData>();
			
		//	// 설마 null이진 않겠지
		//	ClientLocationLog lastLog = currentPathData.getLastLocationLog();

		//	ProcessedPathData lastProcessedPath = ProcessedPathData.create(currentPathData.mined);
		//	lastProcessedPath.start_time = TimeUtil.unixTimestampFromDateTime(lastLog.event_time);
		//	processedList.Add(lastProcessedPath);

		//	foreach(ClientLocationLog log in logList)
		//	{
		//		lastProcessedPath.log_list.Add(log);
		//		lastProcessedPath.end_time = TimeUtil.unixTimestampFromDateTime(log.event_time);

		//		long day_id = TimeUtil.dayCountFromDate(log.event_time, TimeUtil.timezoneOffset());

		//		// 날이 바뀌여서 stat충전함
		//		if (statData.last_day_id != day_id)
		//		{
		//			statData.processRefill(day_id);
		//			//if (nftItem.max_heart > 0)
		//			//{
		//			//	statData.heart += nftItem.max_heart;
		//			//}

		//			//statData.distance += nftItem.max_distance;
		//			//statData.last_day_id = day_id;

		//			Debug.Log($"refill heart[{statData.heart}] distance[{statData.distance}] bonus_heart[{statData.bonus_heart}] bonus_distance[{statData.bonus_distance}]");
		//		}

		//		double distance = ClientLocationLog.calcDistance(lastLog, log) * 1000;
		//		double time = ClientLocationLog.calcDeltaTime(lastLog, log) / 60.0;

		//		double speedKMH = 0;

		//		if (time > 0)
		//		{
		//			speedKMH = (distance / 1000.0) / (time / 60);
		//		}

		//		bool exceedMaxSpeed = speedKMH > maxSpeed;

		//		// 2022.12.06 제한 속도를 초과 하면 State을 소비 하지 않음
		//		if( exceedMaxSpeed == false)
		//		{
		//			statData.last_distance += distance;
		//			statData.last_time += time;

		//			int distanceReduce = (int)System.Math.Floor(statData.last_distance / distanceReduceRate);
		//			int heartReduce = (int)System.Math.Floor(statData.last_time / heartReduceRate);

		//			if (distanceReduce > 0)
		//			{
		//				int distanceDelta = distanceReduce * distanceReduceRate;
		//				if (distanceDelta > statData.distance)
		//				{
		//					distanceDelta = statData.distance;
		//				}

		//				statData.distance -= distanceDelta;
		//				statData.used_distance += distanceDelta;
		//				Debug.Log($"reduce distance: delta[{distanceDelta}] distance[{statData.distance}]");

		//				statData.last_distance -= distanceDelta;
		//			}

		//			// 무제한 모드 제외
		//			if (heartReduce > 0 && nftItem.max_heart > 0)
		//			{
		//				if (heartReduce > statData.heart)
		//				{
		//					heartReduce = statData.heart;
		//				}

		//				statData.heart -= heartReduce;
		//				statData.used_heart += heartReduce;
		//				Debug.Log($"reduce heart: delta[{heartReduce}] heart[{statData.heart}]");

		//				statData.last_time -= heartReduce * heartReduceRate;
		//			}
		//		}

		//		lastLog = log;

		//		//
		//		int mined = 0;
				
		//		mined = statData.isMinable() && exceedMaxSpeed  == false ? 1 : 0;

		//		if ( lastProcessedPath.mined != mined)
		//		{
		//			lastProcessedPath = ProcessedPathData.create(mined);
		//			lastProcessedPath.start_time = TimeUtil.unixTimestampFromDateTime(log.event_time);
		//			lastProcessedPath.end_time = lastProcessedPath.start_time;
		//			lastProcessedPath.log_list.Add(log);	// 하나 넣어줌
		//			processedList.Add(lastProcessedPath);
		//		}
		//	}

		//	return processedList;
		//}
	}
}
