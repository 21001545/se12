using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UISelectMode : UISingletonPanel<UISelectMode>
	{
		public GameObject go_warning;
		public Animator anim_warning;

		public Image imageBasicMode;
		public Image imageProMode;


		private int _currentMode;
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			base.open(param, transitionType, closeType);

			selectMode(ViewModel.PlayMode.Mode);
		}

		private void selectMode(int mode)
		{
			_currentMode = mode;

			Color colorOn = UIStyleDefine.ColorStyle.gray850;
			Color colorOff = UIStyleDefine.ColorStyle.gray850;
			colorOff.a = 0;

			if ( mode == PlayModeViewModel.PlayMode.Basic)
			{
				imageBasicMode.color = colorOn;
				imageProMode.color = colorOff;
			}
			else
			{
				imageBasicMode.color = colorOff;
				imageProMode.color = colorOn;
			}
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				go_warning.SetActive(true);
			}
			else if( type == TransitionEventType.start_close)
			{
				go_warning.SetActive(false);
			}
		}

		public void onClick_BasicMode()
		{
			selectMode(PlayModeViewModel.PlayMode.Basic);
		}

		public void onClick_ProMode()
		{
			//anim_warning.SetTrigger("show_up");

			selectMode(PlayModeViewModel.PlayMode.Pro);
		}

		public void onClick_Confirm()
		{
			ViewModel.PlayMode.Mode = _currentMode;
			close();
		}

		public void onClick_Background()
		{
			close();
		}
	}
}
