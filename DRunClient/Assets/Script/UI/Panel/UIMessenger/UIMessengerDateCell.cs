using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIMessengerDateCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_date;

    public void setup(System.DateTime date)
    {
        txt_date.text = date.ToLongDateString();
    }

}
