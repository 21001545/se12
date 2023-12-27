using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class TripLogGroup
	{
		private int _month;
		private List<ClientTripLog> _logList;
		private TimeSpan _totalTime;
		
		public int getMonth()
		{
			return _month;
		}

		public DateTime getMonthDateTime()
		{
			int year = _month / 100;
			int month = _month % 100;

			return new DateTime(year, month, 1);
		}

		public TimeSpan getTotalTime()
		{
			if( _totalTime.TotalSeconds == 0)
			{
				calcTotalTime();
			}

			return _totalTime;
		}

		public List<ClientTripLog> getLogList()
		{
			return _logList;
		}

		public static TripLogGroup create(int month)
		{
			TripLogGroup group = new TripLogGroup();
			group.init(month);
			return group;
		}

		private void init(int month)
		{
			_month = month;
			_logList = new List<ClientTripLog>();
			_totalTime = TimeSpan.Zero;
		}

		public void addLog(ClientTripLog log)
		{
			_logList.Add(log);
		}

		private void calcTotalTime()
		{
			foreach(ClientTripLog log in _logList)
			{
				_totalTime += (log.end_time - log.begin_time);
			}
		}

	}
}
