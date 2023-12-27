using DRun.Client.Logic;
using DRun.Client.Logic.Wallet;
using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Festa.Client.CSMessageID;

namespace DRun.Client
{
	public class UISpending : UISingletonPanel<UISpending>
	{
		public TMP_Text text_from;
		public TMP_Text text_to;
		
		public Image image_asset;
		public TMP_Text text_assetname;
		public UIInputField input_amount;

		public TMP_Text text_balance;
		public TMP_Text text_balance2;
		public TMP_Text text_gasfee;
		public TMP_Text text_amount_placeholder;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private WalletWithdrawViewModel vmWithdraw => ViewModel.Withdraw;
		private WalletViewModel vmWallet => ViewModel.Wallet;
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			resetBindings();
			base.open(param, transitionType, closeType);

			input_amount.text = "";
			input_amount.hideClearButton();

			// 가스비 업데이트
			GetFeeProcessor step = GetFeeProcessor.create();
			step.run(result => {
				updateWalletFee();
			});
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			
			_bindingManager.makeBinding(vmWithdraw, nameof(vmWithdraw.From), updateFrom);
			_bindingManager.makeBinding(vmWithdraw, nameof(vmWithdraw.To), updateTo);
			_bindingManager.makeBinding(vmWithdraw, nameof(vmWithdraw.AssetType), updateAssetType);
		}

		private void updateFrom(object obj)
		{
			text_from.text = vmWithdraw.getAddressName(vmWithdraw.From);

			if( vmWithdraw.From == WalletWithdrawViewModel.AddressConst.spending)
			{
				text_amount_placeholder.text = StringCollection.get("wallet.withdraw.amount_placeholder_to_wallet", 0);
			}
			else
			{
				text_amount_placeholder.text = StringCollection.get("wallet.withdraw.amount_placeholder", 0);
			}

			updateBalance();
			updateWalletFee();
		}

		private void updateTo(object obj)
		{
			text_to.text = vmWithdraw.getAddressName(vmWithdraw.To);
		}

		private void updateAssetType(object obj)
		{
			text_assetname.text = vmWithdraw.getAssetTypeName(vmWithdraw.AssetType);

			updateBalance();
			updateWalletFee();
		}

		//
		private void updateBalance()
		{
			string stringBalance = getBalance();

			text_balance2.text = text_balance.text = StringCollection.getFormat("wallet.withdraw.btn_balance", 0, stringBalance, vmWithdraw.getAssetTypeName(vmWithdraw.AssetType));
		}

		private void updateWalletFee()
		{
			string fee = vmWallet.getFee(vmWithdraw.AssetType);

			if( vmWithdraw.From == WalletWithdrawViewModel.AddressConst.spending)
			{
				text_gasfee.text = "";
			}
			else
			{
				text_gasfee.text = StringCollection.getFormat("wallet.withdraw.gasfee", 0, fee);
			}
		}

		private string getBalance()
		{
			if (vmWithdraw.From == WalletWithdrawViewModel.AddressConst.spending)
			{
				return StringUtil.toDRNStringDefault(ViewModel.Wallet.DRN_Balance.balance);
			}
			else if(vmWithdraw.From == WalletWithdrawViewModel.AddressConst.wallet)
			{
				ClientWalletBalance balance = ViewModel.Wallet.getWalletBalance(ClientWalletBalance.AssetType.DRNT);
				return balance.getDisplayLiquid();
			}

			return "0";
		}

		private long getBalancePeb()
		{
			if (vmWithdraw.From == WalletWithdrawViewModel.AddressConst.spending)
			{
				return ViewModel.Wallet.DRN_Balance.balance;

			}
			else if (vmWithdraw.From == WalletWithdrawViewModel.AddressConst.wallet)
			{
				ClientWalletBalance balance = ViewModel.Wallet.getWalletBalance(ClientWalletBalance.AssetType.DRNT);

				long balance_peb = GlobalRefDataContainer.getRefDataHelper().toPeb(balance.liquid);

				return balance_peb;
			}

			return 0;
		}

		public void onClick_Back()
		{
			UIWallet.getInstance().open();
		}

		public void onClick_Swap()
		{
			vmWithdraw.SwapFromTo();
		}

		public void onClick_SelectAssetType()
		{
			List<int> assetList = new List<int>();
			assetList.Add(ClientWalletBalance.AssetType.DRNT);
			UISelectAssetType.getInstance().open(StringCollection.get("wallet.withdraw.asset_title", 0), vmWithdraw.AssetType, assetList, selectResult => {
				vmWithdraw.AssetType = selectResult.assetType;
				vmWithdraw.TokenID = selectResult.tokenID;
			});
		}

		public void onClick_SetAmountAll()
		{
			input_amount.text = getBalance();
		}

		public void onValueChanged_Amount(string value)
		{

		}

		public void onClick_Send()
		{
			long amount = getInputAmount();
			long balance = getBalancePeb();

			// 입력 오류
			if ( amount == 0)
			{
				UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.zero_amount", 0));
				return;
			}

			// 자릿수 체크
			if( StringUtil.GetSignificantNumberOfDecimalPlaces(StringUtil.toDRN(amount)) > 4)
			{
				UIPopup.spawnError(StringCollection.getFormat("wallet.withdraw.popup.invalid_decimalplaces", 0, 4));
				return;
			}

			// 잔액 부족
			if( amount >= balance)
			{
				UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.notenough_balance", 0));
				return;
			}

			// 최소 보유 금액 체크
			if(vmWithdraw.From == WalletWithdrawViewModel.AddressConst.spending)
			{
				double min_balance = GlobalRefDataContainer.getConfigDouble(RefConfig.Key.Wallet.withraw_drnt_min, 22.5);
				long min_balance_peb = GlobalRefDataContainer.getRefDataHelper().toPeb(min_balance);
				if( balance < min_balance_peb)
				{
					UIPopup.spawnError(StringCollection.getFormat("wallet.withdraw.popup.low_balance", 0, min_balance));
					return;
				}
			}

			BaseLogicStepProcessor step = null;
			string confirm_message = "";

			if( vmWithdraw.From == WalletWithdrawViewModel.AddressConst.spending)
			{
				step = SpendingToWalletProcessor.create(amount);
				confirm_message = StringCollection.getFormat("wallet.withdraw.popup.spending_to_wallet", 0, StringUtil.toDRNStringDefault(amount));
			}
			else if(vmWithdraw.From == WalletWithdrawViewModel.AddressConst.wallet)
			{
				step = WalletToSpendingProcessor.create(amount);
				confirm_message = StringCollection.getFormat("wallet.withdraw.popup.wallet_to_spending", 0, StringUtil.toDRNStringDefault(amount));
			}
			else
			{
				throw new System.Exception("invalid withdraw address");
			}


			UIPopup.spawnYesNo(confirm_message, () => {

				step.run(result => {

					if (result.failed())
					{
						if( result.cause().Message.Contains("ERR_0418003"))
						{
							UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.fail.notenough_fee", 0));
						}
						else
						{
							UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.fail", 0));
						}
					}
					else
					{
						UIWallet.getInstance().open();
						UIPopup.spawnOK(StringCollection.get("wallet.withdraw.popup.success", 0));
					}
				});

			});
		}

		private long getInputAmount()
		{
			try
			{
				return GlobalRefDataContainer.getRefDataHelper().toPeb(input_amount.text);
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				return 0;
			}
		}

	}
}
