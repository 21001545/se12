using DRun.Client.ViewModel;
using Festa.Client.Module;
using System;
using UnityEngine;

namespace DRun.Client.Running
{
	public class LocalRunningStatusData
	{
		public const int VERSION = 3;

		public int running_type;
		public int running_sub_type;
		public int running_id;
		public int goal;
		public int status;
		public long startTime;
		public long gpsAvailableTime;
		public long trackingStartTime;
		public long lastGPSQueryTime;
		public double weight;
		public int nft_token_id;
		public int path_count;
	}
}
