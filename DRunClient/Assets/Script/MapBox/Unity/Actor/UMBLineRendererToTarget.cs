using UnityEngine;

namespace Festa.Client.MapBox
{
	[RequireComponent(typeof(LineRenderer))]
	public class UMBLineRendererToTarget : MonoBehaviour
	{
		public Transform target;

		private LineRenderer _renderer;

		void LateUpdate()
		{
			if( _renderer == null)
			{
				_renderer = GetComponent<LineRenderer>();
			}

			_renderer.SetPosition(1, target.localPosition);
		}
	}
}
