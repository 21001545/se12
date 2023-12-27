using UnityEngine;

namespace Festa.Client.Module
{
	public class FloatSmoothDamper
	{
		private float _cur_value;
		private float _target_value;
		private float _last_velocity;
		private float _smooth_time;
		private float _epsilon;

		public static FloatSmoothDamper create(float value,float smooth_time,float epsilon = 0.001f)
		{
			FloatSmoothDamper damper = new FloatSmoothDamper();
			damper.init( value, smooth_time, epsilon);
			return damper;
		}

		private void init(float value,float smooth_time,float epsilon)
		{
			_target_value = _cur_value = value;
			_last_velocity = 0;
			_smooth_time = smooth_time;
			_epsilon = epsilon;
		}

		public void reset(float value)
		{
			_target_value = _cur_value = value;
			_last_velocity = 0;
		}

		public void setTarget(float v)
		{
			_target_value = v;
		}

		public float getCurrent()
		{
			return _cur_value;
		}

		public float getTarget()
		{
			return _target_value;
		}

		public bool update()
		{
			if( _target_value != _cur_value)
			{
				_cur_value = Mathf.SmoothDamp( _cur_value, _target_value, ref _last_velocity, _smooth_time);

				if( Mathf.Abs( _target_value - _cur_value) < _epsilon)
				{
					_cur_value = _target_value;
				}

				return true;
			}
			return false;
		}
	}
}
