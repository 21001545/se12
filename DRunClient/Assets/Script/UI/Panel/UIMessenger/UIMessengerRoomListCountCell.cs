using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerRoomListCountCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_text;

    public void setup(string text)
    {
        txt_text.text = text;
    }
}
