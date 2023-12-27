using UnityEngine;

namespace Festa.Client.Module
{
	public abstract class AbstractInputModule
	{
		public abstract Vector2 getTouchPosition();
		public abstract bool isTouchDown();
		public abstract bool isTouchUp();
		public abstract bool isTouchDrag();
		public abstract float wheelScroll();

		public abstract bool isMultiTouchDown();
		public abstract bool isMultiTouchDrag();
		public abstract bool isMultiTouchUp();
		public abstract int getMultiTouchCount();
		public abstract Vector2 getMultiTouchPosition(int index);

		public abstract Quaternion getAttitudeOrientation();
	}
}

