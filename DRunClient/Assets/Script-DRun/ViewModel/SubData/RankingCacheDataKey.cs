namespace DRun.Client.ViewModel
{
	public struct RankingCacheDataKey
	{
		public int mode_type;
		public int mode_sub_type;
		public int time_type;
		public long time_id;

		public override int GetHashCode()
		{
			return mode_type * 10 + time_type;
		}

		public override bool Equals(object obj)
		{
			return Equals((RankingCacheDataKey)obj);
		}

		public bool Equals(RankingCacheDataKey key)
		{
			return mode_type == key.mode_type &&
				mode_sub_type == key.mode_sub_type &&
				time_type == key.time_type &&
				time_id == key.time_id;
		}

		public static RankingCacheDataKey create(int mode_type,int mode_sub_type,int time_type,long time_id)
		{
			RankingCacheDataKey key = new RankingCacheDataKey();
			key.mode_type = mode_type;
			key.mode_sub_type = mode_sub_type;
			key.time_type = time_type;
			key.time_id = time_id;
			return key;
		}
	}
}
