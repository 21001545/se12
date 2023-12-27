using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module;
using Festa.Client.NetData;
using Festa.Client.RefData;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class HealthViewModel : AbstractViewModel
	{
		private int	_todayStepCount;
		private int _todayStepGoalCount;
		private int _todayCalorie;
		private int _todayWalkDistance;
		private ClientHealthLogCumulation _stepCumulation;
		private ClientHealthLogCumulation _distanceCumulation;
		private ClientHealthLogCumulation _calorieCumulation;
		private ClientBody _body;

		private HealthTodayTotalRankViewModel _todayStepTotalRank;
		private HealthTodayFriendRankViewModel _todayStepFriendRank;

		public int TodayStepCount
		{
			get
			{
				return _todayStepCount;
			}
			set
			{
				Set<int>(ref _todayStepCount, value);
			}
		}

		public int TodayStepGoalCount
		{
			get
			{
				return _todayStepGoalCount;
			}
			set
			{
				Set(ref _todayStepGoalCount, value);
			}
		}

		// 단위 : cal
		public int TodayCalorie
        {
			get
            {
				return _todayCalorie;
            }
			set
            {
                Set<int>(ref _todayCalorie, value);
            }

        }

		// 단위 : 미터
        public int TodayWalkDistance
        {
            get
            {
                return _todayWalkDistance;
            }
            set
            {
                Set<int>(ref _todayWalkDistance, value);
            }

        }

        public ClientHealthLogCumulation StepCumulation
		{
			get
			{
				return _stepCumulation;
			}
			set
			{
				Set(ref _stepCumulation, value);
			}
		}

		public ClientHealthLogCumulation DistanceCumulation
		{
			get
			{
				return _distanceCumulation;
			}
			set
			{
				Set(ref _distanceCumulation, value);
			}
		}

		public double getTotalCumulatedDistance_mi ()
        {
			return _distanceCumulation.total / 1.609344f;
        }

		public ClientHealthLogCumulation CalorieCumulation
		{
			get
			{
				return _calorieCumulation;
			}
			set
			{
				Set(ref _calorieCumulation, value);
			}
		}

		public ClientBody Body
		{
			get
			{
				return _body;
			}
			set
			{
				Set(ref _body, value);
			}
		}

		public HealthTodayFriendRankViewModel TodayStepFriendRank => _todayStepFriendRank;
		public HealthTodayTotalRankViewModel TodayStepTotalRank => _todayStepTotalRank;

		public static HealthViewModel create()
		{
			HealthViewModel vm = new HealthViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();
			_todayStepTotalRank = HealthTodayTotalRankViewModel.create();
			_todayStepFriendRank = HealthTodayFriendRankViewModel.create();

			_todayStepCount = -1;
			_todayStepGoalCount = 0;
			_todayCalorie = 0;
			_todayWalkDistance = 0;
		}

		public void updateCumulation(Dictionary<int,ClientHealthLogCumulation> dic)
		{
			foreach(KeyValuePair<int,ClientHealthLogCumulation> item in dic)
			{
				if( item.Key == HealthDataType.step)
				{
					StepCumulation = item.Value;
					TodayStepCount = (int)item.Value.today_total;
					TodayStepGoalCount = (int)item.Value.today_goal;
				}
				else if( item.Key == HealthDataType.distance)
				{
					DistanceCumulation = item.Value;
					TodayWalkDistance = (int)_distanceCumulation.today_total;
				}
				else if( item.Key == HealthDataType.calories)
				{
					CalorieCumulation = item.Value;
					TodayCalorie = (int)_calorieCumulation.today_total;
				}
			}
		}

		// weight = kg
		// return kcal
		public double calcCalories(int trip_type, double speed, double weight,double duration_minutes)
		{
			if( speed == 0)
			{
				return 0;
			}

			RefMETs ref_mets = findMETS(trip_type, speed);
			if( ref_mets == null)
			{
				return 0;
			}

			double mets = ref_mets.mets;
			double oxygen = (3.5 * weight * duration_minutes) * mets;
			double calories = oxygen / 1000.0 * 5;

//			Debug.Log($"calories:[{calories}]kcal trip_type:[{trip_type}] mets:[{mets}] speed:[{speed}]km/h weight:[{weight}]kg duration[{duration_minutes}]min");

			return calories;
		}

		public RefMETs findMETS(int trip_type,double speed)
		{
			Dictionary<int,RefData.RefData> dic = GlobalRefDataContainer.getInstance().getMap<RefMETs>();
			foreach(KeyValuePair<int,RefData.RefData> item in dic)
			{
				RefMETs ref_mets = item.Value as RefMETs;

				//if( ref_mets.acitivity_type != trip_type)
				//{
				//	continue;
				//}

				if(speed >= ref_mets.speed_min && speed < ref_mets.speed_max)
				{
					return ref_mets;
				}
			}

			Debug.LogError($"can't find METs:trip_type[{trip_type}] speed[{speed}]");
			return null;
		}
	}
}
