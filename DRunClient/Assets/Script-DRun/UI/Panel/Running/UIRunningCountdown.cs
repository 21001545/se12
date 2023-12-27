using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIRunningCountdown : UISingletonPanel<UIRunningCountdown>
	{
		public UIFullscreenBlurredImage baseImage;
		public Texture[] baseTextures;
		public TMP_Text text_countdown;

		private IntervalTimer _timer;
		private int _remainSeconds;
		private static CanvasGroup _runningStatusCanvasGroup;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
			_timer = IntervalTimer.create(1.0f, false, false);
			_timer.stop();
			_runningStatusCanvasGroup = UIRunningStatus.getInstance().transform.GetChild(1).GetComponent<CanvasGroup>();
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				_timer.stop();
				_remainSeconds = 3;
				text_countdown.text = _remainSeconds.ToString();

				setupBackground();
			}
			else if (type == TransitionEventType.end_open)
			{
				_timer.setNext();

				//미리 준비
				UIHome.getInstance().close();
				
				// NOTE: 2023-01-20 이윤상
				// 카운트 다운 시작 전 찰나에
				// Running Status 가 보여서 UI container 만 잠깐 안보이게 꺼두기.
				// this.close() 에서 다시 켬.
				_runningStatusCanvasGroup.alpha = 0;

				UIRunningStatus.getInstance().open();
			}
		}

		public override void close(int transitionType = 0)
		{
			_runningStatusCanvasGroup.alpha = 1;

			base.close(transitionType);
		}

		public override void update()
		{
			base.update();

			if( _timer.update())
			{
				_remainSeconds -= 1;
				if( _remainSeconds == 0)
				{
					this.close();
				}
				else
				{
					_timer.setNext();
					text_countdown.text = _remainSeconds.ToString();
				}
			}
		}

		public void onClick_Skip() => this.close();

		private void setupBackground()
		{
			if( ClientMain.instance.getViewModel().Running.RunningType == ClientRunningLogCumulation.RunningType.promode)
			{
				baseImage.texture = baseTextures[0];
			}
			else
			{
				baseImage.texture = baseTextures[1];
			}
		}
	}
}
