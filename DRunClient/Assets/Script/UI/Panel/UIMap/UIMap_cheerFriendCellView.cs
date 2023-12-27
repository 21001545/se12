using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.EnhancedScroller;
using TMPro;
using Festa.Client;
using Festa.Client.NetData;
using System;
using Festa.Client.Module.Net;
using ResultCode = Festa.Client.ResultCode;
using Festa.Client.Logic;
using Festa.Client.Module.UI;
//using UnityEngine.UIElements;

public class UIMap_cheerFriendCellView : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private UIPhotoThumbnail photo;

    public Button btn_cheer;
    public GameObject go_adds;
    public Button btn_add;
    public Button btn_added;
    public Button btn_messenger;

    private UIMap_cheersScroller _owner;
	private ClientTripCheerable _cheerable;

    private ClientNetwork Network => ClientMain.instance.getNetwork();

    public void setup(UIMap_cheersScroller owner,ClientTripCheerable cheerable)
    {
        _owner = owner;
        _cheerable = cheerable;

        setupProfileUI();

        // 나머지 UI설정
        setupStatusUI();
    }

    public void onClickCheer()
    {
        MapPacket req = Network.createReq(CSMessageID.Trip.CheerTripReq);
        req.put("id", _cheerable.account_id);
        req.put("sub_id", _cheerable.trip_id);
        
        // 응원의 종류 (이모티콘 종류 등등)
        req.put("object_type", 1);
        req.put("object_id", 1);

        Network.call(req, ack => { 
            if( ack.getResult() == ResultCode.ok)
            {
                _cheerable._isAlreadyCheered = true;
                
                setupStatusUI();
            }
        });
    }

    public void onClickFollow()
    {
        MapPacket req = Network.createReq(CSMessageID.Social.FollowReq);
        req.put("id", _cheerable.account_id);

        Network.call(req, ack => {
            if (ack.getResult() == ResultCode.ok)
            {
                // 내꺼 업데이트
                ClientMain.instance.getViewModel().updateFromPacket(ack);
                _cheerable._isFollow = true;
                switchFollow(true);
            }
        });
    }

    public void onClickUnfollow()
    {
        MapPacket req = Network.createReq(CSMessageID.Social.UnfollowReq);
        req.put("id", _cheerable.account_id);

        Network.call(req, ack => {
            if (ack.getResult() == ResultCode.ok)
            {
                // 내꺼 업데이트
                ClientMain.instance.getViewModel().updateFromPacket(ack);
                _cheerable._isFollow = false;
                switchFollow(false);
            }
        });
    }

    public void onClickMessenger()
    {
        OpenChatRoomProcessor step = OpenChatRoomProcessor.create(_cheerable.account_id);
        step.run(result =>
        {
            if (result.succeeded())
            {
                UIMessenger.getInstance().setup(step.getRoomViewModel());
                UIMessenger.getInstance().open();
                UIMessenger.getInstance().setHaveToClose(true);
                UIMap.getInstance().cheerPanel.close(TransitionEventType.openImmediately);
                UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(UIMap.getInstance(), UIMessenger.getInstance());
                stack.addPrev(UIMainTab.getInstance());
            }
        });
    }

    private void setupStatusUI()
    {
		btn_cheer.gameObject.SetActive(_cheerable._isAlreadyCheered == false);
        go_adds.gameObject.SetActive(_cheerable._isAlreadyCheered && _cheerable._isFollow == false);
        if(_cheerable._isAlreadyCheered && _cheerable._isFollow == false)
        {
            switchFollow(_cheerable._isFollow);
        }
		btn_messenger.gameObject.SetActive(_cheerable._isAlreadyCheered && _cheerable._isFollow == true);
	}

    private void switchFollow(bool follow)
    {
        if (follow)
        {
            btn_add.gameObject.SetActive(false);
            btn_added.gameObject.SetActive(true);
        }
        else
        {
            btn_add.gameObject.SetActive(true);
            btn_added.gameObject.SetActive(false);
        }
    }

	// 로딩이 늦게 될 수 있다
	private void setupProfileUI()
    {
		ClientMain.instance.getProfileCache().getProfileCache(_cheerable.account_id, result => {
			if (result.succeeded())
			{
				ClientProfileCache cache = result.result();
				if (cache._accountID == _cheerable.account_id)
				{
					txt_name.text = cache.Profile.name;
					photo.setImageFromCDN(cache.Profile.getPicktureURL(GlobalConfig.fileserver_url));
				}
			}
		});
    }
    
}