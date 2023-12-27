using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIMessengerPhotoDetailCell : ReusableMonoBehaviour
{
    [SerializeField]
    private UIPhotoThumbnail _thumbnail;

    [SerializeField]
    private GameObject go_selected;

    private UnityAction _selectCallback;

    public void setup(UnityAction selectCallback)
    {
        _selectCallback = selectCallback;
        _thumbnail.setEmpty();
    }

    public void setup(string url, UnityAction selectCallback)
    {
        _selectCallback = selectCallback;
        _thumbnail.setImageFromCDN(url);
    }

    public void setup(NativeGallery.NativePhotoContext context, UnityAction selectCallback)
    {
        _selectCallback = selectCallback;
        _thumbnail.setImageFromFile(context);
    }

    public void onClicked()
    {
        _selectCallback?.Invoke();
    }

    public void select(bool isSelect)
    {
        go_selected.SetActive(isSelect);
    }
}
