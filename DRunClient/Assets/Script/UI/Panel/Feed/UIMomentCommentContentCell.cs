using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMomentCommentContentCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_name;
    [SerializeField]
    private TMP_Text txt_message;
    [SerializeField]
    private TMP_Text txt_time;

    [SerializeField]
    private UIPhotoThumbnail _onwerThumbnail;

    [SerializeField]
    private ContentSizeFitter _sizeFiter;

    public Vector2 getComponentSize()
    {
        var rt = this.transform as RectTransform;
        var size = txt_message.GetPreferredValues();
        size.y += 60 + 4 + 16;
        return size;
    }

    public void setup(ClientMoment moment)
    {
        txt_name.text = moment._profile.name;
        txt_message.text = moment.story;
        txt_time.text = UIMomentComment.formatTime(DateTime.UtcNow - moment.update_time);

        _onwerThumbnail.setImageFromCDN(moment._profile.getPicktureURL(GlobalConfig.fileserver_url));
    }
}
