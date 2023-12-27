//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Festa.Client.Module;
//using Festa.Client.NetData;
//using Festa.Client.RefData;

//namespace Festa.Client.ViewModel
//{
//	public class StepRewardViewModel : AbstractViewModel
//	{
//		private ObservableList<ClientStepReward> _stepRewardList;
//		private ClientDailyBoost _daily_boost;
//		private ObservableDictionary<int, ClientWalkLevel> _walkLevelDic;
//		private ClientWalkLevel _CurrentWalkLevel;
//		private int _dailyMaxStep;

//		public ObservableList<ClientStepReward> StepRewardList
//		{
//			get
//			{
//				return _stepRewardList;
//			}
//			//set
//			//{
//			//	_stepRewardList = value;
//			//	notifyPropetyChanged("StepRewardList");
//			//}
//		}

//		public ObservableDictionary<int,ClientWalkLevel> WalkLevelDic
//		{
//			get
//			{
//				return _walkLevelDic;
//			}
//		}

//		public ClientDailyBoost DailyBoost
//		{
//			get
//			{
//				return _daily_boost;
//			}
//			set
//			{
//				Set(ref _daily_boost, value);
//			}
//		}

//		public ClientWalkLevel CurrentWalkLevel
//		{
//			get
//			{
//				return _CurrentWalkLevel;
//			}
//			set
//			{
//				Set(ref _CurrentWalkLevel, value);
//			}
//		}

//		public int DailyMaxStep
//		{
//			get
//			{
//				return _dailyMaxStep;
//			}
//			set
//			{
//				Set(ref _dailyMaxStep, value);
//			}
//		}

//		public static StepRewardViewModel create()
//		{
//			StepRewardViewModel vm = new StepRewardViewModel();
//			vm.init();
//			return vm;
//		}

//		protected override void init()
//		{
//			base.init();
//			_stepRewardList = new ObservableList<ClientStepReward>();
//			_walkLevelDic = new ObservableDictionary<int, ClientWalkLevel>();
//			_CurrentWalkLevel = null;
//		}

//		public void setWalkLevel(List<ClientWalkLevel> list)
//		{
//			ClientWalkLevel new_current_walk_level = null;

//			foreach(ClientWalkLevel level in list)
//			{
//				_walkLevelDic.put(level.level, level);
			
//				if( level.status == ClientWalkLevel.StatusType.active_not_subscribe ||
//					level.status == ClientWalkLevel.StatusType.active_subscribe)
//				{
//					new_current_walk_level = level;
//				}
//			}

//			CurrentWalkLevel = new_current_walk_level;
//			calcDailyMaxStep();
//		}

//		public ClientWalkLevel getWalkLevel(int level)
//		{
//			return _walkLevelDic.get(level);
//		}

//		private void calcDailyMaxStep()
//		{
//			RefWalkLevel ref_walk_level = GlobalRefDataContainer.getInstance().get<RefWalkLevel>(_CurrentWalkLevel.level);
//			DailyMaxStep = ref_walk_level.daily_max_steps;
//		}
//	}
//}
