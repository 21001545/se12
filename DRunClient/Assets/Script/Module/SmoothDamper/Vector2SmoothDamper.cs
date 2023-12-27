using UnityEngine;

namespace Festa.Client.Module
{
	public class Vector2SmoothDamper
	{
		private Vector2 _cur;
		private Vector2 _target;
		private Vector2 _last_velocity;
		private float _smooth_time;
		private float _epsilon;

		public static Vector2SmoothDamper create(Vector2 value,float smooth_time,float epsilon = 1.0f)
		{
			Vector2SmoothDamper damper= new Vector2SmoothDamper();
			damper.init( value, smooth_time, epsilon);
			return damper;
		}

		public void init(Vector2 value,float smooth_time,float epsilon = 1.0f)
		{
			_target = _cur = value;
			_last_velocity = Vector2.zero;
			_smooth_time = smooth_time;
			_epsilon = epsilon;
		}

		public void reset(Vector2 value)
		{
			_cur = _target = value;
			_last_velocity = Vector2.zero;
		}

		public void resetVelocity()
		{
			_last_velocity = Vector2.zero;
		}

		public void setTarget(Vector2 v)
		{
			_target = v;
		}

		public Vector2 getCurrent()
		{
			return _cur;
		}

		public Vector2 getTarget()
		{
			return _target;
		}

		public bool update()
		{
			if( _target != _cur)
			{
				if( (_target - _cur).magnitude < _epsilon)
				{
					_cur = _target;
					return true;
				}


				_cur = Vector2.SmoothDamp( _cur, _target, ref _last_velocity, _smooth_time);
				return true;
			}

			return false;
		}

	}
}