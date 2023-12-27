namespace DRun.Client.ViewModel
{
	public class ClaimedWeeklyRewardData
	{
		public int week_id;
		public long reward_amount;

		public static ClaimedWeeklyRewardData create(int week_id,long reward_amount)
		{
			ClaimedWeeklyRewardData data = new ClaimedWeeklyRewardData();
			data.week_id = week_id;
			data.reward_amount = reward_amount;
			return data;
		}
	}
}
