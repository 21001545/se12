using Festa.Client.MapBox;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.NetData
{
	public class ClientRunningLog
	{
		public int running_id;
		public int running_type;
		public int running_sub_type;
		public string pfp;
		public DateTime start_time;
		public DateTime end_time;
		public int goal;
		public long drn_total;
		public long drn_running;
		public long drn_bonus;

		public int running_time;
		public double distance;
		public double mine_distance;
		public double velocity;
		public int step_count;
		public double calories;

		public int used_heart;
		public int used_distance;
		public int used_stamina;

		public List<ClientTripPathData> pathList;

		public MBLongLatCoordinate getStartLocation()
		{
			if( pathList.Count == 0)
			{
				return MBLongLatCoordinate.zero;
			}

			return pathList[0].getFirstLocation();
		}

		public MBLongLatCoordinate getEndLocation()
		{
			if( pathList.Count == 0)
			{
				return MBLongLatCoordinate.zero;
			}

			return pathList[pathList.Count - 1].getLastLocation();
		}

		public bool isProMode()
		{
			return running_type == ClientRunningLogCumulation.RunningType.promode;
		}

		public float getGoalRatio()
		{
			if (running_type != ClientRunningLogCumulation.RunningType.marathon)
			{
				return 0.0f;
			}

			if( goal == 0)
			{
				return 0.0f;
			}	

			float ratio = 0;

			// 목표 (분)
			if (running_sub_type == ClientRunningLogCumulation.MarathonType._free_time)
			{
				ratio = (float)running_time / ((float)goal * 60);
			}
			// 목표 (미터)
			else
			{
				ratio = (float)(distance / ((double)goal / 1000.0));
			}

			return Mathf.Clamp(ratio, 0, 1);
		}

	}
}
