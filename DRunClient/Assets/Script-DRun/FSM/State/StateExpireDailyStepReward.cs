using DRun.Client.Logic.BasicMode;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client
{
	public class StateExpireDailyStepReward : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.expire_daily_step_reward;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("check daily reward...", 90);

			ExpireDailyRewardProcessor step = ExpireDailyRewardProcessor.create();
			step.run(result => { 
				if( result.failed())
				{
					UIPopup.spawnOK("#일일 보상 처리중 오류 발생", () => {
						UISelectServer.getInstance().open();
					});
				}
				else
				{
					changeToNextState();
				}
			});
		}
	}
}
