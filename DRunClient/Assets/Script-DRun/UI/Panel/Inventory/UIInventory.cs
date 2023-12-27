using DRun.Client.Logic.ProMode;
using DRun.Client.Logic.Wallet;
using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
    public class UIInventory : UISingletonPanel<UIInventory>, IEnhancedScrollerDelegate
    {
		#region field

		#region UI ref

		[Header("========== Main ==========")]
		public GameObject page_nothing;
		public GameObject page_pfp;
        public TMP_Text text_select_pfp;
        public Image image_select_pfp;
        public Sprite[] sprite_select_pfp;
        public GameObject loading;

		[Space(20)]
        [Header("========== DRNT ==========")]
        [SerializeField]
        private TMP_Text _txt_drnAmount;

        [Space(10)]
        [Header("========== Top Icon ==========")]
        [SerializeField]
        private Button _btn_noti;

        [SerializeField]
        private Button _btn_profile;

        [Header("========== Scroller ==========")]
        public EnhancedScroller scroller;
        public UIInventoryCell_PFPItem cellPFPItem;


        #endregion UI ref

        #region data

        private bool _selectPFP;

        public bool SelectPFP => _selectPFP;

        ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        #endregion data

        #endregion field

        #region override

        public override void initSingleton(SingletonInitializer initializer)
        {
            base.initSingleton(initializer);

            scroller.Delegate = this;
		}

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
        {
            this.resetBindings();
            base.open(param, transitionType, closeType);

			
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
            {
				_selectPFP = false;
				updateSelectPFPButton();
				loadData();
            }
		}

		#endregion override

		#region behaviours

		private void resetBindings()
        {
            if (_bindingManager.getBindingList().Count > 0)
                return;

            WalletViewModel walletVM = ViewModel.Wallet;
            ProModeViewModel proVM = ViewModel.ProMode;

			_bindingManager.makeBinding(walletVM, nameof(walletVM.DRN_Balance), onChange_wallet);
			_bindingManager.makeBinding(walletVM, nameof(walletVM.NFTITemMap), onChange_NFTItemMap);
            _bindingManager.makeBinding(proVM, nameof(proVM.EquipedNFTItem), onChange_EquipedNFTItem);
        }
        private void onChange_wallet(object _)
        {
            _txt_drnAmount.text = StringUtil.toDRN(ViewModel.Wallet.DRN_Balance.balance).ToString("F2");
        }
        
        private void onChange_NFTItemMap(object obj)
        {
            if( scroller.Container != null)
            {
				scroller.ReloadData();
			}

            page_nothing.SetActive(ViewModel.Wallet.getNFTItemList().Count == 0);
		}

		private void onChange_EquipedNFTItem(object obj)
        {
            if(scroller.Container != null)
            {
				scroller.RefreshActiveCellViews();
			}
		}

		private void loadData()
        {
            // 지갑이 없네
            if( ViewModel.Wallet.Wallet == null)
            {
				//if (scroller.Container != null)
				//{
				//	scroller.ReloadData();
				//}

				page_nothing.SetActive(true);
                page_pfp.SetActive(false);
				loading.SetActive(false);
				return;
            }

            page_nothing.SetActive(false);
            page_pfp.SetActive(true);
            loading.SetActive(true);

            if( scroller.Container != null)
            {
				scroller.ReloadData();
			}

            List<BaseStepProcessor> stepList = new List<BaseStepProcessor>();
            stepList.Add(QueryNFTProcessor.create());
            stepList.Add(UpdateNFTBonusProcessor.create());

            BaseStepProcessor.runSteps(stepList.GetEnumerator(), result => {
				loading.SetActive(false);
			});
		}

        public void onClick_SelectPFP()
        {
            if( checkChangeLimit() == false)
            {
                return;
            }


            _selectPFP = !_selectPFP;
            updateSelectPFPButton();
            scroller.RefreshActiveCellViews();
        }

        public void endSelectPFP()
        {
            _selectPFP = false;
            updateSelectPFPButton();
			scroller.RefreshActiveCellViews();
		}

        private void updateSelectPFPButton()
        {
            if( _selectPFP)
            {
				text_select_pfp.text = StringCollection.get("inventory.select_pfp.on", 0);
				image_select_pfp.sprite = sprite_select_pfp[1];

				image_select_pfp.color = text_select_pfp.color = UIStyleDefine.ColorStyle.gray200;
			}
			else
            {
                text_select_pfp.text = StringCollection.get("inventory.select_pfp.off", 0);
                image_select_pfp.sprite = sprite_select_pfp[0];

				image_select_pfp.color = text_select_pfp.color = UIStyleDefine.ColorStyle.gray400;
			}

        }

        public void onClick_Setting()
        {
            UISetting.getInstance().open();
        }

        public void onClick_Wallet()
        {
            UIWallet.getInstance().open();
        }

        private bool checkChangeLimit()
        {
            ClientNFTChangeLimit changeLimit = ViewModel.ProMode.NFTChangeLimit;
            if( changeLimit == null)
            {
                return true;
            }

            DateTime utcNow = DateTime.UtcNow;
            if( utcNow >= changeLimit.change_unlock_time)
            {
                return true;
            }

            TimeSpan remainTime = changeLimit.change_unlock_time - utcNow;

            string message = StringCollection.getFormat("inventory.select_pfp.fail.change_limit", 0, remainTime.Hours, remainTime.Minutes);

            UIPopup.spawnError(message);

            return false;
        }


		#endregion behaviours

		#region ScrollerDelegate

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
            if( ViewModel.Wallet.Wallet == null)
            {
                return 0;
            }

            return ViewModel.Wallet.getNFTItemList().Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
            return 128.0f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
            UIInventoryCell_PFPItem cell = (UIInventoryCell_PFPItem)scroller.GetCellView(cellPFPItem);
            cell.setup(ViewModel.Wallet.getNFTItemList()[dataIndex]);
            return cell;
		}

		#endregion

	}
}
