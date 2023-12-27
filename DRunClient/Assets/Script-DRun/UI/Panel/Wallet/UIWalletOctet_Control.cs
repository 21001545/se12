using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.RefData;
using System.Collections.Generic;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIWalletOctet_Control : UIWalletScrollerCellView
	{
		public TMP_Text drn_balance;
		public TMP_Text wallet_address;

		public Animator tooltipAnimator;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

        [SerializeField] 
        private Vector2 _copyAddressMessageSpawnPosition = new(20, -704);

		public void setup()
		{
			ClientWalletBalance data = ViewModel.Wallet.getWalletBalance(ClientWalletBalance.AssetType.DRNT);
			if (data != null)
			{
				drn_balance.text = data.getDisplayLiquid();
			}

			string address = ViewModel.Wallet.Wallet.address;
			if( address.Length > 16)
			{
				string first = address.Substring(0, 8);
				string second = address.Substring(address.Length - 8, 8);
				address = $"{first}...{second}";
			}

			wallet_address.text = address;
		}

		public void onClick_WalletAddress()
        {
            GUIUtility.systemCopyBuffer = ViewModel.Wallet.Wallet.address;

            string addressCopiedDisplayText = GlobalRefDataContainer.getStringCollection().get("wallet.octet.address_copy", 0);
            UIToast.spawn(addressCopiedDisplayText, _copyAddressMessageSpawnPosition)
                .autoClose(2.5f)
                .setType(UIToastType.check)
                .useBackdrop(false)
                .withTransition<FadePanelTransition>();
        }

		public void onClick_Receive()
		{
			UIWalletReceive.getInstance().open();
		}

		public void onClick_ToSpending()
		{
			if( GlobalConfig.isOpenWalletFeature() == false)
			{
				spawnComingSoon();
				return;
			}

			ViewModel.Withdraw.resetForWalltToSpending();
			UISpending.getInstance().open();
		}

		public void onClick_ToExternal()
		{
			if (GlobalConfig.isOpenWalletFeature() == false)
			{
				spawnComingSoon();
				return;
			}

			List<int> assetList = new List<int>();
			assetList.Add(ClientWalletBalance.AssetType.DRNT);
			assetList.Add(ClientWalletBalance.AssetType.ETH);
			assetList.Add(ClientWalletBalance.AssetType.NFT);

			UISelectAssetType.getInstance().open(StringCollection.get("wallet.withdraw.asset_title_for_external", 0),
				ClientWalletBalance.AssetType.DRNT, assetList, selectResult =>
				{
					if( selectResult.assetType != ClientWalletBalance.AssetType.NONE)
					{
						ViewModel.Withdraw.resetForExternal();
						ViewModel.Withdraw.AssetType = selectResult.assetType;
						ViewModel.Withdraw.TokenID = selectResult.tokenID;
						UIWithdrawToExternal.getInstance().open();
					}
				});
		}

		public void onClick_Trade()
		{
			spawnComingSoon();
		}

		public void onClick_Tooltip()
		{
			tooltipAnimator.SetTrigger("show");
		}

		private void spawnComingSoon()
		{
			UIToast.spawn(
					StringCollection.get("popup.coming_soon", 0),
					new(20, -606))
				.setType(UIToastType.error)
				.withTransition<FadePanelTransition>()
				.autoClose(3.0f);
		}
	}
}
