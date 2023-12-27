using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using UnityEngine;

public class UIStatistics_exploreScroller : MonoBehaviour, IEnhancedScrollerDelegate
{
    public class ListType
    {
        public static int list = 0;
        public static int gallery = 1;
    }

    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private UIStatistics_myTrip _myTripCellView;
    [SerializeField]
    private UIStatistics_detailDate _detailDateCellView;
    [SerializeField]
    private UIStatistics_listTicket _listTicketCellView;
    [SerializeField]
    private UIStatistics_galleryGroup _galleryGroupCellView;
    [SerializeField]
    private UIStatistics_galleryItem _galleryItem;

    // 여기에 데이터를 담자
    private List<CellBase> _allDataList = new List<CellBase>();         // 스크롤에 전달할 전체 데이터
    private List<CellBase> _listTypeData = new List<CellBase>();       // 리스트 타입
    private List<CellBase> _galleryTypeData = new List<CellBase>();    // 갤러리 타입
    private string[] _myTripSummary = new string[4];

    private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

    #region cell types

    class CellType
    {
        public static readonly int myTrip = 0;
        public static readonly int detailDate = 1;
        public static readonly int listTicket = 2;
        public static readonly int galleryGroup = 3;
    }

    public class CellBase
    {
        public float height;
        public int type;
        public CellBase(float height, int type)
        {
            this.height = height;
            this.type = type;
        }
    };

    public class MyTrip : CellBase
    {
        public int listType;
        public string[] descList;
        public MyTrip(string[] descList, int listType) : base(398f, CellType.myTrip)
        {
            this.descList = descList;
            this.listType = listType;
        }

        public MyTrip() : base(398f, CellType.myTrip)
        {
        }
    }

    public class DetailDate : CellBase
    {
        public DateTime date;
        public int tripCount;
        public TimeSpan totalTime;
        public DetailDate(DateTime date, int tripCount, TimeSpan totalTime) : base(25f, CellType.detailDate)
        {
            this.date = date;
            this.tripCount = tripCount;
            this.totalTime = totalTime;
        }
    }

    public class ListTicket : CellBase
    {
        // 가능하다면 여기서는 그냥 데이터 클래스를 통째로 가지고 있어도 될 것 같은데,,
        public DateTime date;
        public string title;
        public TimeSpan totalTime;
        public int tripType;

        public ClientTripLog _log;

        public ListTicket(ClientTripLog log)
            : base(100.0f, CellType.listTicket)
        {
            _log = log;
            date = log.begin_time.ToLocalTime();
            title = log.name;   // ???
            totalTime = log.end_time - log.begin_time;
            tripType = log.trip_type;
        }

        //public ListTicket(DateTime date, string title, TimeSpan totalTime, int tripType) : base(100f, CellType.listTicket)
        //{
        //    this.date = date;
        //    this.title = title;
        //    this.totalTime = totalTime;
        //    this.tripType = tripType;
        //}
    }

    public class GalleryGroup : CellBase
    {
        public int month;
        public List<ListTicket> items;

        public GalleryGroup(int month,List<ListTicket> list) : base(240.0f, CellType.galleryGroup)
        {
            items = list;
			height = 240.0f + 248f * ((items.Count - 1) / 2);   // 결국 높이를 내가 잡아줘야 하는군
		}

		//// 여기서 주는 셀 높이는 임시,,
		//public GalleryGroup(List<ListTicket> items) : base(240f, CellType.galleryGroup)
		//      {
		//          this.items = items;
		//          height += 248f * ((items.Count - 1) / 2);   // 결국 높이를 내가 잡아줘야 하는군
		//      }
	}

    #endregion

    public void init()
    {
        _scroller.Delegate = this;
        _scroller.cellViewWillRecycle = onCellViewWillRecycle;
	}

