using System;

namespace DRun.Client.NetData
{
	public class ClientNFTBonus
	{
		public int nft_count;
		public int nft_item_id;
		public int heart;
		public int max_heart;
		public int distance;
		public int max_distance;
		public DateTime nft_count_check_time;
		public DateTime next_refill_time;

		public bool hasBonus()
		{
			return max_heart > 0 || max_distance > 0;
		}
	}
}
