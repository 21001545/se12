using DRun.Client.NetData;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using System.Collections.Generic;
using UnityEngine;

namespace Festa.Client.ViewModel
{
	public class WalletViewModel : AbstractViewModel
	{
		private int _festa_coin;
		private ClientDRNBalance _drn_balance;
		private Dictionary<int, ClientDRNTransaction> _drnTransactionMap;

		private ClientWallet _wallet;
		private Dictionary<int,ClientWalletBalance> _walletBalanceMap;

		private bool _walletPinHashChecked;
		private DRNChangeDeferrer _drnChangeDeferrer;
		
		private JsonObject _walletFee;

		private ClientNFTMetadataConfig _nftMetadataConfig;
		
		private Dictionary<int, ClientNFTItem> _nftItemMap;
		private Dictionary<int, ClientNFTWithdrawExternal> _nftWithdrawExternalMap;
		private List<ClientNFTItem> _nftItemList;	// 인벤토리 보여주기용
	
		public int FestaCoin
		{
			get
			{
				return _festa_coin;
			}
			set
			{
				Set<int>(ref _festa_coin, value);
			}
		}

		public ClientDRNBalance DRN_Balance
		{
			get
			{
				return _drn_balance;
			}
			set
			{
				_drnChangeDeferrer.defer(value.balance - _drn_balance?.balance ?? 0);
				Set(ref _drn_balance, value);
			}
		}

		public ClientWallet Wallet
		{
			get
			{
				return _wallet;
			}
			set
			{
				Set(ref _wallet, value);
			}
		}

		public bool WalletPinHashChecked
		{
			get
			{
				return _walletPinHashChecked;
			}
			set
			{
				Set(ref _walletPinHashChecked, value);
			}
		}

		public JsonObject WalletFee
		{
			get
			{
				return _walletFee;
			}
			set
			{
				Set(ref _walletFee, value);
			}
		}

		public Dictionary<int,ClientWalletBalance> WalletBalanceMap
		{
			get
			{
				return _walletBalanceMap;
			}
			set
			{
				Set(ref _walletBalanceMap, value);
			}
		}

		public ClientNFTMetadataConfig NFTMetadataConfig
		{
			get
			{
				return _nftMetadataConfig;
			}
			set
			{
				Set(ref _nftMetadataConfig, value);
			}
		}

		public Dictionary<int,ClientNFTItem> NFTITemMap
		{
			get
			{
				return _nftItemMap;
			}
		}

		public Dictionary<int,ClientNFTWithdrawExternal> NFTWithdrawExternalMap
		{
			get
			{
				return _nftWithdrawExternalMap;
			}
		}

		public static WalletViewModel create()
		{
			WalletViewModel vm = new WalletViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_wallet = null;
			_walletBalanceMap = new Dictionary<int, ClientWalletBalance>();
			_walletPinHashChecked = false;
			_walletFee = new JsonObject();
			_drnTransactionMap = new Dictionary<int, ClientDRNTransaction>();
			_nftItemMap = new Dictionary<int, ClientNFTItem>();
			_nftWithdrawExternalMap = new Dictionary<int, ClientNFTWithdrawExternal>();
			_nftItemList = new List<ClientNFTItem>();
			_drnChangeDeferrer = new DRNChangeDeferrer(10);
		}

		public override void reset()
		{
			clearNFTItems();
			_walletPinHashChecked = false;
			_drnChangeDeferrer.reset();
		}

		protected override void bindPacketParser()
		{
			bind(MapPacketKey.ClientAck.festa_coin, updateFestaCoin);
			bind(MapPacketKey.ClientAck.drn_balance, updateDRNBalance);
			bind(MapPacketKey.ClientAck.drn_transaction, updateDRNTransaction);
			bind(MapPacketKey.ClientAck.wallet, updateWallet);
			bind(MapPacketKey.ClientAck.wallet_balance, updateWalletBalance);
			bind(MapPacketKey.ClientAck.nft_item, updateNFTItem);
			bind(MapPacketKey.ClientAck.nft_metadata_config, updateNFTMetadataConfig);
			bind(MapPacketKey.ClientAck.nft_withdraw_external, updateNFTWithdrawExternal);
		}

