using UnityEngine;

namespace Festa.Client.Module
{
	public class DoubleVector2SmoothDamper
	{
		private DoubleVector2 _cur;
		private DoubleVector2 _target;
		private DoubleVector2 _last_velocity;
		private double _smooth_time;
		private double _epsilon;
		private double _max_speed;

		public static DoubleVector2SmoothDamper create(DoubleVector2 value, double smooth_time,double epsilon)
		{
			DoubleVector2SmoothDamper damper = new DoubleVector2SmoothDamper();
			damper.init(value, smooth_time, epsilon);
			return damper;
		}

		public void init(DoubleVector2 value, double smooth_time,double epsilon)
		{
			_target = _cur = value;
			_last_velocity = DoubleVector2.zero;
			_smooth_time = smooth_time;
			_epsilon = epsilon;
			_max_speed = Mathf.Infinity;
		}

		public void reset(DoubleVector2 v)
		{
			_cur = _target = v;
			_last_velocity = DoubleVector2.zero;
		}

		public void rescale(double scalar)
		{
			_cur *= scalar;
			_target *= scalar;
			_last_velocity *= scalar;
		}

		public DoubleVector2 getLastVelocity()
		{
			return _last_velocity;
		}

		public void setTarget(DoubleVector2 v)
		{
			_target = v;
		}

		public void setTarget2(DoubleVector2 v,double smoot_time,double max_speed)
		{
			_target = v;
			_smooth_time = smoot_time;
			_max_speed = max_speed;
		}

		public DoubleVector2 getCurrent()
		{
			return _cur;
		}

		public DoubleVector2 getTarget()
		{
			return _target;
		}

		public bool update()
		{
			if (_target != _cur)
			{
				if ((_target - _cur).magnitude < _epsilon)
				{
					_cur = _target;
					return true;
				}


				_cur = MathUtil.SmoothDamp(_cur, _target, ref _last_velocity, _smooth_time, _max_speed, Time.deltaTime);
				return true;
			}

			return false;
		}
	}
}
