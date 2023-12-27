namespace DRun.Client.ViewModel
{
	public struct RunningLogCacheKey
	{
		public int running_type;
		public int running_id;

		public override int GetHashCode()
		{
			return running_type;
		}

		public override bool Equals(object obj)
		{
			return base.Equals((RunningLogCacheKey)obj);
		}

		public bool Equals(RunningLogCacheKey key)
		{
			return running_type == key.running_type &&
				running_id == key.running_id;
		}

		public static RunningLogCacheKey create(int running_type,int running_id)
		{
			RunningLogCacheKey key = new RunningLogCacheKey();
			key.running_type = running_type;
			key.running_id = running_id;
			return key;
		}

	}
}
