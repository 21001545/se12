using Assets.Script_DRun.Logic.Ranking;
using DRun.Client.Logic.Ranking;
using DRun.Client.NetData;
using DRun.Client.ViewModel;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIRanking : UISingletonPanel<UIRanking>, IEnhancedScrollerDelegate
	{
		public UIColorToggleButton[] pageTabButton;
		public UITabSlide pageTabSlide;
		public EnhancedScroller scroller;
		public GameObject loading;

		public UIRankingCell_Item cellItem;
		public UIRankingCell_Void cellVoid;
		public UIRankingCell_MyItemTarget cellMyItemTarget;
		public UIRankingCell_MyItem cellMyItem;
		public UIRankingCell_Void cellEmptyRankers;
		public UIRankingCell_Void cellSearchedNothing;
		public UIRankingCell_LoadingMore cellLoadingMore;

		public GameObject layout_marathon_type;
		public GameObject layout_time_tab;
		public GameObject layout_search;

		[Header("================ sub tab =================")]
		public UIAnimationToggleButton[] timeTabButton;

		[Header("================ Search ==================")]
		public UIColorToggleButton btn_search;
		public TMP_InputField input_name;

		[Header("================ MarathonType ============")]
		public UIRanking_SelectMarathonType selectMarathoneType;
		public TMP_Text text_marahton_type;



		public class PageType
		{
			public const int basicmode = 0;
			public const int promode = 1;
			public const int marathon = 2;
		}

		private static int[] _modeTypeFromPageType = new int[]
		{
			ClientRankingData.ModeType.step,
			ClientRankingData.ModeType.promode,
			ClientRankingData.ModeType.marathon
		};

		public class TimeTabType
		{
			public const int total = 0;
			public const int day = 1;
			public const int week = 2;
			public const int month = 3;
		}

		private static int[] _timeTypeFromTimeTabType = new int[]
		{
			ClientRunningLogCumulation.TimeType.total,
			ClientRunningLogCumulation.TimeType.day,
			ClientRunningLogCumulation.TimeType.week,
			ClientRunningLogCumulation.TimeType.month
		};

		public class CellType
		{
			public const int item = 0;
			public const int item_void = 1;
			public const int myitem_target = 2;
			public const int loading_more = 3;
		}

		public class CellData
		{
			public int type;
			public float height;

			public ClientRankingData data;
			public UIRankingCell_BaseItem cellSource;

			public static CellData create(int type,UIRankingCell_BaseItem cellSource)
			{
				CellData data = new CellData();
				data.type = type;
				data.height = cellSource.height;
				data.cellSource = cellSource;
				return data;
			}

			public static CellData create(int type, UIRankingCell_BaseItem cellSource, ClientRankingData rankingData)
			{
				CellData data = new CellData();
				data.type = type;
				data.height = cellSource.height;
				data.data = rankingData;
				data.cellSource = cellSource;
				return data;
			}
		}

		private int _currentPageTab = -1;
		private List<CellData> _dataList;

		private int _currentTimeTab = -1;
		private RankingCacheData _rankingData;

		private int _currentMarathonType = -1;

		private bool _searchingName;
		private string _searchedName;
		private IntervalTimer _timerSearchName;

		private int _pageIndex = 0;
		private bool _loadingMore = false;

		public class BuildType
		{
			public const int normal = 1;
			public const int search = 2;
		}

		public RankingCacheData getRankingData()
		{
			return _rankingData;
		}

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
			scroller.cellViewWillRecycle += onCellViewWillRecycle;
			scroller.scrollerScrolled = onScollerScrolled;
			loading.SetActive(false);

			_dataList = new List<CellData>();
			_searchingName = false;
			_timerSearchName = IntervalTimer.create(1.0f, false, false);
			_searchedName = "";

			layout_time_tab.SetActive(true);
			layout_search.SetActive(false);

			selectMarathoneType.gameObject.SetActive(false);

			setPageTab(PageType.basicmode, true);
			setMarathonType(ClientRunningLogCumulation.MarathonType._5k);
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				text_marahton_type.text = StringCollection.get("marathon.select_goal.option", _currentMarathonType - 1);

				if ( _currentTimeTab == -1)
				{
					setTimeTab(TimeTabType.total);
				}
				buildData(0);
			}
		}

		public override void update()
		{
			base.update();

			searchName();
		}

		private void loadMoreData()
		{
			_loadingMore = true;

			int mode_type = getCurrentModeType();
			int mode_sub_type = getCurrentModeSubType();

			float f_pageIndex = (float)_rankingData.getRankingList().Count / (float)BuildRankingDataProcessor.pageCount;
			_pageIndex = Mathf.CeilToInt(f_pageIndex);

			BuildRankingDataProcessor step = BuildRankingDataProcessor.create(mode_type, mode_sub_type, getCurrentTimeType(), _pageIndex);
			step.run(result => {
				//loading.SetActive(false);
				_loadingMore = false;

				if (result.succeeded())
				{
//#if UNITY_EDITOR
//					// 테스트
//					RankingCacheData data = step.getCacheData();
//					if( data.getRankingList().Count < BuildRankingDataProcessor.pageCount)
//					{
//						ClientRankingData appendItem = _rankingData.getRankingList()[ _rankingData.getRankingList().Count - 1];
//						if( data.getRankingList().Count > 0)
//						{
//							appendItem = data.getRankingList()[data.getRankingList().Count - 1];
//						}

//						int appendCount = BuildRankingDataProcessor.pageCount - data.getRankingList().Count;
//						for(int i = 0; i < appendCount; ++i)
//						{
//							appendItem = appendItem.testCopy();
//							data.getRankingList().Add(appendItem);
//						}
//					}
//#endif
					buildData(step.getCacheData(), BuildType.normal, true);
				}
			});
		}

		private void buildData(int pageIndex)
		{
			int mode_type = getCurrentModeType();
			int mode_sub_type = getCurrentModeSubType();
			
			loading.SetActive(true);
			_dataList.Clear();
			cellMyItem.gameObject.SetActive(false);
			if( scroller.Container != null)
			{
				scroller.ReloadData();
			}

			_pageIndex = pageIndex;
			BuildRankingDataProcessor step = BuildRankingDataProcessor.create(mode_type, mode_sub_type, getCurrentTimeType(), _pageIndex);
			step.run(result => {
				loading.SetActive(false);

				if (result.succeeded())
				{
					buildData(step.getCacheData(), BuildType.normal, false);
				}
			});
		}

		private void buildData(RankingCacheData cacheData,int buildType,bool appendOnly)
		{
			if(appendOnly == false || _rankingData == null)
			{
				_rankingData = cacheData;
			}
			else
			{
				_rankingData.append(cacheData);
			}

			_dataList.Clear();
			foreach (ClientRankingData data in _rankingData.getRankingList())
			{
				if( data.account_id == _rankingData.getMyRankingData().account_id)
				{
					_dataList.Add(CellData.create(CellType.myitem_target, cellMyItemTarget, data));
				}
				else
				{
					_dataList.Add(CellData.create(CellType.item, cellItem, data));
				}
			}

			if( _dataList.Count == 0)
			{
				if (buildType == BuildType.normal)
				{
					_dataList.Add(CellData.create(CellType.item_void, cellEmptyRankers));
				}
				else
				{
					_dataList.Add(CellData.create(CellType.item_void, cellSearchedNothing));
				}
			}
			
			if( buildType == BuildType.normal && cacheData.getRankingList().Count == BuildRankingDataProcessor.pageCount)
			{
				_dataList.Add(CellData.create(CellType.loading_more, cellLoadingMore));
			}

			cellMyItem.gameObject.SetActive(true);
			cellMyItem.setup(cacheData.getMyRankingData(), getDataIndexFromAccount(cacheData.getMyRankingData().account_id));

			if (scroller.Container != null)
			{
				var scrollPosition = scroller.ScrollPosition;

				scroller.ReloadData();

				if(appendOnly)
				{
					scroller.ScrollPosition = scrollPosition;
				}
			}
		}

		private int getDataIndexFromAccount(int account_id)
		{
			for(int i = 0; i < _dataList.Count; ++i)
			{
				CellData data = _dataList[i];
				if( data.data != null && data.data.account_id == account_id)
				{
					return i;
				}
			}

			return -1;
		}

		private int getCurrentModeType()
		{
			return _modeTypeFromPageType[_currentPageTab];
		}

		private int getCurrentTimeType()
		{
			return _timeTypeFromTimeTabType[_currentTimeTab];
		}

		private int getCurrentModeSubType()
		{
			if( getCurrentModeType() == ClientRankingData.ModeType.marathon)
			{
				return _currentMarathonType;
			}
			else
			{
				return 1;
			}
		}


		private RankingCacheData buildSampleData(int mode_type,int mode_sub_type)
		{
			List<ClientRankingData> rankingList = new List<ClientRankingData>();
			ClientRankingData myRankingData;

			for(int i =0; i < 100; ++i)
			{
				ClientRankingData data = new ClientRankingData();
				data.account_id = i;
				data.mode_type = mode_type;
				data.mode_sub_type = mode_sub_type;
				data.rank = i;

				int random = RandomUtil.intRandom(0, 100);

				if( random < 10)
				{
					data.prev_rank = -1;
				}
				else if( random < 20 && i > 1)
				{
					data.prev_rank = data.rank - 1;
				}
				else if( random < 30)
				{
					data.prev_rank = data.rank + 1;
				}
				else
				{
					data.prev_rank = data.rank;
				}

				if( mode_type == ClientRankingData.ModeType.step)
				{
					data.score = (100 - i) * 100 + 100;
				}
				else if( mode_type == ClientRankingData.ModeType.promode)
				{
					data.score = GlobalRefDataContainer.getRefDataHelper().toPeb((100 - i) * 0.5 + 1);
				}
				else if( mode_type == ClientRankingData.ModeType.marathon)
				{
					data.score = (100 - i) * 100;
				}


				rankingList.Add(data);
			}

			myRankingData = rankingList[30];
			//myRankingData = new ClientRankingData();
			//myRankingData.rank = 30;
			//myRankingData.prev_rank = 30;
			//myRankingData.account_id = 111111;
			//myRankingData.mode_type = mode_type;
			//myRankingData.mode_sub_type = mode_sub_type;
			//myRankingData.score = 100000;

			return RankingCacheData.create(rankingList, myRankingData);
		}

		#region PageType
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

			layout_marathon_type.SetActive(_currentPageTab == PageType.marathon);
			layout_search.SetActive(false);

			return true;
		}

		public void onClick_PageTab_BasicMode()
		{
			if( setPageTab(PageType.basicmode, false))
			{
				buildData(0);
			}
		}

		public void onClick_PageTab_ProMode()
		{
			if (setPageTab(PageType.promode, false))
			{
				buildData(0);
			}
		}

		public void onClick_PageTab_Marathon()
		{
			if (setPageTab(PageType.marathon, false))
			{
				buildData(0);
			}
		}

		#endregion

		#region subPage

		public void onClickTimeTab(int tab)
		{
			if( _currentTimeTab == tab)
			{
				return;
			}

			if( setTimeTab(tab))
			{
				buildData(0);
			}
		}

		public bool setTimeTab(int tab)
		{
			if( _currentTimeTab == tab)
			{
				return false;
			}

			_currentTimeTab = tab;
			for(int i = 0; i < timeTabButton.Length; ++i)
			{
				timeTabButton[i].setStatus(i == tab);
			}

			return true;
		}

		public bool isSearchMode()
		{
			return btn_search.status == true;
		}

		public void onClick_Search()
		{
			_searchedName = "";
			_searchingName = false;

			input_name.text = "";
			input_name.ActivateInputField();

			_timerSearchName.setNext();

			layout_search.SetActive(true);
			btn_search.setStatus(true);
			btn_search.interactable = false;
		}

		public void onClick_Cancel()
		{
			btn_search.setStatus(false);
			btn_search.interactable = true;
			layout_search.SetActive(false);
			input_name.text = "";
			_searchedName = "";
			buildData(0);
		}

		#endregion

		#region marathon_type
		private bool setMarathonType(int type)
		{
			if( _currentMarathonType == type)
			{
				return false;
			}

			_currentMarathonType = type;

			if (StringCollection != null)
			{
				text_marahton_type.text = StringCollection.get("marathon.select_goal.option", type - 1);
			}

			return true;
		}

		public void onClick_MarathonType()
		{
			selectMarathoneType.open(_currentMarathonType, newType => {

				if(setMarathonType(newType))
				{
					buildData(0);
				}

			});
		}

		#endregion

		#region scroller
		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellData data = _dataList[dataIndex];
			EnhancedScrollerCellView cellView = scroller.GetCellView(data.cellSource);

			if( data.type == CellType.item)
			{
				((UIRankingCell_Item)cellView).setup(data.data);
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

		public void onCellViewWillRecycle(EnhancedScrollerCellView cellView)
		{
			
		}

		private void onScollerScrolled(EnhancedScroller scroller,Vector2 val,float scrollPosition)
		{
			if( scroller.NormalizedScrollPosition >= 1.0f && _loadingMore == false)
			{
				if( _dataList[ _dataList.Count - 1].type == CellType.loading_more)
				{
					loadMoreData();
				}
			}
		}

		#endregion

		#region Search

		public void onValueChanged_InputName(string value)
		{
			if(_searchingName)
			{
				return;
			}
		}

		private void searchName()
		{
			if( _timerSearchName.update() == false)
			{
				return;
			}

			// 같으면 넘어감
			string currentName = input_name.text;
			if( currentName == _searchedName)
			{
				_timerSearchName.setNext();
				return;
			}

			if( string.IsNullOrEmpty(currentName))
			{
				_searchedName = "";
				buildData(0);
				_timerSearchName.setNext();
			}
			else
			{
				searchNameFromServer(currentName);
			}
		}
		
		private void searchNameFromServer(string name)
		{
			_searchedName = name;

			int mode_type = getCurrentModeType();
			int mode_sub_type = getCurrentModeSubType();

			_searchingName = true;

			loading.SetActive(true);
			_dataList.Clear();
			cellMyItem.gameObject.SetActive(false);
			if (scroller.Container != null)
			{
				scroller.ReloadData();
			}

			BuildRankingDataByNameProcessor step = BuildRankingDataByNameProcessor.create(mode_type, mode_sub_type, getCurrentTimeType(), name);
			step.run(result => {

				_searchingName = false;
				_timerSearchName.setNext();

				loading.SetActive(false);

				if (result.succeeded())
				{
					buildData(step.getCacheData(),BuildType.search, false);
				}
			});
		}

		#endregion
	}
}
