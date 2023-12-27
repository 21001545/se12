using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class StateRegisterPush : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.register_push;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			
		
		
		}
	}
}
