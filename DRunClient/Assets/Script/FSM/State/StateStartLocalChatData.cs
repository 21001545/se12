using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class StateStartLocalChatData : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.start_local_chatdata;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("process local chatdata...", 80);

			ClientMain.instance.getLocalChatData().start(GlobalConfig.gameserver_url, result => { 
			
				if( result.failed())
				{
					UIPopup.spawnOK("start local chatdata fail", () => {

						_owner.changeState(ClientStateType.sleep);					

					});
				}
				else
				{
					changeToNextState();
				}
			});
		}
	}
}
