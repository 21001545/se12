using DRun.Client.Logic.Record;
using DRun.Client.NetData;
using DRun.Client.Record;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections.Generic;
using UnityEngine;

namespace DRun.Client
{
	public class UIRecord : UISingletonPanel<UIRecord>, IEnhancedScrollerDelegate
	{
		public UIColorToggleButton[] pageTabButton;
		public UITabSlide pageTabSlide;
		public EnhancedScroller scroller;
		public GameObject loading;

		public UIRecordCell_Statistics cellStatitics;
		public UIRecordCell_HistoryTitle cellHistoryTitle;
		public UIRecordCell_Log cellLog;
		public UIRecordCell_NoLog cellNoLog;
		public UIRecordCell_Void cellVoid;

		public class PageType
		{
			public const int promode = 0;
			public const int marathon = 1;
		}

		public class CellType
		{
			public const int statistics = 0;
			public const int log_title = 1;
			public const int no_log = 2;
			public const int log_item = 3;
			public const int item_void = 4;
		}

		public class CellData
		{
			public int type;
			public float cellHeight;
			public ClientRunningLog log;

			public static CellData create(int type,float height)
			{
				CellData data = new CellData();
				data.type = type;
				data.cellHeight = height;
				return data;
			}

			public static CellData create(int type,float height, ClientRunningLog log)
			{
				CellData data = new CellData();
				data.type = type;
				data.cellHeight = height;
				data.log = log;
				return data;
			}
		}

		private int _currentPageTab = -1;
		private bool _rebuild = true;
		private List<CellData> _dataList;
		private List<CellData> _headerList;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private Dictionary<int, GraphData> _graphDataMap;

		public int getCurrentPageTab()
		{
			return _currentPageTab;
		}

		public int getCurrentPageRunningType()
		{
			if( _currentPageTab == PageType.promode)
			{
				return ClientRunningLogCumulation.RunningType.promode;
			}
			else
			{
				return ClientRunningLogCumulation.RunningType.marathon;
			}
		}

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
			scroller.cellViewWillRecycle += onCellViewWillRecycle;
			loading.SetActive(false);

			_dataList = new List<CellData>();
			_headerList = new List<CellData>();

			_headerList.Add(CellData.create(CellType.statistics, cellStatitics.height));
			_headerList.Add(CellData.create(CellType.log_title, cellHistoryTitle.height));

			_graphDataMap = new Dictionary<int, GraphData>();

			setPageTab(PageType.promode, true);

		}

		//public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		//{
		//	base.open(param, transitionType, closeType);
		//}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				if(_rebuild)
				{
					buildData();
					_rebuild = false;
				}
			}
		}

		private void buildData()
		{
			if( _currentPageTab == PageType.promode)
			{
				buildData( ClientRunningLogCumulation.RunningType.promode);
			}
			else
			{
				buildData(ClientRunningLogCumulation.RunningType.marathon);
			}
		}

		//private void buildData_MarathonMode()
		//{
		//	_dataList.Clear();
		//	//_dataList.AddRange(_headerList);
		//	_dataList.Add(CellData.create(CellType.item_void, 125.75f));
		//	_dataList.Add(CellData.create(CellType.no_log, cellNoLog.height));
		//	_dataList.Add(CellData.create(CellType.item_void, 125.75f));

		//	if (scroller.Container)
		//	{
		//		scroller.ReloadData();
		//	}
		//}

		public void markAsRebuildData()
		{
			_rebuild = true;
		}

		private void buildData(int running_type)
		{
			loading.SetActive(true);
			_dataList.Clear();
			//if (scroller.Container)
			//{
			//	scroller.ReloadData();
			//}

			BuildRecordDataProcessor step = BuildRecordDataProcessor.create(Network.getAccountID(),
													running_type,
													ViewModel.Running.RunningConfig.next_running_id,
													ViewModel.Record);
			step.run(result => {
				loading.SetActive(false);

				if (result.succeeded())
				{
					_graphDataMap = step.getGraphDataMap();

					_dataList.Clear();
					_dataList.AddRange(_headerList);

					List<ClientRunningLog> logList = step.getLogList();
					if (logList.Count > 0)
					{
						foreach (ClientRunningLog log in step.getLogList())
						{
							_dataList.Add(CellData.create(CellType.log_item, cellLog.height, log));
						}

						_dataList.Add(CellData.create(CellType.item_void, 40));
					}
					else
					{
						_dataList.Add(CellData.create(CellType.no_log, cellNoLog.height));
					}

					if (scroller.Container != null)
					{
						scroller.ReloadData();
					}
				}
			});
		}

		#region PageTab

		public bool setPageTab(int page,bool NoAnimation)
		{
			if( _currentPageTab == page)
			{
				return false;
			}

			_currentPageTab = page;
			for(int i = 0; i < pageTabButton.Length; ++i)
			{
				pageTabButton[i].setStatus(i == page);
			}

			pageTabSlide.setTab(page, NoAnimation);
			return true;
		}

		public void onClick_PageTab_ProMode()
		{
			if( setPageTab(PageType.promode, false))
			{
				buildData();
			}
		}

		public void onClick_PageTab_Marathon()
		{
			if( setPageTab(PageType.marathon, false))
			{
				buildData();
			}
		}

		#endregion

		#region scroller

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _dataList.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return _dataList[dataIndex].cellHeight;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellData data = _dataList[dataIndex];

			EnhancedScrollerCellView cellView = null;

			if( data.type == CellType.statistics)
			{
				UIRecordCell_Statistics cell_instance = (UIRecordCell_Statistics)scroller.GetCellView(cellStatitics);
				cellView = cell_instance;

				cell_instance.setup(_graphDataMap);
			}
			else if( data.type == CellType.log_title)
			{
				cellView = scroller.GetCellView(cellHistoryTitle);
			}
			else if( data.type == CellType.no_log)
			{
				UIRecordCell_NoLog cell_instance = (UIRecordCell_NoLog)scroller.GetCellView(cellNoLog);
				cellView = cell_instance;

				cell_instance.setup(_currentPageTab);
			}
			else if( data.type == CellType.log_item)
			{
				UIRecordCell_Log cell_instance = (UIRecordCell_Log)scroller.GetCellView(cellLog);
				cellView = cell_instance;

				cell_instance.setup(data.log);
			}
			else if( data.type == CellType.item_void)
			{
				cellView = (UIRecordCell_Void)scroller.GetCellView(cellVoid);
			}

			return cellView;
		}

		public void onCellViewWillRecycle(EnhancedScrollerCellView cellView)
		{
			if (cellView is UIRecordCell_Log)
			{
				UIRecordCell_Log cellLog = (UIRecordCell_Log)cellView;
				cellLog.onWillRecycle();
			}
		}

		#endregion
	}
}
