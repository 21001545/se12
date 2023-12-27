using DRun.Client.Module;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.ViewModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client 
{
    public class UIMarket : UISingletonPanel<UIMarket>
    {
        #region field

        #region UI ref
        
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
        
        #endregion UI ref
        
        #region data
        
        ClientViewModel ViewModel => ClientMain.instance.getViewModel();
        
        #endregion data

        #endregion field
        
        #region override

        public override void initSingleton(SingletonInitializer initializer) 
        {
            base.initSingleton(initializer);
        }

        public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close) 
        {
            this.resetBindings();
            base.open(param, transitionType, closeType);
        }

        #endregion override
        
        #region behaviours

        private void resetBindings() 
        {
            if (_bindingManager.getBindingList().Count > 0)
                return;

            WalletViewModel walletVM = ViewModel.Wallet;

			_bindingManager.makeBinding(walletVM, nameof(walletVM.DRN_Balance), onChange_wallet);
		}
        private void onChange_wallet(object _) 
        {
            _txt_drnAmount.text = StringUtil.toDRN(ViewModel.Wallet.DRN_Balance.balance).ToString("F2");
        }

        public void onClick_Wallet()
        {
            UIWallet.getInstance().open();
        }

        public void onClick_Setting()
        {
            UISetting.getInstance().open();
        }

        #endregion behaviours
        
    }
}
