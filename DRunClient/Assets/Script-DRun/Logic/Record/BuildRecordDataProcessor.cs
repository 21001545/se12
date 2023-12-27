using DRun.Client.NetData;
using DRun.Client.Record;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.Module.Net;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using Void = Festa.Client.Module.Void;

namespace DRun.Client.Logic.Record
{
	public class BuildRecordDataProcessor : BaseLogicStepProcessor
	{
		private int _target_account_id;
		private int _running_type;
		private int _running_sub_type;
		private int _next_running_id;
		private RecordViewModel _cacheVM;

		public class QueryData
		{
			public int query_type;
			public int query_data_type;
			public int type;
			public long id_begin;
			public long id_end;
			public int count;

			public bool need_server_query;
			public List<ClientRunningLogCumulation> logList;
		}
		
		private Dictionary<int, QueryData> _cumulationLogMap;
		private List<ClientRunningLog> _logList;
		private int _logBegin;
		private int _logEnd;
		private bool _needReadLogFromServer;

		private const int _query_latest_log_count = 5;

		private int[] _queryTimeTypes = new int[] {
			ClientRunningLogCumulation.TimeType.week,
			ClientRunningLogCumulation.TimeType.month,
			ClientRunningLogCumulation.TimeType.year,
			ClientRunningLogCumulation.TimeType.total
		};

		private Dictionary<int, GraphData> _graphDataMap;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public List<ClientRunningLog> getLogList()
		{
			return _logList;
		}

		public Dictionary<int, GraphData> getGraphDataMap()
		{
			return _graphDataMap;
		}

		public static BuildRecordDataProcessor create(int target_account_id,int running_type, int next_running_id, RecordViewModel cacheVM)
		{
			BuildRecordDataProcessor processor = new BuildRecordDataProcessor();
			processor.init(target_account_id,running_type, next_running_id, cacheVM);
			return processor;
		}

		private void init(int target_account_id,int running_type,int next_running_id,RecordViewModel cacheVM)
		{
			base.init();

			_target_account_id = target_account_id;
			_running_type = running_type;
			_cacheVM = cacheVM;
			_next_running_id = next_running_id;

			if( running_type == ClientRunningLogCumulation.RunningType.promode)
			{
				_running_sub_type = 1;
			}
			else
			{
				_running_sub_type = ClientRunningLogCumulation.MarathonType._sum;
			}
		}

		protected override void buildSteps()
		{
			// 주, 년 합산 값 얻어오기
			_stepList.Add(readAverage_Week);
			_stepList.Add(readAverage_Month);
			_stepList.Add(readAverage_Year);
			_stepList.Add(readAverage_Total);

			// 그래프를 위한 데이터 얻어오기
			_stepList.Add(prepareCumluationMap);
			_stepList.Add(readCumulationLogFromCache);
			_stepList.Add(readCumulationLogFromServer);

			// 로그 얻어오기
			_stepList.Add(readLogFromCache);
			_stepList.Add(readLogFromServer);
			_stepList.Add(sortLog);

			// 그래프 데이터 만들기
			_stepList.Add(buildGraphData);
		}

		private void prepareCumluationMap(Handler<AsyncResult<Void>> handler)
		{
			_cumulationLogMap = new Dictionary<int, QueryData>();

			foreach(int timeType in _queryTimeTypes)
			{
				QueryData data = createQueryData(timeType);
				_cumulationLogMap.Add(data.type, data);
			}

			handler(Future.succeededFuture());
		}

		private void readAverage_Week(Handler<AsyncResult<Void>> handler)
		{
			int type = ClientRunningLogCumulation.TimeType.week;
			long id = TimeUtil.thisWeekCount();

			readSingleLogCumulation(type, id, handler);
		}

		private void readAverage_Month(Handler<AsyncResult<Void>> handler)
		{
			int type = ClientRunningLogCumulation.TimeType.month;
			long id = TimeUtil.thisMonthCount();

			readSingleLogCumulation(type, id, handler);
		}

		private void readAverage_Year(Handler<AsyncResult<Void>> handler)
		{
			int type = ClientRunningLogCumulation.TimeType.year;
			long id = TimeUtil.thisYearCount();

			readSingleLogCumulation(type, id, handler);
		}

		private void readAverage_Total(Handler<AsyncResult<Void>> handler)
		{
			int type = ClientRunningLogCumulation.TimeType.total;
			long id = 0;

			readSingleLogCumulation(type, id, handler);
		}

