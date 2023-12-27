using DRun.Client.Running;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.NetData
{
	public class ClientTripPathData
	{
		public int trip_id;
		public int path_id;
		public int trip_type;
		public int mined;
		public int step_count;
		public double min_lon;
		public double min_lat;
		public double min_alt;
		public double size_lon;
		public double size_lat;
		public double size_alt;
		public long path_begin_time;
		public long path_end_time;
		public List<double> path_list;	// 2021.12.15 정밀도 이슈가 있어서 수정함
		public List<int> path_time_list;

		// gps 지점별 칼로리 계산을 위해
		[SerializeOption(SerializeOption.NONE)]
		private double _weight;

		[SerializeOption(SerializeOption.NONE)]
		private double _duration = -1;

		[SerializeOption(SerializeOption.NONE)]
		private double _calorie = 0;

		[SerializeOption(SerializeOption.NONE)]
		private double _length = -1;

		[SerializeOption(SerializeOption.NONE)]
		private KalmanLatLong _filter;

		public double getDuration()
		{
			return _duration;
		}

		public double getLength()
		{
			return _length;
		}

		public double getCalorie()
		{
			return _calorie;
		}


		public static ClientTripPathData create(int trip_type,int mined,double weight,List<ClientLocationLog> logList)
		{
			ClientTripPathData data = new ClientTripPathData();
			data.init(trip_type,mined,weight,logList);
			return data;
		}

		public static ClientTripPathData cloneForSave(ClientTripPathData s)
		{
			ClientTripPathData d = new ClientTripPathData();
			d.trip_id = s.trip_id;
			d.path_id = s.path_id;
			d.trip_type = s.trip_type;
			d.mined = s.mined;
			d.step_count = s.step_count;
			d.min_lon = s.min_lon;
			d.min_lat = s.min_lat;
			d.min_alt = s.min_alt;
			d.size_lon = s.size_lon;
			d.size_lat = s.size_lat;
			d.size_alt = s.size_alt;
			d.path_begin_time= s.path_begin_time;
			d.path_end_time= s.path_end_time;

			d.path_list = new List<double>();
			d.path_list.AddRange(s.path_list);
			
			d.path_time_list = new List<int>();
			d.path_time_list.AddRange(s.path_time_list);

			return d;
		}

		private void init(int trip_type,int mined, double weight,List<ClientLocationLog> logList)
		{
			trip_id = 0;
			path_id = 0;
			this.trip_type = trip_type;
			this.mined = mined;

			path_begin_time = 0;
			path_end_time = TimeUtil.unixTimestampUtcNow();
			path_list = new List<double>();
			path_time_list = new List<int>();
			step_count = 0;

			_weight = weight;
			_calorie = -1;
			_length = -1;
			_duration = -1;

			if ( logList != null && logList.Count > 0)
			{
				appendLocation(logList);
			}
		}

		public void setStepCount(int count)
		{
			this.step_count = count;
		}

		public void appendLocation(List<ClientLocationLog> logList)
		{
			double max_lon;
			double max_lat;
			double max_alt;

			if( path_list.Count == 0)
			{
				min_lon = double.MaxValue;
				min_lat = double.MaxValue;
				min_alt = double.MaxValue;
				max_lon = double.MinValue;
				max_lat = double.MinValue;
				max_alt = double.MinValue;
			}
			else
			{
				max_lon = min_lon + size_lon;
				max_lat = min_lat + size_lat;
				max_alt = min_alt + size_alt;
			}

			if (path_begin_time == 0 && logList.Count > 0)
			{
				path_begin_time = TimeUtil.unixTimestampFromDateTime(logList[0].event_time);
			}

			foreach(ClientLocationLog log in logList)
			{
				long event_time = TimeUtil.unixTimestampFromDateTime(log.event_time);
				long deltaTime = path_time_list.Count > 0 ? deltaTime = event_time - path_end_time : 0;

				double longitude = log.longitude;
				double latitude = log.latitude;

				// filter를 여기에 넣어 주자
				if( _filter == null)
				{
					_filter = new KalmanLatLong(5.0f);
					_filter.SetState(latitude, longitude, (float)log.accuracy, event_time);
				}
				else
				{
					_filter.Process(latitude, longitude, (float)log.accuracy, event_time);
				}

				longitude = _filter.Lng;
				latitude = _filter.Lat;

				//
				path_list.Add(longitude);
				path_list.Add(latitude);
				path_list.Add(log.altitude);
				path_time_list.Add((int)(event_time - path_begin_time));

				min_lon = System.Math.Min(longitude, min_lon);
				min_lat = System.Math.Min(latitude, min_lat);
				min_alt = System.Math.Min(log.altitude, min_alt);

				max_lon = System.Math.Max(log.longitude, max_lon);
				max_lat = System.Math.Max(log.latitude, max_lat);
				max_alt = System.Math.Max(log.altitude, max_alt);
				
				path_end_time = TimeUtil.unixTimestampFromDateTime(log.event_time);
			}

			size_lon = max_lon - min_lon;
			size_lat = max_lat - min_lat;
			size_alt = max_alt - min_alt;

			calcStatus();
		}

		// 오류 수정
		public void recalcBound()
		{
			double max_lon = double.MinValue;
			double max_lat = double.MinValue;
			double max_alt = double.MinValue;

			int count = path_list.Count / 3;
			for (int i = 0; i < count; ++i)
			{
				double lon = path_list[i * 3 + 0];
				double lat = path_list[i * 3 + 1];
				double alt = path_list[i * 3 + 2];

				if( i == 0)
				{
					max_lon = min_lon = lon;
					max_lat = min_lat = lat;
					max_alt = min_alt = alt;
				}
				else
				{
					min_lon = System.Math.Min(min_lon, lon);
					min_lat = System.Math.Min(min_lat, lat);
					min_alt = System.Math.Min(min_alt, alt);

					max_lon = System.Math.Max(max_lon, lon);
					max_lat = System.Math.Max(max_lat, lat);
					max_alt = System.Math.Max(max_alt, lat);
				}
			}

			size_lon = max_lon - min_lon;
			size_lat = max_lat - min_lat;
			size_alt = max_alt - min_alt;
		}

		public void loadingPostProcess(double weight)
		{
			_weight = weight;
			calcStatus();
		}

		public MBLongLatCoordinate getCenter()
		{
			return new MBLongLatCoordinate(min_lon + size_lon / 2, min_lat + size_lat / 2);
		}

		public MBLongLatCoordinate getMin()
		{
			return new MBLongLatCoordinate(min_lon, min_lat);
		}

		public MBLongLatCoordinate getMax()
		{
			return new MBLongLatCoordinate(min_lon + size_lon, min_lat + size_lat);
		}

		public MBLongLatCoordinate getLastLocation()
		{
			int count = path_list.Count / 3;
			if( count == 0)
			{
				return MBLongLatCoordinate.zero;
			}
			else
			{
				return new MBLongLatCoordinate(path_list[(count - 1) * 3 + 0], path_list[(count - 1) * 3 + 1]);
			}
		}

		public MBLongLatCoordinate getFirstLocation()
		{
			int count = path_list.Count / 3;
			if (count == 0)
			{
				return MBLongLatCoordinate.zero;
			}
			else
			{
				return new MBLongLatCoordinate(path_list[0], path_list[1]);
			}
		}

		public int getLocationCount()
		{
			return path_list.Count / 3;
		}


		public ClientLocationLog getLastLocationLog()
		{
			int index = getLocationCount() - 1;
			if (index < 0)
			{
				return null;
			}

			return getLocationLog(index);
		}

		public ClientLocationLog getLocationLog(int index)
		{
			if (index >= (path_list.Count / 3))
			{
				return null;
			}

			return ClientLocationLog.create(path_list[index * 3 + 0], path_list[index * 3 + 1], path_list[index * 3 + 2], 0, -1, -1, path_time_list[index] + path_begin_time);
		}

		#region calcStatus

		// 런닝 지도 화면의 경로 최적화와 같은 값을 사용
		private static double _simplifyThreshold = 400.0 / 4096.0;
		private static int _zoom = 18;

		public void calcStatus()
		{
			int count;

			// Tile좌표 만들기
			List<GPSTilePosition> tilePosList = new List<GPSTilePosition>();
			count = path_list.Count / 3;
			for(int i = 0; i < count; ++i)
			{
				long time = path_time_list[i] + path_begin_time;
				double longitude = path_list[i * 3 + 0];
				double latitude = path_list[i * 3 + 1];
				double altitude = path_list[i * 3 + 2];

				double tile_x;
				double tile_y;

				MapBoxUtil.getTileXY(longitude, latitude, _zoom, out tile_x, out tile_y);
				tilePosList.Add( GPSTilePosition.create(i, time, longitude, latitude, altitude));
			}

			// 최적화
			List<GPSTilePosition> optPosList = GPSPathSimplify.Simplify(tilePosList, _simplifyThreshold, false);

			// 경로 시간
			_duration = ((double)(optPosList[ optPosList.Count - 1].time - path_begin_time)) / TimeUtil.msSecond;

			_length = 0;
			_calorie = 0;
			GPSTilePosition lastPos = optPosList[0];
			for(int i = 1; i < optPosList.Count; ++i)
			{
				GPSTilePosition curPos = optPosList[i];

				long delta_time = curPos.time - lastPos.time;

				double distance = MapBoxUtil.distance(lastPos.gps_pos.x, lastPos.gps_pos.y, curPos.gps_pos.x, curPos.gps_pos.y);
				double calorie = calcCalorie(_weight, distance, delta_time);

				//Debug.Log($"calcCalorie: weight[{_weight}] distance[{distance}] delta_time[{delta_time}] calorie[{calorie}]");

				_length += distance;
				_calorie += calorie;

				lastPos = curPos;
			}

			//Debug.Log($"path status:length[{_length}] duration[{_duration}] calorie[{_calorie}]");
		}

		private double calcCalorie(double weight, double distance, long deltaTime)
		{
			double durationMinutes = (double)deltaTime / 1000.0 / 60.0;
			if (durationMinutes == 0)
			{
				return 0;
			}

			double speedKMH = distance / (durationMinutes / 60.0);

			return GlobalRefDataContainer.getRefDataHelper().calcCalories(1, speedKMH, weight, durationMinutes);
		}

		//private void calcSimplifiedLength()
		//{
		//	_simplifiedLength = 0;
		//	if (_tilePosList.Count < 2)
		//	{
		//		return;
		//	}

		//	List<GPSTilePosition> simplifiedList = GPSPathSimplify.Simplify(_tilePosList, _threshold, false);
		//	MBLongLatCoordinate lastPos = MBLongLatCoordinate.fromTileXY(16, simplifiedList[0].tile_pos);

		//	for (int i = 1; i < simplifiedList.Count; ++i)
		//	{
		//		MBLongLatCoordinate curPos = MBLongLatCoordinate.fromTileXY(16, simplifiedList[i].tile_pos);
		//		_simplifiedLength += curPos.distanceFrom(lastPos);
		//		lastPos = curPos;
		//	}
		//}

		//private void calcCalorie()
		//{
		//	_calorie = 0;
		//	int count = path_list.Count / 3;
		//	for (int i = 1; i < count; ++i)
		//	{
		//		MBLongLatCoordinate from = new MBLongLatCoordinate(path_list[(i - 1) * 3 + 0], path_list[(i - 1) * 3 + 1]);
		//		MBLongLatCoordinate to = new MBLongLatCoordinate(path_list[i * 3 + 0], path_list[i * 3 + 1]);

		//		long delta = path_time_list[i] - path_time_list[i - 1];

		//		_calorie += calcCalorie(_weight, from.distanceFrom(to), delta);
		//	}
		//}

		//private void calcLength()
		//{
		//	_length = 0;
		//	int count = path_list.Count / 3;
		//	for (int i = 1; i < count; ++i)
		//	{
		//		MBLongLatCoordinate from = new MBLongLatCoordinate(path_list[(i - 1) * 3 + 0], path_list[(i - 1) * 3 + 1]);
		//		MBLongLatCoordinate to = new MBLongLatCoordinate(path_list[i * 3 + 0], path_list[i * 3 + 1]);

		//		_length += from.distanceFrom(to);
		//	}
		//}

		//private void rebuildTilePosList()
		//{
		//	_tilePosList = new List<GPSTilePosition>();
		//	int count = path_list.Count / 3;
		//	for(int i = 0; i < count; ++i)
		//	{
		//		MBTileCoordinateDouble tilePos = MBTileCoordinateDouble.fromLonLat(path_list[i * 3 + 0], path_list[i * 3 + 1], 16);
		//		_tilePosList.Add(new GPSTilePosition(path_time_list[i] + path_begin_time, tilePos.tile_pos));
		//	}
		//}


		//private void calcDuration()
		//{
		//	if( path_time_list.Count == 0)
		//	{
		//		_duration = 0;
		//	}
		//	else
		//	{
		//		long tick = path_time_list[path_time_list.Count - 1];
		//		_duration = (double)tick / (double)TimeUtil.msSecond;
		//	}
		//}



		#endregion
	}
}
