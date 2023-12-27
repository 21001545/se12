using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMessengerBubblePhotoCell : EnhancedScrollerCellView
{
    [SerializeField]
    private UIPhotoThumbnail _thumbnail;

    [SerializeField]
    protected RectTransform rt_photos;

    private ClientChatRoomLog _log;

    public float getHeight(ClientChatRoomLog log)
    {
        JsonArray files = log.payload.getJsonArray("files");

        var count = files.size();

        // 작은거 한개 사이즈 109, 147
        // 큰거 한개 사이즈 220, 147
        // 한개 일땐 220, 297
        var row = ((count - 1) / 2) + 1;
        var height = count == 1 ? 297.0f : 147.0f;

        // 탑 패딩 : 8
        // spacing : 2
        return 8.0f + row * height + (row - 1) * 2.0f;
    }

    public virtual void setup(ClientChatRoomLog log, bool showProfile = true)
    {
        _log = log;
        JsonArray files = log.payload.getJsonArray("files");
        for (int i = rt_photos.childCount; i < files.size(); ++i)
        {
            Instantiate(rt_photos.GetChild(0), rt_photos);
        }
        for (int i = files.size(); i < rt_photos.childCount; ++i)
        {
            rt_photos.GetChild(i).gameObject.SetActive(false);
        }

        var width = files.size() == 1 ? 220.0f : 109.0f;
        var height = files.size() == 1 ? 297.0f : 147.0f;
        for (int i = 0; i < files.size(); ++i)
        {
            var rt = (RectTransform)rt_photos.GetChild(i);
            var thumbnail = rt.GetComponent<UIPhotoThumbnail>();

            rt.gameObject.SetActive(true);

            var column = i % 2;
            var row = i / 2;

            rt.anchoredPosition = new Vector2((2.0f + width) * column, -((height + 2.0f) * row));
            if (i > 0 && i + 1 == files.size() && (i + 1 ) % 2 == 1 )
            {
                // 마지막께 홀수야?
                rt.sizeDelta = new Vector2(width * 2 + 2.0f, height);
            }
            else
            {
                rt.sizeDelta = new Vector2(width, height);
            }

            thumbnail.setEmpty();
            thumbnail.setImageFromCDN(ClientChatRoomLog.getFileURL(files.getString(i)));
        }

        if (_thumbnail != null )
        {
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

    public void onClickPhoto(int index)
    {
        // 2022.07.11 소현 : 선택한 사진 위치에서 열려야 한당
        Vector2 targetPos = rt_photos.anchoredPosition;
        targetPos.x = Mathf.Abs(targetPos.x);
        targetPos.x += rt_photos.rect.width * (0.5f - rt_photos.pivot.x);
        targetPos.y = this.transform.position.y + this.transform.parent.GetComponent<RectTransform>().rect.width;
        UIMessengerPhotoDetail.getInstance().setup(_log, targetPos, index);
        UIMessengerPhotoDetail.getInstance().open();
        ClientMain.instance.getPanelNavigationStack().push(UIMessenger.getInstance(), UIMessengerPhotoDetail.getInstance());
    }
}