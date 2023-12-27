using DRun.Client.Logic.Wallet;
using DRun.Client.Module;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIWithdrawToExternal : UISingletonPanel<UIWithdrawToExternal>
	{
		public Image image_asset;
		public UIPhotoThumbnail image_pfp;
		public TMP_Text text_assetname;
		public TMP_Text text_assetname_amount;

		public UIInputField input_address;
		public UIInputField input_amount;

		public TMP_Text text_balance;
		public TMP_Text text_balance2;
		public TMP_Text text_gasfee;
		public TMP_Text text_gasfee_nft;

		public GameObject root_token;
		public GameObject root_nft;

		public Sprite[] spriteAssetIcons;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private WalletWithdrawViewModel vmWithdraw => ViewModel.Withdraw;
		private WalletViewModel vmWallet => ViewModel.Wallet;
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			resetBindings();
			base.open(param, transitionType, closeType);

			input_address.text = "";
			input_address.hideClearButton();

			input_amount.text = "";
			input_amount.hideClearButton();
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			// 제일 나중에 설정되는 값으로 설정
			_bindingManager.makeBinding(vmWithdraw, nameof(vmWithdraw.TokenID), updateAssetType);
		}

		private void updateAssetType(object obj)
		{

			updateAssetType();
			updateBalance();
			updateWalletFee();
		}

		private void updateAssetType()
		{
			if( vmWithdraw.AssetType == ClientWalletBalance.AssetType.NFT)
			{
				image_asset.gameObject.SetActive(false);
				image_pfp.gameObject.SetActive(true);

				root_token.SetActive(false);
				root_nft.SetActive(true);

				text_assetname.text = vmWithdraw.getAssetTypeName(vmWithdraw.AssetType) + $" #{vmWithdraw.TokenID}";
				text_assetname_amount.text = vmWithdraw.getAssetTypeName(vmWithdraw.AssetType);

				image_pfp.setEmpty();

				ClientMain.instance.getNFTMetadataCache().getMetadata(vmWithdraw.TokenID, cache => { 
					if( cache != null)
					{
						image_pfp.setImageFromCDN(cache.imageUrl);
					}
				});
			}
			else
			{
				image_asset.gameObject.SetActive(true);
				image_pfp.gameObject.SetActive(false);

				root_token.SetActive(true);
				root_nft.SetActive(false);

				image_asset.sprite = spriteAssetIcons[vmWithdraw.AssetType - 1];
				text_assetname.text = vmWithdraw.getAssetTypeName(vmWithdraw.AssetType);
				text_assetname_amount.text = vmWithdraw.getAssetTypeName(vmWithdraw.AssetType);
			}
		}

		private void updateBalance()
		{
			string stringBalance = getBalance();
			text_balance2.text = text_balance.text = StringCollection.getFormat("wallet.withdraw.btn_balance", 0, stringBalance, vmWithdraw.getAssetTypeName(vmWithdraw.AssetType));
		}

		private void updateWalletFee()
		{
			string fee = vmWallet.getFee(vmWithdraw.AssetType);

			text_gasfee_nft.text = text_gasfee.text = StringCollection.getFormat("wallet.withdraw.gasfee", 0, fee);
		}

		private string getBalance()
		{
			ClientWalletBalance balance = ViewModel.Wallet.getWalletBalance(vmWithdraw.AssetType);
			if( balance == null)
			{
				return "0";
			}
			
			return balance.getDisplayLiquid();
		}

		private double getBalanceDouble()
		{
			return double.Parse(getBalance());
		}

		public void onClick_Back()
		{
			UIWallet.getInstance().open();
		}

		public void onClick_Send()
		{
			string address = input_address.text;

			// 주소 체크
			if( string.IsNullOrEmpty(address))
			{
				UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.empty_receiver_address", 0));
				return;
			}

			if( vmWithdraw.AssetType == ClientWalletBalance.AssetType.NFT)
			{
				if (checkGasFee() == false)
				{
					UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.fail.notenough_fee", 0));
					return;
				}

				sendNFT(address);
			}
			else
			{
				sendToken(address);
			}
		}

		private bool checkGasFee()
		{
			ClientWalletBalance balance = ViewModel.Wallet.getWalletBalance(ClientWalletBalance.AssetType.ETH);
			if( balance == null)
			{
				return false;
			}


			double liquid = double.Parse(balance.getDisplayLiquid(),CultureInfo.InvariantCulture);
			double fee = double.Parse(ViewModel.Wallet.getFee(ClientWalletBalance.AssetType.NFT), CultureInfo.InvariantCulture);

			if( liquid < fee)
			{
				return false;
			}

			return true;
		}

		private void sendNFT(string address)
		{
			string confirm_message = StringCollection.getFormat("wallet.withdraw.popup.wallet_to_external_nft", 0, $"#{vmWithdraw.TokenID}");

			UIPopup.spawnYesNo(confirm_message, () => {
				NFTItemToExternalProcessor step = NFTItemToExternalProcessor.create(address, vmWithdraw.TokenID);
				step.run(result => { 
					if( result.failed())
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

		private void sendToken(string address)
		{
			double amount = getInputAmount();
			double balance = getBalanceDouble();

			// 입력 오류
			if (amount == 0)
			{
				UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.zero_amount", 0));
				return;
			}

			int sigNumber = StringUtil.GetSignificantNumberOfDecimalPlaces(amount);
			int sigNumberLimit = 3;

			if( vmWithdraw.AssetType == ClientWalletBalance.AssetType.ETH)
			{
				sigNumberLimit = 4;
			}
			else if( vmWithdraw.AssetType == ClientWalletBalance.AssetType.DRNT)
			{
				sigNumberLimit = 4;
			}

			// 자릿수 체크
			if ( sigNumber > sigNumberLimit)
			{
				UIPopup.spawnError(StringCollection.getFormat("wallet.withdraw.popup.invalid_decimalplaces", 0, sigNumberLimit));
				return;
			}

			// 잔액 부족
			if (amount >= balance)
			{
				UIPopup.spawnError(StringCollection.get("wallet.withdraw.popup.notenough_balance", 0));
				return;
			}

			string confirm_message = StringCollection.getFormat("wallet.withdraw.popup.wallet_to_external", 0, amount, vmWithdraw.getAssetTypeName(vmWithdraw.AssetType));

			UIPopup.spawnYesNo(confirm_message, () => {

				WalletToExternalProcessor step = WalletToExternalProcessor.create(address, vmWithdraw.AssetType, amount.ToString());
				step.run(result => {
					if (result.failed())
					{
						if (result.cause().Message.Contains("ERR_0418003"))
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

		public void onClick_SelectAssetType()
		{
			List<int> assetList = new List<int>();
			assetList.Add(ClientWalletBalance.AssetType.DRNT);
			assetList.Add(ClientWalletBalance.AssetType.ETH);
			assetList.Add(ClientWalletBalance.AssetType.NFT);
			UISelectAssetType.getInstance().open(StringCollection.get("wallet.withdraw.asset_title_for_external", 0), vmWithdraw.AssetType, assetList, selectResult => {
				if( selectResult.assetType != ClientWalletBalance.AssetType.NONE)
				{
					vmWithdraw.AssetType = selectResult.assetType;
					vmWithdraw.TokenID = selectResult.tokenID;	// 값이 변하지 않아서 호출이 않되는군아

					updateAssetType(null);
				}
			});
		}

		public void onClick_SetAmountAll()
		{
			input_amount.text = getBalance();
		}

		//
		private double getInputAmount()
		{
			try
			{
				return double.Parse(input_amount.text);
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				return 0;
			}
		}

	}
}
