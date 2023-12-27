using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Events;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Festa.Client.Logic.TripChartDataBuilder;

namespace Festa.Client.Logic
{
	public class TripChartDataBuilder : BaseStepProcessor
	{
		private ClientTripLog _log;
		private int _speedUnitType;
		private int _altUnitType;

		public class TickData
		{
			public int time;
			public double speed;
			public double altitude;

			public TickData(int _time,double _speed,double _altitude)
			{
				this.time = _time;
				this.speed = _speed;
				this.altitude = _altitude;
			}

			public static TickData lerp(int time,TickData begin,TickData end)
			{
				double ratio = (time - begin.time) / (double)(end.time - begin.time);

				double speed = (end.speed - begin.speed) * ratio + begin.speed;
				double altitude = (end.altitude - begin.altitude) * ratio + begin.altitude;

				return new TickData(time, speed, altitude);
			}
		}

		private List<TickData> _tickList;
		private List<float> _speedChart;
		private List<float> _altitudeChart;
		private int _tickBeginTime;
		private int _tickEndTime;

		public List<float> getSpeedChart()
		{
			return _speedChart;
		}

		public List<float> getAltitudeChart()
		{
			return _altitudeChart;
		}

		// 항상 0일것 같은데
		public int getChartBeginTime()
		{
			return _tickBeginTime;
		}

		public int getChartEndTime()
		{
			return _tickEndTime;
		}

		public static TripChartDataBuilder create(ClientTripLog log,int speed_unit_type,int alt_unit_type)
		{
			TripChartDataBuilder builder = new TripChartDataBuilder();
			builder.init(log, speed_unit_type, alt_unit_type);
			return builder;
		}

		private void init(ClientTripLog log,int speed_unit_type,int alt_unit_type)
		{
			base.init();

			_log = log;
			_speedUnitType = speed_unit_type;
			_altUnitType = alt_unit_type;
		}

		protected override void buildSteps()
		{
			_stepList.Add(buildTickList);
			_stepList.Add(buildChart);
		}

		private void buildTickList(Handler<AsyncResult<Module.Void>> handler)
		{
			_tickList = new List<TickData>();

			MBLongLatCoordinate lastLocation = MBLongLatCoordinate.zero;
			int last_time = 0;

			foreach (ClientTripPathData path in _log.path_data)
			{
				int count = path.path_list.Count / 3;

				for(int i = 0; i < count; ++i)
				{
					int time = path.path_time_list[i];
					MBLongLatCoordinate location = new MBLongLatCoordinate(path.path_list[i * 3 + 0], path.path_list[i*3 + 1]);
					double altitude = path.path_list[i * 3 + 2];

					if( _altUnitType == UnitDefine.DistanceType.ft)
					{
						altitude = UnitDefine.m_2_f(altitude);
					}

					if( lastLocation.isZero())
					{
						lastLocation = location;
						last_time = time;
						_tickList.Add(new TickData(time, 0, altitude));
					}
					else
					{
						double distance = location.distanceFrom(lastLocation);

						// 그럴 수 있다
						if( last_time == time)
						{
							continue;
						}

						double duration = (double)(time - last_time) / (double)TimeUtil.msHour;

						double speed = duration == 0 ? 0 : distance / duration;
						if( _speedUnitType == UnitDefine.DistanceType.mil)
						{
							speed = UnitDefine.km_2_mil(speed);
						}

						_tickList.Add(new TickData(time, speed, altitude));

						lastLocation = location;
						last_time = time;
					}
				}
			}

			if( _tickList.Count == 0)
			{
				handler(Future.failedFuture("tick data is empty"));
				return;
			}

			// 첫번째 속도는 바로 다음 속도를 그대로 복사
			if( _tickList.Count > 1)
			{
				_tickList[0].speed = _tickList[1].speed;
			}

			_tickBeginTime = _tickList[0].time;
			_tickEndTime = _tickList[_tickList.Count - 1].time;

			handler(Future.succeededFuture());
		}

		private TickData pickData(int time)
		{
			for(int i = 1; i < _tickList.Count; ++i)
			{
				TickData begin = _tickList[i - 1];
				TickData end = _tickList[i];

				if( time >= begin.time && time <= end.time)
				{
					return TickData.lerp(time, begin, end);
				}
			}

			Debug.Log($"can't pick data: time[{time}]");

			return new TickData(time, 0, 0);
		}

		private void buildChart(Handler<AsyncResult<Module.Void>> handler)
		{
			_speedChart = new List<float>();
			_altitudeChart = new List<float>();

			int count = 24;
			
			for(int i = 0; i < count; ++i)
			{
				int tick_time = i * (_tickEndTime - _tickBeginTime) / (count - 1) + _tickBeginTime;

				TickData tickData = pickData(tick_time);

				_speedChart.Add((float)tickData.speed);
				_altitudeChart.Add((float)tickData.altitude);
			}

			handler(Future.succeededFuture());
		}
	}
}
