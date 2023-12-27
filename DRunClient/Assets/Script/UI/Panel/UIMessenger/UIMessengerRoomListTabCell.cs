using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIMessengerRoomListTabCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text[] txt_texts;

    [SerializeField]
    private Image[] img_lines;

    private UnityAction<int> _tabSelectCallback;

    public void setup(UnityAction<int> tabSelectCallback)
    {
        _tabSelectCallback = tabSelectCallback;
    }

    public void onClickTab(int index)
    {
        for( var i =0; i < txt_texts.Length;++i)
        {
            txt_texts[i].color = index == i ? ColorChart.primary_300 : ColorChart.gray_400;
        }

        for (var i = 0; i < img_lines.Length; ++i)
        {
            img_lines[i].gameObject.SetActive(index == i);
        }

        _tabSelectCallback?.Invoke(index);
    }
}
