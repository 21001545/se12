using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Festa.Client.Module
{
	public abstract class TouchScreenKeyboardUtil
	{
		private static TouchScreenKeyboardUtil _instance;

		//private float _keyboardHeight;
		private FloatSmoothDamper _heightDamper;
		private float _hideStartTime;
		private bool _lastVisible;

		public static void createInstance()
		{

#if !UNITY_EDITOR

	#if UNITY_ANDROID
			TouchScreenKeyboardUtil util = new TouchScreenKeyboardUtil_Android();
	#elif UNITY_IOS
			TouchScreenKeyboardUtil util = new TouchScreenKeyboardUtil_iOS();
	#endif

#else
			TouchScreenKeyboardUtil util = new TouchScreenKeyboardUtil_Editor();
#endif

			util.init();
			_instance = util;
		}

		public static TouchScreenKeyboardUtil getInstance()
		{
			return _instance;
		}

		protected virtual void init()
		{
			_heightDamper = FloatSmoothDamper.create(0.0f, 0.1f, 1.0f);
			_lastVisible = false;
			_hideStartTime = 0;
		}

		public virtual void update()
		{
			if( isVisible())
			{
				_heightDamper.setTarget(getTouchScreenKeyboardHeight());
			}
			else
			{
				if( _lastVisible)
				{
					_hideStartTime = Time.realtimeSinceStartup;
				}

				if( Time.realtimeSinceStartup - _hideStartTime > 0.3f)
				{
					_heightDamper.setTarget(0);
				}
			}

			_heightDamper.update();
			_lastVisible = isVisible();
		}

		public virtual float getHeight()
		{
			return _heightDamper.getCurrent();
			//return _keyboardHeight;
		}
		
		public virtual bool isVisible()
		{
			return TouchScreenKeyboard.visible;
		}

		protected abstract float getTouchScreenKeyboardHeight();

		public virtual void waitForKeyboardHidingComplete(UnityAction callback)
		{
			ClientMain.instance.StartCoroutine(_waitForKeyboardHidingComplete(callback));
		}

		public IEnumerator _waitForKeyboardHidingComplete(UnityAction callback)
		{
			yield return new WaitWhile(() => {
				return TouchScreenKeyboard.visible;
			});

			// 그래도 좀더 기다려보자
			yield return new WaitForSeconds(0.2f);

			callback();
		}
    }
}