		private void updateFestaCoin(object obj,MapPacket ack)
		{
			_festa_coin = (int)obj;
		}

		private void updateDRNBalance(object obj,MapPacket ack)
		{
			DRN_Balance = (ClientDRNBalance)obj;
		}

		private void updateDRNTransaction(object obj,MapPacket ack)
		{
			List<ClientDRNTransaction> list = ack.getList<ClientDRNTransaction>(MapPacketKey.ClientAck.drn_transaction);

			foreach(ClientDRNTransaction transaction in list)
			{
				if( _drnTransactionMap.ContainsKey( transaction.transaction_id))
				{
					_drnTransactionMap.Remove( transaction.transaction_id );
				}

				_drnTransactionMap.Add( transaction.transaction_id, transaction );
			}
		}

		private void updateWallet(object obj,MapPacket ack)
		{
			Wallet = (ClientWallet)obj;
		}

		private void updateWalletBalance(object obj,MapPacket ack)
		{
			List<ClientWalletBalance> list = ack.getList<ClientWalletBalance>(MapPacketKey.ClientAck.wallet_balance);
			foreach(ClientWalletBalance balance in list)
			{
				if( _walletBalanceMap.ContainsKey( balance.asset_type))
				{
					_walletBalanceMap.Remove( balance.asset_type);
				}

				_walletBalanceMap.Add(balance.asset_type, balance);


			}

			notifyPropetyChanged("WalletBalanceMap");
		}

		public ClientWalletBalance getWalletBalance(int asset_type)
		{
			ClientWalletBalance data;
			if( _walletBalanceMap.TryGetValue(asset_type, out data))
			{
				return data;
			}
			else
			{
				return null;
			}
		}

		public string getFee(int asset_type)
		{
			string key = asset_type.ToString();
			if( _walletFee.contains(key) == false)
			{
				return "-";
			}

			try
			{
				JsonObject fee = _walletFee.getJsonObject(key);
				if ( asset_type == ClientWalletBalance.AssetType.NFT)
				{
					return parseNFTFee(fee);
				}
				else
				{
					return parseTokenFee(fee);
				}
			}
			catch(System.Exception e)
			{
				Debug.LogException(e);
				return "-";
			}

		}

		/*
				{
				  "withdrawalFee": {
					"idx": 2,
					"gasLimit": "21000.00000000000000000000",
					"gasPrice": "2.00000000000000000000",
					"unit": "gwei",
					"createdDate": "2021-10-12T08:53:45.914Z",
					"modifiedDate": "2022-01-27T09:54:01.000Z"
				  },
				  "gatheringFee": {
					"idx": 2,
					"gasLimit": "21000.00000000000000000000",
					"gasPrice": "2.00000000000000000000",
					"unit": "gwei",
					"createdDate": "2021-10-12T08:53:45.914Z",
					"modifiedDate": "2022-01-27T09:54:01.000Z"
				  }
				}
		*/

		public string parseTokenFee(JsonObject data)
		{
			JsonObject fee = data.getJsonObject("withdrawalFee");

			string unit = fee.getString("unit");
			if (unit != "gwei")
			{
				Debug.LogWarning($"unknown unit type:{unit}");
				return "-";
			}

			double price = double.Parse(fee.getString("gasPrice"));
			double limit = double.Parse(fee.getString("gasLimit"));

			double fee_coin = price * limit / 1000000000.0;
			return fee_coin.ToString();
		}

		/*
		{
			"gasPrice": "string",
			"gasLimit": "string",
			"maxPriorityFeePerGas": "string",
			"maxFee": "string"
		}
		*/
		public string parseNFTFee(JsonObject fee)
		{
			double price = double.Parse(fee.getString("gasPrice"));
			double limit = double.Parse(fee.getString("gasLimit"));

			double fee_coin = price * limit / 1000000000.0;
			return fee_coin.ToString();
		}


