using Festa.Client;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client
{
	public class StateSignIn : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.signin;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("trip status...", 41);

			UISignIn.getInstance().open();
		}
	}
}
