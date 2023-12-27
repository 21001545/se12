using UnityEngine;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using ETouchPhase = UnityEngine.InputSystem.TouchPhase;
using AttitudeSensor = UnityEngine.InputSystem.AttitudeSensor;

namespace Festa.Client.Module
{
	public class InputModule_Mobile : AbstractInputModule
	{
		private Vector2[] _screen_position;
		private int _multi_touch_count;

		//private BaseInput _event_system_input;

		public static InputModule_Mobile create()
		{
			InputModule_Mobile input = new InputModule_Mobile();
			input.init();
			return input;
		}

		private void init()
		{
			//			_event_system_input = EventSystem.current.currentInputModule.input;
			_screen_position = new Vector2[10];
			_multi_touch_count = 0;

			UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();

			// if(AttitudeSensor.current == null)
			// {
			// 	//Debug.LogWarning("AttitudeSensor.current is null");x
			// 	UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.AttitudeSensor>();
			// }

			if(AttitudeSensor.current != null && AttitudeSensor.current.enabled == false)
			{
				UnityEngine.InputSystem.InputSystem.EnableDevice(AttitudeSensor.current);
			}

		}

		public override Vector2 getTouchPosition()
		{
			return _screen_position[ 0];
		}

		protected void setScreenPosition(Vector2 pos)
		{
			_screen_position[ 0] = pos;
		}

		public override bool isTouchDown()
		{
			for(int i = 0; i < ETouch.activeTouches.Count; ++i)
			{
				ETouch t = ETouch.activeTouches[i];
				if( t.phase == ETouchPhase.Began)
				{
					setScreenPosition(t.screenPosition);
					return true;
				}
			}

			return false;
		}

		public override bool isTouchUp()
		{
			for (int i = 0; i < ETouch.activeTouches.Count; ++i)
			{
				ETouch t = ETouch.activeTouches[i];

				if ( t.phase == ETouchPhase.Canceled ||
					t.phase == ETouchPhase.Ended)
				{
					setScreenPosition(t.screenPosition);
					return true;
				}
			}

			//if( _event_system_input.mousePresent && _event_system_input.GetMouseButtonUp(0))
			//{
			//	setScreenPosition(_event_system_input.mousePosition);
			//	return true;
			//}

			return false;
		}

		public override bool isTouchDrag()
		{
			for (int i = 0; i < ETouch.activeTouches.Count; ++i)
			{
				ETouch t = ETouch.activeTouches[i];

				if ( t.phase == ETouchPhase.Moved ||
					t.phase == ETouchPhase.Stationary)
				{
					setScreenPosition(t.screenPosition);
					return true;
				}
			}

			//if( _event_system_input.mousePresent && _event_system_input.GetMouseButton(0))
			//{
			//	setScreenPosition(_event_system_input.mousePosition);
			//	return true;
			//}

			return false;
		}

		public override float wheelScroll()
		{
			return 0;
		}

		public override bool isMultiTouchDown()
		{
			_multi_touch_count = 0;
			for (int i = 0; i < ETouch.activeTouches.Count; ++i)
			{
				ETouch t = ETouch.activeTouches[i]; 
				if( t.phase == ETouchPhase.Began ||
					t.phase == ETouchPhase.Stationary ||
					t.phase == ETouchPhase.Began)
				{
					if( _multi_touch_count < _screen_position.Length)
					{
						_screen_position[_multi_touch_count] = t.screenPosition;
						++_multi_touch_count;
					}
				}
			}
			return _multi_touch_count > 1;
		}

		public override bool isMultiTouchDrag()
		{
			_multi_touch_count = 0;
			for (int i = 0; i < ETouch.activeTouches.Count; ++i)
			{
				ETouch t = ETouch.activeTouches[i];
				if (t.phase == ETouchPhase.Moved ||
					t.phase == ETouchPhase.Stationary ||
					t.phase == ETouchPhase.Began)
				{
					if( _multi_touch_count < _screen_position.Length)
					{
						_screen_position[_multi_touch_count] = t.screenPosition;
						++_multi_touch_count;
					}
				}
			}

			return _multi_touch_count > 1;
		}

		public override bool isMultiTouchUp()
		{
			bool any_touch = false;
			for (int i = 0; i < ETouch.activeTouches.Count; ++i)
			{
				ETouch t = ETouch.activeTouches[i];
				if (t.phase == ETouchPhase.Moved ||
					t.phase == ETouchPhase.Stationary ||
					t.phase == ETouchPhase.Began)
				{
					any_touch = true;
					break;
				}
			}

			return any_touch == false;
		}

		public override int getMultiTouchCount()
		{
			return _multi_touch_count;
		}

		public override Vector2 getMultiTouchPosition(int index)
		{
			return _screen_position[index];
		}
		
		public override Quaternion getAttitudeOrientation()
		{
			if(AttitudeSensor.current != null && AttitudeSensor.current.enabled == false)
			{
				Debug.LogWarning("AttitudeSensor disabled : trying enable");
				UnityEngine.InputSystem.InputSystem.EnableDevice(AttitudeSensor.current);
			}

			if( AttitudeSensor.current != null && AttitudeSensor.current.enabled)
			{
				return AttitudeSensor.current.attitude.ReadValue();
			}
			else
			{
				return Quaternion.identity;
			}
		}
	}
}

