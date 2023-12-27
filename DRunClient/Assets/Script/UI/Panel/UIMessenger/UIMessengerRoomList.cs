using EnhancedUI.EnhancedScroller;
using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Festa.Client.Module;
using Festa.Client;
using Festa.Client.NetData;
using UnityEngine.UI;
using UnityEngine.Events;
using Festa.Client.Logic;

public class UIMessengerRoomList : UISingletonPanel<UIMessengerRoomList>, IEnhancedScrollerDelegate
{
    public class CellType
    {
        public static readonly int tab = 0;
        public static readonly int count = 1;
        public static readonly int profile = 2;
        public static readonly int room = 3;
        public static readonly int order = 4;
    };

    public class CellDataBase
    {
        public int type;
        public float height;

        public CellDataBase(int type, float height)
        {
            this.type = type;
            this.height = height;
        }
    }

    public class OrderCellData : CellDataBase
    {
        public OrderCellData() : base(CellType.order, 44.0f)
        {
        }
    }

    public class TabCellData : CellDataBase
    {
        public UnityAction<int> tabSelectCallback;
        public TabCellData(UnityAction<int> tabSelectCallback) : base(CellType.tab, 56.0f)
        {
            this.tabSelectCallback = tabSelectCallback;
        }
    }

    public class CountCellData : CellDataBase
    {
        public string message;
        public CountCellData(string message) : base(CellType.count, 31.0f)
        {
            this.message = message;
        }
    }

    public class ProfileCellData : CellDataBase
    {
        public ClientProfileCache profile;
        public ProfileCellData(ClientProfileCache profile) : base(CellType.profile, 72.0f)
        {
            this.profile = profile;
        }
    }

    public class RoomCellData : CellDataBase
    {
        public ChatRoomViewModel viewModel;
        public RoomCellData(ChatRoomViewModel viewModel) : base(CellType.room, 72.0f)
        {
            this.viewModel = viewModel;
        }
    }

    [SerializeField]
    private TMP_InputField tf_searchField;

    [SerializeField]
    private Button btn_searchFieldReset;

    [SerializeField]
    private EnhancedScroller _roomScroller;

    [SerializeField]
    private UIMessengerRoom _roomPrefab;

    [SerializeField]
    private UIMessengerRoomListTabCell _tabCellPrefab;

    [SerializeField]
    private UIMessengerRoomListCountCell _countCellPrefab;

    [SerializeField]
    private EnhancedScrollerCellView _orderCellPrefab;

    private IntervalTimer _timerRefreshRoomList;
    private ChatViewModel chatVM => ClientMain.instance.getViewModel().Chat;

    public enum ListType
    {
        Normal,
        Search,
    };

    public enum SearchType
    {
        Total = 0,
        Friends = 1,
        MessageRoom = 2,
    };


    private ListType _listType = ListType.Normal;
    private SearchType _searchType = SearchType.Total;

    private List<CellDataBase> _data = new List<CellDataBase>();

    public override void initSingleton(SingletonInitializer initializer)
    {
        base.initSingleton(initializer);

        _roomScroller.Delegate = this;
        _timerRefreshRoomList = IntervalTimer.create(2.0f, true, false);
        tf_searchField.onSubmit.AddListener(onSubmitSearchField);
    }

    public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
    {
        resetBindings();

        base.open(param, transitionType, closeType);
    }

    public void onClickTab(int index)
    {
        _searchType = (SearchType)index;
        onSubmitSearchField(tf_searchField.text);
    }

    public override void update()
	{
        base.update();

        if( gameObject.activeSelf && _timerRefreshRoomList.update())
		{
            refreshRoomList();
		}
	}

    private void resetBindings()
	{
        if( _bindingManager.getBindingList().Count > 0)
		{
            return;
		}

        ChatViewModel chatVM = ClientMain.instance.getViewModel().Chat;

        _bindingManager.makeBinding(chatVM, nameof(chatVM.RoomList), updateRoomList);
    }

    private void updateRoomList(object obj)
	{
        if( _roomScroller.Container != null)
		{
            _roomScroller.ReloadData();
		}
    }

    public override void onTransitionEvent(int type)
    {
        base.onTransitionEvent(type);
        if (type == TransitionEventType.start_open)
        {
            // open에서하면 EnhancedScroller가 아직..활성화가 안되어있네?
            updateRoomList(null);
        }
    }

    public void onClickSearchReset()
    {
        tf_searchField.text = "";
    }

    private Coroutine _cachedSubmitSearchFieldCoroutine = null;
    public void onValueChangedSearchField(string message)
    {
        btn_searchFieldReset.gameObject.SetActive(message.Length > 0);
        if ( message.Length == 0 )
        {
            _listType = ListType.Normal;
            updateRoomList(null);
        }

        if (_cachedSubmitSearchFieldCoroutine != null)
        {
            StopCoroutine(_cachedSubmitSearchFieldCoroutine);
            _cachedSubmitSearchFieldCoroutine = null;
        }

        if ( message.Length > 0)
            StartCoroutine(submitSearchFieldCoroutine(message));
    }

