using EnhancedUI.EnhancedScroller;
using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerTimeCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_time;

    public void setup(System.DateTime time, bool isMine)
    {
        time = time.AddMilliseconds(TimeUtil.timezoneOffset());
        txt_time.text = time.ToString("h:mm tt");

        txt_time.horizontalAlignment = isMine ? HorizontalAlignmentOptions.Right : HorizontalAlignmentOptions.Left;        
    }
}
