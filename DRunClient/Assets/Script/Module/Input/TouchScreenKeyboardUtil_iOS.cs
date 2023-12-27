using UnityEngine;

namespace Festa.Client.Module
{
	public class TouchScreenKeyboardUtil_iOS : TouchScreenKeyboardUtil
	{
		protected override float getTouchScreenKeyboardHeight()
		{
			return TouchScreenKeyboard.area.height;
		}
	}
}
