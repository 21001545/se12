using Festa.Client.Module;
using Festa.Client.RefData;

namespace Festa.Client.Logic
{
	public static class SocialScoreLogic
	{
		public static int getNowSeasonID()
		{
			return getSeasonID(TimeUtil.unixTimestampUtcNow());
		}

		public static int getSeasonID(long time)
		{
			long config_first_time = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Social.social_score_season_first_time, 6060) * TimeUtil.msMinute;
			long config_period = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Social.social_score_season_period, 10080) * TimeUtil.msMinute;
			return (int)((time - config_first_time) / config_period);
		}

		public static int reductScore(int season_id, int score, int current_season_id)
		{
			int config_reduction_rate = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Social.social_score_season_reduction_rate, 50);
			int season_step = current_season_id - season_id;

			for (int i = 0; i < season_step; ++i)
			{
				score = score * config_reduction_rate / 100;
			}

			return score;
		}
	}
}
