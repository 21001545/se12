using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{
	public class DoubleSmoothDamper
	{
		private double _cur_value;
		private double _target_value;
		private double _last_velocity;
		private double _smooth_time;
		private double _epsilon;

		public static DoubleSmoothDamper create(double value,double smooth_time,double epsilon = 0.001)
		{
			DoubleSmoothDamper damper = new DoubleSmoothDamper();
			damper.init(value, smooth_time, epsilon);
			return damper;
		}

		private void init(double value,double smooth_time,double epsilon)
		{
			_target_value = _cur_value = value;
			_last_velocity = 0;
			_smooth_time = smooth_time;
			_epsilon = epsilon;
		}

		public void setSmoothTime(double smooth_time)
		{
			_smooth_time = smooth_time;
		}

		public void reset(double value)
		{
			_target_value = _cur_value = value;
			_last_velocity = 0;
		}

		public void rescale(double scalar)
		{
			_cur_value *= scalar;
			_target_value *= scalar;
			_last_velocity *= scalar;
		}

		public void setTarget(double v)
		{
			_target_value = v;
		}

		public double getCurrent()
		{
			return _cur_value;
		}

		public double getTarget()
		{
			return _target_value;
		}

		public bool update()
		{
			if (_target_value != _cur_value)
			{
				_cur_value = MathUtil.SmoothDamp(_cur_value, _target_value, ref _last_velocity, _smooth_time, (double)Mathf.Infinity, Time.deltaTime);

				if (System.Math.Abs(_target_value - _cur_value) < _epsilon)
				{
					_cur_value = _target_value;
				}

				return true;
			}
			return false;
		}
	}
}
