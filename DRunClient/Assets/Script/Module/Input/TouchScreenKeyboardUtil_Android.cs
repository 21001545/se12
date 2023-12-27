using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client.Module
{
	public class TouchScreenKeyboardUtil_Android : TouchScreenKeyboardUtil
	{
		private AndroidJavaObject _objPlayer;
		private AndroidJavaObject _objView;

		protected override void init()
		{
			base.init();

			using (AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				_objPlayer = classPlayer.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
				_objView = _objPlayer.Call<AndroidJavaObject>("getView");
			}
		}

		protected override float getTouchScreenKeyboardHeight()
		{
			float decorHeight = 0;

			//using(AndroidJavaObject dialog = _objPlayer.Get<AndroidJavaObject>("mSoftInputDialog"))
			//{
			//	if( dialog != null)
			//	{
			//		AndroidJavaObject decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
			//		if (decorView != null)
			//		{
			//			decorHeight = decorView.Call<int>("getHeight");
			//		}
			//	}
			//}

			using (AndroidJavaObject rect = new AndroidJavaObject("android.graphics.Rect"))
			{
				_objView.Call("getWindowVisibleDisplayFrame", rect);

				return Screen.height - rect.Call<int>("height") + decorHeight;
			}
		}
	}
}
