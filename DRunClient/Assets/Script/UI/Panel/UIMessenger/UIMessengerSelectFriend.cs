using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
//using PhoneNumbers;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIMessengerSelectFriend : UISingletonPanel<UIMessengerSelectFriend>, IEnhancedScrollerDelegate
{
    [SerializeField]
    private TMP_InputField tf_searchField;

    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private UIMessengerRoom _roomPrefab;

    [SerializeField]
    private GameObject go_confirmPopup;

    [SerializeField]
    private GameObject go_loading;

    [SerializeField]
    private Image img_moreIndicator;

    [SerializeField]
    private TMP_Text txt_price;

    private Color _moreIndicatorColor = new Color(0.0f, 0.0f, 0.0f, 1.0f);

    private SocialViewModel ViewModel => ClientMain.instance.getViewModel().Social;
    private ClientNetwork Network => ClientMain.instance.getNetwork();

    //private bool _isLoading = false;
    private bool _moreRequest = false;

    private List<ClientProfileCache> _searchList = null;

    private Action _confirmOpenChatRoomCallback;

    public void Start()
    {
        _scroller.ScrollRect.onValueChanged.AddListener(onScrollScrolled);

        // 오픈 가격을 여기서 설정하자.
        txt_price.text = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Chat.dm_open_price, 400).ToString("N0");
    }

    public override void initSingleton(SingletonInitializer initializer)
    {
        base.initSingleton(initializer);

        _scroller.Delegate = this;
    }

    public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
    {
        base.open(param, transitionType, closeType);

        _searchList = null;

        go_confirmPopup.SetActive(false);
        reloadFriendList(false);
    }

    public override void close(int transitionType = 0)
    {
        ClientMain.instance.getPanelNavigationStack().pop();
    }

    public void onValueChangedSearchField(string value)
    {
        if (value.Length == 0)
        {
            // 검색이 없는거임.
            _scroller.ReloadData();
        }
    }

    // 검색창에 입력을 완료 했을 경우
    public void onSubmitSearchField(string query)
    {
        if (query.Length == 0 )
        {
            return;
        }

        var queryType = SearchAccountProcessor.QueryType.name;
        try
        {
            LocaleManager locale = ClientMain.instance.getLocale();

            //var util = PhoneNumberUtil.GetInstance();
            //PhoneNumber number = util.Parse(query, locale.getCountryCode());
            //query = util.Format(number, PhoneNumberFormat.E164);
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }

        SearchAccountProcessor processor = SearchAccountProcessor.create(queryType, query);
        go_loading.SetActive(true);
        processor.run(result =>
        {
            go_loading.SetActive(false);
            tf_searchField.interactable = true;
            if (result.succeeded())
            {
                // 음, 이거 통합 기능 만들어야 하는데..
                List<ClientProfileCache> accountList = processor.getResultList();

                // 2022.05.03 이강희 이미 채팅이 열린 유저는 제외
                ChatViewModel chatViewModel = ClientMain.instance.getViewModel().Chat;
                _searchList = new List<ClientProfileCache>();
                foreach (ClientProfileCache profile in accountList)
                {
                    if (profile.Profile._isFollow == false)
					{
                        continue;
					}

                    if( chatViewModel.findDirectMessageRoom( profile._accountID) != null)
					{
                        continue;
					}
                    
                    _searchList.Add(profile);
                }

                _scroller.ReloadData();
            }
        });
    }

    private void reloadFriendList(bool more = false)
    {
        if (more == false && _searchList != null)
        {
            reloadScrollerData(more);
            return;
        }

        go_loading.SetActive(true);
        //_isLoading = true;
        Vector2Int range = ViewModel.getFollowNextPage();
        QueryFollowProcessor processor = QueryFollowProcessor.create(Network.getAccountID(), range,ViewModel);
        processor.run(result =>
        {
            // 2022.05.03 이강희

            ChatViewModel chatVM = ClientMain.instance.getViewModel().Chat;

            _searchList = new List<ClientProfileCache>();
            foreach(ClientFollow follow in ViewModel.FollowList)
			{
                _searchList.Add(follow._profileCache);
			}

            //_isLoading = false;
            go_loading.SetActive(false);
            reloadScrollerData(more);
        });
    }

    private void reloadScrollerData(bool keepScrollPosition)
    {
        // 아직 초기화 전이다
        if( _scroller.Container == null)
		{
            return;
		}

        float prev_pos = _scroller.ScrollPosition;
        _scroller.ReloadData();

        if (keepScrollPosition)
        {
            _scroller.ScrollPosition = prev_pos;
        }
    }

    public void onClickConfirmClose()
    {
        go_confirmPopup.SetActive(false);
    }

    public void onClickConfirm()
    {
        if(_confirmOpenChatRoomCallback != null)
		{
            _confirmOpenChatRoomCallback();
        }
        go_confirmPopup.SetActive(false);

        // TODO, 돈내고 룸 열어!
    }

    private void onScrollScrolled(Vector2 position)
    {
        if (position.y < 0.0f)
        {
            if (img_moreIndicator.gameObject.activeSelf == false)
                img_moreIndicator.gameObject.SetActive(true);

            float delta = -position.y * _scroller.ScrollSize;
            float alpha = delta / img_moreIndicator.rectTransform.rect.height;

            _moreIndicatorColor.a = alpha;
            img_moreIndicator.color = _moreIndicatorColor;

            // indicator가 나와야 할 것 같은데? 인디케이터 크기가 40 쯤 되려나?
            if (delta >= img_moreIndicator.rectTransform.rect.height)
            {
                // 좀 더 사진을 땡겨와보자
                // drag를 끝냈으면 하는데,
                _moreRequest = true;
            }
            else
            {
                _moreRequest = false;
            }
        }
        else
        {
            if (img_moreIndicator.gameObject.activeSelf)
                img_moreIndicator.gameObject.SetActive(false);
        }
    }

    public void onScrollEndDrag(BaseEventData e)
    {
        if (_moreRequest)
        {
            reloadFriendList(true);
            _moreRequest = false;
        }
    }

    private void onSendCallback(ClientProfileCache profile)
    {
        if ( profile == null )
        {
            return;
        }

        bool room_opended = ClientMain.instance.getViewModel().Chat.findDirectMessageRoom(profile._accountID) != null;

        // 맞팔이 아니면 돈내야됨
        if ( room_opended == false && profile.Profile.isFollowEachOther() == false )
        {
            _confirmOpenChatRoomCallback = () =>
                {
                    startOpenChatRoom(profile);
                };
            go_confirmPopup.SetActive(true);
        }
        else
        {
            startOpenChatRoom(profile);
        }
    }

    //
    private void startOpenChatRoom(ClientProfileCache profile)
	{
        if ( profile.Profile.isFollowEachOther() == false )
        {
            var cost = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Chat.dm_open_price, 400);
            // 돈계산을 해야한다.
            if (ClientMain.instance.getViewModel().Wallet.FestaCoin < cost)
            {
                var sc = GlobalRefDataContainer.getStringCollection();
                UIPopup.spawnOK(sc.get("tree.purchase.popup.notenough.coin", 0));
                return;
            }
        }
        OpenChatRoomProcessor step = OpenChatRoomProcessor.create(profile._accountID);
        step.run(result => { 
            if( result.succeeded())
			{
                UIMessenger.getInstance().setup(step.getRoomViewModel());
                UIMessenger.getInstance().open();
                ClientMain.instance.getPanelNavigationStack().push(this, UIMessenger.getInstance());
            }
            else
            {
                var sc = GlobalRefDataContainer.getStringCollection();
                UIPopup.spawnOK(sc.get("messageopen.popup.title", 0), sc.get("messageopen.popup.error", 0), () => { 
                
                });
            }
        });
    }

    #region Enhanced Scroller Delegate
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _searchList == null ? 0 : _searchList.Count;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 72.0f;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        UIMessengerRoom rowItem = scroller.GetCellView(_roomPrefab) as UIMessengerRoom;
        var profile = _searchList[dataIndex];
        rowItem.setup(profile, profile.Profile.isFollowEachOther(), onSendCallback);

        return rowItem;
    }
    #endregion
}
