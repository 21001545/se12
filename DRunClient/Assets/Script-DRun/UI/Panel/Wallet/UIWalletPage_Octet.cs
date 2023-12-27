using DRun.Client.Logic.Wallet;
using DRun.Client.NetData;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client
{
	public class UIWalletPage_Octet : UIWalletPage, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;
		public GameObject loading;
		public UIWalletOctet_Control cellControl;
		public UIWalletOctet_Balance cellBalance;

		public class CellType
		{
			public const int control = 0;
			public const int balance = 1;
		}

		public class CellData
		{
			public int type;
			public float height;
			public ClientWalletBalance data;

			public static CellData create(int type,UIWalletScrollerCellView cellSource,ClientWalletBalance data)
			{
				CellData cellData = new CellData();
				cellData.type = type;
				cellData.height = cellSource.height;
				cellData.data = data;
				return cellData;
			}
		}

		private List<CellData> _dataList;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public override void initialize()
		{
			base.initialize();

			scroller.Delegate = this;
			loading.SetActive(false);

			_dataList = new List<CellData>();
		}

		public override void onShow()
		{
			base.onShow();

			updateBalance();
		}

		private void updateBalance()
		{
			loading.SetActive(true);

			_dataList.Clear();
			if( scroller.Container != null)
			{
				scroller.ReloadData();
			}

			GetBalanceProcessor step = GetBalanceProcessor.create();
			step.run(result => {
				loading.SetActive(false);

				if ( result.succeeded())
				{
					_dataList.Clear();
					_dataList.Add(CellData.create(CellType.control, cellControl, null));

					ClientWalletBalance drnt = ViewModel.Wallet.getWalletBalance(ClientWalletBalance.AssetType.DRNT);
					ClientWalletBalance etc = ViewModel.Wallet.getWalletBalance(ClientWalletBalance.AssetType.ETH);

					if (drnt != null)
					{
						_dataList.Add(CellData.create(CellType.balance, cellBalance, drnt));
					}
					if (etc != null)
					{
						_dataList.Add(CellData.create(CellType.balance, cellBalance, etc));
					}

					if( scroller.Container != null)
					{
						scroller.ReloadData();
					}
				}
			});
		}

		#region
		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			EnhancedScrollerCellView cellView = null;
			CellData data = _dataList[dataIndex];

			if( data.type == CellType.control)
			{
				UIWalletOctet_Control cellInstance = (UIWalletOctet_Control)scroller.GetCellView(cellControl);
				cellView = cellInstance;

				cellInstance.setup();
			}
			else if( data.type == CellType.balance)
			{
				UIWalletOctet_Balance cellInstance = (UIWalletOctet_Balance)scroller.GetCellView(cellBalance);
				cellView = cellInstance;

				cellInstance.setup(data.data);
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
