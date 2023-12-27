using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.LocalDB;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MessageDataType
{
    public static readonly int profile = 0;
    public static readonly int date = 1;
    public static readonly int time = 2;
    public static readonly int my_message = 3;
    public static readonly int friend_message = 4;
    public static readonly int my_photo = 5;
    public static readonly int friend_photo = 6;
}

public class MessageDataBase
{
    // 뭐 없네. is-as 보단 type 체킹이 더 빠르겟지
    public int messageType;

    // 데이터별로 높이가 다를 수 있군!
    public float height;

    // 높이 계산이 끝낫나? 최적화 때문에라도 중복해서 하지 말자.
    public bool alreadyCalculate = true;

    public MessageDataBase(int messageType, float height, bool alreadyCalculate)
    {
        this.messageType = messageType; 
        this.height = height;
        this.alreadyCalculate = alreadyCalculate;
    }
}

public class ProfileMessageData : MessageDataBase
{
    public ClientProfileCache profile;
    public ProfileMessageData(ClientProfileCache profile) : base(MessageDataType.profile, 333.0f, true)
    {
        this.profile = profile;
    }
}

public class DateMessageData : MessageDataBase
{
    public DateTime time;
    public DateMessageData(DateTime time) : base(MessageDataType.date, 41.0f, true)
    {
        this.time = time;
    }
}

public class TimeMessageData : MessageDataBase
{
    public DateTime time;
    public bool isMine;
    public TimeMessageData(DateTime time, bool isMine) : base(MessageDataType.time, 30.0f, true)
    {
        this.time = time;
        this.isMine = isMine;
    }
}

public class MessageData : MessageDataBase
{
    public ClientChatRoomLog log;
    public int photoCount = 0;
    public MessageData(ClientChatRoomLog log, bool isMine) : base(isMine ? MessageDataType.my_message : MessageDataType.friend_message, 305.0f, false)
    {
        this.log = log;

        JsonArray files = log.payload.getJsonArray("files");
        if (files != null )
        {
            photoCount = files.size();
            this.messageType = isMine ? MessageDataType.my_photo : MessageDataType.friend_photo;
            this.alreadyCalculate = true;
        }
    }
}

public class UIMessenger : UISingletonPanel<UIMessenger>, IEnhancedScrollerDelegate
{
    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private RectTransform rt_pivot;

    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private RectTransform rt_inputField;

    [SerializeField]
    private TMP_InputField tf_inputField;

    [SerializeField]
    private TMP_InputField tf_searchField;

    [SerializeField]
    private GameObject go_searchField;
    [SerializeField]
    private CanvasGroup can_searchFieldTop;

    [SerializeField]
    private TMP_Text txt_searchResultCount;

    [SerializeField]
    private Button btn_send;

    [SerializeField]
    private Button btn_searchUp;

    [SerializeField]
    private Button btn_searchDown;

    // 현재 검색 cell index.
    private int currentSearchCellIndex = -1;
    private List<LDB_ChatRoomLog> _searchResultList;
    private string _searchText;

    [SerializeField]
    private GameObject go_top;

    [SerializeField]
    private GameObject go_galleryPicker;
    [SerializeField]
    private RectTransform rect_galleryPicker;
    private float _galleryPickerHeight = 322f;

    [SerializeField]
    private UIMessengerProfileCell _profileCellPrefab;

    [SerializeField]
    private UIMessengerDateCell _dateCellPrefab;

    [SerializeField]
    private UIMessengerBubbleMyCell _bubbleMyCellPrefab;

    [SerializeField]
    private UIMessengerBubbleFriendCell _bubbleFriendCellPrefab;

    [SerializeField]
    private UIMessengerBubblePhotoCell _bubbleMyPhotoPrefab;

    [SerializeField]
    private UIMessengerBubblePhotoCell _bubbleFriendPhotoPrefab;
    [SerializeField]
    private UIMessengerTimeCell _timeCellPrefab;

    [SerializeField]
    private GameObject go_restrictPopup;

    [SerializeField]
    private UIToggleButton btn_picture;

