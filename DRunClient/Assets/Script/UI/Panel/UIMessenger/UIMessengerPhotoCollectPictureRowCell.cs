using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerPhotoCollectPictureRowCell : EnhancedScrollerCellView
{
    [SerializeField]
    private UIGalleryPickerItem[] _items;

    public void setup(List<string> pictures)
    {
        for (int i = 0; i < _items.Length; ++i)
        {
            UIGalleryPickerItem item = _items[i];

            if (i < pictures.Count)
            {
                item.enable(true);
                item.setup(i, pictures[i]);
            }
            else
            {
                item.enable(false);
            }
        }
    }
}
