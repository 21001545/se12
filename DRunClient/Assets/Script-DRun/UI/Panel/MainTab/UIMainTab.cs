using Festa.Client.Module;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIMainTab : UISingletonPanel<UIMainTab>
	{
		public Animator animator;
		public Image[] tabImage;
		public Sprite[] normalImage;
		public Sprite[] selectedImage;
		public TMP_Text[] tabText;

		public static class Tab
		{
			public const int statistics = 0;
			public const int ranking = 1;
			//public const int market = 2;
			public const int event_list = 2;
			public const int inventory = 3;
			public const int home = 4;
		}

		private static int trigger_home_on = Animator.StringToHash("home_on");
		private static int trigger_home_off = Animator.StringToHash("home_off");

		private int _current_tab = -1;

		public int getCurrentTab()
		{
			return _current_tab;
		}

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				if( _current_tab == -1)
				{
					applyTab(Tab.home);
				}
			}
		}

		public void onClick(int tab)
		{
			applyTab(tab);
		}

		public void applyTab(int tab)
		{
			if( _current_tab == tab)
			{
				return;
			}

			for(int i = 0; i < tabImage.Length; ++i)
			{
				Image image = tabImage[i];
				if( image != null)
				{
					if (i == tab)
					{
						image.sprite = selectedImage[i];
					}
					else
					{
						image.sprite = normalImage[i];
					}
				}

				TMP_Text text = tabText[i];
				if( text != null)
				{
					if( i == tab)
					{
						text.color = UIStyleDefine.ColorStyle.white;
					}
					else
					{
						text.color = UIStyleDefine.ColorStyle.gray500;
					}
				}
			}

			if( _current_tab != Tab.home && tab == Tab.home)
			{
				animator.SetTrigger(trigger_home_on);
			}
			else if( _current_tab == Tab.home && tab != Tab.home)
			{
				animator.SetTrigger(trigger_home_off);
			}

			_current_tab = tab;

			openCurrentTabPage();
		}

		public void OnEnable()
		{
			if( _current_tab != -1)
			{
				animator.SetTrigger(_current_tab == Tab.home ? trigger_home_on : trigger_home_off);
			}
		}

		public void openCurrentTabPage()
		{
			if (_current_tab == Tab.home)
			{
				UIHome.getInstance().open();
			}
			else if (_current_tab == Tab.statistics)
			{
				UIRecord.getInstance().open();
			}
			else if( _current_tab == Tab.ranking)
			{
				UIRanking.getInstance().open();
			}
			else if (_current_tab == Tab.event_list) 
			{
				//UIMarket.getInstance().open();
				UIEvent.getInstance().open();
			}
			else if (_current_tab == Tab.inventory)
            {
				UIInventory.getInstance().open();
            }
			
		}
	}
}
