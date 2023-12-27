using DRun.Client.Logic.SignIn;
using Festa.Client;
using Festa.Client.Module.FSM;
using Festa.Client.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client
{
	public class StateLogin : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.login;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("logging in...", 50);

			if( UILoading.getInstance().gameObject.activeSelf == false)
			{
				UILoading.getInstance().open();
			}

			LoginProcessor step = LoginProcessor.create(_viewModel.SignIn.EMail);
			step.run(result => { 
			
				if( result.failed())
				{
					UIPopup.spawnOK("##로그인 실패", () => {
						_owner.changeState(ClientStateType.select_server);
					});
				}
				else
				{
					if( _viewModel.Profile.Signup.step == SignUpStep.complete_body)
					{
						changeToNextState();
					}
					else
					{
						restartSignUp();
					}
				}
			});
		}

		private void restartSignUp()
		{
			UIPanelOpenParam param = UIPanelOpenParam.create();
			param.put("signUpStep", _viewModel.Profile.Signup.step);
			UISignUp.getInstance().open(param);
		}
	}

}
