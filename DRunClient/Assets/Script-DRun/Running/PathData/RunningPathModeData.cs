namespace DRun.Client.Running
{
	public abstract class RunningPathModeData
	{
		public abstract RunningPathModeData createContinue();
		public abstract RunningPathModeData clone();
		public abstract void printDebugInfo();
	}
}
