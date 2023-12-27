using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{
	public class IntervalTimer
	{
		private float _next_time;
		private float _interval;
		private bool _auto;

		public static IntervalTimer create(float interval,bool auto,bool startup_invoke)
		{
			IntervalTimer t = new IntervalTimer();
			t.init(interval, auto,startup_invoke);
			return t;
		}

		public void init(float interval,bool auto,bool startup_invoke)
		{
			if (startup_invoke)
			{
				_next_time = Time.realtimeSinceStartup;
			}
			else
			{
				_next_time = Time.realtimeSinceStartup + interval;
			}

			_interval = interval;
			_auto = auto;
		}

		public bool update()
		{
			if( _next_time != 0 && Time.realtimeSinceStartup >= _next_time)
			{
				if( _auto)
				{
					_next_time = Time.realtimeSinceStartup + _interval;
				}
				else
				{
					_next_time = 0;
				}

				return true;
			}

			return false;
		}

		public void setNext()
		{
			_next_time = Time.realtimeSinceStartup + _interval;
		}

		public void setNext(float new_interval)
		{
			_interval = new_interval;
			setNext();
		}

		public void stop()
		{
			_next_time = 0;
		}
	}
}
