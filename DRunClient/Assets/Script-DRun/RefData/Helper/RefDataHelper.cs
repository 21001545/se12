using Festa.Client;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client.RefData
{
	public class RefDataHelper
	{
		private double _peb2drn;
		private double _drn2peb;
		private RefDataContainer _refDataContainer;
		private List<RefNFTStaminaEfficiency> _nftStaminaEfficiencyList;
		private List<RefMETs> _metsList;

		public static RefDataHelper create(RefDataContainer refDataContainer)
		{
			RefDataHelper helper = new RefDataHelper();
			helper.init(refDataContainer);
			return helper;
		}

		private void init(RefDataContainer refDataContainer)
		{
			_refDataContainer = refDataContainer;

			int configPebToDRN = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.peb_to_drn, 8);
			_peb2drn = 1.0f / Math.Pow(10, configPebToDRN);
			_drn2peb = Math.Pow(10, configPebToDRN);

			createNFTStaminaEfficiencyList();
			createMETsList();

			int[] test_value = new int[]
			{
				0, 1, 25, 60, 80, 100
			};

			foreach(int value in test_value)
			{
				Debug.Log($"efficiency: ratio[{value}] effciency[{getNFTStaminaEfficiencyDRN(value)}] ");
			}
		}

		private void createNFTStaminaEfficiencyList()
		{
			_nftStaminaEfficiencyList = new List<RefNFTStaminaEfficiency>();
			Dictionary<int, Festa.Client.RefData.RefData> dic = _refDataContainer.getMap<RefNFTStaminaEfficiency>();
			foreach(KeyValuePair<int,Festa.Client.RefData.RefData> item in dic)
			{
				if( item.Value != null)
				{
					_nftStaminaEfficiencyList.Add((RefNFTStaminaEfficiency)item.Value);
				}
			}
		}

		private void createMETsList()
		{
			_metsList = new List<RefMETs>();
			Dictionary<int, Festa.Client.RefData.RefData> dic = _refDataContainer.getMap<RefMETs>();
			foreach(KeyValuePair<int,Festa.Client.RefData.RefData> item in dic)
			{
				_metsList.Add(item.Value as RefMETs);
			}

			_metsList.Sort((a, b) => {
				if( a.speed_min < b.speed_min)
				{
					return -1;
				}
				else if( a.speed_min > b.speed_min)
				{
					return 1;
				}

				return 0;
			});
		}

		public long toPeb(double drn)
		{
			return (long)(drn * _drn2peb);
		}

		public double toDRN(long peb)
		{
			return peb * _peb2drn;
		}

		public long toPeb(string drn_string)
		{	
			try
			{
				double drn = double.Parse(drn_string);
				return toPeb(drn);
			}
			catch(Exception e)
			{
				Debug.LogException(e);
				return 0;
			}
		}

		public RefNFTGrade getNFTGrade(int grade)
		{
			return _refDataContainer.get<RefNFTGrade>(grade);
		}

		public int getNFTStaminaEfficiencyDRN(int staminaRatio)
		{
			foreach(RefNFTStaminaEfficiency item in _nftStaminaEfficiencyList)
			{
				if( item.check( staminaRatio))
				{
					return item.efficiency_DRN;
				}
			}

			return 0;
		}

		// weight = kg
		// return kcal
		public double calcCalories(int trip_type, double speed, double weight, double duration_minutes)
		{
			if (speed == 0)
			{
				return 0;
			}

			RefMETs ref_mets = findMETS(trip_type, speed);
			if (ref_mets == null)
			{
				return 0;
			}

			double mets = ref_mets.mets;
			double oxygen = (3.5 * weight * duration_minutes) * mets;
			double calories = oxygen / 1000.0 * 5;

			return calories;
		}

		public RefMETs findMETS(int trip_type, double speed)
		{
			foreach(RefMETs ref_mets in _metsList)
			{
				if (speed >= ref_mets.speed_min && speed < ref_mets.speed_max)
				{
					return ref_mets;
				}
			}

			Debug.LogWarning($"can't find METs:trip_type[{trip_type}] speed[{speed}]");
			return null;
		}

		public double calcStaminaCost(int nftGrade, int refllAmount)
        {
            if (refllAmount == 0)
                return 0;

			RefNFTGrade refGrade = getNFTGrade(nftGrade);

			int staminaMax = refGrade.stamina * 100;

			// 충전되는 percent만큼으로 계산
			double configCostRate = GlobalRefDataContainer.getConfigDouble(RefConfig.Key.DRun.stamina_RecoveryCostRate, 0.1);
			double refillAmountPercent = (double)refllAmount * 100.0 / (double)staminaMax;

			double costDRN = configCostRate * refillAmountPercent;

			return costDRN;
		}

		// 현재 레벨
		public double calcBoostExpCost(int nftLevel, int boostAmount)
		{
			if (boostAmount == 0)
				return 0;

			RefNFTLevel refLevel = GlobalRefDataContainer.getInstance().get<RefNFTLevel>(nftLevel + 1);
			if (refLevel == null)
			{
				return 0;
			}

			int max_exp = refLevel.required_disance * 1000;

			long boost_drnt_peb = toPeb(refLevel.boost_drnt);
			long costPeb = boostAmount * boost_drnt_peb / max_exp;

			return toDRN(costPeb);
		}
	}
}
