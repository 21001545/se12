using Festa.Client.Module.FSM;

namespace Festa.Client
{
	public class StateCheckPermissioniOS : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.check_permission_ios;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			changeToNextState();

			UILoading.getInstance().setProgress("check permission...", 35);
		}
	}
}