    [SerializeField]
    private UIToggleButton btn_camera;

    #region Gallery Picker

    [SerializeField]
    private EnhancedScroller _galleryPickerScroller;

    [SerializeField]
    private UIGalleryPickerItem _galleryItemPrefab;

    private UIGalleryPickerScrollDelegate _galleryPickerScrollDelegate;
    private FloatSmoothDamper _scrollRectDamper;
    private FloatSmoothDamper _galleryPickerDamper;
    private bool _isGalleryPickerDirty = false;
    private float[] _prevScrollInfo = new float[3];

    #endregion

    private bool _haveToClose = false;  // 명시적으로 닫아야 하는지??

    private List<MessageDataBase> _data;
    private ClientProfileCache _profile;
    private ChatRoomViewModel _roomVM;

    private IntervalTimer _timerLatestLog;

    // 새로운 메시지가 입력되었을 때 무조건 아래로 스크롤 유지 해줄 것인가?
    private bool _forceScrollBottomOnReload = true;

    private LocalChatDataManager LocalChatData => ClientMain.instance.getLocalChatData();

    private bool _validInputFIeld = false;
    private float _keyboardHeight = 0.0f; // 현재 키보드 높이
    private float _inputFieldHeight = 0.0f; // 인풋필드 높이
    private int _inputFieldLineCount = 0;
/*    private Animator _animator;

    public void Start()
    {
        _animator = GetComponent<Animator>();
    }*/

    public override void initSingleton(SingletonInitializer initializer)
    {
        base.initSingleton(initializer);
        
        _scroller.Delegate = this;
        _scrollRectDamper = FloatSmoothDamper.create(0.0f, 0.08f);
        _galleryPickerDamper = FloatSmoothDamper.create(-_galleryPickerHeight, 0.1f);

        tf_inputField.onSubmit.AddListener(onSumbitInputField);
        tf_searchField.onSubmit.AddListener(onSubmitSearchField);

        _timerLatestLog = IntervalTimer.create(1.0f, true, false);
    }

    public void setup(ChatRoomViewModel roomVM)
    {
        if (_roomVM != roomVM)
        {
            tf_inputField.text = "";
        }

        _roomVM = roomVM;

        bool eachFollow = _roomVM.DMTargetProfile.Profile.isFollowEachOther() == false;

        btn_picture.IsOn = eachFollow;
        btn_camera.IsOn = eachFollow;

        _profile = _roomVM.DMTargetProfile;
        txt_name.text = _profile.Profile.name;
    }

    public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
    {
        resetBindings();

        _scrollRectDamper = FloatSmoothDamper.create(0f, 0.1f);

        base.open(param, transitionType, closeType);

        // 로컬에서 로딩해야 되는 경우
        Vector2Int logRangeInVM = _roomVM.getVMLogIDRange();
        if( logRangeInVM.y < _roomVM.LocalLastLogID)
		{
            int begin = _roomVM.RoomData.begin_log_id;
            int end = _roomVM.LocalLastLogID;

            // 과거 로그는 일단 100개만 로딩
            if( begin < end - 100)
			{
                begin = end - 100;
			}

            queryChatLogFromLocal(begin, end);
		}

        // 서버에서 로딩해야 되는 경우
        if( _roomVM.LastLogID > _roomVM.LocalLastLogID)
		{
            queryChatLog();
		}

        _forceScrollBottomOnReload = true;
        _haveToClose = false;
    }

