using EnhancedUI.EnhancedScroller;
using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerPhotoCollectDateCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_time;

    public void setup(System.DateTime time)
    {
        time = time.AddMilliseconds(TimeUtil.timezoneOffset());
        txt_time.text = time.ToString("yyyy. M. dd");
    }
}
