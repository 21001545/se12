using Festa.Client.LocalDB;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.NetData;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client
{
	public class HealthDevice_EditorDev : AbstractHealthDevice
	{
		private IntervalTimer _timer;
		private long _last_record_time;
		//private List<ClientHealthLogData> _logList;

		private MultiThreadWorker _multiThreadWorker;
		private string _base_path;
		private string _db_path;
		private SQLiteConnection _connection;

		public static HealthDevice_EditorDev _instance;

		public int stepCountRangeMin = 5;
		public int stepCountRangeMax = 20;

		protected override void init()
		{
			base.init();
			//_logList = new List<ClientHealthLogData>();
			_timer = IntervalTimer.create(1.5f, true, false);
			_instance = this;
		}

		public override bool isAvailable()
		{
			return false;
		}

		public override void initDevice(UnityAction<bool> callback)
		{
			_multiThreadWorker = ClientMain.instance.getMultiThreadWorker();
			_base_path = Application.temporaryCachePath + "/local";
			_db_path = _base_path + "/stepdata";

			List<BaseStepProcessor.StepProcessor> stepList = new List<BaseStepProcessor.StepProcessor>();
			stepList.Add(validateDirectory);
			stepList.Add(createDBConnection);
			stepList.Add(prepareTables);

			BaseStepProcessor.runSteps (0, stepList, false, result => { 
				if( result.failed())
				{
					callback(false);
				}
				else
				{
					callback(true);
				}
			});
		}

		private void validateDirectory(Handler<AsyncResult<Module.Void>> handler)
		{
			if( Directory.Exists(_base_path) == false)
			{
				try
				{
					Directory.CreateDirectory(_base_path);
					handler(Future.succeededFuture());
				}
				catch(Exception e)
				{
					handler(Future.failedFuture(e));
				}
			}
			else
			{
				handler(Future.succeededFuture());
			}
		}
		
		private void createDBConnection(Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<SQLiteConnection>(promise => {
				SQLiteConnection conn = new SQLiteConnection(_db_path);
				promise.complete(conn);
			}, result => { 
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					_connection = result.result();
					handler(Future.succeededFuture());
				}
			});
		}

		private void prepareTables(Handler<AsyncResult<Module.Void>> handler)
		{
			_multiThreadWorker.execute<Module.Void>(promise =>
			{
				CreateTableResult result = _connection.CreateTable( typeof(LDB_StepCount));

				promise.complete();
			}, result => { 
			
				if( result.failed())
				{
					handler(Future.failedFuture(result.cause()));
				}
				else
				{
					handler(Future.succeededFuture());
				}
			});
		}

		public override void update()
		{
			base.update();

			if( _timer.update())
			{
				saveStepCount(TimeUtil.unixTimestampUtcNow(), UnityEngine.Random.Range(stepCountRangeMin, stepCountRangeMax));

				//_logList.Add(ClientHealthLogData.create(UnityEngine.Random.Range(stepCountRangeMin, stepCountRangeMax)));
			}
		}

		public void saveStepCount(long time,int value)
		{
			_multiThreadWorker.execute<Module.Void>(promise => {

				LDB_StepCount stepCount = LDB_StepCount.create(time, value);

				int count = _connection.InsertOrReplace(stepCount);
				if( count == 0)
				{
					promise.fail(new Exception("insert fail: stepCount"));
				}
				else
				{
					promise.complete();
				}

			}, result => { 

			});
		}

		public override void setLastRecordTime(long lastRecordTime)
		{
			_last_record_time = lastRecordTime;
		}

		public override void queryNewStepRecord(bool isStartup,List<ClientHealthLogData> result_list, UnityAction callback)
		{
			if( isStartup)
			{
				queryStepPast(result_list, callback);
			}
			else
			{
				queryStepLive(result_list, callback);
			
				//foreach(ClientHealthLogData log in _logList)
				//{
				//	if(log.record_time >= _last_record_time)
				//	{
				//		result_list.Add(log);

				//		if( log.record_time > _last_record_time)
				//		{
				//			_last_record_time = log.record_time;
				//		}
				//	}
				//}

				//callback();
			}
		}

		private void queryStepLive(List<ClientHealthLogData> result_list,UnityAction callback)
		{
			long utc_now = TimeUtil.unixTimestampUtcNow();

			queryStepCountFrom( _last_record_time, count => { 
				
				if( count > 0)
				{
					ClientHealthLogData log = ClientHealthLogData.create(utc_now, count);
					result_list.Add(log);
				}

				_last_record_time = utc_now;
				callback();
			});
		}

		private void queryStepPast(List<ClientHealthLogData> result_list,UnityAction callback)
		{
			long timezone_offset = TimeUtil.timezoneOffset();

			long utc_now = TimeUtil.unixTimestampUtcNow();

			long local_begin_time = _last_record_time + timezone_offset;
			long local_end_time = utc_now + timezone_offset;

			List<QueryStatisticsParam> param_list = new List<QueryStatisticsParam>();

			long local_begin_time_hoursnap = local_begin_time - (local_begin_time % TimeUtil.msHour);

			for (long i = local_begin_time_hoursnap; i < local_end_time; i += TimeUtil.msHour)
			{
				long begin = i;
				long end = i + TimeUtil.msHour;

				if (begin < local_begin_time)
				{
					begin = local_begin_time;
				}

				if (end > local_end_time)
				{
					end = local_end_time;
				}

				QueryStatisticsParam param = new QueryStatisticsParam();
				param.begin = begin - timezone_offset;
				param.end = end - timezone_offset;


				//ClientHealthLogData log = ClientHealthLogData.create(utc_end,(int)UnityEngine.Random.Range( 1, 5));
				//result_list.Add(log);
			}

			queryStepIterator(param_list.GetEnumerator(), result_list, () => {
				_last_record_time = utc_now;
				callback();
			});
		}

		private void queryStepIterator(IEnumerator<QueryStatisticsParam> it,List<ClientHealthLogData> result_list,UnityAction callback)
		{
			if( it.MoveNext() == false)
			{
				callback();
				return;
			}

			QueryStatisticsParam param = it.Current;

			queryStepCountRange(param.begin, param.end, count => { 
				if( count > 0)
				{
					ClientHealthLogData log = ClientHealthLogData.create(param.end, count);
					result_list.Add(log);
				}

				queryStepIterator(it, result_list, callback);
			});
		}

		public void queryStepCountFrom(long begin,UnityAction<int> callback)
		{
			_multiThreadWorker.execute<object>(promise => {
				List<LDB_StepCount> list = _connection.Query<LDB_StepCount>("select * from LDB_StepCount where time >= ?", begin);

				int value = 0;
				foreach(LDB_StepCount stepCount in list)
				{
					value += stepCount.value;
				}

				promise.complete(value);

			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					callback(0);
				}
				else
				{
					callback((int)result.result());
				}
			});
		}

		public override void queryStepCountRange(long begin, long end,UnityAction<int> callback)
		{
			if( begin > end)
			{
				callback(0);
				return;
			}

			_multiThreadWorker.execute<object>(promise => {

				List<LDB_StepCount> list = _connection.Query<LDB_StepCount>("select * from LDB_StepCount where time >= ? and time <= ?", begin, end);

				int value = 0;
				foreach(LDB_StepCount step in list)
				{
					value += step.value;
				}

				promise.complete(value);

			}, result => { 
				if( result.failed())
				{
					Debug.LogException(result.cause());
					callback(0);
				}
				else
				{
					callback((int)result.result());
				}

			});


			//DateTime dtBegin = TimeUtil.dateTimeFromUnixTimestamp(begin);
			//DateTime dtEnd = TimeUtil.dateTimeFromUnixTimestamp(end);

			//int stepCount = 0;

			//foreach(ClientHealthLogData log in _logList)
			//{
			//	if( log.record_time >= dtBegin && log.record_time <= dtEnd )
			//	{
			//		stepCount += log.value;
			//	}
			//}

			//callback(stepCount);
		}
	}
}
