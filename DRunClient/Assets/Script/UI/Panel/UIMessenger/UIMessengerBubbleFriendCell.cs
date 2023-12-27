using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerBubbleFriendCell : UIMessengerBubble
{
    [SerializeField]
    private UIPhotoThumbnail _thumbnail;

    // 임시로 ClientChatRoomLog요걸로 처리 하자
    public void setup(ClientChatRoomLog log, bool showProfile = true)
    {
        base.setup(log);

        _thumbnail.gameObject.SetActive(showProfile);
        if (showProfile)
        {
            ClientMain.instance.getProfileCache().getProfileCache(log.sender_id, result =>
            {
                if (result.succeeded())
                {
                    var cache = result.result();
                    _thumbnail.setImageFromCDN(cache.Profile.getPicktureURL(GlobalConfig.fileserver_url));
                }
            });
        }
    }
}
