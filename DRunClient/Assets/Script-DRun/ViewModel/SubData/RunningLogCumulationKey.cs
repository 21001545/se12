using DRun.Client.NetData;

namespace DRun.Client.ViewModel
{
	public struct RunningLogCumulationKey
	{
		public int running_type;
		public int running_sub_type;
		public int type;
		public long id;

		public override int GetHashCode()
		{
			return running_type * 10 + type;
		}

		public override bool Equals(object obj)
		{
			return Equals((RunningLogCumulationKey)obj);
		}

		public bool Equals(RunningLogCumulationKey key)
		{
			return running_type == key.running_type &&
				running_sub_type == key.running_sub_type &&
				type == key.type &&
				id == key.id;
		}

		public static RunningLogCumulationKey create(ClientRunningLogCumulation log)
		{
			RunningLogCumulationKey key = new RunningLogCumulationKey();
			key.running_type = log.running_type;
			key.running_sub_type = log.running_sub_type;
			key.type = log.type;
			key.id = log.id;
			return key;
		}

		public static RunningLogCumulationKey create(int running_type,int running_sub_type,int type,long id)
		{
			RunningLogCumulationKey key = new RunningLogCumulationKey();
			key.running_type = running_type;
			key.running_sub_type = running_sub_type;
			key.type = type;
			key.id = id;
			return key;
		}
	}
}
