using DRun.Client.Android;
using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Festa.Client
{
	// 2022.10.6 StepsApp을 참고하여 Foregroud Service로 구현

	public class HealthDevice_Android : AbstractHealthDevice
	{
		private CoreServicePlugin plugin => CoreServicePlugin.getInstance();
		private long _lastRecordTime = 0;

		public override bool isAvailable()
        {
			return false;
        }

		public override void initDevice(UnityAction<bool> callback)
		{
			plugin.startService();

			//readPrevious(callback);
			callback(true);
		}

		public override void setLastRecordTime(long lastRecordTime)
		{
			_lastRecordTime = lastRecordTime;
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
			}

		////	throw new NotImplementedException();
		//	if( _cumulative_steps > 0)
		//	{
		//		ClientHealthLogData log = ClientHealthLogData.create(_cumulative_steps);
		//		result_list.Add(log);

		//		_cumulative_steps = 0;
		//	}

		//	callback();
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

			queryStepIterate(param_list.GetEnumerator(), result_list, () => {
				_lastRecordTime = utc_now;
				callback();
				//writePrevious(result => { });
			});
		}

		private void queryStepIterate(IEnumerator<QueryStatisticsParam> it,List<ClientHealthLogData> result_list,UnityAction callback)
		{
			if( it.MoveNext() == false)
			{
				callback();
				return;
			}

			QueryStatisticsParam param = it.Current;

			int value = plugin.getStepCountRange(param.begin, param.end);
			if( value > 0)
			{
				ClientHealthLogData log = ClientHealthLogData.create(param.end, value);
				result_list.Add(log);
			}

			queryStepIterate(it, result_list, callback);
		}

		private void queryStepLive(List<ClientHealthLogData> result_list,UnityAction callback)
		{
			long utc_now = TimeUtil.unixTimestampUtcNow();

			int value = plugin.getStepCountFrom(_lastRecordTime);
			if( value > 0)
			{
				ClientHealthLogData log = ClientHealthLogData.create(utc_now, value);
				result_list.Add(log);
			}

			_lastRecordTime = utc_now;
			callback();
		}

		public override void queryStepCountRange(long begin, long end,UnityAction<int> callback)
		{
			if( begin > end)
			{
				callback(0);
				return;
			}

			callback(plugin.getStepCountRange(begin, end));
		}


	}
}
