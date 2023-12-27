using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Festa.Client.NetData;
using Festa.Client;
using Festa.Client.Module;

public class UIMessengerPhotoDetail : UISingletonPanel<UIMessengerPhotoDetail>
{
    [SerializeField]
    private RectTransform rect_target;

    [SerializeField]
    private TMP_Text txt_name;

    [SerializeField]
    private TMP_Text txt_date;

    [SerializeField]
    private UIPhotoThumbnail _currentPhoto;

    [SerializeField]
    private TMP_Text txt_index;

    [SerializeField]
    private GameObject go_thumbnailList;

    [SerializeField]
    private Transform go_thumbnailListContents;

    [SerializeField]
    private UIPhotoThumbnail _photoThumbnailPrefab;

    private int _currentPhotoIndex;
    private int _totalPhotoCount;

    private JsonArray _files;
    public void onClickBackNavigation()
    {
        ClientMain.instance.getPanelNavigationStack().pop();
    }

    public void setup(ClientChatRoomLog log, Vector2 targetPos, int index = 0)
    {
        targetPos.x /= 375f;
        targetPos.y /= 812f;
        rect_target.pivot = targetPos;
        txt_name.text = "";
        ClientMain.instance.getProfileCache().getProfileCache(log.sender_id, result => { 
            if (result.succeeded())
            {
                var profile = result.result();
                txt_name.text = profile.Profile.name;
            }
        });

        txt_date.text = log.create_time.ToString("yyyy. M. dd");

        _files = log.payload.getJsonArray("files");
        if (_files != null )
        {
            _currentPhotoIndex = 1;
            _totalPhotoCount = _files.size();

            if ( _totalPhotoCount <= 1)
            {
                txt_index.gameObject.SetActive(false);
                go_thumbnailList.gameObject.SetActive(false);
            }
            else
            {
                txt_index.gameObject.SetActive(true);
                go_thumbnailList.gameObject.SetActive(true);

                for (int i = go_thumbnailListContents.childCount; i< _totalPhotoCount; ++i )
                {
                    Instantiate(_photoThumbnailPrefab, go_thumbnailListContents);
                }

                for ( int i = 0; i < go_thumbnailListContents.childCount; ++i )
                {
                    if ( i < _totalPhotoCount)
                    {
                        var cell = go_thumbnailListContents.GetChild(i).GetComponent<UIMessengerPhotoDetailCell>();
                        cell.gameObject.SetActive(true);

                        int cachedIndex = i;
                        cell.setup(ClientChatRoomLog.getFileURL(_files.getString(i)), () => {
                            selectPhoto(cachedIndex);
                        });
                    }
                    else
                    {
                        go_thumbnailListContents.GetChild(i).gameObject.SetActive(false);
                    }
                }
            }

            selectPhoto(index);
        }
    }

    private void selectPhoto(int index)
    {
        _currentPhotoIndex = index;
        txt_index.text = $"{_currentPhotoIndex+1}/{_totalPhotoCount}";

        _currentPhoto.setImageFromCDN(ClientChatRoomLog.getFileURL(_files.getString(_currentPhotoIndex)));

        if (go_thumbnailList.activeSelf)
        {
            for (int i = 0; i < _totalPhotoCount && i < go_thumbnailListContents.childCount; ++i)
            {
                var cell = go_thumbnailListContents.GetChild(i).GetComponent<UIMessengerPhotoDetailCell>();
                cell.select(i == index);
            }
        }
    }
}
