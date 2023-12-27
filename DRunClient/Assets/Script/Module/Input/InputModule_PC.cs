using UnityEngine;
using UnityEngine.InputSystem;

namespace Festa.Client.Module
{
	public class InputModule_PC : AbstractInputModule
	{
		private Vector2 _screen_position;

		private IntervalTimer _attitudeTimer;
		private Vector3 _lastAttitude = Vector3.zero;

		public static InputModule_PC create()
		{
			InputModule_PC input = new InputModule_PC();
			input.init();
			return input;
		}

		private void init()
		{
			_attitudeTimer = IntervalTimer.create(1.0f, true, false);
			_lastAttitude = Random.onUnitSphere;
		}

		public override Vector2 getTouchPosition()
		{
			return _screen_position;
		}

		protected void setScreenPosition(Vector2 pos)
		{
			_screen_position = pos;
		}

		public override bool isTouchDown()
		{
			//UnityEngine.InputSystem.Mouse.current;
			if( Mouse.current.leftButton.wasPressedThisFrame == false)
			{
				return false;
			}

			setScreenPosition(Mouse.current.position.ReadValue());
			return true;
		}

		public override bool isTouchUp()
		{
			if( Mouse.current.leftButton.wasReleasedThisFrame == false)
			{
				return false;
			}

			setScreenPosition(Mouse.current.position.ReadValue());
			return true;
		}

		public override bool isTouchDrag()
		{
			if( Mouse.current.leftButton.isPressed == false)
			{
				return false;
			}

			setScreenPosition(Mouse.current.position.ReadValue());
			return true;
		}

		public override float wheelScroll()
		{
			setScreenPosition(Mouse.current.position.ReadValue());
			return Mouse.current.scroll.y.ReadValue();
		}

		public override bool isMultiTouchDown()
		{
			return false;
		}

		public override bool isMultiTouchDrag()
		{
			return false;
		}

		public override bool isMultiTouchUp()
		{
			return false;
		}

		public override int getMultiTouchCount()
		{
			return 0;
		}

		public override Vector2 getMultiTouchPosition(int index)
		{
			return Vector2.zero;
		}

		public override Quaternion getAttitudeOrientation()
		{
			if(_attitudeTimer.update())
			{
				_lastAttitude.z = Random.Range(0, 360);
			}

			return Quaternion.Euler(0, 0, _lastAttitude.z);
		}

		// 마우스 디버깅용
#if UNITY_EDITOR
		public bool isRightButtonPressed()
		{
			if( Mouse.current.rightButton.wasPressedThisFrame == false)
			{
				return false;
			}

			setScreenPosition(Mouse.current.position.ReadValue());
			return true;
		}

		public bool isRightButtonReleased()
		{
			if( Mouse.current.rightButton.wasReleasedThisFrame == false)
			{
				return false;
			}
			setScreenPosition(Mouse.current.position.ReadValue());
			return true;
		}

		public bool isRightButtonDraging()
		{
			if (Mouse.current.rightButton.isPressed == false)
			{
				return false;
			}

			setScreenPosition(Mouse.current.position.ReadValue());
			return true;
		}
#endif

	}
}