    public override void update()
	{
        base.update();

        if( gameObject.activeSelf )
		{
            if (_timerLatestLog.update())
                queryLatestLog();

            if ( tf_inputField.isFocused)
            {
                // 812.0f -> height of canvas scaler 
                var height = (TouchScreenKeyboardUtil.getInstance().getHeight() / Screen.height) * 812.0f;
                //Debug.Log($"keyboard height : {height}, pivot y size delta : {rt_pivot.sizeDelta.y}");

                // 32.0f 만큼 차이가 난다. (iOS 아래쪽 빈공간 높이가 32 라네!!)
                if (height - 32.0f > 0 && rt_pivot.sizeDelta.y != -(height - 32))
                {
                    height = height - 32.0f;
                    //if (_keyboardHeight != height)
                    //{
                        if ( height < _keyboardHeight )
                        {
                            // 이미 올라와 있는데(사진첩 같은?)
                            // 낮아진 경우..
                            if (isGalleryPickerOpen())
                            {
                                closeGalleryPicker(true);
                            }
                        }
                        _keyboardHeight = height;

                        _prevScrollInfo[0] = _scroller.NormalizedScrollPosition;
                        _prevScrollInfo[1] = _scroller.ScrollPosition;
                        _prevScrollInfo[2] = _scroller.ScrollRectSize;

                        rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -(_keyboardHeight + _inputFieldHeight));

                        setScrollPosition();
                    //}
                }
            }
            else if (!isGalleryPickerOpen() && !_isGalleryPickerDirty && rt_pivot.sizeDelta.y != 0.0f)
            {
                _prevScrollInfo[0] = _scroller.NormalizedScrollPosition;
                _prevScrollInfo[1] = _scroller.ScrollPosition;
                _prevScrollInfo[2] = _scroller.ScrollRectSize;

                _keyboardHeight = 0.0f;

                rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -(_keyboardHeight + _inputFieldHeight));

                setScrollPosition();
            }

