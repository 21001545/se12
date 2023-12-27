using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class StateLocationDevice : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.init_location_device;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("initialize location device...", 13);

			ClientMain.instance.getGPSTracker().initDevice();

			changeToNextState();

			//ClientMain.instance.getLocation().getDevice().initDevice(result => {

			//	if (result == true)
			//	{
			//		changeToNextState();
			//	}
			//	else
			//	{
			//		UIPopup.spawnOK("LocationService Failure\n\nPlease grant below permission(s) on Settings\n<b>[FindLocation]</b>", () => {
			//			changeToNextState();
			//		});
			//	}

			//});
		}
	}
}
