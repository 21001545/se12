using EnhancedUI.EnhancedScroller;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIStatistics_detailDate : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_date;
    [SerializeField]
    private TMP_Text txt_total;

    public void setup(DateTime date, int tripCount, TimeSpan totalTime)
    {
        // 스트링테이블,, 해야함 일단은 이렇게
        txt_date.text = date.ToString("yyyy년 M월");
        txt_total.text = $"탐험 {tripCount}회  {totalTime.Hours}시간 {totalTime.Minutes.ToString("D2")}분 {totalTime.Seconds.ToString("D2")}초";
    }
}
