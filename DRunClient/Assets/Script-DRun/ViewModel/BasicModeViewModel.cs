using DRun.Client.NetData;
using DRun.Client.RefData;
using Festa.Client;
using Festa.Client.MapBox;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Festa.Client.ViewModel;

namespace DRun.Client.ViewModel
{
	public class BasicModeViewModel : AbstractViewModel
	{
		private ClientBasicMode _basicMode;
		private Dictionary<int, ClientBasicDailyStep> _dailyStepMap;
		private Dictionary<int, ClientBasicDailyRewardSlot> _dailyRewardSlotMap;
		private Dictionary<int, ClientBasicWeeklyReward> _weeklyRewardMap;
		private ClaimedWeeklyRewardData _claimedWeeklyRewardData;

		private EntryExpChangeDeferrer _expDrnChangeDeferrer;

		public ClientBasicMode LevelData
		{
			get
			{
				return _basicMode;
			}
			set
			{
				Set(ref _basicMode, value);
			}
		}

		public int TodayStepCount
		{
			get
			{
				int day_id = (int)TimeUtil.todayDayCount();
				ClientBasicDailyStep step;
				if( _dailyStepMap.TryGetValue(day_id, out step) == false)
				{
					return 0;
				}
				else
				{
					int stepCount = step.step_count;
					// NOTE: 실행 시 단 1번만 나오게 변경..
					if (stepCount >= 100_000 && !_expDrnChangeDeferrer.alreadyDeferred)
						_expDrnChangeDeferrer.defer($"+ 1 exp");

					return stepCount;
				}
			}
		}

		public float TodayStepCountRatio
		{
			get
			{
				RefEntry entry = GlobalRefDataContainer.getInstance().get<RefEntry>(3);
				return (float)TodayStepCount / (float)entry.required_steps;
			}
		}

		public int TodayStepCountMax
		{
			get
			{
				RefEntry entry = GlobalRefDataContainer.getInstance().get<RefEntry>(3);
				return entry.required_steps;
			}
		}

		public int DailyRewardCompleteCount
		{
			get
			{
				int count = 0;
				foreach(KeyValuePair<int,ClientBasicDailyRewardSlot> item in _dailyRewardSlotMap)
				{
					if( item.Value.status != ClientBasicDailyRewardSlot.Status.none)
					{
						count++;
					}
				}

				return count;
			}
		}

		public bool DailyRewardAllComplete => _dailyRewardSlotMap
				.Count(kvp => kvp.Value.status == ClientBasicDailyRewardSlot.Status.claimed) == 3;

		public Dictionary<int, ClientBasicDailyRewardSlot> DailyReward => _dailyRewardSlotMap;
		public Dictionary<int, ClientBasicWeeklyReward> WeeklyReward => _weeklyRewardMap;

		public ClaimedWeeklyRewardData ClaimedWeeklyRewardData
		{
			get
			{
				return _claimedWeeklyRewardData;
			}
			set
			{
				Set(ref _claimedWeeklyRewardData, value);
			}
		}

		public static BasicModeViewModel create()
		{
			BasicModeViewModel vm = new BasicModeViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_dailyStepMap = new Dictionary<int, ClientBasicDailyStep>();
			_dailyRewardSlotMap = new Dictionary<int, ClientBasicDailyRewardSlot>();
			_weeklyRewardMap = new Dictionary<int, ClientBasicWeeklyReward>();
			_expDrnChangeDeferrer = new();
		}

		public override void reset()
		{
			_dailyStepMap.Clear();
			_dailyRewardSlotMap.Clear();
			_weeklyRewardMap.Clear();
			_expDrnChangeDeferrer = new EntryExpChangeDeferrer();
		}

		protected override void bindPacketParser()
		{
			bind(MapPacketKey.ClientAck.basic_mode, (obj, ack) => { LevelData = (ClientBasicMode)obj; });
			bind(MapPacketKey.ClientAck.basic_daily_step, updateDailyStep);
			bind(MapPacketKey.ClientAck.basic_daily_reward_slot, updateDailyRewardSlot);
			bind(MapPacketKey.ClientAck.basic_weekly_reward, updateWeeklyReward);
		}

