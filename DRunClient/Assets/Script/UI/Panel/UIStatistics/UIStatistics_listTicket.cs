using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Festa.Client;
using TMPro;
using System;
using static Festa.Client.NetData.ClientTripConfig;
using UnityEngine.Events;

public class UIStatistics_listTicket : EnhancedScrollerCellView
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

    private UnityAction onClickCallback;

    public void setup(UIStatistics_exploreScroller.ListTicket data, UnityAction callback)
    {
        // 스트링테이블,, 해야함 일단은 이렇게
        txt_date.text = data.date.ToString("yyyy.MM.dd");
        txt_title.text = data._log.name;
        // txt_title.text = data.title;
        txt_hour.text = data.totalTime.Hours.ToString("D2");
        txt_min.text = data.totalTime.Minutes.ToString("D2");
        txt_sec.text = data.totalTime.Seconds.ToString("D2");

        img_tripIcon.sprite = UITripEndResult.getInstance().tripIcons[data.tripType];
        onClickCallback = callback;
    }

    public void onClickMore()
    {
        onClickCallback?.Invoke();
    }
}
