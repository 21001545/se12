using Festa.Client.Module.UI;
using UnityEngine;
using System;
using TMPro;
using Festa.Client.Module;

namespace Festa.Client
{
	public class UITripCountDown : UISingletonPanel<UITripCountDown>
	{
		public TMP_Text txt_count;

		private Action _completeCallback;
		private int _count;
		private IntervalTimer _timer;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_timer = IntervalTimer.create(1.0f, true, false);
			_timer.stop();
		}

		public void startCountDown(Action completeCallback)
		{
			_completeCallback = completeCallback;
			
			_timer.setNext();

#if UNITY_EDITOR
			_count = 1;
#else
			_count = 3;
#endif
			txt_count.text = _count.ToString();

			open();
		}

		public override void update()
		{
			base.update();

			if( _timer.update())
			{
				if( _count <= 1)
				{
					_timer.stop();
					_completeCallback();
					close();
					return;
				}
				
				_count--;
				txt_count.text = _count.ToString();
			}
		}
	
	}
}
