using DRun.Client.NetData;
using Festa.Client.Module.MsgPack;
using UnityEngine;

namespace DRun.Client.Running
{
	// 경로 시작 시점의 ProMode Stat데이터
	public class RunningPathStatData : RunningPathModeData
	{
		[SerializeOption(SerializeOption.NONE)]
		public ClientNFTItem _nftItem;
		[SerializeOption(SerializeOption.NONE)]
		public ClientNFTBonus _nftBonus;

		public int begin_heart;
		public int begin_distance;
		public int begin_stamina;

		public int begin_bonus_heart;
		public int begin_bonus_distance;

		public int end_heart;
		public int end_distance;
		public int end_stamina;

		public int end_bonus_heart;
		public int end_bonus_distance;

		public double begin_remain_distance;
		public double end_remain_distance;

		public double begin_remain_time;
		public double end_remain_time;

		public override void printDebugInfo()
		{
			Debug.Log($"heart[{begin_heart},{end_heart}] distance[{begin_distance},{end_distance}]");
			Debug.Log($"bonus_heart[{begin_bonus_heart}] bonus_distance[{begin_bonus_distance},{end_bonus_distance}]");
			Debug.Log($"remain_time[{begin_remain_time},{end_remain_time}] remain_distance[{begin_remain_distance},{end_remain_distance}]");
		}

		public static RunningPathStatData create(ClientNFTItem nftItem,ClientNFTBonus nftBonus)
		{
			RunningPathStatData data = new RunningPathStatData();
			data._nftItem = nftItem;
			data._nftBonus = nftBonus;
			data.begin_heart = nftItem.heart;
			data.begin_distance = nftItem.distance;
			data.begin_stamina = nftItem.stamina;
			data.begin_bonus_heart = nftBonus.heart;
			data.begin_bonus_distance = nftBonus.distance;
			data.begin_remain_distance = 0;
			data.begin_remain_time = 0;
			data.resetEnd();
			return data;
		}

		public static RunningPathStatData createConitnue(RunningPathStatData from)
		{
			RunningPathStatData data = new RunningPathStatData();
			data._nftItem = from._nftItem;
			data._nftBonus = from._nftBonus;
			data.begin_heart = from.end_heart;
			data.begin_distance = from.end_distance;
			data.begin_stamina = from.end_stamina;
			data.begin_bonus_heart = from.end_bonus_heart;
			data.begin_bonus_distance = from.end_bonus_distance;
			data.begin_remain_distance = from.end_remain_distance;
			data.begin_remain_time = from.end_remain_time;
			data.resetEnd();
			return data;
		}

		public override RunningPathModeData createContinue()
		{
			return createConitnue(this);
		}

		public override RunningPathModeData clone()
		{
			RunningPathStatData data = new RunningPathStatData();
			data._nftItem = _nftItem;
			data._nftBonus = _nftBonus;
			data.begin_heart = begin_heart;
			data.begin_distance = begin_distance;
			data.begin_stamina = begin_stamina;
			data.begin_bonus_heart = begin_bonus_heart;
			data.begin_bonus_distance = begin_bonus_distance;
			data.begin_remain_distance = begin_remain_distance;
			data.begin_remain_time = begin_remain_time;
			
			data.end_heart = end_heart;
			data.end_distance = end_distance;
			data.end_stamina = end_stamina;
			data.end_bonus_heart = end_bonus_heart;
			data.end_bonus_distance = end_bonus_distance;
			data.end_remain_distance = end_remain_distance;
			data.end_remain_time = end_remain_time;

			return data;
		}

		public bool isUnlimitHeart()
		{
			return _nftItem.max_heart == 0;
		}

		public void resetEnd()
		{
			end_heart = begin_heart;
			end_distance = begin_distance;
			end_stamina = begin_stamina;

			end_bonus_heart = begin_bonus_heart;
			end_bonus_distance = begin_bonus_distance;

			end_remain_distance = begin_remain_distance;
			end_remain_time = begin_remain_time;
		}

		public void consumeHeart(int heart)
		{
			if( isUnlimitHeart())
			{
				return;
			}

			if( heart <= end_bonus_heart)
			{
				end_bonus_heart -= heart;
				return;
			}

			heart -= end_bonus_heart;
			end_bonus_heart = 0;

			if( heart > end_heart)
			{
				end_heart = 0;
			}
			else
			{
				end_heart -= heart;
			}
		}

		public void consumeDistance(int distance)
		{
			if( distance <= end_bonus_distance)
			{
				end_bonus_distance -= distance;
				return;
			}

			distance -= end_bonus_distance;
			end_bonus_distance = 0;

			if( distance > end_distance)
			{
				end_distance = 0;
			}
			else
			{
				end_distance -= distance;
			}
		}

		public bool isMinable()
		{
			if( _nftItem == null)
			{
				return false;
			}

			// heart 부족
			if (isUnlimitHeart() == false && end_heart <= 0)
			{
				return false;
			}
			
			// distance 부족
			if( end_distance <= 0)
			{
				return false;
			}

			return true;
		}
	}
}
