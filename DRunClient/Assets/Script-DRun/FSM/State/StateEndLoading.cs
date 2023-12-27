using DRun.Client.Logic.Account;
using DRun.Client.Running;
using Festa.Client;
using Festa.Client.Module.FSM;
using UnityEngine;

namespace DRun.Client
{
	public class StateEndLoading : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.end_loading;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("finish loading...", 100);

			int runningStatus = ClientMain.instance.getViewModel().Running.Status;

			if( runningStatus != StateType.none)
			{
				Debug.Log($"open running ui: statue[{runningStatus}]");

				UILoading.getInstance().close();
				UIRunningStatus.getInstance().open(new UIPanelOpenParam_RunFromAppStart());
			}
			else
			{
				UIMainTab.getInstance().open();

				if( UIMainTab.getInstance().getCurrentTab() != UIMainTab.Tab.home)
				{
					UIMainTab.getInstance().applyTab(UIMainTab.Tab.home);
				}
				else
				{
					UIHome.getInstance().open();
				}

			}

			changeToNextState();
		}

#if UNITY_EDITOR

		#region ServerTest

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private void testAcceptInvitationCode()
		{
			AcceptInvitationCodeProcessor step = AcceptInvitationCodeProcessor.create("ICY9B5");
			step.run(result => { 
				
			});
		}

		#endregion

#endif
	}
}
