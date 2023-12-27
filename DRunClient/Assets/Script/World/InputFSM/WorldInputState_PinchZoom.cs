using Festa.Client.Module;
using Festa.Client.Module.FSM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class WorldInputState_PinchZoom : WorldInputStateBehaviour
	{
		private float _initFov;
		private float _initRadius;
		
		private Vector2 _center;
		private float _radius;

		private Vector2 _prevCenter;

		public override int getType()
		{
			return WorldInputStateType.pinch_zoom;
		}

		public override void onEnter(StateBehaviour<object> prev_state, object param)
		{
			if( _inputModule.isMultiTouchDrag() == false)
			{
				_owner.changeState(WorldInputStateType.wait);
				return;
			}

			updateData();

			_initFov = _cameraControl.getFovDamper().getCurrent();
			_initRadius = _radius;
			_prevCenter = _center;
		}

		public override void update()
		{
			if( _inputModule.isMultiTouchDrag())
			{
				updateData();

				// 이동처리

				processTranslate();

				// zoom
				processZoom();
			}
			else if( _inputModule.isMultiTouchUp())
			{
				_owner.changeState(WorldInputStateType.wait);
				_cameraControl.setReturnDefaultTime(0.1f);
			}
		}

		private void processTranslate()
		{
			Vector3 prevTouch = _prevCenter;
			Vector3 nextTouch = _center;

			prevTouch.z = nextTouch.z = 6.21f;

			Vector3 prev = _camera.ScreenToWorldPoint(prevTouch);
			Vector3 next = _camera.ScreenToWorldPoint(nextTouch);

			Vector3 move = _camera.transform.localPosition + (prev - next);

			_cameraControl.translate(move, true);

			_prevCenter = _center;
		}

		private void processZoom()
		{
			float ratio = _initRadius / _radius;
			float fov = _initFov * ratio;

			Vector3 touchPosition = _center;
			touchPosition.z = 6.21f;

			Vector3 prev = _camera.ScreenToWorldPoint(touchPosition);
			_cameraControl.setFov(fov, true);
			Vector3 next = _camera.ScreenToWorldPoint(touchPosition);

			Vector3 move = _camera.transform.localPosition + (prev - next);

			_cameraControl.translate(move, true);
		}

		private void updateData()
		{
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
		}
	}
}
