using DRun.client.Logic.Account;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIStepDebugger : UISingletonPanel<UIStepDebugger>, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;
		public UIStepDebugger_Log cellLog;
		public TMP_InputField inputBeginTime;
		public TMP_InputField inputEndTime;
		public TMP_Text textTotal;
		public GameObject loading;

		private DateTime _beginTime;
		private DateTime _endTime;

		public struct QueryStatisticsParam
		{
			public long begin;
			public long end;
		}

		private List<QueryStatisticsParam> _queryParamList;
		private List<ClientHealthLogData> _logList;

		private static string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss";

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
			_logList = new List<ClientHealthLogData>();
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			base.open(param, transitionType, closeType);

			DateTime now = DateTime.Now;

			inputBeginTime.text = now.AddHours(-6).ToString(_dateTimeFormat);
			inputEndTime.text = now.ToString(_dateTimeFormat);
			textTotal.text = "";
		}

		public void onClick_Close()
		{
			close();
		}

		public void onClick_Query()
		{
			if( parsePeriodTime() == false)
			{
				return;
			}

			buildHourlyList(true);
			query();
		}

		public void onClick_QueryType2()
		{
			if (parsePeriodTime() == false)
			{
				return;
			}

			//buildHourlyList(true);
			buildTenSecondList();
			query();
		}

		public void onClick_WriteLog()
		{
			if( _logList.Count == 0)
			{
				return;
			}

			WriteLogProcessor step = WriteLogProcessor.create();
			JsonObject log = step.appendLog(WriteLogProcessor.LogType.debug);

			JsonObject json_param = new JsonObject();
			log.put("json_param", json_param);

			JsonArray json_loglist = new JsonArray();
			json_param.put("steps", json_loglist);

			foreach(ClientHealthLogData step_log in _logList)
			{
				JsonObject json_step = new JsonObject();
				json_step.put("event_time", step_log.record_time.ToString("u"));
				json_step.put("step_count", step_log.value);

				json_loglist.add(json_step);
			}

			loading.SetActive(true);
			step.run(result => {
				loading.SetActive(false);
			});
		}

		private bool parsePeriodTime()
		{
			if (DateTime.TryParseExact(inputBeginTime.text, _dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _beginTime) == false)
			{
				return false;
			}

			if (DateTime.TryParseExact(inputEndTime.text, _dateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out _endTime) == false)
			{
				return false;
			}

			return true;
		}

		private void buildTenSecondList()
		{
			Debug.Log($"begin[{_beginTime.ToString(_dateTimeFormat)}] end[{_endTime.ToString(_dateTimeFormat)}");

			_queryParamList = new List<QueryStatisticsParam>();

			// 마지막으로 기록된 시간을 가지고 
			long timezone_offset = TimeUtil.timezoneOffset();

			long local_begin_time = TimeUtil.unixTimestampFromDateTime(_beginTime);
			long local_end_time = TimeUtil.unixTimestampFromDateTime(_endTime);

			List<QueryStatisticsParam> param_list = new List<QueryStatisticsParam>();

			long interval = TimeUtil.msSecond * 10;
			long local_begin_time_snapped = local_begin_time - (local_begin_time % interval);

			for(long i = local_begin_time_snapped; i < local_end_time; i += interval)
			{
				long begin = i;
				long end = i + interval;

				if( begin < local_begin_time)
				{
					begin = local_begin_time;
				}

				if( end > local_end_time)
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

				_queryParamList.Add(param);
			}
		}

		private void buildHourlyList(bool type2)
		{
			Debug.Log($"begin[{_beginTime.ToString(_dateTimeFormat)}] end[{_endTime.ToString(_dateTimeFormat)}");

			_queryParamList = new List<QueryStatisticsParam>();

			// 마지막으로 기록된 시간을 가지고 
			long timezone_offset = TimeUtil.timezoneOffset();

			long local_begin_time = TimeUtil.unixTimestampFromDateTime(_beginTime);
			long local_end_time = TimeUtil.unixTimestampFromDateTime(_endTime);

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
					if( type2)
					{
						end -= TimeUtil.msSecond;
					}
				}

				Debug.Log($"hourly[{param_list.Count}] begin[{TimeUtil.dateTimeFromUnixTimestamp(begin).ToString(_dateTimeFormat)}] end[{TimeUtil.dateTimeFromUnixTimestamp(end).ToString(_dateTimeFormat)}]");

				QueryStatisticsParam param = new QueryStatisticsParam();
				param.begin = begin - timezone_offset;
				param.end = end - timezone_offset;

				_queryParamList.Add(param);
			}
		}

		private void query()
		{
			loading.SetActive(true);
			_logList.Clear();
			scroller.ReloadData();

			query(0,_queryParamList.GetEnumerator(), result => {

				setupTotal();

				loading.SetActive(false);
				scroller.ReloadData();
			});
		}

		private void query(int stack_count,IEnumerator<QueryStatisticsParam> it,Handler<AsyncResult<Festa.Client.Module.Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			QueryStatisticsParam param = it.Current;
			++stack_count;

			ClientMain.instance.getHealth().getDevice().queryStepCountRange(param.begin, param.end, count => {
				_logList.Add(ClientHealthLogData.create(param.end, count));

				if( stack_count > 50)
				{
					MainThreadDispatcher.dispatch(() => {
						query(0, it, handler);
					});
				}
				else
				{
					query(stack_count, it, handler);
				}

			});
		}

		private void setupTotal()
		{
			int sum = 0;
			foreach(ClientHealthLogData log in _logList)
			{
				sum += log.value;
			}

			textTotal.text = $"total step:{sum} record:{_logList.Count}";
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UIStepDebugger_Log log = (UIStepDebugger_Log)scroller.GetCellView(cellLog);
			log.setup(_logList[dataIndex]);
			return log;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 20;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _logList.Count;
		}
	}
}
