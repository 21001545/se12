using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMessengerSetting : UISingletonPanel<UIMessengerSetting>, IEnhancedScrollerDelegate
{
    private class PictureData
    {
        public ClientChatRoomLog log;
        public string url;
        public int photoIndex = 0;
    };

    [SerializeField]
    private UIToggle togglePush;

    [SerializeField]
    private GameObject go_confirmPopup;

    [SerializeField]
    private EnhancedScroller _scroller;

    [SerializeField]
    private LayoutElement layout_pictures;

    [SerializeField]
    private UIGalleryPickerItem _galleryPickerItemPrefab;

    private ChatRoomViewModel _roomVM;
    private ClientNetwork Network => ClientMain.instance.getNetwork();

    private List<PictureData> _pictureList = new List<PictureData>();

    public override void initSingleton(SingletonInitializer initializer)
    {
        base.initSingleton(initializer);

        _scroller.Delegate = this;
    }

    public void setup(ChatRoomViewModel roomVM)
	{
        _roomVM = roomVM;
    }

	public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
	{
        resetBindings();

		base.open(param, transitionType, closeType);
	}

    public override void onTransitionEvent(int type)
    {
        base.onTransitionEvent(type);
        if ( type == TransitionEventType.start_open)
        {
            _pictureList.Clear();
            for (int i = 0; i < _roomVM.LogList.Count; ++i)
            {
                ClientChatRoomLog log = _roomVM.LogList[i];

                int logType = log.payload.getInteger("type");
                if (logType == 2 )
                {
                    JsonArray files = log.payload.getJsonArray("files");
                    for ( int k = 0; k < files.size(); ++k)
                    {
                        _pictureList.Add(new PictureData() { log = log, url = ClientChatRoomLog.getFileURL(files.getString(k)), photoIndex = k });
                    }
                }
            }

            // 사진이 없을 경우..
            layout_pictures.preferredHeight = _pictureList.Count == 0 ? 64 : 246;

            _scroller.ReloadData();
        }
    }

    public void onClickBackNavigation()
    {
        ClientMain.instance.getPanelNavigationStack().pop();
    }

    private void resetBindings()
	{
        // 기존 연결 clear하고 다시 연결
        _bindingManager.clearAllBindings();
        _bindingManager.makeBinding(_roomVM, nameof(_roomVM.PushConfig), updatePushConfig);
	}

    private void updatePushConfig(object obj)
	{
        bool isOn = _roomVM.PushConfig == ClientAccountChatRoom.PushConfig.enable;

        togglePush.set(isOn, true, false);
	}

    public void onClickPictureCollect()
    {
        UIMessengerPhotoCollect.getInstance().setup(_roomVM);
        UIMessengerPhotoCollect.getInstance().open();
        ClientMain.instance.getPanelNavigationStack().push(this, UIMessengerPhotoCollect.getInstance());

    }

    // 채팅방 나가기!
    public void onClickLeave()
    {
        go_confirmPopup.SetActive(true);
    }

    public void onClickLeaveConfirm()
    {
        LeaveChatRoomProcessor step = LeaveChatRoomProcessor.create(_roomVM);
        step.run(result =>
        {
            go_confirmPopup.SetActive(false);
            if (result.succeeded())
            {
                // 2022.05.06 이강희
                // UIMessengerSettings 꺼주고
                // UIMessenger도 꺼주어야 한다

                // BackNavigationStack도 정리해줘야 할것 같은데..

                ClientMain.instance.getPanelNavigationStack().clear();

                // hide UIMessengerSettings
                close();

                // open UIMessengerRoomList
                //UIMainTab.getInstance().changeTab(UIMainTab.Tab.messenger);
                UIMessengerRoomList.getInstance().open();
                UIMainTab.getInstance().open();
            }
        });
    }

    public void onClickLeaveCancel()
    {
        go_confirmPopup.SetActive(false);
    }

    public void onChangeTogglePush()
	{
        bool isOn = togglePush.isOn();
        changePushConfig( isOn ? ClientAccountChatRoom.PushConfig.enable : ClientAccountChatRoom.PushConfig.disable );
	}

    private void changePushConfig(int push_config)
	{
        MapPacket req = Network.createReq(CSMessageID.Chat.ChangeChatRoomPushConfigReq);
        req.put("id", _roomVM.ID);
        req.put("data", push_config);

        Network.call(req, ack => { 
            if( ack.getResult() == Festa.Client.ResultCode.ok)
			{
                _roomVM.PushConfig = push_config;
			}
        });
	}

    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        return _pictureList.Count;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        return 123.0f;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        var cell = _scroller.GetCellView(_galleryPickerItemPrefab) as UIGalleryPickerItem;
        cell.setClickCallback(clickDataIndex => {

            Vector2 centerPivot = new Vector2(375f * 0.5f, 812f * 0.5f);
            UIMessengerPhotoDetail.getInstance().setup(_pictureList[dataIndex].log, centerPivot, _pictureList[dataIndex].photoIndex);
            UIMessengerPhotoDetail.getInstance().open();
            ClientMain.instance.getPanelNavigationStack().push(this, UIMessengerPhotoDetail.getInstance());
        });
        cell.setup(dataIndex, _pictureList[dataIndex].url);
        return cell;
    }
}
