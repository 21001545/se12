using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.NetData
{
	public class ClientTripLog
	{
		public int trip_id;
		public string name;
		public DateTime begin_time;
		public DateTime end_time;
		public long period_time;
		public double distance_total;
		public double calorie_total;
		public int coin_total;
		public int coin_bonus;
		public int coin_steps;
		public int step_total;
		public int trip_type;
		public List<ClientTripPathData> path_data;
		public int cheering_count;
		public int photo_count;

		[SerializeOption(SerializeOption.NONE)]
		public List<ClientTripCheering> _cheeringList;

		[SerializeOption(SerializeOption.NONE)]
		public List<ClientTripPhoto> _photoList;

		public bool calcPathBound(out MBLongLatCoordinate center,out MBLongLatCoordinate min,out MBLongLatCoordinate max)
		{
			center = MBLongLatCoordinate.zero;
			min = MBLongLatCoordinate.zero;
			max = MBLongLatCoordinate.zero;

			for (int i = 0; i < path_data.Count; ++i)
			{
				ClientTripPathData data = path_data[i];

				// 2022.08.03 log가 하나도 없을 경우 문제가 생긴다, 서버를 수정하였으나, 나중에도 혹시 몰라서
				if (data.path_list.Count == 0)
				{
					continue;
				}

				if (i == 0)
				{
					min = new MBLongLatCoordinate(data.min_lon, data.min_lat);
					max = new MBLongLatCoordinate(data.min_lon + data.size_lon, data.min_lat + data.size_lat);
				}
				else
				{
					min.pos.x = System.Math.Min(data.min_lon, min.pos.x);
					min.pos.y = System.Math.Min(data.min_lat, min.pos.y);
					max.pos.x = System.Math.Max(data.min_lon + data.size_lon, max.pos.x);
					max.pos.y = System.Math.Max(data.min_lat + data.size_lat, max.pos.y);
				}
			}

			center.pos = (min.pos + max.pos) / 2;

			return min.isZero() == false && max.isZero() == false;
		}

		public MBLongLatCoordinate getStartLocation()
		{
			if( path_data.Count == 0)
			{
				return MBLongLatCoordinate.zero;
			}
			else
			{
				return path_data[0].getFirstLocation();
			}
		}

		public MBLongLatCoordinate getEndLocation()
		{
			if( path_data.Count == 0)
			{
				return MBLongLatCoordinate.zero;
			}
			else
			{
				return path_data[path_data.Count - 1].getLastLocation();
			}
		}

		public int getLocalBeginTimeMonth()
		{
			DateTime localBeginTime = begin_time.ToLocalTime();
			return localBeginTime.Year * 100 + localBeginTime.Month;
		}
	}
}
