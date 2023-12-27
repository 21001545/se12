using Festa.Client;
using Festa.Client.Module.FSM;
using UnityEngine;

namespace DRun.Client
{
	public class StateCheckForceUpdate : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.check_force_update;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			if( RemoteConfig.getBoolean( RemoteConfig.force_update) == false)
			{
				changeToNextState();
			}
			else
			{
				spawnForceUpdatePopup();
			}
		}

		private void spawnForceUpdatePopup()
		{
			string message = RemoteConfig.getString(RemoteConfig.force_update_message);
			string store_url = RemoteConfig.getString(RemoteConfig.store_url);

			UIPopup.spawnOK(message, () => {
				Application.OpenURL(store_url);
			});
		}


	}
}
