using Festa.Client.MapBox;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
https://maddevs.io/blog/reduce-gps-data-error-on-android-with-kalman-filter-and-accelerometer/
*/

namespace Festa.Client
{
	public abstract class AbstractLocationDevice
	{
		protected MBLongLatCoordinate _lastLocation;
		protected MBLongLatCoordinate _lastLocation_NotFiltered;

		protected double _lastAltitude;

		protected float _qMetersPerSecond = 3.0f;
		protected KalmanLatLong _filter;

		public enum AuthorizationType
        {
            NotDetermined = 0,
            Restricted = 1,
            Denied = 2,
            Authorized = 3,
            AuthorizedAlways = 4,
        };

		public MBLongLatCoordinate getLastLocation()
		{
			return _lastLocation;
		}

		public MBLongLatCoordinate getLastLocationNotFiltered()
		{
			return _lastLocation_NotFiltered;
		}

		public double getLastAltitude()
		{
			return _lastAltitude;
		}

		public float getFilterQMetersPerSecond()
		{
			return _qMetersPerSecond;
		}

		public void setFilterQMetersPerSecond(float q)
		{
			_qMetersPerSecond = q;
			_filter = null;
		}

		public void resetFilter()
		{
			_filter = null;
		}

		public virtual void requestPermission()
        {
        }

		public virtual int currentAuthorizationStatus()
        {
			return 0;
        }

		public abstract void initDevice(Action<bool> callback);
		public abstract void queryLocationLog(List<ClientLocationLog> result_list, Action action);

		public abstract void startService();
		public abstract void stopService();
		public abstract void oneTimeRequest();

		public abstract long getLastQueryTime();

		protected virtual void init()
		{

		}

		protected virtual void processFilter(List<DeviceLocationInfo> source_list,List<ClientLocationLog> result_list)
		{
			foreach(DeviceLocationInfo log in source_list)
			{

				if (_filter == null)
				{
					_filter = new KalmanLatLong(_qMetersPerSecond);
					_filter.SetState(log.latitude, log.longitude, log.accuracy, log.timestamp);
				}
				else
				{
					_filter.Process(log.latitude, log.longitude, log.accuracy, log.timestamp);
				}

				_lastLocation = new MBLongLatCoordinate(_filter.Lng, _filter.Lat);
				_lastLocation_NotFiltered = new MBLongLatCoordinate(log.longitude, log.latitude);

				_lastAltitude = log.altitude;

				result_list.Add(ClientLocationLog.create(_filter.Lng, _filter.Lat, _lastAltitude, log.accuracy, -1, -1, log.timestamp));
			}
		}
	}
}
