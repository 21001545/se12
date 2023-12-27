using UnityEngine;

namespace Festa.Client.Module
{
	public class Vector3SmoothDamper
	{
		private Vector3 _cur;
		private Vector3 _target;
		private Vector3 _last_velocity;
		private float _smooth_time;

		public static Vector3SmoothDamper create(Vector3 value,float smooth_time)
		{
			Vector3SmoothDamper damper = new Vector3SmoothDamper();
			damper.init( value, smooth_time);
			return damper;
		}

		private void init(Vector3 value,float smooth_time)
		{
			_target = _cur = value;
			_last_velocity = Vector3.zero;
			_smooth_time = smooth_time;
		}

		public void reset(Vector3 value)
		{
			_last_velocity = Vector3.zero;
			_target = _cur = value;
		}

		public void setTarget(Vector3 v)
		{
			_target = v;
		}

		public Vector3 getCurrent()
		{
			return _cur;
		}

		public bool update()
		{
			if( _target != _cur)
			{
				_cur = Vector3.SmoothDamp( _cur, _target, ref _last_velocity, _smooth_time);
				return true;
			}

			return false;
		}
	}
}