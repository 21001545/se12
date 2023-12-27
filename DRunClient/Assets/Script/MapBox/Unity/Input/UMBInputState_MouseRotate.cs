using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBInputState_MouseRotate : UMBInputStateBehaviour
	{
		private float _initialAngle;
		private Vector3 _initialControlEulerAngle;
		private float _angleSpeed;

		public override int getType()
		{
			return UMBInputStateType.mouse_rotate;
		}

		private Vector2 getDir(Vector2 touchPosition)
		{
			return touchPosition - new Vector2(Screen.width / 2.0f, Screen.height / 2);
		}

		private float getAngle(Vector2 touchPosition)
		{
			Vector2 dir = getDir(touchPosition);
			dir.Normalize();
			return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_initialAngle = getAngle(_inputModule.getTouchPosition());
			_initialControlEulerAngle = _control.getRotateRoot().localEulerAngles;

			_angleSpeed = 0;
			_control.setRotateZVelocity(0);
		}

		public override void update()
		{
#if UNITY_EDITOR
			InputModule_PC pcInputModule = _inputModule as InputModule_PC;
			if( pcInputModule.isRightButtonReleased())
			{
				_control.setRotateZVelocity(_angleSpeed);
				_owner.changeState(UMBInputStateType.wait);
				return;
			}
			else if( pcInputModule.isRightButtonDraging())
			{
				float angle = getAngle(_inputModule.getTouchPosition());

				float delta_angle = angle - _initialAngle;
				float z_angle = _initialControlEulerAngle.z + delta_angle;

				_angleSpeed = (z_angle - _control.getZAngleDamper().getCurrent()) / Time.unscaledDeltaTime;

				_control.rotateZ(z_angle,true);
			}
#endif
		}
	}
}
