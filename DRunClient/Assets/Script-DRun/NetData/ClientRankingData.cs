using Festa.Client;
using Festa.Client.Module.MsgPack;

namespace DRun.Client.NetData
{
	public class ClientRankingData
	{
		public int account_id;
		public int mode_type;
		public int mode_sub_type;
		public int time_type;
		public long time_id;
		public long score;
		public int rank;
		public int prev_rank;

		public static class ModeType
		{
			public const int step = 100;
			public const int promode = 1;
			public const int marathon = 2;
		}

		[SerializeOption(SerializeOption.NONE)]
		public ClientProfileCache _profileCache;

		public ClientRankingData testCopy()
		{
			ClientRankingData data = new ClientRankingData();
			data.account_id = account_id;
			data.mode_type = mode_type;
			data.mode_sub_type = mode_sub_type;
			data.time_type = time_type;
			data.time_id = time_id;
			data.score = score;
			data.prev_rank = data.rank = rank + 1;
			data._profileCache = _profileCache;
			return data;
		}
	}
}
