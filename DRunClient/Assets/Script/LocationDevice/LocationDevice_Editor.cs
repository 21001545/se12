using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class LocationDevice_Editor : AbstractLocationDevice
	{
		private MBLongLatCoordinate _initLocation;
		private MBLongLatCoordinate _mokLocation;
		private double _mokAltitude;
		private double _speed;
		private float _lastUpdateMokTime;
		private float _lastAngle;
		private long _lastQueryTime;

		public static LocationDevice_Editor create()
		{
			LocationDevice_Editor device = new LocationDevice_Editor();
			device.init();
			return device;
		}

		public override long getLastQueryTime()
		{
			return _lastQueryTime;
		}

		protected override void init()
		{
			_initLocation = _mokLocation = new MBLongLatCoordinate(127.0543616485475, 37.50637354896867);
			//_mokLocation = new MBLongLatCoordinate(127.0412, 37.4046);	// 청계산 국사봉 근처
			_mokLocation.pos.x += UnityEngine.Random.Range(-0.001f, 0.001f);
			_mokLocation.pos.y += UnityEngine.Random.Range(-0.001f, 0.001f);
			_speed = 0.00001f;
			_lastAngle = 0;
			_lastUpdateMokTime = Time.realtimeSinceStartup;
			_lastQueryTime = TimeUtil.unixTimestampUtcNow();
		}

		public override void initDevice(Action<bool> callback)
		{
			_lastLocation = _lastLocation_NotFiltered = _mokLocation;

			callback(true);
		}

		public override void startService()
		{
			
		}

		public override void stopService()
		{

		}

		public override void oneTimeRequest()
		{

		}

		public override void queryLocationLog(List<ClientLocationLog> result_list, Action action)
		{
			updateMokLocation();
			//updateMokLocation_NoiseSimulation();

			List<DeviceLocationInfo> list = new List<DeviceLocationInfo>();
			DeviceLocationInfo li = new DeviceLocationInfo();
			li.longitude = _mokLocation.lon;
			li.latitude = _mokLocation.lat;
			li.altitude = _mokAltitude;
			li.accuracy = 1;
			li.timestamp = TimeUtil.unixTimestampUtcNow();

			list.Add(li);

			processFilter(list, result_list);
			action();
		}

		private void updateMokLocation()
		{
			_lastAngle += UnityEngine.Random.Range(-15, 15);

			Vector2 dir = new Vector2(Mathf.Cos(_lastAngle * Mathf.Deg2Rad), Mathf.Sin(_lastAngle * Mathf.Deg2Rad));

			float deltaTime = Time.realtimeSinceStartup - _lastUpdateMokTime;
			_lastUpdateMokTime = Time.realtimeSinceStartup;

			double speed = _speed * 1 * deltaTime;
			//double speed = _speed * deltaTime;

			_mokLocation.pos.x += dir.x * speed;
			_mokLocation.pos.y += dir.y * speed;

			_mokAltitude = UnityEngine.Random.Range(60, 70);
			_lastQueryTime = TimeUtil.unixTimestampUtcNow();
		}

		private void updateMokLocation_NoiseSimulation()
		{
			float extent = 0.0001f;

			if( UnityEngine.Random.value < 0.1)
			{
				_mokLocation.pos.x = _initLocation.pos.x + UnityEngine.Random.Range(-extent, extent);
				_mokLocation.pos.y = _initLocation.pos.y + UnityEngine.Random.Range(-extent, extent);
			}
			//else
			//{
			//	_mokLocation = _initLocation;
			//}

			_mokAltitude = UnityEngine.Random.Range(60, 70);
			_lastQueryTime = TimeUtil.unixTimestampUtcNow();
		}
	}
}
