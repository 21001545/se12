using Drun.Client;
using DRun.Client.Module;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using Script.Module.UI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DRun.Client.Logic.ProMode;

namespace DRun.Client
{
    public sealed class UIPFPDetail : UISingletonPanel<UIPFPDetail>, IEnhancedScrollerDelegate
    {
		#region field

		[Header("========== Scroller ==========")]
		[ReadOnly]
		[SerializeField]
		private ScrollRect _detailScroller;

		[SerializeField]
        private EnhancedScroller _bottomContainerScrollbar;

        [SerializeField]
        private UIPFPDetailPageCellView _detailPageCellViewPrefab;

        //[SerializeField]
        //private float _upperScrollLimit = 3;    

        //[SerializeField]
        //private float _lowerScrollLimit = -3;

        // top
		[Header("============ Top ==============")]
        public TMP_Text txt_drnAmount;
        public TMP_Text txt_pfpIdLevel;
        public Button btn_levelUp;
        
        ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        private RefStringCollection StrCollection => GlobalRefDataContainer.getStringCollection();

        private UIAbstractPanel _backPanel;

        private UIPFPDetailDataContext _dataContext;

        public UIPFPDetailDataContext DataContext => _dataContext;

        #endregion field

        #region scroll view

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            // Prefab 1 개 내부에 다 넣을 거라 return 1;
            return 1;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            return _detailPageCellViewPrefab.cellHeight;
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            UIPFPDetailPageCellView cellView = scroller.GetCellView(_detailPageCellViewPrefab) as UIPFPDetailPageCellView;
            cellView.setupUI();
            
            return cellView;
        }

        #endregion scroll view

        #region override

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            _bottomContainerScrollbar.Delegate = this;

            _detailScroller = _bottomContainerScrollbar.GetComponent<ScrollRect>();
		}

        public override void open(UIPanelOpenParam param = null, int transitionType = 0,
            int closeType = TransitionEventType.start_close)
        {
            resetBindings();

            base.open(param, transitionType, closeType);

            UIPanelOpenParam_PFPDetail pfp_param = (UIPanelOpenParam_PFPDetail)param;

            _backPanel = pfp_param.BackPanel;
            _dataContext = new UIPFPDetailDataContext(pfp_param.NFTItem, pfp_param.NFTBonus);

            setupTopUI();
        }

        public override void onTransitionEvent(int type)
        {
            if( type == TransitionEventType.start_open)
            {
                setupTopUI();
                _bottomContainerScrollbar.ReloadData();
            }
        }

        public override void close(int transitionType = 0)
        {
            _backPanel.open(null, transitionType);
        }

        public void setupTopUI()
        {
            // 2022.12.19 이강희, 횟수 제한 체크는 버튼을 누를때 하자
            bool canLevelUp = _dataContext.ExperienceRatio >= 1.0f && _dataContext.IsMaxLevel == false;
            if( canLevelUp)
            {
                btn_levelUp.gameObject.SetActive(true);
                txt_pfpIdLevel.gameObject.SetActive(false);
            }
            else
            {
                txt_pfpIdLevel.gameObject.SetActive(true);
                btn_levelUp.gameObject.SetActive(false);
                txt_pfpIdLevel.text = $"#{_dataContext.TokenId} Lv.{_dataContext.Level}";
            }
        }

        public override void update()
        {
	        base.update();

            clampScrollRectRange();
        }

        #endregion override

		#region behaviour

		private void clampScrollRectRange()
		{
			//if (_detailScroller.verticalNormalizedPosition > _upperScrollLimit)
			//{
			//	_detailScroller.verticalNormalizedPosition = Mathf.Clamp(
			//		_detailScroller.verticalNormalizedPosition, 0, _upperScrollLimit
			//	);
			//}

			//if (_detailScroller.verticalNormalizedPosition < _lowerScrollLimit)
			//{
			//	_detailScroller.verticalNormalizedPosition = Mathf.Clamp(
			//		_detailScroller.verticalNormalizedPosition, _lowerScrollLimit, _lowerScrollLimit * 0.95f
			//	);
			//}
		}

		private void resetBindings()
        {
            if (_bindingManager.getBindingList().Count > 0)
                return;

            // bind to the view model.
            WalletViewModel walletVM = ViewModel.Wallet;
            _bindingManager.makeBinding(walletVM, nameof(walletVM.DRN_Balance), onChange_wallet);
        }

        private void onChange_wallet(object _)
        {
            txt_drnAmount.text = StringUtil.toDRNStringDefault(ViewModel.Wallet.DRN_Balance.balance);
        }

        #endregion behaviour

        #region callbacks

        public void onClick_closePFPDetail()
        {
            _backPanel.open();
        }

        public void onClick_levelUp()
        {
            CheckNFTLevelUpLimitProcessor checkLimit = CheckNFTLevelUpLimitProcessor.create(_dataContext.TokenId);
            checkLimit.run(result => { 
                if( result.failed())
                {
                    return;
                }

                if( checkLimit.getCheckResult() == false)
                {
                    int daily_limit = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.DRun.daily_levelup_limit, 2);
                    string message = StrCollection.getFormat("pro.levelup.fail.daily_limit", 0, daily_limit);

                    UIToast.spawn(message)
                        .autoClose(3)
                        .setType(UIToastType.alert)
                        .withTransition<FadePanelTransition>();
                    return;
                }

                UIConfirmLevelUp.getInstance().open(new UIPanelOpenParam_ConfirmLevelUp
                {
                    tokenLevelup = _dataContext.TokenId,
                    whereLevelUp = WhereLevelUp.ProMode,
                    afterLevelUp = onCompleteLevelup,
                    formattedPurchaseCost = StrCollection.getFormat("pro.purchase", 0, _dataContext.LevelUpCostAsStr),
                    levelUpCost = _dataContext.LevelUpCost
                });
            });
        }

        private void onCompleteLevelup()
        {
            // NFTItem객체가 갱신되기 때문에.
            _dataContext.NFTItem = ViewModel.Wallet.getNFTItem(_dataContext.TokenId);

            setupTopUI();

            _bottomContainerScrollbar.RefreshActiveCellViews();


            //_dataContext.levelUpCount++;
            //
            // _txt_pfpIdLevel.text = $"#{TokenId} Lv.{(IsMaxLevel ? "Max" : Level)}";

            // TODO: 만렙일 때 어떤 처리??
        }

        #endregion callbacks
    }
}