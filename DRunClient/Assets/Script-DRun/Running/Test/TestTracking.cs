//using DRun.Client.RefData;
//using DRun.Client.ViewModel;
//using Festa.Client;
//using Festa.Client.MapBox;
//using Festa.Client.Module;
//using Festa.Client.NetData;
//using Festa.Client.RefData;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices.WindowsRuntime;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace DRun.Client.Running
//{
//	public class TestTracking
//	{
//		private List<ClientTripPathData> _pathList;
//		private RunningStatData _statData;
		
//		private MBLongLatCoordinate _lastLocation;
//		private long _lastLocationTime;

//		public class ProcessedPathData
//		{
//			public int mined;
//			public int step_count;
//			public List<ClientLocationLog> log_list;
//			public long start_time;
//			public long end_time;

//			public static ProcessedPathData create(int mined)
//			{
//				ProcessedPathData data = new ProcessedPathData();
//				data.mined = mined;
//				data.log_list = new List<ClientLocationLog>();
//				return data;
//			}
//		}

//		public static TestTracking create()
//		{
//			TestTracking testTracking = new TestTracking();
//			testTracking.init();
//			return testTracking;
//		}

//		private void init()
//		{
//			_statData = new RunningStatData();
//			_statData.heart = 6;
//			_statData.distance = 10000;

//			_pathList = new List<ClientTripPathData>();

//			_lastLocation = new MBLongLatCoordinate(127.0543616485475, 37.50637354896867);
//			_lastLocationTime = TimeUtil.unixTimestampFromDateTime(new DateTime(2022, 10, 21, 14, 45, 0));

//			List<ClientLocationLog> logList = new List<ClientLocationLog>();
//			logList.Add(genLocation( 0, 0));
//			_pathList.Add(ClientTripPathData.create(0, 1, logList));
//		}

//		private void appendNewPath(int mined,List<ClientLocationLog> logList)
//		{
//			ClientTripPathData path = ClientTripPathData.create(0, mined, logList);
//			_pathList.Add(path);
//		}

//		public void test()
//		{
//			int count = 60;
//			for(int i = 0; i < count; ++i)
//			{
//				List<ClientLocationLog> list = getLocationList(30, TimeUtil.msSecond * 3, 0.00001f * 6.0f);

//				Debug.Log($"input location: start[{list[0].event_time}] end[{list[list.Count - 1].event_time}");

//				List<ProcessedPathData> p_list = processLocationLog(_statData, _pathList[_pathList.Count - 1], list);

//				for(int j = 0; j < p_list.Count; ++j)
//				{
//					ProcessedPathData p_data = p_list[j];

//					Debug.Log($"p_data: mined[{p_data.mined}] start_time[{TimeUtil.dateTimeFromUnixTimestamp(p_data.start_time)}] end_time[{TimeUtil.dateTimeFromUnixTimestamp(p_data.end_time)}] count[{p_data.log_list.Count}]");
					
//					if (j == 0)
//					{
//						_pathList[_pathList.Count - 1].appendLocation(p_data.log_list);
//					}
//					else
//					{
//						appendNewPath(p_data.mined, p_data.log_list);
//					}
//				}
//			}

//			Debug.Log("------------------------------------------------");

//			foreach(ClientTripPathData pathData in _pathList)
//			{
//				Debug.Log($"mined[{pathData.mined}] start_time[{TimeUtil.dateTimeFromUnixTimestamp(pathData.path_begin_time)}] end_time[{TimeUtil.dateTimeFromUnixTimestamp(pathData.path_end_time)}]");
//			}
//		}

//		private ClientLocationLog genLocation(long delta,double speed)
//		{
//			_lastLocation.pos.x += speed;
//			_lastLocationTime += delta;

//			return ClientLocationLog.create(_lastLocation.lon, _lastLocation.lat, 0, _lastLocationTime);
//		}

//		private List<ClientLocationLog> getLocationList(int count,long delta,double speed)
//		{
//			List<ClientLocationLog> list = new List<ClientLocationLog>();
//			for(int i = 0; i < count; ++i)
//			{
//				list.Add(genLocation(delta, speed));
//			}

//			return list;
//		}


//		public List<ProcessedPathData> processLocationLog(RunningStatData statData, ClientTripPathData currentPathData, List<ClientLocationLog> logList)
//		{
//			RefNFTGrade refGrade = new RefNFTGrade();

//			refGrade.grade = 201;
//			refGrade.stamina = 100;
//			refGrade.heart = 6;
//			refGrade.distance = 10000;

//			int heartReduceRate = 1;
//			int distanceReduceRate = 100;

//			//
//			List<ProcessedPathData> processedList = new List<ProcessedPathData>();

//			// 설마 null이진 않겠지
//			ClientLocationLog lastLog = currentPathData.getLastLocationLog();

//			ProcessedPathData lastProcessedPath = ProcessedPathData.create(currentPathData.mined);
//			lastProcessedPath.start_time = TimeUtil.unixTimestampFromDateTime(lastLog.event_time);
//			processedList.Add(lastProcessedPath);

//			long last_day_id = TimeUtil.dayCountFromDate(lastLog.event_time, TimeUtil.timezoneOffset());
//			double last_distance = 0;   // 미터 단위
//			double last_time = 0;       // 분단위

//			foreach (ClientLocationLog log in logList)
//			{
//				lastProcessedPath.log_list.Add(log);
//				lastProcessedPath.end_time = TimeUtil.unixTimestampFromDateTime(log.event_time);

//				long day_id = TimeUtil.dayCountFromDate(log.event_time, TimeUtil.timezoneOffset());

//				// 날이 바뀌여서 stat충전함
//				if (last_day_id != day_id)
//				{
//					statData.heart += refGrade.heart;
//					statData.distance += refGrade.distance;
//					last_day_id = day_id;

//					Debug.Log($"refill heart[{statData.heart}] distance[{statData.distance}]");
//				}

//				double distance = ClientLocationLog.calcDistance(lastLog, log) * 1000;
//				double time = ClientLocationLog.calcDeltaTime(lastLog, log) / 60.0;

//				last_distance += distance;
//				last_time += time;

//				int distanceReduce = (int)System.Math.Floor(last_distance / distanceReduceRate);
//				int heartReduce = (int)System.Math.Floor(last_time / heartReduceRate);

//				if (distanceReduce > 0)
//				{
//					statData.distance = System.Math.Max(0, statData.distance - distanceReduce * distanceReduceRate);
//					Debug.Log($"reduce distance: delta[{distanceReduce * distanceReduceRate}] distance[{ statData.distance}]");

//					last_distance -= distanceReduce * distanceReduceRate;
//				}

//				if (heartReduce > 0)
//				{
//					statData.heart = System.Math.Max(0, statData.heart - heartReduce);
//					Debug.Log($"reduct heart: delta[{heartReduce}] heart[{statData.heart}]");

//					last_time -= heartReduce * heartReduceRate;
//				}

//				lastLog = log;

//				//
//				int mined = (statData.heart > 0 && statData.distance > 0) ? 1 : 0;

//				if (lastProcessedPath.mined != mined)
//				{
//					lastProcessedPath = ProcessedPathData.create(mined);
//					lastProcessedPath.start_time = TimeUtil.unixTimestampFromDateTime(log.event_time);
//					lastProcessedPath.log_list.Add(log);    // 하나 넣어줌
//					processedList.Add(lastProcessedPath);
//				}
//			}

//			return processedList;
//		}
//	}
//}
