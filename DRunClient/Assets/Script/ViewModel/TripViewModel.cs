using Festa.Client.Module;
using Festa.Client.NetData;
using Festa.Client.MapBox;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.Module.Net;
using System;
using System.Linq;

namespace Festa.Client.ViewModel
{
	public class TripViewModel : AbstractViewModel
	{
		private ClientTripConfig _data;
		private ClientTripCheeringConfig _cheeringConfig;
		private List<ClientTripLog> _logList;
		private List<ClientTripPathData> _currentTripPathDataList;
		private List<ClientTripPhoto> _currentTripPhotoList;
		private long _tryStartTripTime;
		private long _lastQueryLogTime;

		private List<ClientTripCheerable> _cheerableListByDistance;
		private List<ClientTripCheerable> _cheerableListByFollow;

		private int _totalCheerableCount;
		private long _lastQueryCheerableTime;

		private int _latestCheeringID;
		private List<ClientTripCheering> _currentUnreadCheeringList;

		public ClientTripConfig Data
		{
			get
			{
				return _data;
			}
			set
			{
				Set<ClientTripConfig>(ref _data, value);
			}
		}

		public ClientTripCheeringConfig CheeringConfig
		{
			get
			{
				return _cheeringConfig;
			}
			set
			{
				Set(ref _cheeringConfig, value);
			}
		}

		public List<ClientTripLog> LogList
		{
			get
			{
				return _logList;
			}
			set
			{
				_logList = value;
				notifyPropetyChanged("LogList");
			}
		}

		// 현재 기록중인 경로
		public List<ClientTripPathData> CurrentTripPathDataList
		{
			get
			{
				return _currentTripPathDataList;
			}
		}

		public List<ClientTripPhoto> CurrentTripPhotoList
		{
			get
			{
				return _currentTripPhotoList;
			}
			set
			{
				Set(ref _currentTripPhotoList, value);
			}
		}


		public ClientTripPathData CurrentTripPathData
		{
			get
			{
				if (_currentTripPathDataList == null || _currentTripPathDataList.Count == 0)
				{
					return null;
				}

				return _currentTripPathDataList[_currentTripPathDataList.Count - 1];
			}
		}

		public List<ClientTripCheering> CurrentUnreadCheeringList
		{
			get
			{
				return _currentUnreadCheeringList;
			}
			set
			{
				Set(ref _currentUnreadCheeringList, value);
			}
		}

		public int LatestCheeringID
		{
			get
			{
				return _latestCheeringID;
			}
			set
			{
				Set(ref _latestCheeringID, value);
			}
		}

		public long TryStartTripTime
		{
			get
			{
				return _tryStartTripTime;
			}
			set
			{
				Set(ref _tryStartTripTime, value);
			}
		}

		public long LastQueryLogTime
		{
			get
			{
				return _lastQueryLogTime;
			}
			set
			{
				Set(ref _lastQueryLogTime, value);
			}
		}

		public List<ClientTripCheerable> CheerableListByDistance
		{
			get
			{
				return _cheerableListByDistance;
			}
			set
			{
				Set(ref _cheerableListByDistance, value);
			}
		}
		
		public List<ClientTripCheerable> CheerableListByFollow
		{
			get
			{
				return _cheerableListByFollow;
			}
			set
			{
				Set(ref _cheerableListByFollow, value);
			}
		}

		public int TotalCheerableCount
		{
			get
			{
				return _totalCheerableCount;
			}
			set
			{
				Set(ref _totalCheerableCount, value);
			}
		}

		public long LastQueryCheerableTime
		{
			get
			{
				return _lastQueryCheerableTime;
			}
			set
			{
				Set(ref _lastQueryCheerableTime, value);
			}
		}
	
		public static TripViewModel create()
		{
			TripViewModel vm = new TripViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_logList = new List<ClientTripLog>();
			_currentTripPathDataList = new List<ClientTripPathData>();
			_currentTripPhotoList = new List<ClientTripPhoto>();
			_cheerableListByDistance = new List<ClientTripCheerable>();
			_cheerableListByFollow = new List<ClientTripCheerable>();
			_currentUnreadCheeringList = new List<ClientTripCheering>();
			_latestCheeringID = 0;
			_lastQueryLogTime = 0;
		}

		public override void updateFromAck(MapPacket ack)
		{
			if( ack.contains(MapPacketKey.ClientAck.trip_config))
			{
				Data = (ClientTripConfig)ack.get(MapPacketKey.ClientAck.trip_config);
			}
			if( ack.contains(MapPacketKey.ClientAck.trip_cheering_config))
			{
				CheeringConfig = (ClientTripCheeringConfig)ack.get(MapPacketKey.ClientAck.trip_cheering_config);
			}
			if( ack.contains(MapPacketKey.ClientAck.trip_last_path))
			{
				_currentTripPathDataList = ack.getList<ClientTripPathData>(MapPacketKey.ClientAck.trip_last_path);
			}
			if( ack.contains(MapPacketKey.ClientAck.trip_last_photo))
			{
				CurrentTripPhotoList = ack.getList<ClientTripPhoto>(MapPacketKey.ClientAck.trip_last_photo);
			}
			if (ack.contains(MapPacketKey.ClientAck.trip_log))
			{
				AppendTripLog(ack.getList<ClientTripLog>(MapPacketKey.ClientAck.trip_log));
			}
		}