    IEnumerator submitSearchFieldCoroutine(string message)
    {
        yield return new WaitForSeconds(0.5f);
        onSubmitSearchField(message);
    }

    public void onSubmitSearchField(string message)
    {
        if (_cachedSubmitSearchFieldCoroutine != null)
        {
            StopCoroutine(_cachedSubmitSearchFieldCoroutine);
            _cachedSubmitSearchFieldCoroutine = null;
        }

        if (message.Length == 0)
        {
            return;
        }

        _listType = ListType.Search;

        List<ChatRoomViewModel> roomList = chatVM.searchRoom(message);

        var sc = GlobalRefDataContainer.getStringCollection();
        _data = new List<CellDataBase>();
        _data.Add(new TabCellData(onClickTab));

        if ( _searchType == SearchType.Total || _searchType == SearchType.Friends)
        {
            _data.Add(new CountCellData(sc.getFormat("message.roomlist.category.friend", 0, roomList.Count)));
            _data.Add(new OrderCellData());
            // 2022.05.04 이강희
            foreach (ChatRoomViewModel room in roomList)
            {
                _data.Add(new ProfileCellData(room.DMTargetProfile));
            }
        }

        if (_searchType == SearchType.Total || _searchType == SearchType.MessageRoom)
        {
            _data.Add(new CountCellData(sc.getFormat("message.roomlist.category.room", 0, roomList.Count)));

            foreach (ChatRoomViewModel room in roomList)
            {
                _data.Add(new RoomCellData(room));
            }
        }
        updateRoomList(null);
    }

    public void onSendCallback(ClientProfileCache profile)
    {
        // 룸 열어!!!
        OpenChatRoomProcessor step = OpenChatRoomProcessor.create(profile._accountID);
        step.run(result =>
        {
            if (result.succeeded())
            {
                UIMessenger.getInstance().setup(step.getRoomViewModel());
                UIMessenger.getInstance().open();
                ClientMain.instance.getPanelNavigationStack().push(this, UIMessenger.getInstance());
            }
        });
    }


    public void onClickNewChat()
    {
        UIMessengerSelectFriend.getInstance().open();

        UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIMessengerSelectFriend.getInstance());
        stack.addPrev(UIMainTab.getInstance());
    }

    public void onOpenCallback(ChatRoomViewModel roomVM)
	{
        UIMessenger.getInstance().setup(roomVM);
        UIMessenger.getInstance().open();

        UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIMessenger.getInstance());
        stack.addPrev(UIMainTab.getInstance());
	}

    private void refreshRoomList()
	{
        RefreshChatRoomListProcessor step = RefreshChatRoomListProcessor.create();
        step.run(result => { });
	}

    #region Enghanced Scroll Delegate
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        if (_listType == ListType.Normal)
        {
            ChatRoomViewModel roomVM = chatVM.RoomList[dataIndex];

            UIMessengerRoom rowItem = scroller.GetCellView(_roomPrefab) as UIMessengerRoom;
            rowItem.setup(roomVM, onOpenCallback);
            return rowItem;
        }
        else if ( _listType == ListType.Search)
        {
            var type = _data[dataIndex].type;
            if ( type == CellType.tab)
            {
                UIMessengerRoomListTabCell rowItem = scroller.GetCellView(_tabCellPrefab) as UIMessengerRoomListTabCell;
                rowItem.setup(((TabCellData)_data[dataIndex]).tabSelectCallback);
                return rowItem;
            }
            else if (type == CellType.count)
            {
                UIMessengerRoomListCountCell rowItem = scroller.GetCellView(_countCellPrefab) as UIMessengerRoomListCountCell;
                rowItem.setup(((CountCellData)_data[dataIndex]).message);
                return rowItem;
            }
            else if (type == CellType.profile)
            {
                UIMessengerRoom rowItem = scroller.GetCellView(_roomPrefab) as UIMessengerRoom;
                ProfileCellData data = (ProfileCellData)_data[dataIndex];
                rowItem.setup(data.profile, true, onSendCallback);
                return rowItem;
            }
            else if (type == CellType.room)
            {
                UIMessengerRoom rowItem = scroller.GetCellView(_roomPrefab) as UIMessengerRoom;
                rowItem.setup(((RoomCellData)_data[dataIndex]).viewModel, onOpenCallback);
                return rowItem;
            }
            else if ( type == CellType.order)
            {
                EnhancedScrollerCellView rowItem = scroller.GetCellView(_orderCellPrefab) as EnhancedScrollerCellView;
                return rowItem;
            }
        }

        return null;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        float height = 72.0f;
        if (_listType == ListType.Search)
        {
            height = _data[dataIndex].height;
        }
        return height;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        int count = 0;
        if (_listType == ListType.Normal)
        {
            count = chatVM.RoomList.Count;
        }
        else if ( _listType == ListType.Search)
        {
            count = _data != null ? _data.Count : 0;
        }

        return count;
    }
    #endregion
}
