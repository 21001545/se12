using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using System.Collections.Generic;

namespace DRun.Client.ViewModel
{
	public class RecordViewModel : AbstractViewModel
	{
		private Dictionary<RunningLogCumulationKey, ClientRunningLogCumulation> _cumulationLogMap;
		private HashSet<RunningLogCumulationRangeKey> _cumulationLogRangeCache;

		private Dictionary<RunningLogCacheKey, ClientRunningLog> _logCacheData;
		private Dictionary<int, List<ClientRunningLog>> _latestLogCacheData;

		public Dictionary<RunningLogCumulationKey, ClientRunningLogCumulation> CumulationLogMap => _cumulationLogMap;

		private int getRunningSubType(int running_type)
		{
			if( running_type == ClientRunningLogCumulation.RunningType.promode)
			{
				return 1;
			}
			else
			{
				return ClientRunningLogCumulation.MarathonType._sum;
			}
		}

		// 없으면 빈칸으로라도 만들어줌
		public ClientRunningLogCumulation getCurrentLogCumulation(int running_type,int type)
		{
			long id = calcCurrentCumluationID(type);

			RunningLogCumulationKey key = RunningLogCumulationKey.create(running_type, getRunningSubType(running_type), type, id);
			ClientRunningLogCumulation log;
			if( _cumulationLogMap.TryGetValue( key, out log))
			{
				return log;
			}
			else
			{
				return ClientRunningLogCumulation.createEmpty( running_type, getRunningSubType(running_type), type, id);
			}
		}

		public bool containsLogCumulation(int running_type,int type,long id)
		{
			RunningLogCumulationKey key = RunningLogCumulationKey.create(running_type, getRunningSubType(running_type), type, id);
			return _cumulationLogMap.ContainsKey(key);
		}

		public static RecordViewModel create()
		{
			RecordViewModel viewModel = new RecordViewModel();
			viewModel.init();
			return viewModel;
		}

		protected override void init()
		{
			base.init();
			_cumulationLogMap = new Dictionary<RunningLogCumulationKey, ClientRunningLogCumulation>();
			_cumulationLogRangeCache = new HashSet<RunningLogCumulationRangeKey>();

			_logCacheData = new Dictionary<RunningLogCacheKey, ClientRunningLog>();
			_latestLogCacheData = new Dictionary<int, List<ClientRunningLog>>();
		}

		public void putLogCumulation(List<ClientRunningLogCumulation> logList)
		{
			foreach(ClientRunningLogCumulation log in logList)
			{
				RunningLogCumulationKey key = RunningLogCumulationKey.create(log);

				if( _cumulationLogMap.ContainsKey(key))
				{
					_cumulationLogMap.Remove(key);
				}

				_cumulationLogMap.Add(key, log);
			}

			notifyPropetyChanged("CumulationLogMap");
		}

		public Dictionary<long,ClientRunningLogCumulation> getLogCumulation(int running_type,int running_sub_type,int type,long begin,long end)
		{
			Dictionary<long,ClientRunningLogCumulation> list = new Dictionary<long,ClientRunningLogCumulation>();
			for(long id = begin; id <= end; ++id)
			{
				RunningLogCumulationKey key = RunningLogCumulationKey.create(running_type, running_sub_type, type, id);
				ClientRunningLogCumulation log;

				if( _cumulationLogMap.TryGetValue( key, out log))
				{
					list.Add(log.id,log);
				}
			}

			return list;
		}

		protected override void bindPacketParser()
		{
			bind(MapPacketKey.ClientAck.running_log_cumulation, updateRunningLogCumulation);
		}

		private void updateRunningLogCumulation(object obj,MapPacket ack)
		{
			List<ClientRunningLogCumulation> list = ack.getList<ClientRunningLogCumulation>(MapPacketKey.ClientAck.running_log_cumulation);
			putLogCumulation(list);
		}

		public long calcCurrentCumluationID(int type)
		{
			if( type == ClientRunningLogCumulation.TimeType.day)
			{
				return TimeUtil.todayDayCount();
			}
			else if( type == ClientRunningLogCumulation.TimeType.week)
			{
				return TimeUtil.thisWeekCount();
			}
			else if( type == ClientRunningLogCumulation.TimeType.month)
			{
				return TimeUtil.thisMonthCount();
			}
			else if( type == ClientRunningLogCumulation.TimeType.year)
			{
				return TimeUtil.thisYearCount();
			}
			else if( type == ClientRunningLogCumulation.TimeType.total)
			{
				return 0;
			}

			return -1;
		}

		public void setCumulationRangeCache(RunningLogCumulationRangeKey key)
		{
			_cumulationLogRangeCache.Add(key);
		}

		public bool hasCumulationRangeCache(RunningLogCumulationRangeKey key)
		{
			return _cumulationLogRangeCache.Contains(key);
		}

		public void putLog(List<ClientRunningLog> logList)
		{
			foreach(ClientRunningLog log in logList)
			{
				RunningLogCacheKey key = RunningLogCacheKey.create(log.running_type, log.running_id);
				if( _logCacheData.ContainsKey(key))
				{
					_logCacheData.Remove(key);
				}

				_logCacheData.Add(key, log);
			}
		}

		public ClientRunningLog getLog(int running_type,int running_id)
		{
			RunningLogCacheKey key = RunningLogCacheKey.create(running_type, running_id);
			ClientRunningLog log;
			if( _logCacheData.TryGetValue( key, out log))
			{
				return log;
			}

			return null;
		}
	}
}