		private void updateDailyStep(object obj,MapPacket ack)
		{
			List<ClientBasicDailyStep> list = ack.getList<ClientBasicDailyStep>(MapPacketKey.ClientAck.basic_daily_step);
		
			foreach(ClientBasicDailyStep step in list)
			{
				if( _dailyStepMap.ContainsKey(step.day_id))
				{
					_dailyStepMap.Remove(step.day_id);
				}

				_dailyStepMap.Add(step.day_id, step);
			}

			notifyPropetyChanged("TodayStepCount");
		}

		private void updateDailyRewardSlot(object obj, MapPacket ack)
		{
			List<ClientBasicDailyRewardSlot> list = ack.getList<ClientBasicDailyRewardSlot>(MapPacketKey.ClientAck.basic_daily_reward_slot);
			foreach(ClientBasicDailyRewardSlot slot in list)
			{
				if( _dailyRewardSlotMap.ContainsKey(slot.slot_id))
				{
					_dailyRewardSlotMap.Remove(slot.slot_id);
				}
				_dailyRewardSlotMap.Add(slot.slot_id, slot);
			}

			notifyPropetyChanged("DailyReward");
		}

		private void updateWeeklyReward(object obj, MapPacket ack)
		{
			List<ClientBasicWeeklyReward> list = ack.getList<ClientBasicWeeklyReward>(MapPacketKey.ClientAck.basic_weekly_reward);
			foreach(ClientBasicWeeklyReward reward in list)
			{
				if( _weeklyRewardMap.ContainsKey( reward.week_id))
				{
					_weeklyRewardMap.Remove(reward.week_id);
				}

				_weeklyRewardMap.Add(reward.week_id, reward);
			}

			notifyPropetyChanged("WeeklyReward");
		}

		public ClientBasicWeeklyReward getThisWeekReward()
		{
			int week_id = (int)TimeUtil.thisWeekCount();
			ClientBasicWeeklyReward reward;
			if (_weeklyRewardMap.TryGetValue(week_id, out reward))
			{
				return reward;
			}
			else
			{
				return null;
			}
		}

		public ClientBasicWeeklyReward getLastWeekReward()
		{
			int week_id = (int)TimeUtil.thisWeekCount() - 1;
			ClientBasicWeeklyReward reward;
			if( _weeklyRewardMap.TryGetValue(week_id, out reward))
			{
				return reward;
			}
			else
			{
				return null;
			}
		}

		public TimeSpan getWeekRewardRemainTime()
		{
			DateTime weekEndTime = TimeUtil.dateTimeFromUnixTimestamp(TimeUtil.thisWeekEndTime());
			return weekEndTime - DateTime.UtcNow;
		}

		public int getWeekRewardAmount()
		{
			ClientBasicWeeklyReward reward = getThisWeekReward();
			if (reward == null)
			{
				return 0;
			}
			else
			{
				return (int)reward.amount;
			}
		}

		public ClientBasicWeeklyReward getClaimableWeeklyReward()
		{
			ClientBasicWeeklyReward reward = getLastWeekReward();
			if( reward == null)
			{
				return null;
			}

			if( reward.status == ClientBasicWeeklyReward.Status.claimed)
			{
				return null;
			}

			DateTime utcNow = DateTime.UtcNow;
			if( utcNow < reward.claim_time || utcNow >= reward.expire_time)
			{
				return null;
			}

			return reward;
		}

		public ClaimedWeeklyRewardData PopClaimedWeeklyRewardData()
		{
			ClaimedWeeklyRewardData data = _claimedWeeklyRewardData;
			_claimedWeeklyRewardData = null;

			return data;
		}

		public IDisposable SubscribeToEntryExpDeferrer(IObserver<string> observer)
		{
			return _expDrnChangeDeferrer.Subscribe(observer);
		}

		public void notifyToEntryExpDeferrer()
		{
			_expDrnChangeDeferrer.notify();
		}

		public bool IsSubscribedEntryExpDeferrer => _expDrnChangeDeferrer.ObserversCount > 0;
		public bool IsAlreadyNotifiedExpDefer => _expDrnChangeDeferrer.used;

		public void clearEntryExpDeferrer()
		{
			_expDrnChangeDeferrer.reset();
		}
	}
}
