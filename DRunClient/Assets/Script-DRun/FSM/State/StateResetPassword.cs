using Festa.Client;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client
{
	public class StateResetPassword : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.resetpassword;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UIResetPassword.getInstance().open();
		}
	}
}
