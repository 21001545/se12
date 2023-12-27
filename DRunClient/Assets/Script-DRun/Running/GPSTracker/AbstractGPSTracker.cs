using Festa.Client.Module;
using Festa.Client.NetData;
using System.Collections.Generic;
using UnityEngine.Localization;

namespace DRun.Client.Running
{
	public abstract class AbstractGPSTracker
	{
		protected bool _running;
		protected ClientLocationLog _lastLocationLog;
		protected GPSStatusInfo _statusInfo;

		public abstract void initDevice();
		public abstract void start();
		public abstract void stop();
		public abstract void getLocationFrom(long begin,List<ClientLocationLog> resultList);
		public abstract void getLocationRange(long begin, long end,List<ClientLocationLog> resutList);

		public virtual bool isRunning()
		{
			return _running;
		}
		public virtual ClientLocationLog getLastLocationLog()
		{
			return _lastLocationLog;
		}
		public virtual GPSStatusInfo getStatusInfo()
		{
			return _statusInfo;
		}

		public abstract bool checkPermission();

		protected virtual void init()
		{
			_running = false;
			_lastLocationLog = null;
			_statusInfo = new GPSStatusInfo();
			_statusInfo.reset();
		}

		public static AbstractGPSTracker create()
		{
			AbstractGPSTracker tracker;
#if UNITY_EDITOR
			tracker = new GPSTracker_Editor();
#elif UNITY_ANDROID
			tracker = new GPSTracker_Android();
#elif UNITY_IOS
			tracker = new GPSTracker_iOS();
#else

#endif
			tracker.init();
			return tracker;
		}

		public virtual void update()
		{

		}

		public virtual void updateGPSStatusInfo(List<ClientLocationLog> logList)
		{
			if( logList.Count > 0)
			{
				ClientLocationLog lastLog = logList[logList.Count - 1];
				_statusInfo.lastLocationTime = TimeUtil.unixTimestampFromDateTime(lastLog.event_time);
				_statusInfo.lastAccuracy = lastLog.accuracy;
			}

			long now = TimeUtil.unixTimestampUtcNow();

			// 한번도 신호를 받은 적이 없거나, 3초이상 위치 정보를 못받으면 
			int newStatus = _statusInfo.status;

			if( _statusInfo.lastAccuracy == -1 || (now - _statusInfo.lastLocationTime) >= (TimeUtil.msSecond * 5.0f))
			{
				newStatus = GPSStatusInfo.Status.no_signal;
			}
			else
			{
				newStatus = calcGPSStatusByAccuracy(_statusInfo.lastAccuracy);
			}

			_statusInfo.status = newStatus;
		}

		public virtual int calcGPSStatusByAccuracy(double accuracy)
		{
			if( accuracy > 25.0)
			{
				return GPSStatusInfo.Status.weak;
			}
			else if( accuracy > 10.0)
			{
				return GPSStatusInfo.Status.normal;
			}

			return GPSStatusInfo.Status.good;
		}
	}
}
