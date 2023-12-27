using Festa.Client.Logic;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class StateQueryChatRoomList : ClientStateBehaviour
	{
		public override int getType()
		{
			return ClientStateType.query_chatroom_list;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			UILoading.getInstance().setProgress("query chatroom data...", 81);

			queryRoomList();	
		}

		private void queryRoomList()
		{
			QueryChatRoomListProcessor step = QueryChatRoomListProcessor.create();
			step.run(result => {
				changeToNextState();
			});
		}


	}
}
