using DRun.Client.Logic.BasicMode;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client
{
	public class StateClaimWeeklyReward : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.claim_weekly_reward;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("check weekly reward...", 95);

			ClientBasicWeeklyReward reward = _viewModel.BasicMode.getClaimableWeeklyReward();
			if( reward == null)
			{
				changeToNextState();
				return;
			}

			ClaimWeeklyRewardProcessor step = ClaimWeeklyRewardProcessor.create(reward.week_id);
			step.run(result => {
				changeToNextState();
			});
		}

	}
}
