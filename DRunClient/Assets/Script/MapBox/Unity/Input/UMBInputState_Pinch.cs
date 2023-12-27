using Festa.Client.Module.FSM;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBInputState_Pinch : UMBInputStateBehaviour
	{
		private float _initialRadius;
		private float _initialAngle;
		private float _initialZoom;
		private Vector2 _initialCenter;
		private Vector3 _initialControlEulerAngle;

		private Vector2 _center;
		private float _angle;
		private float _prevAngle;
		private float _radius;

		private Vector2 _lastDirection;
		private float _angleSpeed;

		public override int getType()
		{
			return UMBInputStateType.pinch;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			// 그럴 수 있다
			if( _inputModule.isMultiTouchDrag() == false)
			{
				_owner.changeState(UMBInputStateType.wait);
				return;
			}

			updateData();

			_initialRadius = _radius;
			_initialAngle = _angle;
			_initialZoom = _control.getZoomDamper().getCurrent();
			_initialControlEulerAngle = _control.getRotateRoot().localEulerAngles;
			_initialCenter = _center;

			_prevAngle = _angle;
			_angleSpeed = 0;
			//_control.setRotateZVelocity(0);
		}

		public override void update()
		{
			if( _inputModule.isMultiTouchDrag())
			{
				updateData();

				float ratio = _radius / _initialRadius;

				//
				float initScale = Mathf.Pow(2, _initialZoom);
				float newScale = initScale * ratio;
				float newZoom = Mathf.Log(newScale, 2);
				
				//_direction
				float delta_angle = _angle - _initialAngle;
				float z_angle = _initialControlEulerAngle.z + delta_angle;

				_angleSpeed = (_angle - _prevAngle) / Time.unscaledDeltaTime;
				_angleSpeed = Mathf.Clamp(_angleSpeed, -180.0f, +180.0f);				

				RectTransform zoomRoot = _mapBox.pivot;
				RectTransform scrollParent = _mapBox.scroll_root.parent as RectTransform;

				MBTileCoordinateDouble touchTilePos = _control.screenToTilePos(_center, _control.getCurrentTilePos().zoom);

				Vector2 prev_pos = scrollParent.InverseTransformPoint( _control.tilePosToWorldPosition( touchTilePos));

				_control.zoom( newZoom, true);

				Vector2 next_pos = scrollParent.InverseTransformPoint( _control.tilePosToWorldPosition( touchTilePos));

				_control.scroll(prev_pos - next_pos);
				_control.rotateZ(z_angle,true);
				_control.setRotateZVelocity(0);
			}
			else if( _inputModule.isTouchDrag())
			{
				_owner.changeState(UMBInputStateType.scroll);
				_control.setRotateZVelocity(_angleSpeed);
			}
			else if( _inputModule.isMultiTouchUp())
			{
				_owner.changeState(UMBInputStateType.wait);
				_control.setRotateZVelocity(_angleSpeed);
			}
		}

		//private float calcMultiTouchRadius()
		private void updateData()
		{
			_prevAngle = _angle;

			Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
			Vector2 max = new Vector2(float.MinValue, float.MinValue);

			for(int i = 0; i < _inputModule.getMultiTouchCount(); ++i)
			{
				Vector2 pos = _inputModule.getMultiTouchPosition(i);
				min.x = Mathf.Min(pos.x, min.x);
				min.y = Mathf.Min(pos.y, min.y);
				max.x = Mathf.Max(pos.x, max.x);
				max.y = Mathf.Max(pos.y, max.y);
			}

			Vector2 extent = (max - min) / 2.0f;

			_center = (min + max) / 2.0f;
			_radius = extent.magnitude;

			Vector2 dir = _inputModule.getMultiTouchPosition(1) - _inputModule.getMultiTouchPosition(0);
			dir.Normalize();

			_angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

//			_direction = _inputModule.getMultiTouchPosition(0) - _center;
			//_direction.Normalize();
			//Vector2 center = (min + max) / 2.0f;

			return;
		}
	}
}
