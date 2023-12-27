using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Festa.Client
{
	public class WorldInputState_Wait : WorldInputStateBehaviour
	{
		public override int getType()
		{
			return WorldInputStateType.wait;
		}

		public override void update()
		{
			if( _inputModule.isMultiTouchDown())
			{
				_owner.changeState(WorldInputStateType.pinch_zoom);
			}
			else if( _inputModule.isTouchDown())
			{
				processTouchDown();
			}

#if UNITY_EDITOR
			processMouseWheel();
#endif
		}

		private void processTouchDown()
		{
			if (EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(_inputModule.getTouchPosition());
			if (Physics.Raycast(ray, out hit) == false)
			{
//#if UNITY_EDITOR
//				_owner.changeState(WorldInputStateType.scroll);
//#endif
				return;
			}

			//if( hit.transform.gameObject == _world.getDesertFox().gameObject ||
			//	(_world.getCurrentTree() != null && 
			//	hit.transform.gameObject == _world.getCurrentTree().gameObject))
			//{
			//	_owner.changeState(WorldInputStateType.touch_object, hit.transform.gameObject);
			//}
		}

		private void processMouseWheel()
		{
			if( EventSystem.current.IsPointerOverGameObject())
			{
				return;
			}

			InputModule_PC pcInputModule = _inputModule as InputModule_PC;
			float wheel = pcInputModule.wheelScroll();
			
			if( wheel == 0)
			{
				return;
			}

			float fov = _cameraControl.getFovDamper().getCurrent();
			if( wheel < 0)
			{
				fov *= 0.9f;
			}
			else
			{
				fov *= 1.1f;
			}

			Vector3 touchPosition = _inputModule.getTouchPosition();
			touchPosition.z = 6.21f;

			Vector3 prev = _camera.ScreenToWorldPoint(touchPosition);
			_cameraControl.setFov(fov,true);
			Vector3 next = _camera.ScreenToWorldPoint(touchPosition);

			Vector3 move = _camera.transform.localPosition + (prev - next);

			_cameraControl.translate(move, true);
			_cameraControl.setReturnDefaultTime(0.1f);
		}
	}
}