            if (_isGalleryPickerDirty)
            {
                if (_scrollRectDamper.update())
                {
                    rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -_scrollRectDamper.getCurrent());
                    _galleryPickerDamper.update();
                    rect_galleryPicker.anchoredPosition = new Vector2(rect_galleryPicker.anchoredPosition.x, _galleryPickerDamper.getCurrent());
                    setScrollPosition();
                }
                else
                {
                    _isGalleryPickerDirty = false;
                }
            }
        }
    }

    private void resetBindings()
	{
        // 방이 계속 바뀌는거라서 clear해주고 다시 binding함
        _bindingManager.clearAllBindings();
        _bindingManager.makeBinding(_roomVM, nameof(_roomVM.LogList), updateLogList);
	}

    private void queryChatLog()
	{
        int begin_id = _roomVM.LocalLastLogID + 1;
        if( begin_id < _roomVM.RoomData.begin_log_id)
		{
            begin_id = _roomVM.RoomData.begin_log_id;
		}

        QueryChatLogProcessor step = QueryChatLogProcessor.create(_roomVM, begin_id, _roomVM.LastLogID);
		step.run(result=>{ 
        
        });
	}

    private void queryChatLogFromLocal(int begin,int end)
	{
        QueryChatLogFromLocalProcessor step = QueryChatLogFromLocalProcessor.create(_roomVM, begin, end);
        step.run(result => { });
	}

    // 바뀐게 있는지 먼저 알아보고, 바뀐게 있으면 Log를 받아옴
    private void queryLatestLog()
	{
        QueryChatRoomLatestLogIDProcessor step = QueryChatRoomLatestLogIDProcessor.create(_roomVM);
        step.run(result => { 
            if( _roomVM.LastLogID > _roomVM.LocalLastLogID)
			{
                queryChatLog();
			}
        });
    }

    // 2022.05.03 메세지 하나 추가될때 마다 다시 빌드함
    // 나중에 최적화 할 예정
    private void updateLogList(object obj)
	{
        _data = new List<MessageDataBase>();
        _data.Add(new ProfileMessageData(_profile));

        //
        int myAccountID = ClientMain.instance.getNetwork().getAccountID();

        long lastDay = 0;
        long lastMin = -1;
        long timezoneOffset = TimeUtil.timezoneOffset();
        for(int i = 0; i < _roomVM.LogList.Count; ++i)
		{
            ClientChatRoomLog log = _roomVM.LogList[i];
            if( i == 0)
			{
                // 첫번째는 무조건 날짜를 넣어줌
                _data.Add(new DateMessageData(log.create_time));

                lastDay = (TimeUtil.unixTimestampFromDateTime(log.create_time) + timezoneOffset) / TimeUtil.msDay;
			}
            else
			{
                long curDay = (TimeUtil.unixTimestampFromDateTime(log.create_time) + timezoneOffset) / TimeUtil.msDay;

                // 날짜가 바뀜
                if(curDay != lastDay)
                {
                    lastDay = curDay;
                    _data.Add(new DateMessageData(log.create_time));
                }
            }

            var currentMin = (TimeUtil.unixTimestampFromDateTime(log.create_time) + timezoneOffset) / TimeUtil.msMinute;
            if (_data.Count > 0 && _data[_data.Count - 1].messageType == MessageDataType.time)
            {
                var timeData = _data[_data.Count - 1] as TimeMessageData;
                if (timeData.isMine == ( log.sender_id == myAccountID) )
                {
                    if (lastMin == currentMin)
                    {
                        _data.Insert(_data.Count - 1, new MessageData(log, log.sender_id == myAccountID));
                    }
                    else
                    {
                        _data.Add(new MessageData(log, log.sender_id == myAccountID));
                        _data.Add(new TimeMessageData(log.create_time, log.sender_id == myAccountID));
                    }
                }
                else
                {
                    // 내시간이 아닌데, 그냥 바로 추가.
                    _data.Add(new MessageData(log, log.sender_id == myAccountID));
                    _data.Add(new TimeMessageData(log.create_time, log.sender_id == myAccountID));
                }
            }
            else
            {
                _data.Add(new MessageData(log, log.sender_id == myAccountID));
                _data.Add(new TimeMessageData(log.create_time, log.sender_id == myAccountID));
            }

            lastMin = currentMin;
        }

        // Scroller가 초기화 되기 전일수도 있다
        if ( _scroller.Container != null)
		{
            var prevPosition = _scroller.NormalizedScrollPosition;

            var rectTransform = _scroller.GetComponent<RectTransform>();
            var size = rectTransform.sizeDelta;
            _scroller.ScrollPosition = 0;
            rectTransform.sizeDelta = new Vector2(size.x, float.MaxValue);

            _calculateCellSize = true;
            _scroller.ReloadData();

            rectTransform.sizeDelta = size;

            _calculateCellSize = false;
            _scroller.ReloadData();

            _scroller.Container.anchoredPosition = Vector2.zero;
            if (_forceScrollBottomOnReload)
            {
                _scroller.SetScrollPositionImmediately(_scroller.ScrollSize);
            }
            else
            {
                _scroller.SetScrollPositionImmediately(_scroller.ScrollSize * prevPosition);
            }
        }
    }

    public void setHaveToClose(bool close)
    {
        _haveToClose = close;
    }

    public void onClickBackNavigation()
	{
        // 정리 하고 나가자~
        if (isGalleryPickerOpen())
        {
            //go_galleryPicker.gameObject.SetActive(false);
            closeGalleryPicker(true);
        }

        ClientMain.instance.getPanelNavigationStack().pop();
        if (_haveToClose)
            close();
    }

    private bool isGalleryPickerOpen()
    {
        if (rect_galleryPicker.anchoredPosition.y > -1f)
            return true;

        return false;
    }

    public override void onTransitionEvent(int type)
    {
        base.onTransitionEvent(type);
        if (type == TransitionEventType.start_open)
        {
        }
    }

    public void onValueChangedInputField(string message)
    {
        if (message == "\n")
        {
            tf_inputField.text = "";
            return;
        }

        // 갤러리가 올라와 있으면, 메시지가 없어도 보낼 수 있다
        btn_send.interactable = message.Length > 0 || isGalleryPickerOpen();
        _validInputFIeld = false;
    }

    public void LateUpdate()
    {
        if (_validInputFIeld)
            return;

        _validInputFIeld = true;

        if (_inputFieldLineCount == tf_inputField.textComponent.textInfo.lineCount)
            return;

        if (_inputFieldLineCount >= 4 && _inputFieldLineCount < tf_inputField.textComponent.textInfo.lineCount)
            return;

        _inputFieldLineCount = tf_inputField.textComponent.textInfo.lineCount;

        // 메시지의 길이에 따라 InputField의 높이를 조절해보자        
        var bounds = tf_inputField.textComponent.textBounds;
        // 상하 패딩이 각각 13씩 26 px
        // 키보드 최소 높이는 44.0f
        // 4줄 기준 101.0f 네... 음.. 
        var height = Mathf.Min(101.0f, Mathf.Max(44.0f, Mathf.Floor(bounds.size.y) + 26.0f));

        rt_inputField.sizeDelta = new Vector2(rt_inputField.sizeDelta.x, height);

        var scrollPosition = 0.0f;
        var prevNormalPosition = _scroller.NormalizedScrollPosition;
        var prevScrollPosition = _scroller.ScrollPosition;
        var prevScrollRectSize = _scroller.ScrollRectSize;

        _inputFieldHeight = height - 44.0f;
        rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -(_keyboardHeight + _inputFieldHeight));

        if (prevNormalPosition < 1.0f)
        {
            // 스크롤 유지.
            scrollPosition = prevScrollPosition + (prevScrollRectSize - _scroller.ScrollRectSize);
        }
        else
        {
            // 무조건 아래로 스크롤
            scrollPosition = _scroller.ScrollSize;
        }
        _scroller.SetScrollPositionImmediately(scrollPosition);
    }

    public void onSumbitInputField(string message)
    {
        tf_inputField.text = "";
        sendMessage(message);
    }

    public void onClickSend()
    {
        sendMessage(tf_inputField.text);
    }

    private void sendMessage(string message)
    {
        if (message.Length > 0 )
        {
            message = message.TrimStart('\n');
            message = message.TrimEnd('\n');

            tf_inputField.text = "";

            // 캐럿 초기화를 위한...
            // private 함수였는데...이게..음........직접?해줘야해?
            tf_inputField.textComponent.rectTransform.anchoredPosition = new Vector2(0, 0);
            tf_inputField.AssignPositioningIfNeeded();

            JsonObject payload = new JsonObject();
            payload.put("type", ChatMessageType.text_message);
            payload.put("msg", message);

            SendDirectMessageProcessor processor = SendDirectMessageProcessor.create(_profile._accountID, payload, message);
            processor.run(result =>
            {
                if (result.succeeded())
                {

                }
            });
        }

        if (isGalleryPickerOpen())
        {
            // 이 때 버튼이.. 활성화 되어 있을 수도 있구나?
            btn_send.interactable = false;
            if (_galleryPickerScrollDelegate.getSelectionList().Count > 0)
            {
                _galleryPickerScrollDelegate.getResult(result =>
                {
                    onGalleryPickerFinish(result.result());
                });
            }
        }
    }
    
    public void onClickRestrictPopupClose()
    {
        go_restrictPopup.SetActive(false);
    }

    public void onClickSetting()
    {
        UIMessengerSetting.getInstance().setup(_roomVM);
        UIMessengerSetting.getInstance().open();
        ClientMain.instance.getPanelNavigationStack().push(this, UIMessengerSetting.getInstance());
    }

    #region search
    public void onClickSearch()
    {
        can_searchFieldTop.alpha = 1;
        go_searchField.SetActive(true);
        go_top.SetActive(false);
        //go_inputFieldField.SetActive(false);

        //_animator?.SetTrigger("searchOpen");

        tf_searchField.text = "";
        _searchResultList = null;
        txt_searchResultCount.text = $"(0/0)";
        btn_searchDown.interactable = false;
        btn_searchUp.interactable = false;

        //if ( tf_searchField.isFocused == false )
            tf_searchField.ActivateInputField();
    }

    public void onClickSearchClose()
    {
        DOTween.To(() => can_searchFieldTop.alpha, x => can_searchFieldTop.alpha = x, 0, 0.2f);
        go_top.SetActive(true);
        Invoke("searchFieldInactive", 0.2f);
        //go_inputFieldField.SetActive(true);
        //_animator?.SetTrigger("searchClose");
    }

    private void searchFieldInactive()
    {
        go_searchField.SetActive(false);
    }

    public void onClickSearchReset()
    {
        tf_searchField.text = "";        
    }

    public void onSubmitSearchField(string message)
    {
        searchLog(message);
    }

    // 2022.05.04 이강희 채팅 검색 예제
    /*
     * Local에서 검색하는 예제
     * Local에만 저장되어 있고 roomVM에는 아직 로딩되지 않았을 수 있다
     * 어떻게 할지 고민 필요 (몇년전 대화가 검색되었는데, 그 중간을 다 로딩할 수도 없고...)
     * 
     */
    private void searchLog(string searchText)
	{
        _searchText = searchText;
        tf_searchField.interactable = false;
        LocalChatData.searchChatLog(_roomVM.ID, _roomVM.RoomData.begin_log_id, searchText, result => {
            tf_searchField.interactable = true; 

            if ( result.succeeded())
			{
                var list = result.result();

                // 중복 결과를 거르기 위한 임시..
                Dictionary<int, LDB_ChatRoomLog> temp = new Dictionary<int, LDB_ChatRoomLog>();
                foreach (LDB_ChatRoomLog localLog in list)
				{
                    if (temp.ContainsKey(localLog.log_id))
                        continue;

                    temp.Add(localLog.log_id, localLog);
                    Debug.Log($"log_id[{localLog.log_id}] payload[{localLog.payload}]");                    
				}

                _searchResultList = temp.Values.ToList();
                _searchResultList.Reverse();

                currentSearchCellIndex = _searchResultList.Count > 0 ? 0 : -1;
                updateSearchResult();
                selectSearchResult();
            }
        });
	}

    private void updateSearchResult()
    {
        if (_searchResultList == null )
        {
            return;
        }

        txt_searchResultCount.text = $"({currentSearchCellIndex + 1}/{_searchResultList.Count})";

        btn_searchDown.interactable = currentSearchCellIndex > 0;
        btn_searchUp.interactable = (currentSearchCellIndex + 1) < _searchResultList.Count;
    }

    public void selectSearchResult()
    {
        if (_searchResultList == null || _searchResultList.Count == 0 )
        {
            return;
        }

        var log = _searchResultList[currentSearchCellIndex];

        // TODO, log_id에 해당하는 cell을 바로 찾을 수 있도록 인덱싱을 해두면 좋지만?
        // 아직 _data가 계속 만들어지고 있으므로, 방식이 픽스되면 최적화 하도록 하자.
        for (int i = _data.Count - 1; i >= 0; --i)
        {
            if ( _data[i].messageType == MessageDataType.friend_message || _data[i].messageType == MessageDataType.my_message )
            {
                var messageData = _data[i] as MessageData;
                if ( messageData.log.log_id == log.log_id )
                {
                    // 일단 옮기고, 
                    // 적당히 가운데로 스크롤을 해주고 싶은데..
                    float position = _scroller.GetScrollPositionForDataIndex(i, EnhancedScroller.CellViewPositionEnum.Before);
                    position -= _scroller.ScrollRectSize * 0.5f;
                    _scroller.SetScrollPositionImmediately(position);

                    // Active된 셀을 찾아서, 요놈을 셀렉트 하자!
                    var cellView = _scroller.GetCellViewAtDataIndex(i);
                    var cell = cellView as UIMessengerBubble;
                    if ( cell )
                    {
                        cell.select(_searchText);

                    }
                    break;
                }
            }
        }
    }

    public void onClickSearchUp()
    {
        currentSearchCellIndex++;
        selectSearchResult();
        updateSearchResult();
    }

    public void onClickSearchDown()
    {
        currentSearchCellIndex--;
        selectSearchResult();
        updateSearchResult();
    }
    #endregion

    private void moveGalleryPickerUpDown(float targetHeight)
    {
        // 갤러리피커
        //go_galleryPicker.gameObject.SetActive(true);

        if(targetHeight == 0f)
        {
            // 닫자!!
            _galleryPickerDamper.reset(0f);
            _galleryPickerDamper.setTarget(-_galleryPickerHeight);
        }
        else
        {
            // 열자!
            if (_galleryPickerScrollDelegate == null)
            {
                _galleryPickerScrollDelegate = new UIGalleryPickerScrollDelegate(_galleryPickerScroller, _galleryItemPrefab, false);
                _galleryPickerScrollDelegate.setSelectCallback(onGalleryPickerSelectPhoto);
                _galleryPickerScrollDelegate.loadImagePaths(100, true);
            }
            else
            {
                _galleryPickerScrollDelegate.clear();
                _galleryPickerScrollDelegate.loadImagePaths(100, true);
            }

            _galleryPickerDamper.reset(-_galleryPickerHeight);
            _galleryPickerDamper.setTarget(0f);
        }

        // 인풋필드
        // normal position, prev position, rect size
        _prevScrollInfo[0] = _scroller.NormalizedScrollPosition;
        _prevScrollInfo[1] = _scroller.ScrollPosition;
        _prevScrollInfo[2] = _scroller.ScrollRectSize;

        _keyboardHeight = targetHeight;
        _scrollRectDamper.setTarget(_keyboardHeight + _inputFieldHeight);
        _isGalleryPickerDirty = true;
    }

    private void setScrollPosition()
    {
        float targetScrollPosition = 0f;
        if (_prevScrollInfo[0] < 1f)
            // 스크롤 유지
            targetScrollPosition = _prevScrollInfo[1] + _prevScrollInfo[2] - _scroller.ScrollRectSize;
        else
            // 아래로 스크롤
            targetScrollPosition = _scroller.ScrollSize;

        _scroller.SetScrollPositionImmediately(targetScrollPosition);
    }

    private void closeGalleryPicker(bool immediate)
    {
        //go_galleryPicker.SetActive(false);
        //rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -(_keyboardHeight + _inputFieldHeight));

        if(immediate)
        {
            _prevScrollInfo[0] = _scroller.NormalizedScrollPosition;
            _prevScrollInfo[1] = _scroller.ScrollPosition;
            _prevScrollInfo[2] = _scroller.ScrollRectSize;

            _galleryPickerDamper.reset(-_galleryPickerHeight);
            rect_galleryPicker.anchoredPosition = new Vector2(rect_galleryPicker.anchoredPosition.x, -_galleryPickerHeight);
            _isGalleryPickerDirty = false;
            _keyboardHeight = 0.0f;
            _scrollRectDamper.reset(0f);
            rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -(_keyboardHeight + _inputFieldHeight));

            setScrollPosition();
        }   
        else
        {
            moveGalleryPickerUpDown(0f);
        }
    }

    public void onClickCamera()
    {
        if (_roomVM.DMTargetProfile.Profile.isFollowEachOther() == false)
        {
            go_restrictPopup.SetActive(true);
            return;
        }
    }

    public void onClickPicture()
    {
        if ( _roomVM.DMTargetProfile.Profile.isFollowEachOther() == false )
        {
            go_restrictPopup.SetActive(true);
            return;
        }

        if (isGalleryPickerOpen())
        {
            closeGalleryPicker(false);
            return;
        }

        tf_inputField.DeactivateInputField();

        _keyboardHeight = _galleryPickerHeight - 32.0f;

        // 2022.07.06 소현 : 스르륵 올라갈 거야!
        moveGalleryPickerUpDown(_keyboardHeight);

        //rt_pivot.sizeDelta = new Vector2(rt_pivot.sizeDelta.x, -(_keyboardHeight + _inputFieldHeight));
    }

    public void onClickPictureExpand()
    {
        UIGalleryPicker.getInstance().setFinishCallback(onGalleryPickerFinish);
        UIGalleryPicker.getInstance().setToggleCallback(onGalleryPickerSelectPhoto);
        UIGalleryPicker.getInstance().open();
    }

    public void onGalleryPickerSelectPhoto(int selectPhotoCount)
    {
        btn_send.interactable = selectPhotoCount > 0;
    }

    public void onGalleryPickerFinish(List<NativeGallery.NativePhotoContext> photoList)
    {
        closeGalleryPicker(false);
        btn_send.interactable = false;

        if (photoList.Count > 0)
        {
            // 사진을 보내자
            List<string> fileList = new List<string>();
            foreach(var context in photoList)
            {
                fileList.Add(context.path);
            }

            ChatSendFileProcessor step = ChatSendFileProcessor.create(_roomVM, fileList);
            JobProgressItemViewModel progressVM = step.getJobProgress();

            step.run(result =>
            {


            });
        }
    }

    #region Enhanced Scroller Delegate
    private bool _calculateCellSize = false;
    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var data = _data[dataIndex];
        if (data.messageType == MessageDataType.profile)
        {
            var cell = _scroller.GetCellView(_profileCellPrefab) as UIMessengerProfileCell;
            if ( _calculateCellSize == false)
                cell.setup(((ProfileMessageData)data).profile);
            return cell;
        }
        else if (data.messageType == MessageDataType.date)
        {
            var cell = _scroller.GetCellView(_dateCellPrefab) as UIMessengerDateCell;
            if (_calculateCellSize == false)
                cell.setup(((DateMessageData)data).time);
            return cell;
        }
        else if (data.messageType == MessageDataType.my_message)
        {
            var cell = _scroller.GetCellView(_bubbleMyCellPrefab) as UIMessengerBubbleMyCell;

            if (_calculateCellSize)
            {
                if (data.alreadyCalculate == false)
                {
                    cell.setup(((MessageData)data).log);
                    data.height = cell.getComponentSize().y;
                    data.alreadyCalculate = true;
                }
            }
            else
            {
                cell.setup(((MessageData)data).log);
            }
            return cell;
        }
        else if (data.messageType == MessageDataType.friend_message)
        {
            var cell = _scroller.GetCellView(_bubbleFriendCellPrefab) as UIMessengerBubbleFriendCell;

            if (_calculateCellSize)
            {
                if (data.alreadyCalculate == false)
                {
                    bool hideProfile = dataIndex > 0 && (_data[dataIndex - 1].messageType == MessageDataType.friend_photo || _data[dataIndex - 1].messageType == MessageDataType.friend_message);
                    cell.setup(((MessageData)data).log, !hideProfile);
                    data.height = cell.getComponentSize().y;
                    data.alreadyCalculate = true;
                }
            }
            else
            {
                bool hideProfile = dataIndex > 0 && (_data[dataIndex - 1].messageType == MessageDataType.friend_photo || _data[dataIndex - 1].messageType == MessageDataType.friend_message);
                cell.setup(((MessageData)data).log, !hideProfile);
            }
            return cell;
        }

        else if (data.messageType == MessageDataType.my_photo || data.messageType == MessageDataType.friend_photo)
        {
            MessageData messageData = (MessageData)data;
            UIMessengerBubblePhotoCell cellPrefab = cellPrefab = data.messageType == MessageDataType.my_photo ? _bubbleMyPhotoPrefab : _bubbleFriendPhotoPrefab;

            var cell = _scroller.GetCellView(cellPrefab) as UIMessengerBubblePhotoCell;

            if (_calculateCellSize == false)
            {
                bool hideProfile = data.messageType == MessageDataType.friend_photo && dataIndex > 0 && (_data[dataIndex - 1].messageType == MessageDataType.friend_photo || _data[dataIndex - 1].messageType == MessageDataType.friend_message);
                cell.setup(messageData.log, !hideProfile);
            }
            else
            {
                data.height = cell.getHeight(messageData.log);
            }
            return cell;
        }
        else if (data.messageType == MessageDataType.time)
        {
            var cell = _scroller.GetCellView(_timeCellPrefab) as UIMessengerTimeCell;
            if (_calculateCellSize == false)
                cell.setup(((TimeMessageData)data).time, ((TimeMessageData)data).isMine);
            return cell;
        }

        return null;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        var data = _data[dataIndex];
        return data.height;
    }

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _data == null ? 0 : _data.Count;
    }
    #endregion
}
