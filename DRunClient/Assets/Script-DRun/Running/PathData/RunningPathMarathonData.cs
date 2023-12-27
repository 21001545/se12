using DRun.Client.NetData;

namespace DRun.Client.Running
{
	// 경로 시작 시점의 누적 거리
	public class RunningPathMarathonData : RunningPathModeData
	{
		public int runningSubType;
		public int goal;

		public double begin_distance;		// km
		public double begin_time;           // 초

		public double end_distance;
		public double end_time;

		public override void printDebugInfo()
		{

		}

		public static RunningPathMarathonData create(int subType,int goal)
		{
			RunningPathMarathonData data = new RunningPathMarathonData();
			data.runningSubType = subType;
			data.goal = goal;
			data.begin_distance = 0;
			data.begin_time = 0;
			data.end_distance = 0;
			data.end_time = 0;

			return data;
		}

		public static RunningPathMarathonData createContinue(RunningPathMarathonData from)
		{
			RunningPathMarathonData data = new RunningPathMarathonData();
			data.runningSubType  = from.runningSubType;
			data.goal = from.goal;
			data.begin_distance = from.end_distance;
			data.begin_time = from.begin_time;
			data.end_distance = data.begin_distance;
			data.end_time = data.begin_time;
			return data;
		}

		public override RunningPathModeData createContinue()
		{
			return createContinue(this);
		}

		public override RunningPathModeData clone()
		{
			RunningPathMarathonData data = new RunningPathMarathonData();
			data.runningSubType = runningSubType;
			data.goal = goal;
			data.begin_distance = begin_distance;
			data.begin_time = begin_time;
			data.end_distance = end_distance;
			data.end_time = end_time;
			return data;
		}

		public bool isTimeGoal()
		{
			return runningSubType == ClientRunningLogCumulation.MarathonType._free_time;
		}

		//초
		public double getTimeGoal()
		{
			return (double)goal * 60.0;
		}

		//KM
		public double getDistanceGoal()
		{
			return (double)goal / 1000.0;
		}

		public bool isGoalComplete()
		{
			if( isTimeGoal())
			{
				return end_time >= getTimeGoal();
			}
			else
			{
				return end_distance >= getDistanceGoal();
			}
		}
	}
}
