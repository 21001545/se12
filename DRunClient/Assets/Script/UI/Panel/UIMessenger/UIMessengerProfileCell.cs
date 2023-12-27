using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerProfileCell : EnhancedScrollerCellView
{
    [SerializeField]
    private UIPhotoThumbnail _thumbnail;

    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private TMP_Text txt_followCount;

    [SerializeField]
    private TMP_Text txt_lifeStat;

    [SerializeField]
    private TMP_Text txt_socialScore;

    private ClientProfileCache _profile;

    public void setup(ClientProfileCache profile)
    {
        _profile = profile;

        _thumbnail.setImageFromCDN(profile.Profile.getPicktureURL(GlobalConfig.fileserver_url));

        txt_name.text = profile.Profile.name;
        txt_followCount.text = GlobalRefDataContainer.getStringCollection().getFormat("message.profile.follow.count", 
            0, profile.Social.FollowCumulation.follow_count.ToString("N0"));
        txt_lifeStat.text = GlobalRefDataContainer.getStringCollection().getFormat("message.profile.lifestat.count",
            0, profile.Social.FollowCumulation.follow_back_count.ToString("N0"));
        txt_socialScore.text = "0"; // 이거 어디서 가져와..?            
    }

    public void onClickViewProfile()
    {
        UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
        param.accountID = _profile._accountID;

        UIMessenger.getInstance().gameObject.SetActive(false);
        UIProfile.getInstance().open(param);
        ClientMain.instance.getPanelNavigationStack().push(UIMessenger.getInstance(), UIProfile.getInstance());
    }
}
