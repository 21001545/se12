using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class StateHealthDevice : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.init_health_device;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("initialize health device...", 12);

			ClientMain.instance.getHealth().getDevice().initDevice(result => { 
			
				if( result == true)
				{
					changeToNextState();
				}
				else
				{
					UIPopup.spawnOK("HealthKit Failure\n\nPlease grant below permission(s) on Settings\n<b>[Walk]</b>", () => {

						changeToNextState();

					});
				}
				
			});
		}
	}
}
