using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMessengerRoom : EnhancedScrollerCellView
{
    [SerializeField]
    private UIPhotoThumbnail _thumbnail;

    [SerializeField]
    private TMP_Text txt_recentMessage;

    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private TMP_Text txt_time;

    [SerializeField]
    private GameObject go_count;

    [SerializeField]
    private GameObject go_send;

    [SerializeField]
    private GameObject go_openchatroom;

    [SerializeField]
    private Image img_coin;

    [SerializeField]
    private TMP_Text txt_unreadCount;

    private ClientProfileCache _profileCache;
    UnityAction<ClientProfileCache> _sendCallback;
    UnityAction<ChatRoomViewModel> _openCallback;

    private ChatRoomViewModel _chatRoomViewModel;
    private UIBindingManager _bindingManager;

    private IntervalTimer _timerLatest;

    private void init()
	{
        if( _bindingManager == null)
		{
            _bindingManager = UIBindingManager.create();
        }

        if( _timerLatest == null)
		{
            // config로 빼보면 어떨까?
            _timerLatest = IntervalTimer.create(1.0f, true, false); 
		}
    }

    public void setup(ChatRoomViewModel viewModel,UnityAction<ChatRoomViewModel> openCallback)
    {
        init();

        _chatRoomViewModel = viewModel;
        _openCallback = openCallback;
        makeBinding();

        // AccountChatRoom 아닌 것 같은데?
        txt_time.gameObject.SetActive(true);

        updateUnreadCount(null);

        go_send.gameObject.SetActive(false);
        go_openchatroom.gameObject.SetActive(true);

        // last_log_id를 통해서 마지막 log를 가져와야하나..?
        //ClientMain.instance.getLocalChatData().readAccountChatRoom
        //ClientMain.instance.getLocalChatData().readChatLog

        _profileCache = viewModel.RoomData._dmTargetProfile;
        txt_name.text = _profileCache.Profile.name;
        _thumbnail.setImageFromCDN(_profileCache.Profile.getPicktureURL(GlobalConfig.fileserver_url));
    }

    public void setup(ClientProfileCache profile, bool isFollowEachOther, UnityAction<ClientProfileCache> sendCallback)
    {
        init();

        _chatRoomViewModel = null;
        _bindingManager.clearAllBindings();

        _profileCache = profile;
        _sendCallback = sendCallback;

        txt_name.text = profile.Profile.name;
        txt_recentMessage.text = profile.Profile.message;

        _thumbnail.setImageFromCDN(profile.Profile.getPicktureURL(GlobalConfig.fileserver_url));

        txt_time.gameObject.SetActive(false);
        go_count.gameObject.SetActive(false);
        go_send.gameObject.SetActive(true);
        go_openchatroom.gameObject.SetActive(true);

        // 2022.05.04
        // 이미 채팅방이 열려 있으면 돈 표시 않함
        bool room_opended = ClientMain.instance.getViewModel().Chat.findDirectMessageRoom(profile._accountID) != null;
        img_coin.gameObject.SetActive(room_opended == false && isFollowEachOther == false);
    }

    public void onClickSend()
    {
        _sendCallback?.Invoke(_profileCache);
    }
    
    public void onClickOpenChatRoom()
	{
        if (_sendCallback != null )
        {
            _sendCallback.Invoke(_profileCache);
        }
        else if (_openCallback != null )
        {
            _openCallback.Invoke(_chatRoomViewModel);
        }
	}

    private void makeBinding()
	{
        _bindingManager.clearAllBindings();

        _bindingManager.makeBinding(_chatRoomViewModel, nameof(_chatRoomViewModel.UnreadCount), updateUnreadCount);
        _bindingManager.makeBinding(_chatRoomViewModel, nameof(_chatRoomViewModel.LatestLogTime), updateLatestLogTime);
        _bindingManager.makeBinding(_chatRoomViewModel, nameof(_chatRoomViewModel.RecentMessage), updateRecentMessage);

        _bindingManager.updateAllBindings();
    }

    // 읽지 않은 메세지 수
    private void updateUnreadCount(object obj)
	{
        if (_chatRoomViewModel.UnreadCount <= 0 )
        {
            go_count.SetActive(false);
        }
        else
        {
            go_count.SetActive(true);
            txt_unreadCount.text = _chatRoomViewModel.UnreadCount.ToString();
        }
    }

    // 최신 메세지 시간
    private void updateLatestLogTime(object obj)
	{
        // 재입장 관련 버그 수정
        if( _chatRoomViewModel.LastLogID >= _chatRoomViewModel.RoomData.begin_log_id)
		{
            txt_time.text = UIMomentComment.formatTime(DateTime.UtcNow - _chatRoomViewModel.LatestLogTime);
        }
        else
		{
            txt_time.text = "";
		}
    }

    // 최신 메세지
    private void updateRecentMessage(object obj)
	{
        txt_recentMessage.text = _chatRoomViewModel.RecentMessage;
	}

    void Update()
	{
        if( gameObject.activeSelf && _chatRoomViewModel != null && _timerLatest.update())
		{
            QueryChatRoomLatestLogIDProcessor step = QueryChatRoomLatestLogIDProcessor.create(_chatRoomViewModel);
            step.run(result => { });
        }
	}

}
