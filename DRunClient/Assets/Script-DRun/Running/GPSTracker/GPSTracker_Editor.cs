using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.Running
{
	public class GPSTracker_Editor : AbstractGPSTracker
	{
		private IntervalTimer _timer;
		private MBLongLatCoordinate _lastLocation;

		private List<ClientLocationLog> _logList;

		private double _speed;
		private float _lastAngle;
		private float _lastGenerateLocationTime;
		
		public double speedScale = 1.0f;
		public float updateInterval = 1.0f;
		public bool rotateDirection = true;
		public float accuracy = 30.0f;

		private static GPSTracker_Editor _instance;

		public static GPSTracker_Editor getInstance()
		{
			return _instance;
		}

		protected override void init()
		{
			base.init();

			_instance= this;
		}

		public override bool checkPermission()
		{
			return true;
		}

		public void setLastLocation(MBLongLatCoordinate location)
		{
			_lastLocation = location;
		}

		public override void getLocationFrom(long begin,List<ClientLocationLog> resultList)
		{
			resultList.Clear();

			DateTime dtBegin = TimeUtil.dateTimeFromUnixTimestamp(begin);
			foreach(ClientLocationLog log in _logList)
			{
				if( log.event_time >= dtBegin)
				{
					resultList.Add(log);
				}
			}

			updateGPSStatusInfo(resultList);
		}

		public override void getLocationRange(long begin, long end,List<ClientLocationLog> resultList)
		{
			resultList.Clear();

			DateTime dtBegin = TimeUtil.dateTimeFromUnixTimestamp(begin);
			DateTime dtEnd = TimeUtil.dateTimeFromUnixTimestamp(end);
			foreach (ClientLocationLog log in _logList)
			{
				if( log.event_time >= dtBegin && log.event_time <= dtEnd)
				{
					resultList.Add(log);
				}
			}
		}

		public override void initDevice()
		{
			_lastLocation = new MBLongLatCoordinate(127.0543616485475, 37.50637354896867);
			_lastAngle = 0;
			_speed = 0.00001f;
			_logList = new List<ClientLocationLog>();

			_timer = IntervalTimer.create(0.3f, false, false);
			_timer.stop();
		}

		public override void update()
		{
			if( _running && _timer.update())
			{
				generateNewLocation();

				_timer.setNext(updateInterval);
			}
		}

		public override void start()
		{
			Debug.Log("start gps tracking");

			_running = true;
			_statusInfo.reset();

			// 최초 시작시 gps 신호를 기다리는거 시뮬레이션
			
			//_timer.setNext(UnityEngine.Random.Range(4.5f, 5.0f));	
			_timer.setNext(UnityEngine.Random.Range(0.5f, 1.0f));

			_lastGenerateLocationTime = Time.realtimeSinceStartup;
		}

		public override void stop()
		{
			Debug.Log("stop gps tracking");

			_running = false;
			_timer.stop();
		}

		public void changeUpdateInterval(float interval)
		{
			updateInterval = interval;
			_timer.setNext(interval);
		}

		private void generateNewLocation()
		{
			if( rotateDirection)
			{
				_lastAngle += UnityEngine.Random.Range(-15, 15);
			}

			Vector2 dir = new Vector2(Mathf.Cos(_lastAngle * Mathf.Deg2Rad), Mathf.Sin(_lastAngle * Mathf.Deg2Rad));

			float deltaTime = Time.realtimeSinceStartup - _lastGenerateLocationTime;
			_lastGenerateLocationTime = Time.realtimeSinceStartup;

			double speed = _speed * deltaTime * speedScale / updateInterval;

			_lastLocation.pos.x += dir.x * speed;
			_lastLocation.pos.y += dir.y * speed;

			double altitude = UnityEngine.Random.Range(60, 70);

			long event_time = TimeUtil.unixTimestampUtcNow();

			//Debug.Log($"generate location: event_time[{event_time}] speed[{speed}] deltaTime[{deltaTime}]");

			//
			ClientLocationLog log = ClientLocationLog.create(_lastLocation.pos.x, _lastLocation.pos.y, altitude, accuracy, -1, -1, event_time);
			_logList.Add( log);
		}


	}
}
