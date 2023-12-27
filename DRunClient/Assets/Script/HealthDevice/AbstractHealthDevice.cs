using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Events;

namespace Festa.Client
{
	public abstract class AbstractHealthDevice
	{
		public struct QueryStatisticsParam
		{
			public long begin;
			public long end;
			public string label;
		}

		public static AbstractHealthDevice create<T>() where T : AbstractHealthDevice, new()
		{
			T device = new T();
			device.init();
			return device;
		}

		public class StatisticsType
		{
			public const int today = 0;
			public const int week = 1;
			public const int month = 2;
		}

		protected virtual void init()
		{

		}

		public virtual void update()
		{

		}

		public abstract bool isAvailable();
		public abstract void initDevice(UnityAction<bool> callback);

        public abstract void setLastRecordTime(long lastRecordTime);
		public abstract void queryNewStepRecord(bool isStartup,List<ClientHealthLogData> result_list,UnityAction callback);

		public abstract void queryStepCountRange(long begin, long end,UnityAction<int> callback);
	}
}