		public void appendLocationLogToCurrentPath(List<ClientLocationLog> logList)
		{
			CurrentTripPathData.appendLocation(logList);
			notifyPropetyChanged("CurrentTripPathData");
		}

		public void appendTripPhoto(ClientTripPhoto photo)
		{
			_currentTripPhotoList.Add(photo);
			notifyPropetyChanged("CurrentTripPhotoList");
		}

		public void AppendTripLog(List<ClientTripLog> log_list)
		{
			foreach(ClientTripLog log in log_list)
			{
				if( findLog(log.trip_id) == null)
				{
					//Debug.Log($"append trip log:{log.trip_id}");

					_logList.Add(log);
				}
			}

			_logList.Sort((a, b) => { 
				
				if( a.trip_id > b.trip_id)
				{
					return -1;
				}
				else if( a.trip_id < b.trip_id)
				{
					return 1;
				}

				return 0;
			});

			notifyPropetyChanged("LogList");
		}

		public ClientTripLog findLog(int id)
		{
			for(int i = 0; i < _logList.Count; ++i)
			{
				ClientTripLog log = _logList[i];
				if( log.trip_id == id)
				{
					return log;
				}
			}

			return null;
		}

		// km/s
		public double calcCurrentPace(int unit)
		{
			if (_data.status == ClientTripConfig.StatusType.none)
			{
				return 0;
			}

			double pace = calcCurrentPace_TotalAverage();
			if (unit == UnitDefine.DistanceType.km)
			{
				return pace;
			}
			else
			{
				return UnitDefine.km_2_mil(pace);
			}
		}

		private double calcCurrentPace_TotalAverage()
		{
			double total_distance;
			double total_time;

			// 역계산하면 오차가 있는것 처럼 보여서
			total_distance = _data.distance_total;
			total_time = calcTripPeriodTime().TotalHours;
			
			if( total_time <= 0)
			{
				return 0;
			}

			return total_distance / total_time;

			//total_distance = 0;
			//total_time = 0;
			//for(int i = 0; i < _currentTripPathDataList.Count; ++i)
			//{
			//	ClientTripPathData pathData = _currentTripPathDataList[i];
			//	if( pathData.trip_type == ClientTripConfig.TripType.none)
			//	{
			//		continue;
			//	}

			//	total_distance += pathData.getLength();
			//	total_time += pathData.getDuration();
			//}

			//if( total_time == 0)
			//{
			//	return 0;
			//}

			//return total_distance / (total_time / 3600.0f);
		}

		private double calcCurrentPace_Last10()
		{
			// 최근 10초 동안 이동한 거리 측정
			long begin_time = TimeUtil.unixTimestampUtcNow() - TimeUtil.msSecond * 10;
			long duration = 0;

			double total_distance = 0;

			MBLongLatCoordinate lastLocation = MBLongLatCoordinate.zero;
			long lastTime = 0;

			for (int j = 0; j < _currentTripPathDataList.Count; ++j)
			{
				ClientTripPathData pathData = _currentTripPathDataList[j];

				if (begin_time < pathData.path_end_time)
				{
					continue;
				}

				for (int i = 0; i < pathData.path_list.Count / 3; ++i)
				{
					long time = pathData.path_begin_time + pathData.path_time_list[i];
					if (time < begin_time)
					{
						continue;
					}

					MBLongLatCoordinate curLocation = new MBLongLatCoordinate(pathData.path_list[i * 3 + 0], pathData.path_list[i * 3 + 1]);
					long curTime = pathData.path_begin_time + pathData.path_time_list[i];

					if (lastLocation.isZero() == false)
					{
						total_distance += curLocation.distanceFrom(lastLocation);
						duration += curTime - lastTime;
					}

					lastLocation = curLocation;
					lastTime = curTime;
				}
			}

			if (lastLocation.isZero() == false)
			{
				total_distance += ClientMain.instance.getViewModel().Location.CurrentLocation.distanceFrom(lastLocation);
				duration += TimeUtil.unixTimestampUtcNow() - lastTime;
			}

			double durationHour = (double)duration / (double)TimeUtil.msHour; ;
			if (durationHour == 0)
			{
				return 0;
			}

			return total_distance / durationHour;
		}

