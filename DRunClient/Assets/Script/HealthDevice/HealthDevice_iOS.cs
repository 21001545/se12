using Festa.Client.Module;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using UnityEngine;
using Festa.Client.NetData;

#if UNITY_IPHONE

namespace Festa.Client
{
	public class HealthDevice_iOS : AbstractHealthDevice
	{
		private CMPedometerPlugin _cm_plugin;
		private bool _isAvailable;
		private long _lastRecordTime;

		protected override void init()
		{
			_cm_plugin = CMPedometerPlugin.create();
			_isAvailable = false;
		}

		public override bool isAvailable()
		{
			return _isAvailable;
		}

		public override void initDevice(UnityAction<bool> callback)
		{
			_cm_plugin.initPlugin(cm_result => {

				if (cm_result == false)
				{
					callback(false);
				}
				else
				{
					// test query
					long test_end = TimeUtil.unixTimestampUtcNow();
					long test_begin = test_end - TimeUtil.msHour;
					_cm_plugin.queryStepCountPast(test_begin, test_end, (query_step_result, step) => {

						if( query_step_result == false)
						{
							callback(false);
						}
						else
						{
							callback(true);
						}
					});

				}
			});
		}

		public override void setLastRecordTime(long lastRecordTime)
		{
			_lastRecordTime = lastRecordTime;
		}

		public override void queryNewStepRecord(bool isStartup, List<ClientHealthLogData> result_list, UnityAction callback)
		{
			if( isStartup)
			{
				queryStepPast(result_list, callback);
			}
			else
			{
				queryStepLive(result_list, callback);
			}
		}

		private void queryStepLive(List<ClientHealthLogData> result_list,UnityAction callback)
		{
			long utc_now = TimeUtil.unixTimestampUtcNow();

			_cm_plugin.queryStepCountPast(_lastRecordTime, utc_now, (result, step) => { 
			
				if( step > 0)
				{
					ClientHealthLogData log = ClientHealthLogData.create((int)step);
					result_list.Add(log);
				}

				_lastRecordTime = utc_now;
				callback();
			});

			// 버그 있음
			//_cm_plugin.queryStepCountLive((result, step) => {

			//	if( step > 0)
			//	{
			//		ClientHealthLogData log = ClientHealthLogData.create((int)step);
			//		result_list.Add(log);
			//	}

			//	callback();
			//	//writePrevious(result => { });
			//});
		}

		private void queryStepPast(List<ClientHealthLogData> result_list,UnityAction callback)
		{
			// 마지막으로 기록된 시간을 가지고 
			long timezone_offset = TimeUtil.timezoneOffset();

			long utc_now = TimeUtil.unixTimestampUtcNow();

			long local_begin_time = _lastRecordTime + timezone_offset;
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
				else
				{
					end -= TimeUtil.msSecond;
				}

				QueryStatisticsParam param = new QueryStatisticsParam();
				param.begin = begin - timezone_offset;
				param.end = end - timezone_offset;

				param_list.Add(param);
			}

			queryStepIterate(param_list, 0, result_list, () => {
				_lastRecordTime = utc_now;
				callback();
				//writePrevious(result => { });
			});
		}

		private void queryStepIterate(List<QueryStatisticsParam> param_list,int it,List<ClientHealthLogData> result_list, UnityAction callback)
		{
			if( it >= param_list.Count)
			{
				callback();
				return;
			}

			QueryStatisticsParam param = param_list[it];

			_cm_plugin.queryStepCountPast(param.begin, param.end, (result, step) => {

				DateTime beginDate = TimeUtil.dateTimeFromUnixTimestamp(param.begin);
				DateTime endDate = TimeUtil.dateTimeFromUnixTimestamp(param.end);

				int value = (int)step;
				if (value > 0)
				{
					//Debug.Log(string.Format("query step : begin[{0}] end[{1}] result[{2}] count[{3}]", beginDate.ToString(), endDate.ToString(), result, step));
					ClientHealthLogData log = ClientHealthLogData.create(param.end, value);
					result_list.Add(log);
				}

				queryStepIterate(param_list, it + 1, result_list, callback);
			});
		}

		public override void queryStepCountRange(long begin, long end,UnityAction<int> callback)
		{
			if( begin > end)
			{
				callback(0);
				return;
			}

			_cm_plugin.queryStepCountPast(begin, end, (result, step) => {
				callback((int)step);
			});
		}
    }
}

#endif