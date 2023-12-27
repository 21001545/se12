using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class HealthLogAppender
	{
		private DateTime _record_time;
		private double _value;

		public static HealthLogAppender create()
		{
			HealthLogAppender appender = new HealthLogAppender();
			appender.init();
			return appender;
		}

		private void init()
		{
			_record_time = DateTime.UtcNow;
			_value = 0;
		}

		public ClientHealthLogData append(DateTime record_time,double value)
		{
			_record_time = record_time;
			_value += value;

			if( _value < 1.0)
			{
				return null;
			}

			int intValue = (int)_value;
			_value %= 1.0;

			return ClientHealthLogData.create(record_time, intValue);
		}
	}
}
