using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.RefData;

namespace DRun.Client.ViewModel
{
	public class WalletWithdrawViewModel : AbstractViewModel
	{
		private int _withdrawType;
		private int _assetType;
		private int _tokenID;

		private string _from;
		private string _to;

		private string _amount;

		public int WithdrawType
		{
			get
			{
				return _withdrawType;
			}
			set
			{
				Set(ref _withdrawType, value);
			}
		}

		public string From
		{
			get
			{
				return _from;
			}
			set
			{
				Set(ref _from, value);
			}
		}

		public string To
		{
			get
			{
				return _to;
			}
			set
			{
				Set(ref _to, value);
			}
		}

		public int AssetType
		{
			get
			{
				return _assetType;
			}
			set
			{
				Set(ref _assetType, value);
			}
		}

		public int TokenID
		{
			get
			{
				return _tokenID;
			}
			set
			{
				Set(ref _tokenID, value);
			}
		}

		public string Amount
		{
			get
			{
				return _amount;
			}
			set
			{
				Set(ref _amount, value);
			}
		}

		public class WithdrawTypeConst
		{
			public const int spending = 1;
			public const int external = 2;
		}

		public class AddressConst
		{
			public static string spending = "spending";
			public static string wallet = "wallet";
		}

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public static WalletWithdrawViewModel create()
		{
			WalletWithdrawViewModel vm = new WalletWithdrawViewModel();
			vm.init();
			return vm;
		}

		public void resetForSpendingToWallet()
		{
			WithdrawType = WithdrawTypeConst.spending;
			AssetType = ClientWalletBalance.AssetType.DRNT;
			Amount = "0";
			From = AddressConst.spending;
			To = AddressConst.wallet;
			TokenID = 0;
		}

		public void resetForWalltToSpending()
		{
			WithdrawType = WithdrawTypeConst.spending;
			AssetType = ClientWalletBalance.AssetType.DRNT;
			Amount = "0";
			From = AddressConst.wallet;
			To = AddressConst.spending;
			TokenID = 0;
		}

		public void resetForExternal()
		{
			WithdrawType = WithdrawTypeConst.external;
			AssetType = ClientWalletBalance.AssetType.DRNT;
			Amount = "0";
			From = AddressConst.wallet;
			To = "";
			TokenID = 0;
		}

		public string getAddressName(string address)
		{
			if( address == AddressConst.spending)
			{
				return StringCollection.get("wallet.tab.spending", 0);
			}
			else if( address == AddressConst.wallet)
			{
				return StringCollection.get("wallet.tab.wallet", 0);
			}

			return address;
		}

		public string getAssetTypeName(int assetType)
		{
			if( assetType == ClientWalletBalance.AssetType.ETH)
			{
				return StringCollection.get("wallet.asset.eth", 0);
			}
			else if( assetType == ClientWalletBalance.AssetType.DRNT)
			{
				return StringCollection.get("wallet.asset.drnt", 0);
			}
			else if( assetType == ClientWalletBalance.AssetType.NFT)
			{
				return StringCollection.get("wallet.asset.nft", 0);
			}

			return "N/A";
		}

		public void SwapFromTo()
		{
			string temp = To;

			To = From;
			From = temp;
		}
	}
}
