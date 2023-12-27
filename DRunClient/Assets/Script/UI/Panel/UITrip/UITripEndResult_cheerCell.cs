using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Festa.Client;
using Festa.Client.NetData;

public class UITripEndResult_cheerCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_username;
    [SerializeField]
    private UIPhotoThumbnail _thumbnail;
    [SerializeField]
    private GameObject go_add;
    [SerializeField]
    private GameObject go_lowerText;
    [SerializeField]
    private UIAddFriendButton _addFriendButton;

    private ClientTripCheering _data;

    public void setup(ClientTripCheering data)
    {
        _data = data;

        // 내친구가 아닌 유저가 응원을 했을 경우도 있음
        // 그래서 높은 확률로 profile정보를 얻어오는데 시간이 오래 걸릴 수 있음
        // 그사이에 다른 UI를 표시해줘야 할듯
        setupProfileUI();

        // 데이터에 따라 처리,,
/*        txt_username.text = username;
        _thumbnail.loadFromCDN(profileURL);

        go_add.SetActive(!isMyFriend);
        go_lowerText.SetActive(!isMyFriend);

        if (!isMyFriend)
            _addFriendButton.setAdd();*/
    }

    public void onClickAdd()
    {
        // 데이터처리,,
        _addFriendButton.setAdded();
    }

    private void setupProfileUI()
    {
        //로딩이 완료될때까지만 
        go_add.SetActive(false);
        go_lowerText.SetActive(false);

        ClientMain.instance.getProfileCache().getProfileCache(_data.cheerer_id, result => { 
            if( result.succeeded())
            {
                ClientProfileCache cache = result.result();
                if( cache._accountID == _data.cheerer_id)
                {
                    txt_username.text = cache.Profile.name;
                    _thumbnail.setImageFromCDN(cache.Profile.getPicktureURL(GlobalConfig.fileserver_url));

                    go_add.SetActive(cache.Profile._isFollow == false);
                    go_lowerText.SetActive(cache.Profile._isFollow == false);

                    if( cache.Profile._isFollow == false)
                    {
						_addFriendButton.setAdd();
					}
				}
            }
        });
    }
}