		public List<ClientDRNTransaction> getVisibleTransactionList()
		{
			List<ClientDRNTransaction> list = new List<ClientDRNTransaction>();
			foreach(KeyValuePair<int,ClientDRNTransaction> item in _drnTransactionMap)
			{
				ClientDRNTransaction trn = item.Value;
				if( isVisibleTransaction(trn))
				{
					list.Add(trn);
				}
			}

			list.Sort((a, b) => { 
				if( a.transaction_id < b.transaction_id)
				{
					return 1;
				}
				else if( a.transaction_id > b.transaction_id)
				{
					return -1;
				}

				return 0;
			});

			return list;
		}

		private bool isVisibleTransaction(ClientDRNTransaction trn)
		{
			// 뺄것만 체크하고 넘기자
			//if( trn.type == ClientDRNTransaction.Type.wait_withdraw &&
			//	trn.step == ClientDRNTransaction.Step.done)
			//{
			//	return false;
			//}

			if( trn.type == ClientDRNTransaction.Type.wait_gather &&
				trn.step == ClientDRNTransaction.Step.done)
			{
				return false;
			}

			if( trn.type == ClientDRNTransaction.Type.wait_claim_invitation_send &&
				trn.step == ClientDRNTransaction.Step.done)
			{
				return false;
			}

			if( trn.type == ClientDRNTransaction.Type.start_withdraw)
			{
				// 진행중인 Transaction이 있으면 보여주지 않는다
				return false;
			}

			return true;
		}

		public void clearNFTItems()
		{
			_nftItemMap.Clear();
			buildNFTItemList();
			notifyPropetyChanged("NFTITemMap");
		}

		public void updateNFTItem(object obj,MapPacket ack)
		{
			List<ClientNFTItem> list = ack.getList<ClientNFTItem>(MapPacketKey.ClientAck.nft_item);
			foreach(ClientNFTItem item in list)
			{
				if( _nftItemMap.ContainsKey( item.token_id))
				{
					_nftItemMap.Remove(item.token_id);
				}

				_nftItemMap.Add(item.token_id, item);
			}

			buildNFTItemList();
			notifyPropetyChanged("NFTITemMap");
		}

		private void updateNFTWithdrawExternal(object obj,MapPacket ack)
		{
			List<ClientNFTWithdrawExternal> list = ack.getList<ClientNFTWithdrawExternal>(MapPacketKey.ClientAck.nft_withdraw_external);
			foreach(ClientNFTWithdrawExternal item in list)
			{
				if (_nftWithdrawExternalMap.ContainsKey(item.token_id))
				{
					_nftWithdrawExternalMap.Remove(item.token_id);
				}

				_nftWithdrawExternalMap.Add(item.token_id, item);
			}

			buildNFTItemList();
			notifyPropetyChanged("NFTITemMap");
		}

		private void buildNFTItemList()
		{
			_nftItemList.Clear();
			foreach(KeyValuePair<int,ClientNFTItem> item in _nftItemMap)
			{
				// 외부 출금 중인 NFT는 제외한다
				if( _nftWithdrawExternalMap.ContainsKey( item.Value.token_id))
				{
					continue;
				}

				_nftItemList.Add(item.Value);
			}

			_nftItemList.Sort((a, b) => { 
				if( a.token_id < b.token_id)
				{
					return -1;
				}
				else if( a.token_id > b.token_id)
				{
					return 1;
				}

				return 0;
			});
		}

		public ClientNFTItem getNFTItem(int token_id)
		{
			ClientNFTItem nftItem;
			if( _nftItemMap.TryGetValue(token_id, out nftItem) == false)
			{
				return null;
			}
			return nftItem;
		}

		public List<ClientNFTItem> getNFTItemList()
		{
			return _nftItemList;
		}

		public void updateNFTMetadataConfig(object obj, MapPacket ack)
		{
			NFTMetadataConfig = (ClientNFTMetadataConfig)obj;
		}

		public System.IDisposable SubscribeToDRNChangeDeferrer(System.IObserver<long> observer)
		{
			return _drnChangeDeferrer.Subscribe(observer);	
		}
		
		public void NotifyAccumulatedDRNChange()
		{
			_drnChangeDeferrer.notify();
		}

		public void clearAccumulatedDRNChange()
		{
			_drnChangeDeferrer.reset();
		}
	}
}