		private void readSingleLogCumulation(int type,long id,Handler<AsyncResult<Void>> handler)
		{
			if (_cacheVM.containsLogCumulation(_running_type, type, id))
			{
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.ProMode.QueryRunningLogCumulationReq);
			req.put("id", _target_account_id);
			req.put("query_type", 0);
			req.put("running_type", _running_type);
			req.put("running_sub_type", _running_sub_type);
			req.put("type",type);
			req.put("begin", id);
			req.put("end", id);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<ClientRunningLogCumulation> logList = ack.getList<ClientRunningLogCumulation>("data");
					_cacheVM.putLogCumulation(logList);
					handler(Future.succeededFuture());
				}
			});
		}

		private QueryData createQueryData(int type)
		{
			QueryData data = new QueryData();
			data.type = type;

			// 주: 일~오늘
			if( type == ClientRunningLogCumulation.TimeType.week)
			{
				data.query_type = 0;
				data.query_data_type = ClientRunningLogCumulation.TimeType.day;
				data.id_begin = (TimeUtil.nowUTC_BeginWeek(TimeUtil.timezoneOffset()) + TimeUtil.timezoneOffset())/ TimeUtil.msDay;
				data.id_end = data.id_begin + 7 - 1;
			}
			// 월: 첫날 부터 지금까지
			else if ( type == ClientRunningLogCumulation.TimeType.month)
			{
				data.query_type = 0;
				data.query_data_type = ClientRunningLogCumulation.TimeType.day;
				DateTime now = DateTime.Now;
				DateTime beginDayOfMonth = new DateTime(now.Year, now.Month, 1);
				//DateTime endDayOfMonth = (new DateTime(now.Year, now.Month + 1, 1)).AddDays(-1);
				DateTime endDayOfMonth = beginDayOfMonth.AddMonths(1).AddDays(-1);

				data.id_begin = TimeUtil.unixTimestampFromDateTime(beginDayOfMonth) / TimeUtil.msDay;
				data.id_end = TimeUtil.unixTimestampFromDateTime(endDayOfMonth) / TimeUtil.msDay;
			}
			// 년 : 월별
			else if ( type == ClientRunningLogCumulation.TimeType.year)
			{
				data.query_type = 0;
				data.query_data_type = ClientRunningLogCumulation.TimeType.month;
				DateTime now = DateTime.Now;

				data.id_begin = (now.Year - 1970) * 12 + 0;
				data.id_end = data.id_begin + 12 - 1;
			}
			// 전체 : 최근 12개 로그
			else if( type == ClientRunningLogCumulation.TimeType.total)
			{
				data.query_type = 1;
				data.query_data_type = ClientRunningLogCumulation.TimeType.day;
				data.count = 12; // 최근 12개
				//data.id_end = TimeUtil.todayDayCount();
			}

			return data;
		}

		private void readCumulationLogFromCache(Handler<AsyncResult<Void>> handler)
		{
			readCumulationLogFromCacheIter(_cumulationLogMap.GetEnumerator(), handler);
		}

		private void readCumulationLogFromCacheIter(IEnumerator<KeyValuePair<int,QueryData>> it,Handler<AsyncResult<Festa.Client.Module.Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			QueryData data = it.Current.Value;
			RunningLogCumulationRangeKey key = RunningLogCumulationRangeKey.create(_running_type, 1, data.query_data_type, data.id_begin, data.id_end);

			if(_cacheVM.hasCumulationRangeCache(key) == false)
			{
				data.need_server_query = true;
				readCumulationLogFromCacheIter(it, handler);
				return;
			}

			data.need_server_query = false;
			Dictionary<long, ClientRunningLogCumulation> cacheData = _cacheVM.getLogCumulation(_running_type, 1, data.query_data_type, data.id_begin, data.id_end);

			setupLogList(data, cacheData);

			readCumulationLogFromCacheIter(it, handler);
		}

		private void setupLogList(QueryData queryData,Dictionary<long,ClientRunningLogCumulation> logData)
		{
			queryData.logList = new List<ClientRunningLogCumulation>();
			for (long id = queryData.id_begin; id <= queryData.id_end; ++id)
			{
				ClientRunningLogCumulation log;
				if (logData.TryGetValue(id, out log))
				{
					queryData.logList.Add(log);
				}
				else
				{
					queryData.logList.Add(ClientRunningLogCumulation.createEmpty(_running_type, 1, queryData.query_data_type, id));
				}
			}
		}

		private void readCumulationLogFromServer(Handler<AsyncResult<Void>> handler)
		{
			readCumulationLogFromServerIter(_cumulationLogMap.GetEnumerator(), handler);
		}

		private void readCumulationLogFromServerIter(IEnumerator<KeyValuePair<int,QueryData>> it,Handler<AsyncResult<Void>> handler)
		{
			if( it.MoveNext() == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			QueryData data = it.Current.Value;
			if( data.need_server_query == false)
			{
				readCumulationLogFromCacheIter(it, handler);
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.ProMode.QueryRunningLogCumulationReq);
			req.put("id", _target_account_id);
			req.put("query_type", data.query_type);
			req.put("running_type", _running_type);
			req.put("running_sub_type", 1);
			req.put("type", data.query_data_type);

			if( data.query_type == 0)
			{
				req.put("begin", data.id_begin);
				req.put("end", data.id_end);
			}
			else
			{
				req.put("count", data.count);
			}

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					List<ClientRunningLogCumulation> logList = ack.getList<ClientRunningLogCumulation>("data");
					_cacheVM.putLogCumulation(logList);

					if( data.query_type == 0)
					{
						Dictionary<long, ClientRunningLogCumulation> cacheData = _cacheVM.getLogCumulation(_running_type, 1, data.query_data_type, data.id_begin, data.id_end);
						setupLogList(data, cacheData);
					}
					else
					{
						data.logList = logList;
					}

					readCumulationLogFromServerIter(it, handler);
				}
			});
		}

		private void readLogFromCache(Handler<AsyncResult<Void>> handler)
		{
			_logEnd = _next_running_id - 1;
			_logBegin = _logEnd - _query_latest_log_count + 1;

			if(_logBegin < 1)
			{
				_logBegin = 1;
			}

			_logList = new List<ClientRunningLog>();

			for(int id = _logBegin; id <= _logEnd; ++id)
			{
				ClientRunningLog log = _cacheVM.getLog(_running_type, id);
				if( log != null)
				{
					_logList.Add(log);
				}
			}

			// 희망 갯수보다 적다
			if( _logList.Count < (_logEnd - _logBegin + 1))
			{
				_needReadLogFromServer = true;
			}
			else
			{
				_needReadLogFromServer = false;
			}

			handler(Future.succeededFuture());
		}

		private void readLogFromServer(Handler<AsyncResult<Void>> handler)
		{
			if(_needReadLogFromServer == false)
			{
				handler(Future.succeededFuture());
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.ProMode.QueryRunningLogReq);
			req.put("id", _target_account_id);
			req.put("running_type", _running_type);
			req.put("begin", _logBegin);
			req.put("end", _logEnd);

			Network.call(req, ack => { 
				if( ack.getResult() != ResultCode.ok)
				{
					handler(Future.failedFuture(ack.makeErrorException()));
				}
				else
				{
					_logList = ack.getList<ClientRunningLog>("data");
					_cacheVM.putLog(_logList);

					handler(Future.succeededFuture());
				}
			});
		}

		private void sortLog(Handler<AsyncResult<Void>> handler)
		{
			_logList.Sort((a, b) => { 
				if( a.running_id > b.running_id)
				{
					return -1;
				}
				else if( a.running_id < b.running_id)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			});

			handler(Future.succeededFuture());
		}

		private void buildGraphData(Handler<AsyncResult<Void>> handler)
		{
			_graphDataMap = new Dictionary<int, GraphData>();
			_graphDataMap.Add(ClientRunningLogCumulation.TimeType.week, buildGraphData_Week());
			_graphDataMap.Add(ClientRunningLogCumulation.TimeType.month, buildGraphData_Month());
			_graphDataMap.Add(ClientRunningLogCumulation.TimeType.year, buildGraphData_Year());
			_graphDataMap.Add(ClientRunningLogCumulation.TimeType.total, buildGraphData_Total());

			handler(Future.succeededFuture());
		}

		private void setDistanceRange(GraphData graphData,List<ClientRunningLogCumulation> logList)
		{
			DoubleVector2 range = DoubleVector2.zero;
			for(int i = 0; i < logList.Count; ++i)
			{
				ClientRunningLogCumulation log = logList[i];
				if( i == 0)
				{
					//range.x = log.total_distance;
					range.y = log.total_distance;
				}
				else
				{
					//range.x = System.Math.Min(range.x, log.total_distance);
					range.y = System.Math.Max(range.y, log.total_distance);
				}
			}

			graphData.valueMin = range.x;
			graphData.valueMax = range.y;
		}

		private void setupYAxis(GraphData graphData, List<ClientRunningLogCumulation> logList)
		{
			setDistanceRange(graphData, logList);
			int topDistance = (int)System.Math.Ceiling(graphData.valueMax);

			// 너무 작을 수 있다
			// valueMax를 만져보자
			if( topDistance < 3)
			{
				topDistance = 3;
			}
			else
			{
				topDistance += 3 - (topDistance % 3);
			}

			graphData.valueMax = topDistance;

			graphData.yAxisList.Add("0km");
			graphData.yAxisList.Add($"{topDistance / 3}km");
			graphData.yAxisList.Add($"{topDistance * 2 / 3}km");
			graphData.yAxisList.Add($"{topDistance}km");
		}

		private void setupValue(GraphData graphData, QueryData queryData)
		{
			foreach(ClientRunningLogCumulation log in queryData.logList)
			{
				graphData.valueList.Add(GraphValue.create(log.total_distance, false, log.best_log_id == 0, log));
			}
		}


		private GraphData buildGraphData_Week()
		{
			QueryData queryData = _cumulationLogMap[ClientRunningLogCumulation.TimeType.week];
			GraphData data = new GraphData();
			data.xAxisLabel_Margin = 4.3f;
			data.xAxisLabel_FontSize = 14;
			data.xAxisLabel_Alignment = TMPro.TextAlignmentOptions.MidlineLeft;
			data.valueWidth = 20.0f;
			setupYAxis(data, queryData.logList);
			setupValue(data, queryData);

			for (int i = 0; i < 7; i++)
			{
				data.xAxisList.Add(StringCollection.get("pro.record.graph.dayofweek", i)); // 일
			}

			return data;
		}

		private GraphData buildGraphData_Month()
		{
			QueryData queryData = _cumulationLogMap[ClientRunningLogCumulation.TimeType.month];
			GraphData data = new GraphData();
			data.xAxisLabel_Margin = 4.3f;
			data.xAxisLabel_FontSize = 14;
			data.xAxisLabel_Alignment = TMPro.TextAlignmentOptions.MidlineLeft;
			data.valueWidth = 6.0f;
			setupYAxis(data, queryData.logList);
			setupValue(data, queryData);

			// 5단위로
			int count = queryData.logList.Count / 5;
			for(int i = 0; i < count; ++i)
			{
				data.xAxisList.Add($"{(i * 5 + 1)}");
			}

			return data;
		}

		private GraphData buildGraphData_Year()
		{
			QueryData queryData = _cumulationLogMap[ClientRunningLogCumulation.TimeType.year];
			GraphData data = new GraphData();
			data.xAxisLabel_Margin = 1.5f;
			data.xAxisLabel_FontSize = 12;
			data.xAxisLabel_Alignment = TMPro.TextAlignmentOptions.MidlineLeft;
			data.valueWidth = 12.0f;
			setupYAxis(data, queryData.logList);
			setupValue(data, queryData);

			// 월
			for(int i = 0; i < 12; ++i)
			{
				data.xAxisList.Add(StringCollection.get("pro.record.graph.month", i));
			}

			return data;
		}

		private GraphData buildGraphData_Total()
		{
			QueryData queryData = _cumulationLogMap[ClientRunningLogCumulation.TimeType.total];

			// 최근 날짜가 뒤로 가게 다시 정렬
			queryData.logList.Sort((a, b) => { 
				if( a.id < b.id)
				{
					return -1;
				}
				else if( a.id > b.id)
				{
					return 1;
				}

				return 0;
			});			
			
			
			GraphData data = new GraphData();
			data.xAxisLabel_Margin = 0;
			data.xAxisLabel_FontSize = 12;
			data.xAxisLabel_Alignment = TMPro.TextAlignmentOptions.Center;
			data.valueWidth = 12.0f;
			setupYAxis(data, queryData.logList);
			setupValue(data, queryData);

			foreach(ClientRunningLogCumulation log in queryData.logList)
			{
				DateTime localTime = log.begin_time.ToLocalTime();

				string label = $"{localTime.Month}/{localTime.Day}";
				data.xAxisList.Add(label);
			}

			return data;
		}
	}
}
