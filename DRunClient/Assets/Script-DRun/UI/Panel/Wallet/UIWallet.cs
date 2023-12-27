using System;
using Drun.Client;
using Drun.Client.Logic.Wallet;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using UnityEngine;

namespace DRun.Client
{
    [RequireComponent( typeof(SlideUpDownTransition))]
    public class UIWallet : UISingletonPanel<UIWallet>
    {
        public UIColorToggleButton[] tabButtons;
        public UIWalletPage[] pages;
        public UITabSlide tabSlide;

        [SerializeField]
        [ReadOnly]
        private RectTransform _selfRectTransform;

        public class TabType
        {
            public const int spending = 0;
            public const int wallet = 1;
        }

        private int _currentTab;
        private IntervalTimer _timerIncompleteTransaction;

        private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private ClientNetwork Network => ClientMain.instance.getNetwork();

        public IntervalTimer getTimerIncompleteTransaction()
        {
            return _timerIncompleteTransaction;
        }

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            foreach(UIWalletPage page in pages)
            {
                page.initialize();
            }

            _currentTab = -1;
            _timerIncompleteTransaction = IntervalTimer.create(1.0f, false, false);

            _selfRectTransform = GetComponent<RectTransform>();
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
        {
            resetBindings();

            base.open(param, transitionType, closeType);
            
            // 2022-12-27 윤상
            // close 시 transition animation 이후 원래 위치로 이동 안해서, 강제로 이동.
            // 아마 UIPanel 설정 시에 포지션이 다른 곳으로 이동되서??
            _selfRectTransform.anchoredPosition = Vector2.zero;
        }

        public override void onTransitionEvent(int type)
        {
            if( type == TransitionEventType.start_open)
            {
                if( _currentTab == -1)
                {
                    setTab(TabType.spending, true);
                }
                else
                {
                    // 2023.4.04 - 계정전환하면 이럴 수 있다
                    if( _currentTab == TabType.wallet && ViewModel.Wallet.WalletPinHashChecked == false)
                    {
                        setTab(TabType.spending, true);
                    }
                    else
                    {
						// 정보 갱신필요
						pages[_currentTab].onShow();
					}
				}
            }
        }

        public override void update()
        {
            base.update();

            foreach(UIWalletPage page in pages)
            {
                page.update();
            }

            processIncompleteTransaction();
        }

        private void resetBindings()
        {
            if( _bindingManager.getBindingList().Count > 0)
            {
                return;
            }

            foreach(UIWalletPage page in pages)
            {
                page.resetBinding(_bindingManager);
            }

        }
		
        public void setTab(int tab, bool updateNow)
        {
            if( _currentTab == tab)
            {
                return;
            }

            _currentTab = tab;
            for(int i = 0; i < pages.Length; ++i)
            {
                UIWalletPage page = pages[i];
                page.gameObject.SetActive(i == tab);
                if( i == tab)
                {
                    page.onShow();
                }

                tabButtons[i].setStatus(i == tab);
            }

            tabSlide.setTab(tab, updateNow);
        }

        public void onClick_Spending()
        {
            setTab(TabType.spending, false);
        }

        public void onClick_Wallet()
        {
            if ( ViewModel.Wallet.Wallet == null)
            {
                UICreateWallet.getInstance().open();
            }
            else
            {
                if( ViewModel.Wallet.WalletPinHashChecked == false)
                {
                    UICheckWalletPinHash.getInstance().open(result => { 
                        UIWallet.getInstance().open(transitionType: TransitionEventType.openImmediately);
                        
                        if (result)
                        {
                            UIWallet.getInstance().setTab(TabType.wallet, true);
                        }

                    });
                }
                else
                {
                    setTab(TabType.wallet, false);
                }
            }
        }

        public void onClick_Back()
        {
            UIMainTab.getInstance().open();
            //UIPanelTransitionHelper.moveInDirection(
            //    delta: (
            //        from: 0,
            //        to: Screen.width * 0.5f
            //    ),
            //    targetRectTransform: _selfRectTransform
            //);
            _selfRectTransform.moveInDirection(delta: (from: 0, to: Screen.width * 0.5f));
        }

        private void processIncompleteTransaction()
        {
            //
            if( ClientMain.instance.getFSM().getCurrentState().getType() != ClientStateType.run)
            {
                return;
            }

            if( _timerIncompleteTransaction.update() == false)
            {
                return;
            }

            ProcessIncompleteTransactionProcessor step = ProcessIncompleteTransactionProcessor.create();
            step.run(result => { 
                if( result.failed())
                {
                    _timerIncompleteTransaction.setNext(10.0f);
                }
                else
                {
                    // 남아 있으면 좀더 빠르게 체크
                    if( step.getIncompleteList().Count > 0)
                    {
                        _timerIncompleteTransaction.setNext(1.0f);
                    }
                    else
                    {
                        _timerIncompleteTransaction.setNext(10.0f);
                    }
                }
            });
        }
    }
}