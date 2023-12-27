using DRun.Client.Logic.Wallet;
using DRun.Client.NetData;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UISelectWithdrawNFTItem : UISingletonPanel<UISelectWithdrawNFTItem>, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;
		public UISelectWithdrawNFTItem_Cell cellSource;
		public GameObject loading;

		private UnityAction<UISelectAssetType.SelectResult> _callback;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
		}

		public void open(UnityAction<UISelectAssetType.SelectResult> callback)
		{
			resetBindings();
			base.open();

			_callback = callback;
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				loadData();
			}
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			WalletViewModel walletVM = ViewModel.Wallet;

			_bindingManager.makeBinding(walletVM, nameof(walletVM.NFTITemMap), onChange_NFTItemMap);
		}

		private void onChange_NFTItemMap(object obj)
		{
			if( scroller.Container != null)
			{
				scroller.ReloadData();
			}
		}

		private void loadData()
		{
			loading.SetActive(true);
			if (scroller.Container != null)
			{
				scroller.ReloadData();
			}

			QueryNFTProcessor step = QueryNFTProcessor.create();
			step.run(result => {
				loading.SetActive(false);
			});
		}

		public void onClickItem(UISelectWithdrawNFTItem_Cell item)
		{
			// MainDZ 선택 불가
			ClientNFTItem nftItem = item.getNFTItem();

			//if( ViewModel.ProMode.EquipedNFTItem == nftItem)
			//{
			//	UIToast.spawn(
			//			StringCollection.get("wallet.select_nft.fail.equiped_nft", 0),
			//			new(20, -606))
			//		.setType(UIToastType.error)
			//		.withTransition<FadePanelTransition>()
			//		.autoClose(3.0f);
			//	return;
			//}

			if( nftItem.getStaminaRatio() < 1.0)
			{
				UIToast.spawn(
						StringCollection.get("wallet.select_nft.fail.stamina", 0),
						new(20, -606))
					.setType(UIToastType.error)
					.withTransition<FadePanelTransition>()
					.autoClose(3.0f);
				return;
			}

			_callback(UISelectAssetType.SelectResult.create(ClientWalletBalance.AssetType.NFT, nftItem.token_id));
			close();
		}

		public void onClickClose()
		{
			close();
			_callback(UISelectAssetType.SelectResult.create(ClientWalletBalance.AssetType.NONE));
		}

		#region scroller
		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UISelectWithdrawNFTItem_Cell cell = (UISelectWithdrawNFTItem_Cell)scroller.GetCellView(cellSource);
			cell.setup(ViewModel.Wallet.getNFTItemList()[dataIndex]);
			return cell;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 128.0f;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return ViewModel.Wallet.getNFTItemList().Count;
		}

		#endregion

	}
}
