using Festa.Client.Module.FSM;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBInputState_Scroll : UMBInputStateBehaviour
	{
		private Vector2 _last_pos;

		private Vector2 _velocity;

		public override int getType()
		{
			return UMBInputStateType.scroll;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			//_last_pos = localFromScreen(_inputModule.getTouchPosition());

			RectTransformUtility.ScreenPointToLocalPointInRectangle(_mapBox.scroll_root.parent as RectTransform, _inputModule.getTouchPosition(), _targetCamera, out _last_pos);
			_velocity = Vector2.zero;
		}

		public override void update()
		{
			if( _inputModule.isTouchUp())
			{
				_control.setScrollVelocity(_velocity);
				_owner.changeState(UMBInputStateType.wait);
				return;
			}
			else if( _inputModule.isMultiTouchDrag())
			{
				_owner.changeState(UMBInputStateType.pinch);
			}
			else if( _inputModule.isTouchDrag())
			{
				//				Vector2 cur_pos = localFromScreen(_inputModule.getTouchPosition());

				Vector2 cur_pos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_mapBox.scroll_root.parent as RectTransform, _inputModule.getTouchPosition(), _targetCamera, out cur_pos);

				//_mapBox.scroll_root.anchoredPosition += cur_pos - _last_pos;
				Vector2 delta = cur_pos - _last_pos;
				if( delta != Vector2.zero)
				{
					_control.scroll(delta);
					_last_pos = cur_pos;
				}

				float deltaTime = Time.unscaledDeltaTime;

				Vector2 newVelocity = delta / deltaTime;

				_velocity = Vector2.Lerp(_velocity, newVelocity, deltaTime * 10);

				_mapBox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.none);
			}
		}
	}
}
