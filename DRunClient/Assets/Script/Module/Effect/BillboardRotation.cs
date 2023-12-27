using UnityEngine;

namespace Festa.Client.Module
{
	public class BillboardRotation : MonoBehaviour
	{
		private Camera _camera;

		void LateUpdate()
		{
			if( _camera == null)
			{
				_camera = Camera.main;
			}
			transform.LookAt(transform.position + _camera.transform.rotation * Vector3.forward,
				_camera.transform.rotation * Vector3.up); 
		}
	}
}
