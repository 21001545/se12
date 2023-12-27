using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using TMPro;

public class UIMap_titleCellView : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_title;

    public void setup(string title)
    {
        txt_title.text = title;
    }
}