using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Festa.Client.MapBox
{ 
	class UMBInputState_Wait : UMBInputStateBehaviour
	{
		private bool _down;
		private Vector2 _last_down_position;
		private float _dragThreshold = 20.0f; // 나중에 DPI로 계산

		public override int getType()
		{
			return UMBInputStateType.wait;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			_down = false;
		}

		public override void update()
		{
			if( _inputModule.isMultiTouchDown())
			{
				bool check_control_area = true;
				for(int i = 0; i < _inputModule.getMultiTouchCount() && i < 2; ++i)
				{
					if(isInControlArea( _inputModule.getMultiTouchPosition(i)) == false)
					{
						check_control_area = false;
						break;
					}
				}

				if( check_control_area)
				{
					_owner.changeState(UMBInputStateType.pinch);
				}
			}
			else if( _inputModule.isTouchDown())
			{
				Vector2 pos = _inputModule.getTouchPosition();

				if( isInControlArea( pos))
				{
					_down = true;
					_last_down_position = _inputModule.getTouchPosition();
					_control.setScrollVelocity(Vector2.zero);

					UMBActor pick_actor = _control.pickActorFromScreenPosition(pos);
					if( pick_actor != null)
					{
						_owner.changeState(UMBInputStateType.click_actor, pick_actor);
					}

					//MBTileCoordinateDouble tilePos = tilePosFromScreen(_inputModule.getTouchPosition());
					//Debug.Log($"tx[{(int)(tilePos.tile_x * MapBoxDefine.tile_extent)}] ty[{(int)(tilePos.tile_y * MapBoxDefine.tile_extent)}] sx[{_last_down_position.x}] sy[{_last_down_position.y}]");
				}
			}
			else if( _inputModule.isTouchDrag() && _down)
			{
				Vector2 delta = _inputModule.getTouchPosition() - _last_down_position;
			
				if( delta.magnitude >= _dragThreshold)
				{
					_owner.changeState(UMBInputStateType.scroll);
				}
			}

#if UNITY_EDITOR
			if(_inputModule.wheelScroll() != 0 && isInControlArea( _inputModule.getTouchPosition()))
			{
				float delta = _inputModule.wheelScroll();
				//int zoom = _control.getTargetTilePos().zoom;
				float target_zoom = _control.getZoomDamper().getTarget();
				float speed = 0.1f;

				if (delta > 0)
				{
					target_zoom += speed;
				}
				else if (delta < 0)
				{
					target_zoom += -speed;
				}

				MBTileCoordinateDouble zoomCenterPosition;
				
				if( _mapBox.getViewModel().CurrentLocationMode == UMBDefine.CurrentLocationMode.none)
				{
					zoomCenterPosition = _control.screenToTilePos(_inputModule.getTouchPosition(), _control.getCurrentTilePos().zoom);
				}
				else
				{
					zoomCenterPosition = _mapBox.getControl().getTargetTilePos();
				}

				RectTransform scrollParent = _mapBox.scroll_root.parent as RectTransform;

				Vector3 prevWorld = _control.tilePosToWorldPosition(zoomCenterPosition);
				Vector2 prev = scrollParent.InverseTransformPoint( prevWorld);

				_control.zoom(target_zoom, true);

				//_control.scroll(Vector2.zero);

				Vector3 nextWorld = _control.tilePosToWorldPosition(zoomCenterPosition);
				Vector2 next = scrollParent.InverseTransformPoint(nextWorld);

				_control.scroll(prev - next);

				//MBTileCoordinateDouble resultTilePos = _control.screenToTilePos(_inputModule.getTouchPosition(), _control.getCurrentTilePos().zoom);

				//// zoom이 같다고 전제하고
				//DoubleVector2 diff = touchTilePos.tile_pos - resultTilePos.tile_pos;
				//diff.x *= 4096;
				//diff.y *= 4096;

				//Debug.Log($"[{diff}] - [{prev - next}]");

				if( _mapBox.getViewModel().CurrentLocationMode == UMBDefine.CurrentLocationMode.follow_fitzoom)
				{
					_mapBox.setCurrentLocationMode(UMBDefine.CurrentLocationMode.follow);
				}
			}

			InputModule_PC pcInputModule = _inputModule as InputModule_PC;

			if (pcInputModule.isRightButtonPressed() && isInControlArea(_inputModule.getTouchPosition()))
			{
				_owner.changeState(UMBInputStateType.mouse_rotate);
			}
#endif
		}
	}
}
