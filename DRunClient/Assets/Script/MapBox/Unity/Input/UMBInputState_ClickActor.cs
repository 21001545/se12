using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBInputState_ClickActor : UMBInputStateBehaviour
	{
		public override int getType()
		{
			return UMBInputStateType.click_actor;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			base.onEnter(prev_state, param);
		}

		public override void update()
		{
			if( _inputModule.isTouchUp())
			{
				Vector2 pos = _inputModule.getTouchPosition();
				UMBActor pick_actor = _control.pickActorFromScreenPosition(pos);

				if (pick_actor != null)
				{
					if( inputFSM.OnClickActor != null)
					{
						inputFSM.OnClickActor(pick_actor);
					}
				}
				else
				{
					_owner.changeState(UMBInputStateType.wait);
				}
			}
		}
	}
}
