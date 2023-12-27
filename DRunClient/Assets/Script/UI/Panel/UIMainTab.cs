using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Module.UI;
using Festa.Client.Module;
using Festa.Client.NetData;
using UnityEngine;
using TMPro;

namespace Festa.Client
{
	public class UIMainTab : UISingletonPanel<UIMainTab>
	{
		public class Tab
		{
			public const int main = 0;
			public const int feed = 1;
			public const int profile = 2;
			public const int map = 3;
			public const int messenger = 4;
			//public const int quest = 4;
		}

        [SerializeField]
        private GameObject go_chatBadge;

        [SerializeField]
        private TMP_Text txt_chatBadgeCount;

        public UIMainTab_Item[] tabItems;

		private UIAbstractPanel[] tabPanels; 
		private int _current_tab = Tab.main;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public override void initSingletonPostProcess(SingletonInitializer initializer)
		{
			base.initSingletonPostProcess(initializer);

			tabPanels = new UIAbstractPanel[5] {
				UIMain.getInstance(),
				UIFeed.getInstance(),
				UIProfile.getInstance(),
				UIMap.getInstance(),
				UIMessengerRoomList.getInstance()
			};

			for (int i = 0; i < tabItems.Length; ++i)
			{
				tabItems[i].setSelection(i == 0);
			}
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();
			
			base.open(param, transitionType, closeType);
		}

		private int getTransitionDirection(int target_tab)
		{
			if( target_tab > _current_tab)
			{
				return CoverUpTransition.Direction.next;
			}
			else
			{
				return CoverUpTransition.Direction.prev;
			}
		}

		public void changeTabIcon(int target_tab)
        {
			if (_current_tab == target_tab)
			{
				return;
			}

			_current_tab = target_tab;

			for (int i = 0; i < tabItems.Length; ++i)
			{
				tabItems[i].setSelection(i == _current_tab);
			}
		}

		public void changeTab(int target_tab)
		{
			if( _current_tab == target_tab)
            {
                return;
            }

			if(UITripStatus.getInstance().isActiveAndEnabled)
				UITripStatus.getInstance().close(TransitionEventType.openImmediately);

            //int direction = getTransitionDirection(target_tab);
            tabPanels[target_tab].open(null, TransitionEventType.openImmediately, TransitionEventType.openImmediately);

			if(target_tab == Tab.map && ClientMain.instance.getViewModel().Trip.Data.status != ClientTripConfig.StatusType.none)
            {
				UITripStatus.getInstance().open(null, TransitionEventType.openImmediately);
				UITripStatus.getInstance().toggleMainTab();
			}

			_current_tab = target_tab;

            for (int i = 0; i < tabItems.Length; ++i)
			{
				tabItems[i].setSelection(i == _current_tab);
			}

			//
			//UIBackNavigation.getInstance().clearStack();
			//UIBackNavigation.getInstance().close();

			ClientMain.instance.getPanelNavigationStack().clear();
		}

		//public void onClickWallet()
		//{
		//	changeTab(Tab.wallet);
		//}

		public void onClickMessenger()
		{
            changeTab(Tab.messenger);
        }

        public void onClickFeed()
		{
			if (_current_tab == Tab.feed )
            {
				UIFeed.getInstance().gotoTop();
            }
			else
            {
                changeTab(Tab.feed);
            }
        }

		public void onClickProfile()
		{
			changeTab(Tab.profile);
		}

		public void onClickMain()
		{
			changeTab(Tab.main);
		}

		public void onClickMap()
        {
//            var device = ClientMain.instance.getLocation().getDevice();
//#if !UNITY_EDITOR
//            // TODO, platform enum 추가 하자.
//            if (device.currentAuthorizationStatus() == 3 || device.currentAuthorizationStatus() == 4)
//#else
//			// 일단... ios 외에는 아직.. TODO
//			if ( true )
//#endif
//            {
//                changeTab(Tab.map);
//            }
//            else
//            {
//                var sc = GlobalRefDataContainer.getInstance().getStringCollection();
//                UIPopup.spawnYesNo(sc.get("map.noaccess.poprup.title", 0), sc.get("map.noaccess.poprup.desc", 0), () =>
//                {
//                    if (device.currentAuthorizationStatus() == 2)
//                    {
//                        if (NativeCamera.CanOpenSettings())
//                        {
//                            NativeCamera.OpenSettings();
//                        }
//                        else
//                        {
//                            // 설정 앱을 열 수 없을 때, 유저에게 직접 설정앱을 열어달라고 요청
//                            UIPopup.spawnOK(sc.get("map.noaccesssetting.poprup.title", 0), sc.get("map.noaccesssetting.poprup.desc", 0), () =>
//                            {

//                            });
//                        }
//                    }
//                    else
//                    {
//                        device.requestPermission();
//                    }
//                }, () => { });
//            }
        }

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			ChatViewModel chatVM = ViewModel.Chat;

			_bindingManager.makeBinding(chatVM, nameof(chatVM.TotalUnreadCount), updateChatTotalUnreadCount);
		}

        private void updateChatTotalUnreadCount(object obj)
        {
            go_chatBadge.SetActive(ViewModel.Chat.TotalUnreadCount > 0);

            if (ViewModel.Chat.TotalUnreadCount > 0)
            {
				if (ViewModel.Chat.TotalUnreadCount > 999)
                {
					txt_chatBadgeCount.text = "999+";
                }
				else
                {
                    txt_chatBadgeCount.text = ViewModel.Chat.TotalUnreadCount.ToString();
                }
			}
		}

		//public void onClickFriend()
		//{
		//	changeTab(Tab.friend);
		//}
	}
}
