using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBCameraBound
	{
		private Camera _camera;
		private Plane[] _planes;
		private int _frame;

		public static UMBCameraBound create(Camera targetCamera)
		{
			UMBCameraBound bound = new UMBCameraBound();
			bound.init(targetCamera);
			return bound;
		}

		private void init(Camera targetCamera)
		{
			_camera = targetCamera;
			_frame = 0;
			_planes = new Plane[6];
		}

		public void updatePlanes()
		{
			if( _frame == Time.frameCount)
			{
				return;
			}

			_frame = Time.frameCount;
			GeometryUtility.CalculateFrustumPlanes(_camera, _planes);

			//for(int i = 0; i < 6; ++i)
			//{
			//	Debug.Log(_planes[i].ToString());
			//}
		}

		public bool isVisible(Vector3 pos,Vector3 size)
		{
			return isVisible(new Bounds(pos, size));
		}

		public bool isVisible(Bounds bound)
		{
			updatePlanes();
			return GeometryUtility.TestPlanesAABB(_planes, bound);
		}

	}
}
