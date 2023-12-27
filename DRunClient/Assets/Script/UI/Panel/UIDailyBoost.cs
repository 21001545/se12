using System.Collections.Generic;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace Festa.Client
{
	public class UIDailyBoost : UISingletonPanel<UIDailyBoost>
	{
		public Transform boostCountRoot;
		public UIDailyBoostUsedItem boostCountSource;
		public UnityEngine.UI.Button btn_startBoost;
		public TMP_Text txt_startBoost;

		private List<UIDailyBoostUsedItem> _boostCountList;
		private IntervalTimer _updateTimer;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_boostCountList = new List<UIDailyBoostUsedItem>();
			boostCountSource.gameObject.SetActive(false);

			_updateTimer = IntervalTimer.create(1.0f, true, false);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();
			base.open(param, transitionType, closeType);
		}

		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			initBoostCountList();

			ClientViewModel vm = ClientMain.instance.getViewModel();
			DailyBoostViewModel boost_vm = vm.DailyBoost;

			_bindingManager.makeBinding(boost_vm, nameof(boost_vm.DailyBoost), updateDailyBoost);
		}

		private void initBoostCountList()
		{
			RefConfig config_daily_count = GlobalRefDataContainer.getInstance().get<RefConfig>(RefConfig.Key.DailyBoost.daily_count);

			for(int i = 0; i < config_daily_count.getInteger(); ++i)
			{
				UIDailyBoostUsedItem item = boostCountSource.make<UIDailyBoostUsedItem>(boostCountRoot, GameObjectCacheType.ui);
				_boostCountList.Add(item);
			}
		}

		public void onClickStartBoost()
		{
			//UIBackNavigation.getInstance().close();
			//UIMap.getInstance().open();
			UIMainTab.getInstance().changeTab(UIMainTab.Tab.map);
			UIMap.getInstance().startTrip();
		}

		public override void update()
		{
			base.update();
			if(gameObject.activeSelf && _updateTimer.update())
			{
				updateButtonCoolTime();
			}
		}

		private void updateDailyBoost(object obj)
		{
			RefConfig config_daily_count = GlobalRefDataContainer.getInstance().get<RefConfig>(RefConfig.Key.DailyBoost.daily_count);
			RefConfig config_cool_time = GlobalRefDataContainer.getInstance().get<RefConfig>(RefConfig.Key.DailyBoost.cool_time);

			ClientDailyBoost boost = (ClientDailyBoost)obj;

			int remain_count = getRemainCount(config_cool_time, config_daily_count, boost);

			for (int i = 0; i < _boostCountList.Count; ++i)
			{
				UIDailyBoostUsedItem item = _boostCountList[ i];
				if( i < remain_count)
				{
					item.image_icon.color = Color.blue;
				}
				else
				{
					item.image_icon.color = Color.gray;
				}
			}

			updateButtonCoolTime();
		}

		private void updateButtonCoolTime()
		{
			RefConfig config_cool_time = GlobalRefDataContainer.getInstance().get<RefConfig>(RefConfig.Key.DailyBoost.cool_time);
			RefConfig config_daily_count = GlobalRefDataContainer.getInstance().get<RefConfig>(RefConfig.Key.DailyBoost.daily_count);
	
			ClientDailyBoost boost_config = ClientMain.instance.getViewModel().DailyBoost.DailyBoost;
			
			// 이미 켜져 있는 상태
			if( boost_config.status == ClientDailyBoost.StatusType.on)
			{
				btn_startBoost.interactable = false;
				txt_startBoost.text = "Boost Running...";
				return;
			}
			else if( boost_config.status == ClientDailyBoost.StatusType.off)
			{
				TimeSpan remain_time;
				if (canTurnOnBoost(config_cool_time, config_daily_count, boost_config, out remain_time))
				{
					btn_startBoost.interactable = true;
					txt_startBoost.text = "Start Boost";
					return;
				}
				else
				{
					btn_startBoost.interactable = false;
					txt_startBoost.text = UIFormatter.timePeroid(remain_time) + " left";
				}
			}
		}

		private int getRemainCount(RefConfig config_cool_time, RefConfig config_daily_count, ClientDailyBoost boost_config)
		{
			// 하지만 마지막 사용날짜가 어제이면 아직 충전이 않된 상태니까 사용가능
			if (DateTime.Now.Day > boost_config.last_used_time.ToLocalTime().Day)
			{
				return config_daily_count.getInteger();
			}

			return config_daily_count.getInteger() - boost_config.used_count;
		}

		public static bool canTurnOnBoost(RefConfig config_cool_time,RefConfig config_daily_count,ClientDailyBoost boost_config,out TimeSpan remain_time)
		{
			// 일단 쿨타임 체크
			DateTime cool_time_end = boost_config.last_used_time.AddMinutes(config_cool_time.getInteger());
			remain_time = cool_time_end - DateTime.UtcNow;
			if( remain_time.TotalSeconds <= 0)
			{
				return true;
			}

			if( boost_config.used_count >= config_daily_count.getInteger())
			{
				// 금일 횟수 초과 ?

				// 하지만 마지막 사용날짜가 어제이면 아직 충전이 않된 상태니까 사용가능
				if( DateTime.Now.Day > boost_config.last_used_time.ToLocalTime().Day)
				{
					remain_time = new TimeSpan();
					return true;
				}
				else
				{
					remain_time = TimeUtil.tommorowBeginUTC() - DateTime.UtcNow;
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		//public static int remainBoostCount(RefConfig config_cool_time, RefConfig config_daily_count, ClientDailyBoost boost_config)
		//{

		//}
	}
}
