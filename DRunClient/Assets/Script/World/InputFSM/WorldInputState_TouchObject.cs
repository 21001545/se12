using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Festa.Client
{
	public class WorldInputState_TouchObject : WorldInputStateBehaviour
	{
		private GameObject _downObject;

		public override int getType()
		{
			return WorldInputStateType.touch_object;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_downObject = (GameObject)param;
		}

		public override void update()
		{
			if( _inputModule.isTouchUp())
			{
				processTouchObject();

				_owner.changeState(WorldInputStateType.wait);
			}
			else if( _inputModule.isMultiTouchDown() || _inputModule.isMultiTouchDrag())
			{
				_owner.changeState(WorldInputStateType.pinch_zoom);
			}
		}

		private void processTouchObject()
		{
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(_inputModule.getTouchPosition());
			if (Physics.Raycast(ray, out hit) == false)
			{
				return;
			}

			if( hit.transform.gameObject != _downObject)
			{
				return;
			}

			//if( hit.transform.gameObject == _world.getDesertFox().gameObject)
			//{
			//	_world.getDesertFox().touch();
			//}
			//else if( _world.getCurrentTree() != null && hit.transform.gameObject == _world.getCurrentTree().gameObject)
			//{
			//	_world.getCurrentTree().touch();
			//	_world.getDesertFox().onTreeTouch();
			//}
		}

	}
}
