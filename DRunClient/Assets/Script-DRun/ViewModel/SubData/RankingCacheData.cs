using DRun.Client.NetData;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.ViewModel
{
	public class RankingCacheData
	{
		private List<ClientRankingData> _rankingList;
		private ClientRankingData _myRankingData;

		private Vector2Int _rankRange;

		public List<ClientRankingData> getRankingList()
		{
			return _rankingList;
		}

		public ClientRankingData getMyRankingData()
		{
			return _myRankingData;
		}

		public Vector2Int getRankRange()
		{
			return _rankRange;
		}

		public static RankingCacheData create(List<ClientRankingData> list,ClientRankingData myRankingData)
		{
			RankingCacheData rankingCacheData = new RankingCacheData();
			rankingCacheData.init(list, myRankingData);
			return rankingCacheData;
		}

		private void init(List<ClientRankingData> list,ClientRankingData myRankingData)
		{
			_rankingList = list;
			_myRankingData = myRankingData;

			updateRankRange();
		}

		public void append(RankingCacheData data)
		{
			_rankingList.AddRange(data.getRankingList());

			updateRankRange();
		}

		private void updateRankRange()
		{
			_rankRange = Vector2Int.zero;
			for (int i = 0; i < _rankingList.Count; ++i)
			{
				ClientRankingData data = _rankingList[i];
				if (i == 0)
				{
					_rankRange.x = data.rank;
					_rankRange.y = data.rank;
				}
				else
				{
					_rankRange.x = System.Math.Min(data.rank, _rankRange.x);
					_rankRange.y = System.Math.Max(data.rank, _rankRange.y);
				}
			}
		}
	}
}
