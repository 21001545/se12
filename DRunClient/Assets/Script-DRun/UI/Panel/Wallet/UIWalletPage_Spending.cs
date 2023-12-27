using DRun.Client.Logic.Wallet;
using DRun.Client.NetData;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UIWalletPage_Spending : UIWalletPage, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;

		public UIWalletSpending_AccountTitle cellAccountTitle;
		public UIWalletSpending_Balance cellBalance;
		public UIWalletSpending_LogTitle cellLogTitle;
		public UIWalletSpending_LogEmpty cellLogEmpty;
		public UIWalletSpending_Log cellLog;

		public class CellType
		{
			public const int account_title = 0;
			public const int balance = 1;
			public const int log_title = 2;
			public const int log_empty = 3;
			public const int log = 4;
		}

		public class CellData
		{
			public int type;
			public float height;
			public ClientDRNTransaction trnData;

			public static CellData create(int type, UIWalletScrollerCellView cellViewSource)
			{
				CellData data = new CellData();
				data.type = type;
				data.height = cellViewSource.height;
				return data;
			}

			public static CellData create(ClientDRNTransaction transaction,UIWalletScrollerCellView cellViewSource)
			{
				CellData data = new CellData();
				data.type = CellType.log;
				data.height = cellViewSource.height;
				data.trnData = transaction;
				return data;
			}
		}

		private List<CellData> _dataList;
		private List<CellData> _headerList;
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initialize()
		{
			base.initialize();
			
			scroller.Delegate = this;
			_dataList = new List<CellData>();
			_headerList = new List<CellData>();

			// 임시
			_headerList.Add(CellData.create(CellType.account_title, cellAccountTitle));
			_headerList.Add(CellData.create(CellType.balance, cellBalance));
			_headerList.Add(CellData.create(CellType.log_title, cellLogTitle));

			//_dataList.Add(CellData.create(CellType.log_empty, cellLogEmpty));
		}

		public override void onShow()
		{
			_dataList.Clear();
			_dataList.AddRange(_headerList);

			QueryTransactionProcessor step = QueryTransactionProcessor.create(0, 10);
			step.run(result => {

				List<ClientDRNTransaction> logList = ViewModel.Wallet.getVisibleTransactionList();
				if( logList.Count == 0)
				{
					_dataList.Add(CellData.create(CellType.log_empty, cellLogEmpty));
				}
				else
				{
					foreach(ClientDRNTransaction trn in logList)
					{
						_dataList.Add(CellData.create(trn, cellLog));
					}
				}

				if( scroller.Container != null)
				{
					scroller.ReloadData();
				}
			});
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

		public void onClick_Send()
		{
			if (ViewModel.Wallet.Wallet == null)
			{
				UICreateWallet.getInstance().open();
				return;
			}

			if( GlobalConfig.isOpenWalletFeature() == false)
			{
				spawnComingSoon();
				return;
			}

			UnityAction openSpending = () => {

				ViewModel.Withdraw.resetForSpendingToWallet();
				UISpending.getInstance().open();
			
			};


			if( ViewModel.Wallet.WalletPinHashChecked == false)
			{
				UICheckWalletPinHash.getInstance().open(result => { 
					if( result)
					{
						openSpending();
					}
					else
					{
						UIWallet.getInstance().open();
					}
				});
			}
			else
			{
				openSpending();
			}
		}

		#region scroller
		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			EnhancedScrollerCellView cellView = null;

			CellData data = _dataList[dataIndex];
			if( data.type == CellType.account_title)
			{
				cellView = scroller.GetCellView(cellAccountTitle);
			}
			else if( data.type == CellType.balance)
			{
				UIWalletSpending_Balance cellInstance = (UIWalletSpending_Balance)scroller.GetCellView(cellBalance);
				cellView = cellInstance;

				cellInstance.setup(ViewModel.Wallet.DRN_Balance);
			}
			else if( data.type == CellType.log_title)
			{
				cellView = scroller.GetCellView(cellLogTitle);
			}
			else if( data.type == CellType.log_empty)
			{
				cellView = scroller.GetCellView(cellLogEmpty);
			}
			else if( data.type == CellType.log)
			{
				UIWalletSpending_Log cellInstance = (UIWalletSpending_Log)scroller.GetCellView(cellLog);
				cellView = cellInstance;

				cellInstance.setup(data.trnData);
			}

			return cellView;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return _dataList[dataIndex].height;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _dataList.Count;
		}
		#endregion
	}
}
