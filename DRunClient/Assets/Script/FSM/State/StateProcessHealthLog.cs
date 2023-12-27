using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class StateProcessHealthLog : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.process_health_log;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("process health log...", 51);

			ClientMain.instance.getHealth().initialQuery(() => {
				changeToNextState();
			});
		}
	}
}
