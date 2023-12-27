using Festa.Client.Module;
using Festa.Client.Module.UI;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using Festa.Client.Logic;
using Festa.Client.ViewModel;
using UnityEngine.EventSystems;
using Festa.Client.RefData;
using Festa.Client.Module.Net;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIFriend : UISingletonPanel<UIFriend>
	{
		public RectTransform rtIndiciator;
		public TMP_Text[] tabTexts;
		public UIFixedLayer pageLayer;
		public Button btnBack;

		private bool _initIndicator;
		private Vector2SmoothDamper _tabIndicatorDamper;
		private Vector2[] _tabIndicatorPos;
		private int _curTab;

		[SerializeField]
		private UIFriend_Page_Friend _friendPage;

        [SerializeField]
        private UIFriend_Page_FollowBack _followerPage;

        [SerializeField]
        private GameObject go_upgradeSlotPopup;

        private int _target_account_id;
		private ClientProfileCache _profileCache;

		private SocialViewModel _socialViewModel;
		public SocialViewModel SocialViewModel
		{
			get
			{
				return _socialViewModel;
			}
			set
			{
				Set(ref _socialViewModel, value);
			}
		}

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public int getTargetAccountID()
		{
			return _target_account_id;
		}

		public bool isMyAccount()
		{
			return _target_account_id == Network.getAccountID();
		}

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_tabIndicatorDamper = Vector2SmoothDamper.create(Vector2.zero, 0.1f);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			open(Network.getAccountID(), param);
			base.open(param, transitionType, closeType);

			if (_initIndicator == false)
			{
				initIndicator();
			}

			setupBackNavigation(param);

			if( param != null)
			{
				_target_account_id = param.accountID;
			}
			else
			{
				_target_account_id = Network.getAccountID();
			}

			ClientMain.instance.getProfileCache().getProfileCache(_target_account_id, result => {
				if (result.succeeded())
				{
					_profileCache = result.result();

					SocialViewModel = _profileCache.Social;

					setTab(0);
					
					_friendPage.gameObject.SetActive(false);
                    _followerPage.gameObject.SetActive(false);
                }
			});
		}

        public override void onTransitionEvent(int type)
        {
            base.onTransitionEvent(type);

			if(type == TransitionEventType.end_open)
            {
				_friendPage.gameObject.SetActive(true);
				_friendPage.open();
			}
        }

        private void setupBackNavigation(UIPanelOpenParam param)
		{
			if( param != null && param.hasBackNavigation)
			{
				btnBack.gameObject.SetActive(true);
			}
			else
			{
				btnBack.gameObject.SetActive(false);
			}
		}

		public void open(int target_account_id,UIPanelOpenParam param)
		{
		}

		private void initIndicator()
		{
			_initIndicator = true;

			//RectTransform rtIndicatorParent = rtIndiciator.parent as RectTransform;

			//Vector2 size = rtIndiciator.sizeDelta;
			//size.x = rtIndicatorParent.rect.width / 2;

			//rtIndiciator.sizeDelta = size;

			_tabIndicatorPos = new Vector2[2];
			_tabIndicatorPos[0] = new Vector2(- rtIndiciator.rect.width / 2f, 0);
			_tabIndicatorPos[1] = new Vector2(rtIndiciator.rect.width / 2f, 0);
		}

		public override void update()
		{
			base.update();

			updateTabIndicator();
		}

		public void onClickFollow()
		{
			if (_curTab != 0 )
            {
                setTab(0);

                _friendPage.gameObject.SetActive(true);
                _followerPage.gameObject.SetActive(false);
				_friendPage.open();
            }
		}

		public void onClickFollowBack()
		{
			if ( _curTab != 1 )
            {
                setTab(1);
                _friendPage.gameObject.SetActive(false);
                _followerPage.gameObject.SetActive(true);
                _followerPage.open();
            }
        }

		private void updateTabIndicator()
		{
			if( _tabIndicatorDamper.update())
			{
				rtIndiciator.anchoredPosition = _tabIndicatorDamper.getCurrent();
			}
		}

		private void setTab(int pos)
		{
			for(int i = 0; i < tabTexts.Length; ++i)
			{
				tabTexts[i].color = i == pos ? ColorChart.gray_900 : ColorChart.gray_400;
			}

			_tabIndicatorDamper.setTarget(_tabIndicatorPos[pos]);
			_curTab = pos;
		}

		public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
		}

		public void moveToProfile(int account_id)
		{
			UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
			param.accountID = account_id;

			UIProfile.getInstance().open(param);
			//UIFriend.getInstance().close();

			ClientMain.instance.getPanelNavigationStack().push(UIFriend.getInstance(), UIProfile.getInstance());
		}

		public void requestUpgradeFriendSlot()
        {
            if (isMyAccount() == false)
            {
                return;
            }

            RefFriendSlot refFriendSlot = GlobalRefDataContainer.getInstance().get<RefFriendSlot>(_socialViewModel.FriendConfig.slot_level + 1);
            if (refFriendSlot == null)
            {
                // 다음 레벨은 없다
                return;
            }

            RefShopItem refShopItem = GlobalRefDataContainer.getInstance().get<RefShopItem>(refFriendSlot.purchase_shop_item_id);
            if (refShopItem == null)
            {
                // 오류
                return;
            }

            if (ClientMain.instance.getViewModel().Wallet.FestaCoin < refShopItem.cost)
            {
                UIPopup.spawnOK(StringCollection.get("tree.purchase.popup.notenough.coin", 0));
                return;
            }

            UIBlockingInput.getInstance().open();

            MapPacket req = Network.createReq(CSMessageID.Shop.PurchaseShopItemReq);
            req.put("id", refShopItem.id);

            Network.call(req, ack =>
            {
                UIBlockingInput.getInstance().close();

                if (ack.getResult() == ResultCode.ok)
                {
                    ClientMain.instance.getViewModel().updateFromPacket(ack);
                    //_friendPage.updateFriendCount();
					_followerPage.updateFriendCount();
                }

				onClickCloseUpgradeFriendSlotLevel();

			});
        }

		public void onClickAddFriends()
        {
			UIAddFriend.getInstance().open();
			ClientMain.instance.getPanelNavigationStack().push(this, UIAddFriend.getInstance());
		}

		public void onClickUpgradeFriendSlotLevel()
        {
			go_upgradeSlotPopup.SetActive(true);
		}

		public void onClickCloseUpgradeFriendSlotLevel()
        {
            go_upgradeSlotPopup.SetActive(false);
        }
	}
}
