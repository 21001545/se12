using Festa.Client.Module;
using System.Collections.Generic;

namespace DRun.Client.Running
{
	public class GPSBound
	{
		private bool _isInit;
		private DoubleVector3 _min;
		private DoubleVector3 _max;

		public GPSBound create()
		{
			GPSBound bound = new GPSBound();
			bound._isInit= true;
			return bound;
		}

		public DoubleVector3 getMin()
		{
			return _min;
		}

		public DoubleVector3 getMax()
		{
			return _max;
		}

		public DoubleVector3 getCenter()
		{
			return (_min + _max) / 2.0;
		}

		public DoubleVector3 getSize()
		{
			return (_max - _min);
		}

		public void reset()
		{
			_isInit = true;
		}

		public void updateBound(List<GPSTilePosition> posList)
		{
			foreach(GPSTilePosition pos in posList)
			{
				if( _isInit)
				{
					_min = _max = new DoubleVector3( pos.gps_pos.x, pos.gps_pos.y, pos.gps_alt);
					_isInit = false;
				}
				else
				{
					_min.x = System.Math.Min(_min.x, pos.gps_pos.x);
					_min.y = System.Math.Min(_min.y, pos.gps_pos.y);
					_min.z = System.Math.Min(_min.z, pos.gps_alt);

					_max.x = System.Math.Max(_max.x, pos.gps_pos.x);
					_max.y = System.Math.Max(_max.y, pos.gps_pos.y);
					_max.z = System.Math.Max(_max.z, pos.gps_alt);
				}
			}
		}

	}
}
