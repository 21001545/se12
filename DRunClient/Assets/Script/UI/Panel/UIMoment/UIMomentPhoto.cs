using Festa.Client;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMomentPhoto : EnhancedUI.EnhancedScroller.EnhancedScrollerCellView
{
    [SerializeField]
    private UIPhotoThumbnail _thumbnail;

    [SerializeField]
    private RectTransform _location_root;

    [SerializeField]
    private TMP_Text text_location;

    [SerializeField]
    private RectTransform rt_inputLocation;

    private UnityAction _addLocationCallback;
    public void setEmpty()
	{
        _thumbnail.setEmpty();
        _addLocationCallback = null;

    }

    public void setup(string url)
    {
        _location_root.gameObject.SetActive(false);
        rt_inputLocation.gameObject.SetActive(false);

        _thumbnail.setImageFromCDN(url);        
    }

    public void setup(string url, PlaceData place, int[] tag, bool isMine)
    {
        _location_root.gameObject.SetActive(place != null);
        if( place != null)
		{
            text_location.text = place.getAddress();
        }
        else
		{
            text_location.text = "";
		}

        rt_inputLocation.gameObject.SetActive(isMine && !_location_root.gameObject.activeSelf);

        // 가변 크기 대응..
        var element = text_location.transform.GetComponent<LayoutElement>();
        if ( element != null )
        {
            var size = text_location.GetPreferredValues();
            element.preferredWidth = Mathf.Min(size.x, 270);
        }

        _thumbnail.setImageFromCDN(url);
    }

    public void onClickAddLocation()
    {
        // 위치 UI 띄우고, 편집 요청을 해야하네?
        // 흠..
        _addLocationCallback?.Invoke();
    }

    public void setAddLocationCallback(UnityAction callback)
    {
        _addLocationCallback = callback;
    }
}
