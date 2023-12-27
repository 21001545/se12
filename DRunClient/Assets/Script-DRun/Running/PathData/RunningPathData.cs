using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.Running
{
	public class RunningPathData : CustomSerializer
	{
		private const int _zoom = GPSTilePosition.zoom;
		private const float _filterThreshold = 30.0f;	// (Q_Meter_per_second)

		public const int VERSION = 4;

		private List<GPSTilePosition> _rawList;
		private List<GPSTilePosition> _list;

		private int _runningType;
		private double _weight;

		private int _path_id;
		private bool _paused;

		private RunningPathModeData _modeData;

		private RunningPathStatData _statData => (_modeData as RunningPathStatData);
		private RunningPathMarathonData _marathonData => (_modeData as RunningPathMarathonData);

		private KalmanLatLong _filter;

		public class SubPath
		{
			public Vector2Int range;

			public double distance;
			public double duration;
			public double calorie;

			public bool visible;
			public bool minable;
			public bool exceedSpeedLimit;
			public bool invalidStepCount;

			public int stepCount;

			public static SubPath create(int beginIndex)
			{
				SubPath path = new SubPath();
				path.range = new Vector2Int(beginIndex, beginIndex);
				path.distance = 0;
				path.duration = 0;
				path.calorie = 0;
				path.visible = true;
				path.minable = false;
				path.exceedSpeedLimit = false;
				path.invalidStepCount = false;
				path.stepCount = 0;
				return path;
			}

			public void pack(MessagePacker packer)
			{
				packer.packInt(range.x);
				packer.packInt(range.y);
				packer.packDouble(distance);
				packer.packDouble(duration);
				packer.packDouble(calorie);
				packer.packBoolean(visible);
				packer.packBoolean(minable);
				packer.packBoolean(exceedSpeedLimit);
				packer.packBoolean(invalidStepCount);
				packer.packInt(stepCount);
			}

			public static SubPath unpack(MessageUnpacker unpacker)
			{
				SubPath path = new SubPath();
				path.range = Vector2Int.zero;
				path.range.x = unpacker.unpackInt();
				path.range.y = unpacker.unpackInt();
				path.distance = unpacker.unpackDouble();
				path.duration = unpacker.unpackDouble();
				path.calorie = unpacker.unpackDouble();
				path.visible = unpacker.unpackBoolean();
				path.minable = unpacker.unpackBoolean();
				path.exceedSpeedLimit = unpacker.unpackBoolean();
				path.invalidStepCount = unpacker.unpackBoolean();
				path.stepCount = unpacker.unpackInt();
				return path;
			}
		}

		private List<SubPath> _tempSubPathList;
		private List<SubPath> _subPathList;

		public bool isProMode()
		{
			return _runningType == ClientRunningLogCumulation.RunningType.promode;
		}

		public bool isMarathonMode()
		{
			return _runningType == ClientRunningLogCumulation.RunningType.marathon;
		}

		public bool isPaused()
		{
			return _paused;
		}

		public int getPathID()
		{
			return _path_id;
		}

		public List<GPSTilePosition> getRawList()
		{
			return _rawList;
		}

		public List<GPSTilePosition> getPosList()
		{
			return _list;
		}

		public List<SubPath> getTempSubPathList()
		{
			return _tempSubPathList;
		}

		public List<SubPath> getSubPathList()
		{
			return _subPathList;
		}

		public SubPath getLastSegment()
		{
			if( _subPathList.Count == 0)
			{
				return null;
			}

			return _subPathList[_subPathList.Count - 1];
		}

		public RunningPathStatData getStatData()
		{
			return _statData;
		}

		// 시간 계산 가능한 path
		public bool isTimeActive()
		{
			if( isProMode())
			{
				return _paused == false;	
			}
			else
			{
				SubPath lastPath = getLastSegment();
				if (lastPath == null)
				{
					// 음
					return false;
				}

				return lastPath.visible;
			}
		}

		//// 마라톤 모드 : 골달성을 하지 못했을 경우
		//// 프로모드 : 아직 채굴 가능한 경우
		//public bool isActive()
		//{
		//	SubPath lastPath = getLastSegment();
		//	if (lastPath == null)
		//	{
		//		// 음
		//		return false;
		//	}

		//	if( isProMode())
		//	{
		//		return _paused == false;
		//		//return lastPath.minable;
		//	}
		//	else
		//	{
		//		return lastPath.visible;
		//	}
		//}

		// _list에 최소한 2개의 지점이 있어야 된다
		public bool isValidPath()
		{
			return _list != null && _list.Count > 1;
		}

		public bool hasGPSPosition()
		{
			return _list != null && _list.Count > 0;
		}

		public GPSTilePosition getFirstPosition()
		{
			return _list[0];
		}

		public GPSTilePosition getLastPosition()
		{
			return _list[_list.Count - 1];
		}

		public static RunningPathData create(int path_id,int runningType,bool paused,double weight,RunningPathModeData modeData)
		{
			RunningPathData data = new RunningPathData();
			data.init(path_id,runningType, paused, weight, modeData);
			return data;
		}

		// 시간 계산 오류를 수정하기 위해서 임의의 GPS 위치를 추가해준다
		// (위치는 동일 시간만 뒤로)

		public RunningPathData createContinue(bool paused)
		{
			if( _rawList.Count == 0)
			{
				Debug.LogError("_rawList.Count == 0");
			}

			RunningPathModeData modeData = _modeData.createContinue();
			RunningPathData pathData = create(_path_id + 1, _runningType, paused, _weight, modeData);
			
			if( _rawList.Count > 0)
			{
				long now = TimeUtil.unixTimestampUtcNow();

				GPSTilePosition lastPosition = _rawList[_rawList.Count - 1];

				GPSTilePosition pathEndPosition = GPSTilePosition.create(_rawList.Count, now, lastPosition.gps_pos.x, lastPosition.gps_pos.y, lastPosition.gps_alt);
				GPSTilePosition pathStartPosition = GPSTilePosition.create(0, now, lastPosition.gps_pos.x, lastPosition.gps_pos.y, lastPosition.gps_alt);

				_rawList.Add(pathEndPosition);
				pathData._rawList.Add(pathStartPosition);
			}
			return pathData;
		}

		private void init(int path_id,int runningType,bool paused,double weight, RunningPathModeData modeData)
		{
			_path_id = path_id;
			_runningType = runningType;
			_paused = paused;
			_weight = weight;
			_modeData = modeData;
			_rawList = new List<GPSTilePosition>();
			_subPathList = new List<SubPath>();
			_tempSubPathList = new List<SubPath>();
		}

		public void appendLog(List<ClientLocationLog> logList)
		{
			long lastLocationTime = 0;
			if( _rawList.Count > 0)
			{
				lastLocationTime =_rawList[_rawList.Count - 1].time;
			}

			// raw position 추가
			for(int i = 0; i < logList.Count; ++i)
			{
				ClientLocationLog log = logList[i];

				// 2023.02.23 그럴 수 있나봐
				long event_time = TimeUtil.unixTimestampFromDateTime(log.event_time);

				if( lastLocationTime != 0 && event_time <= lastLocationTime)
				{
					//Debug.Log($"[{_path_id}] discard past location log: last_time[{lastLocationTime}] log_time[{event_time}]");
					continue;
				}
				//else
				//{
				//	//Debug.Log($"[{_path_id}] append location log: log_time[{event_time}]");
				//}

				if (_filter == null)
				{
					_filter = new KalmanLatLong(_filterThreshold);
					_filter.SetState(log.latitude, log.longitude, (float)log.accuracy, event_time);
				}
				else
				{
					_filter.Process(log.latitude, log.longitude, (float)log.accuracy, event_time);
				}

				//Debug.Log($"append raw position:time[{_filter.TimeStamp}]");

				GPSTilePosition pos = GPSTilePosition.create(_rawList.Count, _filter.TimeStamp, _filter.Lng, _filter.Lat, log.altitude);
				_rawList.Add(pos);
			}

			// filtering
			_list = GPSPathSimplify.Simplify(_rawList, GPSPathSimplify.defaultThreshold, false);

			// 계산
			process();
		}

		private void process()
		{
			// 경로 시간
			if ( processBasic() == false)
			{
				return;
			}

			// 각 모드별 detail하게 
			if( isProMode())
			{
				processProModeFirstStep();
			}
			else
			{
				processMarathonMode();
			}
		}

		public void processAfterUpdateStepCount()
		{
			if( isProMode())
			{
				processProModeSecondStep();
			}
			//else
			//{
			//	_subPathList.Clear();
			//	_subPathList.AddRange(_tempSubPathList);
			//}
		}

		private double calcCalorie(double weight, double distance, long deltaTime)
		{
			double durationMinutes = (double)deltaTime / 1000.0 / 60.0;
			if (durationMinutes == 0)
			{
				return 0;
			}

			double speedKMH = distance / (durationMinutes / 60.0);

			return GlobalRefDataContainer.getRefDataHelper().calcCalories(1, speedKMH, weight, durationMinutes);
		
		}

		private bool processBasic()
		{
			if (_list.Count <= 1)
			{
				return false;
			}

			double maxSpeed = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_max_speed, 30);

			GPSTilePosition lastPos = _list[0];
			for (int i = 1; i < _list.Count; ++i)
			{
				GPSTilePosition curPos = _list[i];

				long delta_time = curPos.time - lastPos.time;
				double distance = MapBoxUtil.distance(lastPos.gps_pos.x, lastPos.gps_pos.y, curPos.gps_pos.x, curPos.gps_pos.y);
				double calorie = calcCalorie(_weight, distance, delta_time);

				curPos.deltaDistance = distance;
				curPos.deltaCalorie = calorie;
				curPos.deltaTime = ((double)delta_time) / (double)TimeUtil.msSecond;
				curPos.deltaSpeedKMH = 0;
				if( curPos.deltaTime > 0)
				{
					curPos.deltaSpeedKMH = curPos.deltaDistance * 3600 / curPos.deltaTime;
				}
				curPos.exceedSpeedLimit = curPos.deltaSpeedKMH > maxSpeed;

				lastPos = curPos;
			}

			return true;
		}

		// 2023.02.20 걸음 수 제한 적용

		//private void processProMode()
		//{
			
		//	_subPathList.Clear();
		//	int heartReduceRate = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.heart_ReduceRate, 1);
		//	int distanceReduceRate = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.distance_ReduceUnit, 100);
		//	double maxSpeed = GlobalRefDataContainer.getRefConfigWithDefault(RefConfig.Key.DRun.running_max_speed, 30);

		//	_statData.resetEnd();
			
		//	SubPath curPath = SubPath.create(0);
		//	GPSTilePosition lastPos = _list[0];
		//	lastPos.exceedSpeedLimit = _list[1].exceedSpeedLimit;

		//	curPath.minable = _statData.isMinable();

		//	for(int i = 1; i < _list.Count; ++i)
		//	{
		//		GPSTilePosition pos = _list[i];
		//		if( _paused || pos.exceedSpeedLimit == lastPos.exceedSpeedLimit)
		//		{
		//			curPath.range.y = i;
		//		}
		//		else
		//		{
		//			_subPathList.Add(curPath);
		//			curPath = SubPath.create(i - 1);
		//			curPath.range.y = i;
		//		}

		//		curPath.distance += pos.deltaDistance;
		//		curPath.duration += pos.deltaTime;
		//		curPath.calorie += pos.deltaCalorie;

		//		if( _paused || pos.exceedSpeedLimit)
		//		{
		//			continue;
		//		}

		//		_statData.end_remain_distance += pos.deltaDistance * 1000;
		//		_statData.end_remain_time += pos.deltaTime / 60;

		//		int consumeDistanceUnit = (int)System.Math.Floor(_statData.end_remain_distance / distanceReduceRate);
		//		int consumeHeart = (int)System.Math.Floor(_statData.end_remain_time / heartReduceRate);

		//		if (consumeDistanceUnit > 0)
		//		{
		//			_statData.consumeDistance(consumeDistanceUnit * distanceReduceRate);

		//			_statData.end_remain_distance -= consumeDistanceUnit * distanceReduceRate;
		//		}

		//		if (consumeHeart > 0 && _statData.isUnlimitHeart() == false)
		//		{
		//			_statData.consumeHeart(consumeHeart);
		//			_statData.end_remain_time -= consumeHeart * heartReduceRate;
		//		}

		//		if(curPath.minable != _statData.isMinable())
		//		{
		//			_subPathList.Add(curPath);
		//			curPath = SubPath.create(i);
		//			curPath.range.y = i;
		//			curPath.minable = _statData.isMinable();
		//		}
		//	}

		//	if( _subPathList.Contains(curPath) == false)
		//	{
		//		_subPathList.Add(curPath);
		//	}
		//}

		private void processMarathonMode()
		{
			_tempSubPathList.Clear();
			double maxSpeed = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_max_speed, 30);

			_marathonData.end_time = _marathonData.begin_time;
			_marathonData.end_distance = _marathonData.begin_distance;

			SubPath curPath = SubPath.create(0);
			GPSTilePosition lastPos = _list[0];

			lastPos.exceedSpeedLimit = _list[1].exceedSpeedLimit;

			for(int i = 1; i < _list.Count; ++i)
			{
				GPSTilePosition pos = _list[i];

				if( pos.exceedSpeedLimit == lastPos.exceedSpeedLimit)
				{
					curPath.range.y = i;
				}
				else
				{
					_tempSubPathList.Add(curPath);
					curPath = SubPath.create(i - 1);
					curPath.range.y = i;
				}

				curPath.distance += pos.deltaDistance;
				curPath.duration += pos.deltaTime;
				curPath.calorie += pos.deltaCalorie;
				curPath.exceedSpeedLimit = pos.exceedSpeedLimit;

				if ( pos.exceedSpeedLimit == false && _marathonData.isGoalComplete() == false)
				{
					_marathonData.end_distance += pos.deltaDistance;
					_marathonData.end_time += pos.deltaTime;

					if( _marathonData.isGoalComplete())
					{
						_tempSubPathList.Add(curPath);
						curPath = SubPath.create(i);
						curPath.visible = false;
					}
				}
			}

			if(_tempSubPathList.Contains(curPath) == false)
			{
				_tempSubPathList.Add(curPath);
			}

			_subPathList.Clear();
			_subPathList.AddRange(_tempSubPathList);

			debugSubPathData();
		}

		private void processProModeFirstStep()
		{
			_tempSubPathList.Clear();

			SubPath curPath = SubPath.create(0);
			GPSTilePosition lastPos = _list[0];
			lastPos.exceedSpeedLimit = _list[1].exceedSpeedLimit;

			int configCheckStepDistance = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_check_step_distance, 100);
			int configCheckTime = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_check_time, 60);

			double checkStepDistanceKM = (double)configCheckStepDistance / 1000.0;

			for(int i = 1; i < _list.Count; ++i)
			{
				GPSTilePosition pos = _list[i];

				bool changePath = false;
				if( _paused == false)
				{
					// 속도 초과 상태가 변경됨
					if( pos.exceedSpeedLimit != lastPos.exceedSpeedLimit)
					{
						changePath = true;
					}

					if( changePath == false && pos.exceedSpeedLimit == false && curPath.distance > checkStepDistanceKM)
					{
#if UNITY_EDITOR
						Debug.Log($"cut by distance : path_id[{_path_id}] sub_id[{_tempSubPathList.Count}]");
#endif
						changePath = true;
					}

					if( changePath == false && pos.exceedSpeedLimit == false && curPath.duration > configCheckTime)
					{
#if UNITY_EDITOR
						Debug.Log($"cut by time : path_id[{_path_id}] sub_id[{_tempSubPathList.Count}]");
#endif
						changePath = true;
					}
				}
				
				if( changePath == false)
				{
					curPath.range.y = i;
				}
				else
				{
					_tempSubPathList.Add(curPath);
					curPath = SubPath.create(i - 1);
					curPath.range.y = i;
				}

				curPath.distance += pos.deltaDistance;
				curPath.duration += pos.deltaTime;
				curPath.calorie += pos.deltaCalorie;
				curPath.exceedSpeedLimit = pos.exceedSpeedLimit;

				lastPos = pos;
			}

			if(_tempSubPathList.Contains(curPath) == false)
			{
				_tempSubPathList.Add(curPath);
			}
		}

		private void processProModeSecondStep()
		{
			_subPathList.Clear();

			// 멈춤상태
			if ( _paused)
			{
				_subPathList.AddRange(_tempSubPathList);
				return;
			}

			int heartReduceRate = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.heart_ReduceRate, 1);
			int distanceReduceRate = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.distance_ReduceUnit, 100);
			int configMinStepCount = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_min_step_count, 700);

			_statData.resetEnd();

			foreach (SubPath path in _tempSubPathList)
			{
				// 속도 초과 구간 skip
				if( path.exceedSpeedLimit)
				{
					_subPathList.Add(path);
					continue;
				}

				// 걸음 수 제한 skip
				int stepCountPerOneKM = (int)((double)path.stepCount / path.distance);

				if(stepCountPerOneKM < configMinStepCount)
				{
					path.invalidStepCount = true;
					_subPathList.Add(path);
					continue;
				}

				// 이미 채굴 불가 상태
				if( _statData.isMinable() == false)
				{
					_subPathList.Add(path);
					continue;
				}

				// stat 계산
				//path.minable = _statData.isMinable();
				GPSTilePosition lastPos = _list[path.range.x];

				SubPath curPath = SubPath.create(path.range.x);
				curPath.minable = _statData.isMinable();
				curPath.stepCount = path.stepCount;	// 일단 걸음 수는 채굴구간이 다 가져 간다

				for(int i = path.range.x + 1; i <= path.range.y; ++i)
				{
					GPSTilePosition pos = _list[i];

					curPath.distance += pos.deltaDistance;
					curPath.duration += pos.deltaTime;
					curPath.calorie += pos.deltaCalorie;

					_statData.end_remain_distance += pos.deltaDistance * 1000.0;
					_statData.end_remain_time += pos.deltaTime / 60.0;

					int consumeDistanceUnit = (int)System.Math.Floor(_statData.end_remain_distance / distanceReduceRate);
					int consumeHeart = (int)System.Math.Floor(_statData.end_remain_time / heartReduceRate);

					if( consumeDistanceUnit > 0)
					{
						_statData.consumeDistance(consumeDistanceUnit * distanceReduceRate);
						_statData.end_remain_distance -= consumeDistanceUnit * distanceReduceRate;
					}

					if( consumeHeart > 0 && _statData.isUnlimitHeart() == false)
					{
						_statData.consumeHeart(consumeHeart);
						_statData.end_remain_time -= consumeHeart * heartReduceRate;
					}

					// 스탯이 소진되어 (하트,거리) 채굴이 불가능 해졌다
					if( _statData.isMinable() == false)
					{
						_subPathList.Add(curPath);
						curPath = SubPath.create(i - 1);
						curPath.range.y = path.range.y;
						curPath.minable = false;
						break;
					}
					else
					{
						curPath.range.y = i;
					}

					lastPos = pos;
				}

				if( _subPathList.Contains(curPath) == false)
				{
					_subPathList.Add(curPath);
				}
			}

			debugSubPathData();
		}

		private void debugSubPathData()
		{
			if( GlobalConfig.isInhouseBranch() == false)
			{
				return;
			}

			Debug.Log($"path_id[{_path_id}] rawList[{_rawList?.Count}] list[{_list?.Count}] dist[{calcTotalDistance()}] time[{calcTotalTime(false)}]");
			if( isProMode())
			{
				Debug.Log($"heart[{_statData.begin_heart},{_statData.end_heart}] distance[{_statData.begin_distance},{_statData.end_distance}] bonus_heart[{_statData.begin_bonus_heart},{_statData.end_bonus_heart}] bonus_distance[{_statData.begin_bonus_distance},{_statData.end_bonus_distance}]");
				Debug.Log($"remain_distance[{_statData.begin_remain_distance.ToString("N1")},{_statData.end_remain_distance.ToString("N1")}] remain_time[{_statData.begin_remain_time.ToString("N2")},{_statData.end_remain_time.ToString("N2")}]");
			}

			for (int i = 0; i < _subPathList.Count; ++i)
			{
				SubPath path = _subPathList[i];

				int stepCountPerOneKM = (int)((double)path.stepCount / path.distance);

				Debug.Log($"[{_path_id}][{i}]: range[{path.range.x},{path.range.y}] dist[{path.distance.ToString("N3")}] time[{path.duration}] cal[{path.calorie}] step[{path.stepCount},{stepCountPerOneKM}] v[{path.visible}] m[{path.minable}] esp[{path.exceedSpeedLimit}] ist[{path.invalidStepCount}]");
			}
		}

		//private bool isCountableSegment(SubPath segment)
		//{
		//	if( isProMode() && segment.minable == false)
		//	{
		//		return false;
		//	}
		//	else if( isMarathonMode() && segment.visible == false)
		//	{
		//		return false;
		//	}

		//	return true;
		//}

		//private bool isCountableSegmentForTime(SubPath segment,bool isMinableOnly)
		//{
		//	if (isProMode())
		//	{
		//		if(isMinableOnly && segment.minable == false)
		//		{
		//			return false;
		//		}

		//		return true;
		//	}
		//	else if (isMarathonMode() && segment.visible == false)
		//	{
		//		return false;
		//	}

		//	return true;
		//}

		public double calcTotalDistance()
		{
			// 멈춘 구간은 포함 않됨
			if( _paused)
			{
				return 0;
			}

			double totalDistance = 0;
			foreach (SubPath segment in _subPathList)
			{
				totalDistance += segment.distance;
			}

			return totalDistance;
		}

		public double calcTotalMineDistance()
		{
			if( _paused)
			{
				return 0;
			}

			if( isProMode() == false)
			{
				Debug.LogWarning("calcTotalMineDistance function can only used in ProMode");
				return 0;
			}

			double totalMineDistance = 0;
			foreach(SubPath segment in _subPathList)
			{
				if( segment.minable == false)
				{
					continue;
				}

				totalMineDistance += segment.distance;
			}
			return totalMineDistance;
		}

		public double calcTotalTime(bool isMinableOnly)
		{
			if( _paused || _rawList.Count < 2)
			{
				return 0;
			}

			GPSTilePosition first = _rawList[0];
			GPSTilePosition second = _rawList[_rawList.Count - 1];

			return (double)(second.time - first.time) / TimeUtil.msSecond;
		}

		public double calcTotalCalorie()
		{
			if( _paused)
			{
				return 0;
			}

			double totalCalorie = 0;
			foreach(SubPath segment in _subPathList)
			{
				totalCalorie += segment.calorie;
			}

			return totalCalorie;
		}

		public int calcTotalStep()
		{
			if( _paused)
			{
				return 0;
			}

			int totalStep = 0;
			foreach(SubPath segment in _subPathList)
			{
				totalStep += segment.stepCount;
			}

			return totalStep;
		}

		public void toTriPathData(List<ClientTripPathData> resultList)
		{
			GPSBound bound = new GPSBound();
			List<GPSTilePosition> posList = new List<GPSTilePosition>();

			for(int i = 0; i < _subPathList.Count; ++i)
			{
				SubPath segment = _subPathList[i];
				ClientTripPathData path = new ClientTripPathData();

				path.trip_id = 0;
				path.path_id = resultList.Count + 1;
				path.trip_type = _paused ? ClientTripConfig.TripType.none : ClientTripConfig.TripType.running;
				path.mined = segment.minable ? 1 : 0;
				path.step_count = 0;

				posList.Clear();
				for(int j = segment.range.x; j <= segment.range.y; ++j)
				{
					posList.Add(_list[j]);
				}

				bound.reset();
				bound.updateBound(posList);

				path.min_lon = bound.getMin().x;
				path.min_lat = bound.getMin().y;
				path.min_alt = bound.getMin().z;

				path.size_lon = bound.getSize().x;
				path.size_lat = bound.getSize().y;
				path.size_alt = bound.getSize().z;

				path.path_end_time = path.path_begin_time = posList[0].time;
				path.path_list = new List<double>();
				path.path_time_list = new List<int>();

				foreach(GPSTilePosition pos in posList)
				{
					path.path_end_time = pos.time;

					path.path_list.Add(pos.gps_pos.x);
					path.path_list.Add(pos.gps_pos.y);
					path.path_list.Add(pos.gps_alt);
					path.path_time_list.Add((int)(pos.time - path.path_begin_time));
				}

				resultList.Add(path);
			}
		}

		public RunningPathData cloneForSave()
		{
			RunningPathData path = new RunningPathData();
			path._rawList = new List<GPSTilePosition>(_rawList);
			path._list = null;
			path._runningType = _runningType;
			path._weight = _weight;
			path._path_id = _path_id;
			path._paused = _paused;
			path._modeData = _modeData.clone();

			path._tempSubPathList = new List<SubPath>();
			path._subPathList = new List<SubPath>();

			path._tempSubPathList.AddRange(_tempSubPathList);
			path._subPathList.AddRange(_subPathList);

			return path;
		}

		public LongVector2 getSubPathTimeRange(SubPath subPath)
		{
			LongVector2 result = new LongVector2();
			result.x = _list[subPath.range.x].time;
			result.y = _list[subPath.range.y].time;
			return result;
		}

		// 자잘한거 다 저장할 필요 없음

		void CustomSerializer.pack(MessagePacker packer)
		{
			packer.packInt(_runningType);
			packer.packDouble(_weight);
			packer.packInt(_path_id);
			packer.packBoolean(_paused);

			ObjectPacker objPacker = ObjectPacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);
			objPacker.pack(packer, _modeData);

			packer.packArrayHeader(_rawList.Count);
			foreach(GPSTilePosition pos in _rawList)
			{
				pos.pack(packer);
			}

			packSubPathList(_tempSubPathList, packer);
			packSubPathList(_subPathList, packer);
		}

		void CustomSerializer.unpack(MessageUnpacker unpacker)
		{
			_runningType = unpacker.unpackInt();
			_weight = unpacker.unpackDouble();
			_path_id = unpacker.unpackInt();
			_paused = unpacker.unpackBoolean();

			ObjectUnpacker objUnpacker = ObjectUnpacker.create(GlobalObjectFactory.getInstance(), SerializeOption.ALL);
			_modeData = objUnpacker.unpack(unpacker) as RunningPathModeData;

			if( _modeData == null)
			{
				throw new System.Exception("_modeData is null");
			}

#if UNITY_EDITOR
			_modeData.printDebugInfo();
#endif

			int count = unpacker.unpackArrayHeader();
			_rawList = new List<GPSTilePosition>();
			for(int i = 0; i < count; ++i)
			{
				_rawList.Add(GPSTilePosition.unpack(i, unpacker));
			}

			_tempSubPathList = unpackSubPathList(unpacker);
			_subPathList = unpackSubPathList(unpacker);
		}

		private void packSubPathList(List<SubPath> list,MessagePacker packer)
		{
			packer.packArrayHeader(list.Count);
			foreach (SubPath path in list)
			{
				path.pack(packer);
			}
		}

		private List<SubPath> unpackSubPathList(MessageUnpacker unpacker)
		{
			List<SubPath> pathList = new List<SubPath>();
			int count = unpacker.unpackArrayHeader();
			for (int i = 0; i < count; ++i)
			{
				SubPath path = SubPath.unpack(unpacker);
				pathList.Add(path);
			}

			return pathList;
		}

		public void postProcessLoad(ClientNFTItem nftItem,ClientNFTBonus nftBonus)
		{
			if( isProMode())
			{
				_statData._nftItem = nftItem;
				_statData._nftBonus = nftBonus;
			}

			// _filter 세팅
			if( _rawList.Count > 0)
			{
				GPSTilePosition pos = _rawList[_rawList.Count - 1];

				_filter = new KalmanLatLong(_filterThreshold);
				_filter.SetState(pos.gps_pos.y, pos.gps_pos.x, 1.0f, pos.time);	// accuracy값은 음..
			}

			// filtering
			_list = GPSPathSimplify.Simplify(_rawList, GPSPathSimplify.defaultThreshold, false);

			// 계산
			//process();
			//processAfterUpdateStepCount();
		}

		public static GPSTilePosition getLastPosition(List<RunningPathData> pathList)
		{
			GPSTilePosition pos = null;
			for(int i = pathList.Count - 1; i >= 0; --i)
			{
				RunningPathData path = pathList[i];
				if( path.hasGPSPosition())
				{
					pos = path.getLastPosition();
					break;
				}
			}

			return null;
		}
	}
}
