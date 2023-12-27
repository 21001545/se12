using Festa.Client.Module;
using UnityEngine;

namespace Festa.Client.MapBox
{
	public class UMBDeco : ReusableMonoBehaviour
	{
		public float radius;

		private Vector2 _tilePosition;
		private float _tileRadius;

		private Vector3 _initScale;
		private Vector3SmoothDamper _scaleDamper;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			_tileRadius = radius * transform.localScale.x * MapBoxDefine.tile_extent;
			_scaleDamper = Vector3SmoothDamper.create(Vector3.zero, 0.2f);
			_initScale = transform.localScale;
		}

		public override void onReused()
		{
		}

		public void initSource()
		{
			_tileRadius = radius * transform.localScale.x * MapBoxDefine.tile_extent;
		}

		public float getMBRadius()
		{
			return _tileRadius;
		}

		public void setup(Vector2 tilePosition,float angle)
		{
			_tilePosition = tilePosition;
			transform.localPosition = new Vector3(tilePosition.x, -tilePosition.y);
			transform.localPosition /= MapBoxDefine.tile_extent;
			transform.localEulerAngles = new Vector3(angle, -90, 90);

			transform.localScale = Vector3.zero;
			_scaleDamper.reset(Vector3.zero);
			_scaleDamper.setTarget(_initScale);
		}

		public bool isOverlap(Vector2 tilePosition,float tileRadius)
		{
			Vector2 diff = tilePosition - _tilePosition;
			return diff.magnitude <= (tileRadius + _tileRadius);
		}

		public bool update()
		{
			if( _scaleDamper.update())
			{
				transform.localScale = _scaleDamper.getCurrent();
				return false;
			}
			else
			{
				return true;
			}
		}

//#if UNITY_EDITOR
//		void OnDrawGizmos()
//		{
//			GizmoExtension.drawCircleLine(transform, Vector3.zero, radius, Color.blue);
//		}
//#endif

	}
}
