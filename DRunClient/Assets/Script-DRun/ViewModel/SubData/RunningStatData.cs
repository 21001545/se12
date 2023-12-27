//using DRun.Client.NetData;
//using DRun.Client.RefData;
//using Festa.Client;
//using Festa.Client.Module;
//using Festa.Client.Module.MsgPack;
//using System;
//using UnityEngine;

//namespace DRun.Client.ViewModel
//{
//	public class RunningStatData
//	{
//		public int nft_token_id;
//		public int level;
//		public int grade;

//		public int heart;
//		public int distance;

//		public int bonus_heart;
//		public int bonus_distance;

//		public int stamina;
//		public long day_id;

//		public double last_distance;
//		public double last_time;
//		public long last_day_id;

//		public int used_heart;
//		public int used_distance;
//		public int used_stamina;

//		public int staminaEfficiency;

//		[SerializeOption(SerializeOption.NONE)]
//		public ClientNFTItem _nftItem;

//		[SerializeOption(SerializeOption.NONE)]
//		public ClientNFTBonus _nftBonus;

//		public void resetForStart(ClientNFTItem nftItem,ClientNFTBonus nftBonus)
//		{
//			if (nftItem != null)
//			{
//				_nftItem = nftItem;
//				_nftBonus = nftBonus;
//				nft_token_id = nftItem.token_id;

//				level = nftItem.level;
//				grade = nftItem.grade;

//				heart = nftItem.heart;
//				distance = nftItem.distance;
//				stamina = nftItem.stamina;
//				day_id = (int)TimeUtil.dayCountFromDate(DateTime.UtcNow, TimeUtil.timezoneOffset());

//				bonus_heart = nftBonus.heart;
//				bonus_distance = nftBonus.distance;

//				last_day_id = day_id;
//				last_distance = 0;
//				last_time = 0;

//				used_heart = 0;
//				used_distance = 0;
//				used_stamina = 0;
//				staminaEfficiency = 0;

//				calcStaminaEfficiency();
//			}
//			else
//			{
//				_nftItem = null;
//				_nftBonus = null;

//				nft_token_id = 0;

//				level = 0;
//				grade = 0;

//				heart = 0;
//				distance = 0;
//				stamina = 0;
//				day_id = (int)TimeUtil.dayCountFromDate(DateTime.UtcNow, TimeUtil.timezoneOffset());

//				last_day_id = day_id;
//				last_distance = 0;
//				last_time = 0;

//				used_heart = 0;
//				used_distance = 0;
//				used_stamina = 0;
//				staminaEfficiency = 0;
//			}
//		}

//		public void processRefill(long new_day_id)
//		{
//			if( _nftItem.max_heart > 0)
//			{
//				heart += _nftItem.max_heart;
//			}

//			if( _nftBonus.max_heart > 0)
//			{
//				bonus_heart += _nftBonus.max_heart;
//			}

//			distance += _nftItem.max_distance;
//			last_day_id = new_day_id;
//		}

//		public void reduceDistance(int distanceDelta)
//		{
//			if( distanceDelta <= 0)
//			{
//				return;
//			}

//			int remainDelta = distanceDelta;

//			// 보너스 먼저 소비
//			if( bonus_distance > 0)
//			{
//				if(remainDelta > bonus_distance)
//				{
//					used_distance += bonus_distance;

//					remainDelta -= bonus_distance;
//					bonus_distance = 0;
//				}
//				else
//				{
//					used_distance += remainDelta;

//					bonus_distance -= remainDelta;
//					remainDelta = 0;
//				}
//			}

//			// 스탯 소비
//			if(remainDelta >= 0)
//			{
//				if(remainDelta > distance)
//				{
//					remainDelta = distance;
//				}

//				distance -= remainDelta;
//				used_distance-= remainDelta;
//			}

			
//		}

//		public void reduceHeart(int heartReduce)
//		{
//			if( heartReduce <= 0 || _nftItem.max_heart <= 0)
//			{
//				return;
//			}
//		}

//		private void calcStaminaEfficiency()
//		{
//			int ratio = (int)(getStaminaRatio() * 100);

//			staminaEfficiency = GlobalRefDataContainer.getRefDataHelper().getNFTStaminaEfficiencyDRN(ratio);

//			Debug.Log($"stamina efficiency: ratio[{ratio}] efficiency[{staminaEfficiency}]");
//		}

//		public float getDistanceRatio()
//		{
//			if (_nftItem == null)
//				return 0;

//			return (float)distance / (float)_nftItem.max_distance;
//		}

//		public float getHeartRatio()
//		{
//			if (_nftItem == null)
//				return 0;

//			if (_nftItem.max_heart == 0)
//			{
//				return 1.0f;
//			}

//			return (float)heart / (float)_nftItem.max_heart;
//		}

//		public float getStaminaRatio()
//		{
//			if (_nftItem == null)
//				return 0;

//			RefNFTGrade refGrade = GlobalRefDataContainer.getRefDataHelper().getNFTGrade(grade);
//			return (float)stamina / ((float)refGrade.stamina * 100.0f);
//		}
		
//		public bool isMinable()
//		{
//			if (_nftItem == null)
//				return false;

//			if ( distance <= 0)
//			{
//				return false;
//			}

//			if( _nftItem.max_heart > 0 && heart == 0)
//			{
//				return false;
//			}

//			return true;
//		}


//	}
//}