		public TimeSpan calcTripPeriodTime()
		{
			long total = 0;

			for(int i = 0; i < _currentTripPathDataList.Count - 1; ++i)
			{
				ClientTripPathData path = _currentTripPathDataList[i];
				if( path.trip_type != ClientTripConfig.TripType.none)
				{
					total += path.path_end_time - path.path_begin_time;
				}
			}

			//
			if( _data.status == ClientTripConfig.StatusType.trip && CurrentTripPathData != null)
			{
				total += System.Math.Max(0, TimeUtil.unixTimestampUtcNow() - CurrentTripPathData.path_begin_time);
			}

			return TimeSpan.FromMilliseconds(total);
		}

		public bool calcCurrentTripPathBound(out MBLongLatCoordinate min,out MBLongLatCoordinate max)
		{
			min = MBLongLatCoordinate.zero;
			max = MBLongLatCoordinate.zero;

			int valid_count = 0;

			for(int i = 0; i < _currentTripPathDataList.Count; ++i)
			{
				ClientTripPathData path = _currentTripPathDataList[i];
				if( path.path_list.Count == 0)
				{
					continue;
				}

				if( valid_count == 0)
				{
					min = path.getMin();
					max = path.getMax();
				}
				else
				{
					MBLongLatCoordinate path_min = path.getMin();
					MBLongLatCoordinate path_max = path.getMax();

					min.pos.x = System.Math.Min(min.pos.x, path_min.pos.x);
					min.pos.y = System.Math.Min(min.pos.y, path_min.pos.y);
					max.pos.x = System.Math.Max(max.pos.x, path_max.pos.x);
					max.pos.y = System.Math.Max(max.pos.y, path_max.pos.y);
				}

				valid_count++;
			}

			return valid_count > 0;
		}

		// 나의 위치가 center가 되도록 했을때, 현재 경로를 포함하는 bound를 구한다
		public bool calcCurrentTripPathBoundWithCenterPosition(MBLongLatCoordinate center,out MBLongLatCoordinate min,out MBLongLatCoordinate max)
		{
			if(calcCurrentTripPathBound( out min, out max) == false)
			{
				return false;
			}

			DoubleVector2 diff_to_min = (min.pos - center.pos).Abs();
			DoubleVector2 diff_to_max = (max.pos - center.pos).Abs();

			DoubleVector2 extent;
			extent.x = System.Math.Max(diff_to_min.x, diff_to_max.x);
			extent.y = System.Math.Max(diff_to_min.y, diff_to_max.y);

			//
			min.pos = center.pos - extent;
			max.pos = center.pos + extent;

			return true;
		}

		public List<TripLogGroup> makeLogGroupByMonth()
		{
			Dictionary<int, TripLogGroup> groupMap = new Dictionary<int, TripLogGroup>();
			foreach(ClientTripLog log in _logList)
			{
				int month = log.getLocalBeginTimeMonth();
				TripLogGroup group;

				if( groupMap.TryGetValue( month, out group) == false)
				{
					group = TripLogGroup.create(month);
					groupMap.Add(month, group);
				}

				group.addLog(log);
			}

			List<TripLogGroup> groupList = groupMap.Values.ToList();
			groupList.Sort((a, b) => { 
				
				if( a.getMonth() > b.getMonth())
				{
					return -1;
				}
				else if( a.getMonth() < b.getMonth())
				{
					return 1;
				}

				return 0;
			
			});

			return groupList;
		}

		public void removeTripCheerable(int account_id)
		{
			removeTripCheerable(_cheerableListByDistance, account_id);
			removeTripCheerable(_cheerableListByFollow, account_id);

			TotalCheerableCount = _cheerableListByDistance.Count + _cheerableListByFollow.Count;
		}

		private void removeTripCheerable(List<ClientTripCheerable> targetList,int account_id)
		{
			foreach(ClientTripCheerable cheerable in targetList)
			{
				if( cheerable.account_id == account_id)
				{
					targetList.Remove(cheerable);
					break;
				}
			}
		}

		public void resetCurrentUnreadTripCheeringList()
		{
			_latestCheeringID = 0;
			_currentUnreadCheeringList.Clear();
		}

		public void appendCurrentUnreadTripCheeringList(List<ClientTripCheering> list)
		{
			foreach(ClientTripCheering cheering in list)
			{
				if( cheering.slot_id > _latestCheeringID)
				{
					_latestCheeringID = cheering.cheer_id;
				}

				_currentUnreadCheeringList.Add(cheering);
			}

			Debug.Log($"append unread cheering list: count[{list.Count}]");
			notifyPropetyChanged("CurrentUnreadCheeringList");
		}

		public void popCurrentUnreadlTripCheeringList(List<ClientTripCheering> output)
		{
			output.AddRange(_currentUnreadCheeringList);
			_currentUnreadCheeringList.Clear();
		}
	}
}
