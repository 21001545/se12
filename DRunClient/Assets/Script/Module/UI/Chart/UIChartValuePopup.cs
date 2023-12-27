using AwesomeCharts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIChartValuePopup : ChartValuePopupAbstract
{
    [SerializeField]
    private TMP_Text text;

    public override void SetText(string text)
    {
        this.text.text = text;
    }
}
