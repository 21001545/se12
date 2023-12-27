using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.RefData;
using DRun.Client.Running;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.ViewModel
{
	public class RunningViewModel : AbstractViewModel
	{
		private int _running_type;
		private int _running_sub_type;
		private int _running_id;
		private int _goal;
		private int		 _status;
		private int		 _prevStatus;
		private DateTime _startTime;            // 시작 버튼을 누른 시간
		private DateTime _gpsAvailableTime;		// GPS 신호가 유효한 시간
		private DateTime _trackingStartTime;    // 실제 기록으로 인정하기 시작한 시간
		private long _lastGPSQueryTime;
		private DateTime _endTime;
		private int _gpsSignalStatus;			// GPS 수신감도

		private bool _gpsChecked;
		private bool _firstMoveChecked;
		private ClientLocationLog _firstMoveCheckLocation;
		private RunningPathData _currentPathData;
		private double _weight;
		private int _nftTokenID;

		private List<RunningPathData> _pathList; // 이동 경로
		private List<RunningPathEvent> _pathEventList;

		private long _drnTotal;
		private long _drnRunning;
		private long _drnBonus;
		private TimeSpan _totalTime;
		private double _distance;
		private double _mineDistance;
		private double _velocity;
		private int _stepCount;
		private double _calories;
		private float _goalRatio;

		private int _nft_heart;
		private int _nft_bonus_heart;
		private int _nft_distance;
		private int _nft_bonus_distance;
		private int _nft_stamina;
		private int _nft_final_stamina;

		private float _nft_heart_ratio;
		private float _nft_distance_ratio;
		private float _nft_stamina_ratio;

		private int _used_nft_heart;
		private int _used_nft_distance;
		private int _used_nft_stamina;
		
		//private RunningStatData _statData;

		private MBLongLatCoordinate _startLocation;
		private MBLongLatCoordinate _currentLocation;

		//
		private ClientRunningConfig _runningConfig;

		#region Properties_Running

		public int RunningType
		{
			get
			{
				return _running_type;
			}
			set
			{
				Set(ref _running_type, value);
			}
		}

		public int RunningSubType
		{
			get
			{
				return _running_sub_type;
			}
			set
			{
				Set(ref _running_sub_type, value);
			}
		}

		public int RunningID
		{
			get
			{
				return _running_id;
			}
			set
			{
				Set(ref _running_id, value);
			}
		}

		public int Goal
		{
			get
			{
				return _goal;
			}
			set
			{
				Set(ref _goal, value);
			}
		}

		public float GoalRatio
		{
			get
			{
				return _goalRatio;
			}
			set
			{
				Set(ref _goalRatio, value);
			}
		}

		public int Status
		{
			get
			{
				return _status;
			}
			set
			{
				Set(ref _status, value);
			}
		}

		public int PrevStatus
		{
			get
			{
				return _prevStatus;
			}
			set
			{
				Set(ref _prevStatus, value);
			}
		}

		public DateTime StartTime
		{
			get
			{
				return _startTime;
			}
			set
			{
				Set(ref _startTime, value);
			}
		}

		public DateTime GPSAvailableTime
		{
			get
			{
				return _gpsAvailableTime;
			}
			set
			{
				Set(ref _gpsAvailableTime, value);
			}
		}

		public int GPSSignalStatus
		{
			get
			{
				return _gpsSignalStatus;
			}
			set
			{
				Set(ref _gpsSignalStatus, value);
			}
		}

		public bool GPSChecked
		{
			get
			{
				return _gpsChecked;
			}
			set
			{
				Set(ref _gpsChecked, value);
			}
		}

		public bool FirstMoveChecked
		{
			get
			{
				return _firstMoveChecked;
			}
			set
			{
				Set(ref _firstMoveChecked, value);
			}
		}

		public ClientLocationLog FirstMoveCheckLocation
		{
			get
			{
				return _firstMoveCheckLocation;
			}
			set
			{
				Set(ref _firstMoveCheckLocation, value);
			}
		}

		public DateTime TrackingStartTime
		{
			get
			{
				return _trackingStartTime;
			}
			set
			{
				Set(ref _trackingStartTime, value);
			}
		}

		public long LastGPSQueryTime
		{
			get
			{
				return _lastGPSQueryTime;
			}
			set
			{
				Set(ref _lastGPSQueryTime, value);
			}
		}

		public DateTime EndTime
		{
			get
			{
				return _endTime;
			}
			set
			{
				Set(ref _endTime, value);
			}
		}

		public List<RunningPathData> PathList
		{
			get
			{
				return _pathList;
			}
		}

		public RunningPathData CurrentPathData
		{
			get
			{
				return _currentPathData;
			}
			set
			{
				Set(ref _currentPathData, value);
			}
		}

		public MBLongLatCoordinate StartLocation
		{
			get
			{
				return _startLocation;
			}
			set
			{
				Set(ref _startLocation, value);
			}
		}

		public MBLongLatCoordinate CurrentLocation
		{
			get
			{
				return _currentLocation;
			}
			set
			{
				Set(ref _currentLocation, value);
			}
		}

		public List<RunningPathEvent> PathEventList
		{
			get
			{
				return _pathEventList;
			}
		}

		#endregion

		#region Properties_Status

		public long DRNTotal
		{
			get
			{
				return _drnTotal;
			}
			set
			{
				Set(ref _drnTotal, value);
			}
		}

		public long DRNRunning
		{
			get
			{
				return _drnRunning;
			}
			set
			{
				Set(ref _drnRunning, value);
			}
		}

		public long DRNBonus
		{
			get
			{
				return _drnBonus;
			}
			set
			{
				Set(ref _drnBonus, value);
			}
		}

		public double Distance
		{
			get
			{
				return _distance;
			}
			set
			{
				Set(ref _distance, value);
			}
		}

		public double MineDistance
		{
			get
			{
				return _mineDistance;
			}
			set
			{
				Set(ref _mineDistance, value);
			}
		}

		public double Velocity
		{
			get
			{
				return _velocity;
			}
			set
			{
				Set(ref _velocity, value);
			}
		}

		public int StepCount
		{
			get
			{
				return _stepCount;
			}
			set
			{
				Set(ref _stepCount, value);
			}
		}

		public double Calories
		{
			get
			{
				return _calories;
			}
			set
			{
				Set(ref _calories, value);
			}
		}

		public TimeSpan TotalTime
		{
			get
			{
				return _totalTime;
			}
			set
			{
				Set(ref _totalTime, value);
			}
		}

		public ClientNFTItem NFTItem
		{
			get
			{
				if( isMarathonMode())
				{
					return null;
				}

				return _currentPathData.getStatData()._nftItem;
			}
		}

		public int NFTHeart
		{
			get
			{
				return _nft_heart;
			}
			set
			{
				Set(ref _nft_heart, value);
			}
		}

		public int NFTDistance
		{
			get
			{
				return _nft_distance;
			}
			set
			{
				Set(ref _nft_distance, value);
			}
		}

		public int NFTStamina
		{
			get
			{
				return _nft_stamina;
			}
			set
			{
				Set(ref _nft_stamina, value);
			}
		}

		public int NFTFinalStamina
		{
			get
			{
				return _nft_final_stamina;
			}
			set
			{
				Set(ref _nft_final_stamina, value);
			}
		}

		public int NFTBonusHeart
		{
			get
			{
				return _nft_bonus_heart;
			}
			set
			{
				Set(ref _nft_bonus_heart, value);
			}
		}

		public int NFTBonusDistance
		{
			get
			{
				return _nft_bonus_distance;
			}
			set
			{
				Set(ref _nft_bonus_distance, value);
			}
		}

		public float NFTHeartRatio
		{
			get
			{
				return _nft_heart_ratio;
			}
			set
			{
				Set(ref _nft_heart_ratio, value);
			}
		}

		public float NFTDistanceRatio
		{
			get
			{
				return _nft_distance_ratio;
			}
			set
			{
				Set(ref _nft_distance_ratio, value);
			}
		}

		public float NFTStaminaRatio
		{
			get
			{
				return _nft_stamina_ratio;
			}
			set
			{
				Set(ref _nft_stamina_ratio, value);
			}
		}

		// ui갱신용
		public int UpdateStat
		{
			get;set;
		}

		#endregion

		#region Proerties_Server

		public ClientRunningConfig RunningConfig
		{
			get
			{
				return _runningConfig;
			}
			set
			{
				Set(ref _runningConfig, value);
			}
		}
		#endregion

		//#region PFPStat

		//public RunningStatData StatData
		//{
		//	get
		//	{
		//		return _statData;
		//	}
		//}

		//#endregion

		// 손가락 아퍼서 만듬
		public bool isProMode()
		{
			return _running_type == ClientRunningLogCumulation.RunningType.promode;
		}

		public bool isMarathonMode()
		{
			return _running_type == ClientRunningLogCumulation.RunningType.marathon;
		}

		public static RunningViewModel create()
		{
			RunningViewModel viewModel = new RunningViewModel();
			viewModel.init();
			return viewModel;
		}

		protected override void init()
		{
			base.init();

			_pathList = new List<RunningPathData>();
			_pathEventList = new List<RunningPathEvent>();
			_status = StateType.none;
			//_statData = new RunningStatData();
		}

		protected override void bindPacketParser()
		{
			bind(MapPacketKey.ClientAck.running_config, updateRunningConfig);
		}

		private void updateRunningConfig(object obj,MapPacket ack)
		{
			RunningConfig = (ClientRunningConfig)obj;
		}
			
		public void resetForStart(int running_type,int running_sub_type,
					int goal,ClientProMode statData,ClientNFTItem nftItem,ClientNFTBonus nftBonus,double weight)
		{
			DateTime now = DateTime.UtcNow;

			RunningType = running_type;
			RunningSubType = running_sub_type;
			RunningID = _runningConfig.next_running_id;
			Goal = goal;
			StartTime = now;
			TrackingStartTime = now;
			GPSAvailableTime = now;
			EndTime = now;

			GPSSignalStatus = GPSStatusInfo.Status.no_signal;
			GPSChecked = false;
			FirstMoveChecked = false;
			FirstMoveCheckLocation = null;

			_pathList.Clear();
			_currentPathData = null;
			_weight = weight;
			_nftTokenID = isProMode() ? nftItem.token_id : 0;

			startPath(nftItem, nftBonus);
			calcStatus();

			writePathEvent(RunningPathEvent.EventType.reset, null);
		}

		private void startPath(ClientNFTItem nftItem,ClientNFTBonus nftBonus)
		{
			RunningPathModeData pathSnapData;
			if (isProMode())
			{
				pathSnapData = RunningPathStatData.create(nftItem, nftBonus);
				//_statData.resetForStart(nftItem, nftBonus);
			}
			else
			{
				pathSnapData = RunningPathMarathonData.create(RunningSubType, _goal);
				//_statData.resetForStart(null, null);
			}

			RunningPathData pathData = RunningPathData.create(1, _running_type, false, _weight, pathSnapData);
			addPathData(pathData);
		}

		public void resetForContinue(LocalRunningStatusData statusData,List<RunningPathData> pathList)
		{
			RunningType = statusData.running_type;
			RunningSubType = statusData.running_sub_type;
			RunningID = statusData.running_id;
			Goal = statusData.goal;

			StartTime = TimeUtil.dateTimeFromUnixTimestamp(statusData.startTime);
			GPSAvailableTime = TimeUtil.dateTimeFromUnixTimestamp(statusData.gpsAvailableTime);
			TrackingStartTime = TimeUtil.dateTimeFromUnixTimestamp(statusData.trackingStartTime);
			LastGPSQueryTime = statusData.lastGPSQueryTime;

			GPSSignalStatus = GPSStatusInfo.Status.no_signal;
			GPSChecked = true;
			FirstMoveChecked = true;

			//_statData = statusData.statData;

			_weight = statusData.weight;
			_nftTokenID = statusData.nft_token_id;

			_pathList = pathList;
			_currentPathData = pathList[pathList.Count - 1];
			_startLocation = pathList[0].getFirstPosition();

			GPSTilePosition lastPosition = RunningPathData.getLastPosition(pathList);

			if( lastPosition != null)
			{
				_currentLocation = lastPosition;
			}
			else
			{
				_currentLocation = _startLocation;
			}

			writePathEvent(RunningPathEvent.EventType.reset, null);
			foreach(RunningPathData path in pathList)
			{
				writePathEvent(RunningPathEvent.EventType.create_path, path);
			}

			calcStatus();
		}

		private void addPathData(RunningPathData pathData)
		{
			_pathList.Add(pathData);
			CurrentPathData = pathData;

			writePathEvent(RunningPathEvent.EventType.create_path, pathData);
		}

		public void splitPath(bool pause)
		{
			RunningPathData pathData = CurrentPathData.createContinue(pause);
			addPathData(pathData);

			calcStatus();
		}

		//public void appendNewPathDataContinueFromCurrentPath(int trip_type,int mined)
		//{
		//	List<ClientLocationLog> logList = null;
		//	if(_currentPathData != null)
		//	{
		//		logList = new List<ClientLocationLog>();
		//		logList.Add(_currentPathData.getLastLocationLog());
		//	}

		//	appendNewPathData(trip_type, mined, logList);
		//}

		//public void appendNewPathData(int trip_type,int mined,List<ClientLocationLog> logList)
		//{
		//	ClientTripPathData newPath = ClientTripPathData.create(trip_type,mined,_weight,logList);
		//	newPath.path_id = _next_path_id;
		//	++_next_path_id;

		//	_pathList.Add(newPath);

		//	CurrentPathData = newPath;

		//	writePathEvent(RunningPathEvent.EventType.create_path, newPath);
		//}

		public void appendCurrentPathLog(List<ClientLocationLog> logList)
		{
			if( CurrentPathData != null)
			{
				CurrentPathData.appendLog(logList);
				
				notifyPropetyChanged("CurrentPathData");
				//writePathEvent(RunningPathEvent.EventType.append_log, CurrentPathData);
			}

			calcStatus();
		}

		public void calcStatus()
		{
			Distance = calcTotalDistance();
			MineDistance = calcMineDistance();
			Velocity = calcAverageVelocity();
			TotalTime = TimeSpan.FromSeconds(calcTotalTime());
			StepCount = calcTotalStepCount();
			Calories = calcTotalCalories();

			DRNRunning = calcRunningDRN(MineDistance);
			DRNBonus = calcBonusDRN(DRNRunning);
			DRNTotal = DRNRunning + DRNBonus;

			GoalRatio = calcGoalRatio();

			//Debug.Log($"distance[{Distance}] Velocity[{Velocity}] DRNRunning[{DRNRunning}] TotalTime[{totalTime}] Calories[{Calories}]");
			calcNFTStat();
			calcLastVelocity();

			//Debug.Log($"mineDistance[{MineDistance}] drnTotal[{DRNTotal}] drnTotalDisplay[{StringUtil.toDRNStringDefault(DRNTotal)}]");
		}

		//시간일때는 초단위값
		public int getCurrentGoalResult()
		{
			if (_running_type != ClientRunningLogCumulation.RunningType.marathon)
			{
				return 0;
			}

			if (_running_sub_type == ClientRunningLogCumulation.MarathonType._free_time)
			{
				return (int)TotalTime.TotalSeconds;
			}
			else
			{
				return (int)(Distance * 1000);
			}
		}

		private float calcGoalRatio()
		{
			if( _running_type != ClientRunningLogCumulation.RunningType.marathon)
			{
				return 0.0f;
			}

			float ratio = 0;

			// 목표 (분)
			if( _running_sub_type == ClientRunningLogCumulation.MarathonType._free_time)
			{
				ratio = (float)TotalTime.TotalMinutes / (float)Goal;
			}
			// 목표 (미터)
			else
			{
				ratio = (float)(Distance / ((double)Goal / 1000.0));
			}

			return Mathf.Clamp(ratio, 0, 1);
		}

		private long calcRunningDRN(double distance)
		{
			if( isMarathonMode())
			{
				return 0;
			}

			int distanceMeter = (int)System.Math.Floor(distance * 1000);

			double configAcquireDRN = GlobalRefDataContainer.getConfigDouble(RefConfig.Key.DRun.running_pro_acquiredDRN, 0.5);
			int minDistance = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.running_pro_minDistanceForMining, 100);

			// 최소 100미터는 걸어야 채굴 된다
			if( distanceMeter < minDistance)
			{
				return 0;
			}


			double drn_per_meter = configAcquireDRN / 1000.0;

			// 스태미너에 따른 채굴 효율 반영
			drn_per_meter = drn_per_meter * calcStaminaEfficiencyDRN() / 100.0;

			long peb = GlobalRefDataContainer.getRefDataHelper().toPeb(drn_per_meter * distanceMeter);
			return peb;
		}

		private long calcBonusDRN(long running_drn)
		{
			if (isMarathonMode())
			{
				return 0;
			}

			RefNFTLevel refLevel = GlobalRefDataContainer.getInstance().get<RefNFTLevel>( NFTItem.level);
			
			return (long)(running_drn * (double)refLevel.mining_bonus_percent / 100.0);
		}

		private double calcTotalCalories()
		{
			double calorie = 0;
			foreach(RunningPathData path in _pathList)
			{
				calorie += path.calcTotalCalorie();
			}

			return calorie;
		}

		private double calcTotalDistance()
		{
			double distance = 0;
			foreach(RunningPathData path in _pathList)
			{
				distance += path.calcTotalDistance();
			}

			// 오류 수정			
			if( isMarathonMode() && _running_sub_type != ClientRunningLogCumulation.MarathonType._free_time)
			{
				// goal보다 멀리 갈 수 없다
				double goal_km = (double)Goal / 1000.0;
				if( distance > goal_km)
				{
					distance = goal_km;
				}
			}
			
			return distance;
		}

		private double calcMineDistance()
		{
			if( isMarathonMode())
			{
				return calcTotalDistance();
			}
			else
			{
				double distance = 0;
				foreach(RunningPathData path in _pathList)
				{
					distance += path.calcTotalMineDistance();
				}

				return distance;
			}
		}

		private double calcTotalTime()
		{
			if (_pathList.Count == 0)
			{
				return 0;
			}

			double prevPathTime = 0;
			double activePathTime = 0;
			double totalTime = 0;

			for (int i = 0; i < _pathList.Count - 1; ++i)
			{
				prevPathTime += _pathList[i].calcTotalTime(false);
			}

			if( _currentPathData.isTimeActive())
			{
				List<GPSTilePosition> rawList = _currentPathData.getRawList();
				if( rawList.Count > 0)
				{
					long pathBeginTime = rawList[0].time;
					activePathTime = (DateTime.UtcNow - TimeUtil.dateTimeFromUnixTimestamp(pathBeginTime)).TotalSeconds;
				}
			}
			else
			{
				prevPathTime += _currentPathData.calcTotalTime(false);
			}

			totalTime = prevPathTime + activePathTime;
//			Debug.Log($"totalTime[{totalTime.ToString("N2")}] inactivePathTime[{prevPathTime.ToString("N2")}] activePathTime[{activePathTime.ToString("N2")}]");

			//Debug.Log($"totalTime[{totalTime}]");

			//for(int i = 0; i < _pathList.Count - 1; ++i)
			//{
			//	ClientTripPathData path = _pathList[i];
			//	if( path.trip_type == ClientTripConfig.TripType.none)
			//	{
			//		continue;
			//	}

			//	if( isProMode() && path.mined == 0)
			//	{
			//		continue;
			//	}

			//	totalTime += path.getDuration();
			//}

			//ClientTripPathData lastPath = _pathList[_pathList.Count - 1];
			//if (lastPath.trip_type != ClientTripConfig.TripType.none && (isMarathonMode() || lastPath.mined == 1))
			//{
			//	totalTime += (DateTime.UtcNow - TimeUtil.dateTimeFromUnixTimestamp(lastPath.path_begin_time)).TotalSeconds;
			//}

			return totalTime;
		}

		private double calcAverageVelocity()
		{
			double totalTime = 0;
			foreach (RunningPathData path in _pathList)
			{
				totalTime += path.calcTotalTime(true);
			}

			if (totalTime <= 0)
			{
				return 0;
			}
			else
			{
				return Distance * 3600.0 / totalTime;
			}
		}

		public int calcTotalStepCount()
		{
			int stepCount = 0;
			foreach(RunningPathData path in _pathList)
			{
				stepCount += path.calcTotalStep();
			}

			return stepCount;
		}

		private void calcNFTStat()
		{
			if( isMarathonMode())
			{
				NFTHeart = 0;
				NFTDistance = 0;
				NFTStamina = 0;
				NFTFinalStamina = 0;
				NFTBonusHeart = 0;
				NFTBonusDistance = 0;
				NFTHeartRatio = 0;
				NFTDistanceRatio = 0;
				NFTStaminaRatio = 0;

				_used_nft_heart = 0;
				_used_nft_distance = 0;
				_used_nft_stamina = 0;
			}
			else
			{
				RunningPathStatData firstStatData = _pathList[0].getStatData();
				RunningPathStatData statData = _currentPathData.getStatData();

				RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(firstStatData._nftItem.grade);

				int max_heart = firstStatData._nftItem.max_heart;
				int max_distance = firstStatData._nftItem.max_distance;
				int max_stamina = refGrade.stamina * 100;

				NFTHeart = statData.end_heart;
				NFTDistance = statData.end_distance;
				NFTBonusHeart = statData.end_bonus_heart;
				NFTBonusDistance = statData.end_bonus_distance;

				NFTHeartRatio = Mathf.Clamp( max_heart == 0 ? 0 : (float)NFTHeart / (float)max_heart, 0, 1);
				NFTDistanceRatio = Mathf.Clamp( (float)NFTDistance / (float)max_distance, 0, 1);
				NFTStaminaRatio = Mathf.Clamp( (float)NFTStamina / (float)max_stamina, 0, 1);

				_used_nft_heart = (firstStatData.begin_heart - statData.end_heart) +
								  (firstStatData.begin_bonus_heart - statData.end_bonus_heart);
				_used_nft_distance = (firstStatData.begin_distance - statData.end_distance) +
									(firstStatData.begin_bonus_distance - statData.end_bonus_distance);


#if UNITY_EDITOR
				Debug.Log($"heart[{firstStatData.begin_heart - statData.end_heart}], heart_bonus[{firstStatData.begin_bonus_heart - statData.end_bonus_heart}]");
				Debug.Log($"distance[{firstStatData.begin_distance - statData.end_distance}], distance_bonus[{firstStatData.begin_bonus_distance - statData.end_bonus_distance}]");
#endif

				calcStamina();
			}

			notifyPropetyChanged("UpdateStat");
		}

		private void calcLastVelocity()
		{
//#if UNITY_EDITOR
//			if( _currentPathData != null)
//			{
//				List<GPSTilePosition> rawList = _currentPathData.getRawList();
//				List<GPSTilePosition> list = _currentPathData.getPosList();

//				if( rawList.Count > 1 && list.Count > 1)
//				{
//					GPSTilePosition last = list[list.Count - 1];

//					GPSTilePosition rawBegin = rawList[rawList.Count - 2];
//					GPSTilePosition rawEnd = rawList[rawList.Count - 1];

//					double distance = MapBoxUtil.distance( rawBegin.gps_pos, rawEnd.gps_pos );
//					double time = (double)(rawEnd.time - rawBegin.time) / (double)TimeUtil.msSecond;
//					double velocty = distance * 3600.0 / time;

//					Debug.Log($"rawVelocity[{velocty} km/h] lastVelocity[{last.deltaSpeedKMH} km/h]");
//				}
//			}
//#endif
		}

		private void calcStamina()
		{
			RunningPathStatData firstStatData = _pathList[0].getStatData();
			RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(NFTItem.grade);

			int max_stamina = refGrade.stamina * 100;

			// km당 감소 비율
			double reductPercent = refGrade.reducepercent_distance * MineDistance / 100.0;
			int reductValue = (int)(reductPercent * (float)max_stamina);

			if (firstStatData.begin_stamina - reductValue < 0)
			{
				reductValue = firstStatData.begin_stamina;
			}

			_used_nft_stamina = reductValue;
			//NFTStamina = 
			NFTStamina = firstStatData.begin_stamina;
			NFTFinalStamina = firstStatData.begin_stamina - reductValue;
			NFTStaminaRatio = (float)NFTStamina / (float)max_stamina;
//#if UNITY_EDITOR
//			Debug.Log($"mineDistance[{MineDistance}] delta[{reductValue}] stamina[{NFTStamina}]");
//#endif

		}

		private double calcStaminaEfficiencyDRN()
		{
			// 런닝 시작 시점의 stamina_ratio가 필요함
			RunningPathStatData firstStatData = _pathList[0].getStatData();
			RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(firstStatData._nftItem.grade);

			double runningStartStaminaRatio = (double)firstStatData.begin_stamina / (double)(refGrade.stamina * 100);

			return GlobalRefDataContainer.getRefDataHelper().getNFTStaminaEfficiencyDRN((int)(runningStartStaminaRatio * 100));
		}

		public void writePathEvent(int type, RunningPathData pathData)
		{
			_pathEventList.Add(RunningPathEvent.create(type, pathData));

			notifyPropetyChanged("PathEventList");
		}

		public void popPathEvent(List<RunningPathEvent> outputList)
		{
			outputList.AddRange(_pathEventList);
			_pathEventList.Clear();
		}

		//public void notifyRunningStatUpdate()
		//{
		//	notifyPropetyChanged("StatData");
		//}

		// 현재 상태 그대로 저장
		public ClientRunningLog createRunningLog()
		{
			ClientRunningLog log = new ClientRunningLog();
			log.running_type = _running_type;
			log.running_sub_type = _running_sub_type;
			log.running_id = _running_id;
			log.pfp = _nftTokenID.ToString();
			log.start_time = _startTime;
			log.end_time = DateTime.UtcNow;
			log.goal = _goal;
			log.drn_total = _drnTotal;
			log.drn_running = _drnRunning;
			log.drn_bonus = _drnBonus;
			log.running_time = (int)_totalTime.TotalSeconds;
			log.distance = _distance;
			log.mine_distance = _mineDistance;
			log.velocity = _velocity;
			log.step_count = _stepCount;
			log.calories = _calories;
			log.used_heart = _used_nft_heart;
			log.used_distance = _used_nft_distance;
			log.used_stamina = _used_nft_stamina;

			log.pathList = new List<ClientTripPathData>();
			foreach(RunningPathData path in _pathList)
			{
				path.toTriPathData(log.pathList);
			}

			return log;
		}

		public LocalRunningStatusData createLocalRunningStatusData()
		{
			LocalRunningStatusData data = new LocalRunningStatusData();
			data.running_type = _running_type;
			data.running_sub_type = _running_sub_type;
			data.running_id = _running_id;
			data.goal = _goal;
			data.status = _status;
			data.path_count = _pathList.Count;
			data.startTime = TimeUtil.unixTimestampFromDateTime(_startTime);
			data.gpsAvailableTime = TimeUtil.unixTimestampFromDateTime(_gpsAvailableTime);
			data.trackingStartTime = TimeUtil.unixTimestampFromDateTime(_trackingStartTime);
			data.lastGPSQueryTime = _lastGPSQueryTime;
			//data.statData = _statData;
			data.weight = _weight;
			data.nft_token_id = _nftTokenID;
			return data;
		}

	}
}
