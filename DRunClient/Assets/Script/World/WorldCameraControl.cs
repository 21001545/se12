using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	[RequireComponent(typeof(Camera))]
	public class WorldCameraControl : MonoBehaviour
	{
		public Camera uiCamera;

		private Camera _camera;
		private FloatSmoothDamper _fovDamper;
		private Vector3SmoothDamper _positionDamper;
		private float _returnDefaultTime;
		private Vector3 _defaultPosition;

		public static Vector2 _fovRange = new Vector2(10.0f, 60.0f);
		public static float _fovDefault = 45.0f;


		public FloatSmoothDamper getFovDamper()
		{
			return _fovDamper;
		}

		public Vector3SmoothDamper getPositionDamper()
		{
			return _positionDamper;
		}

		public void setFov(float fov,bool updateNow)
		{
			fov = Mathf.Clamp(fov, _fovRange.x, _fovRange.y);

			if( updateNow)
			{
				_fovDamper.reset(fov);
				uiCamera.fieldOfView =_camera.fieldOfView = _fovDamper.getCurrent();
			}
			else
			{
				_fovDamper.setTarget(fov);
			}
		}

		public void translate(Vector3 position,bool updateNow)
		{
			if( updateNow)
			{
				_positionDamper.reset(position);
				_camera.transform.localPosition = position;
			}
			else
			{
				_positionDamper.setTarget(position);
			}
		}

		public void setReturnDefaultTime(float delta)
		{
			_returnDefaultTime = Time.time + delta;
		}

		private void Awake()
		{
			_camera = GetComponent<Camera>();
			_defaultPosition = _camera.transform.localPosition;
			_fovDamper = FloatSmoothDamper.create(45, 0.5f, 0.1f);
			_positionDamper = Vector3SmoothDamper.create(_defaultPosition, 0.5f);
			_returnDefaultTime = 0;
		}

		private void Update()
		{
			if( _fovDamper.update())
			{
				uiCamera.fieldOfView = _camera.fieldOfView = _fovDamper.getCurrent();
			}

			if( _positionDamper.update())
			{
				_camera.transform.localPosition = _positionDamper.getCurrent();
			}

			if (_returnDefaultTime != 0 && Time.time >= _returnDefaultTime)
			{
				_fovDamper.setTarget(_fovDefault);
				_positionDamper.setTarget(_defaultPosition);
				_returnDefaultTime = 0;
			}
		}

	}
}