    public void setupData()
    {
        _listTypeData.Clear();
        _galleryTypeData.Clear();

        // 데이터는 전부 테스트용 임시!!
        _myTripSummary[0] = "달리기를 많이 했어요";
        _myTripSummary[1] = "아침형 인간이네요";
        _myTripSummary[2] = "대치동을 주로 탐험해요";
        _myTripSummary[3] = "1시간 이상 탐험하는 편이에요";

        List<TripLogGroup> groupList = ViewModel.Trip.makeLogGroupByMonth();

        foreach(TripLogGroup group in groupList)
        {
            _listTypeData.Add(new DetailDate(group.getMonthDateTime(), group.getLogList().Count, group.getTotalTime()));
            _galleryTypeData.Add(new DetailDate(group.getMonthDateTime(), group.getLogList().Count, group.getTotalTime()));

            List<ListTicket> listTicketList = new List<ListTicket>();

            foreach(ClientTripLog log in group.getLogList())
            {
				listTicketList.Add(new ListTicket(log));
            }

            _listTypeData.AddRange(listTicketList);
            _galleryTypeData.Add(new GalleryGroup(group.getMonth(), listTicketList));
		}
    }

    public void loadList(int type)
    {
        _allDataList.Clear();
        _allDataList.Add(new MyTrip(_myTripSummary, type));
        UIStatistics.getInstance().changeListType(type);

        if (type == ListType.list)
        {
            _allDataList.AddRange(_listTypeData);
        }
        else if (type == ListType.gallery)
        {
            _allDataList.AddRange(_galleryTypeData);
        }

        // clear all을 하게 되면 cellView를 재사용 하지 않고 전부 지워버림
        //        _scroller.ClearAll();
        if (_scroller.Container != null)
        {
            _scroller.ReloadData();
            Debug.Log("list reloaded");
        }
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var data = _allDataList[dataIndex];

        if(data.type == CellType.myTrip)
        {
            MyTrip tripData = (MyTrip)data;
            UIStatistics_myTrip item = scroller.GetCellView(_myTripCellView) as UIStatistics_myTrip;
            item.setup(tripData.descList);
            item.setListTypeImage(tripData.listType);
            return item;
        }
        else if(data.type == CellType.detailDate)
        {
            DetailDate tripData = (DetailDate)data;
            UIStatistics_detailDate item = scroller.GetCellView(_detailDateCellView) as UIStatistics_detailDate;
			item.setup(tripData.date, tripData.tripCount, tripData.totalTime);
            return item;
        }
        else if(data.type == CellType.listTicket)
        {
            ListTicket tripData = (ListTicket)data;
            UIStatistics_listTicket item = scroller.GetCellView(_listTicketCellView) as UIStatistics_listTicket;
			item.setup(tripData, () =>
            {
                UIStatisticsTripDetail.getInstance().open();
                UIStatisticsTripDetail.getInstance().setup(tripData._log);
                UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(UIStatistics.getInstance(), UIStatisticsTripDetail.getInstance());
            });
            return item;
        }
        else if(data.type == CellType.galleryGroup)
        {
            GalleryGroup tripData = (GalleryGroup)data;
            UIStatistics_galleryGroup item = scroller.GetCellView(_galleryGroupCellView) as UIStatistics_galleryGroup;
            item.setup(tripData, _galleryItem);
			//for (int i = 0; i < tripData.items.Count; ++i)
   //         {
   //             UIStatistics_galleryItem panel = Instantiate(_galleryItem, item.transform);
   //             panel.setup(tripData.items[i], () =>
   //             {
   //                 UIStatisticsTripDetail.getInstance().open();
   //                 UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(UIStatistics.getInstance(), UIStatisticsTripDetail.getInstance());
   //             });
   //         }

            return item;
        }

        return null;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return _allDataList[dataIndex].height;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _allDataList.Count;
    }

    // 경로 텍스쳐 테이터는 메모리를 많이 먹는다, 불필요 한게 있으면 지워야 한다
	public void onCellViewWillRecycle(EnhancedScrollerCellView cellView)
    {
        if( cellView is UIStatistics_galleryGroup)
        {
            UIStatistics_galleryGroup groupItem = cellView as UIStatistics_galleryGroup;
            groupItem.onWillRecycle();
		}
    }


}
