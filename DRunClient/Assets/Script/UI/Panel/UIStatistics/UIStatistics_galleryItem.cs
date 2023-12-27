using Festa.Client;
using Festa.Client.RefData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Festa.Client.Module;
using Festa.Client.NetData;
using Festa.Client.Logic;

public class UIStatistics_galleryItem : ReusableMonoBehaviour
{
    [SerializeField]
    private TMP_Text txt_date;
    [SerializeField]
    private TMP_Text txt_title;
    [SerializeField]
    private TMP_Text txt_hour;
    [SerializeField]
    private TMP_Text txt_min;
    [SerializeField]
    private TMP_Text txt_sec;
    [SerializeField]
    private Image img_tripIcon;
    [SerializeField]
    private TMP_Text txt_tripType;
    [SerializeField]
    private RawImage snapshot_image;

    private TextureCacheItemUsage _textureUsage;
    private ClientTripLog _tripLog;

    private UnityAction onClickCallback;

    private TextureCache textureCache => ClientMain.instance.getTextureCache();

    public override void onCreated(ReusableMonoBehaviour source)
    {
        _textureUsage = null;
    }

    public override void onDelete()
    {
        clear();
    }

    //public void setup(DateTime date, string title, TimeSpan totalTime, int tripType, UnityAction callback)
    //{
    //    // 스트링테이블,, 해야함 일단은 이렇게
    //    txt_date.text = date.ToString("yyyy.MM.dd");
    //    txt_title.text = title;
    //    txt_hour.text = totalTime.Hours.ToString("D2");
    //    txt_min.text = totalTime.Minutes.ToString("D2");
    //    txt_sec.text = totalTime.Seconds.ToString("D2");

    //    img_tripIcon.sprite = UITripEndResult.getInstance().tripIcons[tripType];
    //    txt_tripType.text = GlobalRefDataContainer.getStringCollection().get("triproute.category", tripType);
    //    onClickCallback = callback;
    //}

    public void setup(UIStatistics_exploreScroller.ListTicket item, UnityAction callback)
    {
        _tripLog = item._log;

		// 스트링테이블,, 해야함 일단은 이렇게
		txt_date.text = item.date.ToString("yyyy.MM.dd");
        txt_title.text = _tripLog.name;
        txt_hour.text = item.totalTime.Hours.ToString("D2");
        txt_min.text = item.totalTime.Minutes.ToString("D2");
        txt_sec.text = item.totalTime.Seconds.ToString("D2");

        img_tripIcon.sprite = UITripEndResult.getInstance().tripIcons[item.tripType];
        txt_tripType.text = GlobalRefDataContainer.getStringCollection().get("triproute.category", item.tripType);
        onClickCallback = callback;

        StartCoroutine(loadSnapshot());
    }

    public void clear()
    {
        snapshot_image.texture = null;

        if( _textureUsage != null)
        {
			textureCache.deleteUsage(_textureUsage);
            _textureUsage = null;
		}
	}

    public void onClickMore()
    {
        onClickCallback?.Invoke();
    }

    private IEnumerator loadSnapshot()
    {
        yield return new WaitForEndOfFrame();

        // 나중에 로딩중 이미지 같은걸로 교체 ??
        clear();

        Vector2Int size = calcImageSize();

        TripLogSnaphotProcessor step = TripLogSnaphotProcessor.create(_tripLog, size, textureCache);
        step.run(result => { 
            if( result.succeeded())
            {
                TextureCacheItemUsage textureUsage = step.getTextureUsage();

                if( step.getLog() != _tripLog)
                {
                    textureCache.deleteUsage(textureUsage);
                }
                else
                {
                    snapshot_image.texture = textureUsage.texture;
                    _textureUsage = textureUsage;
                }
            }
        });
    }

    private Vector2Int calcImageSize()
    {
        Vector3[] localCorner = new Vector3[4]; ;
        RectTransform rt = snapshot_image.transform as RectTransform;
        rt.GetLocalCorners(localCorner);

        Rect thisRect = rt.rect;
        Rect parentRect = ((RectTransform)rt.parent).rect;

        Vector2Int result = new Vector2Int();
        result.x = (int)(localCorner[2].x - localCorner[0].x);
        result.y = (int)(localCorner[2].y - localCorner[0].y);

       // Debug.Log($"calcImageSize:{result}");
        
        return result;
    }
}
