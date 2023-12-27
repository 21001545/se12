using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMonthCalendarCell : MonoBehaviour
{
    [SerializeField]
    private Image img_bg;
    [SerializeField]
    private TMP_Text txt_day;

    public class MonthlyCellState
    {
        public static int low = 0;
        public static int medium = 1;
        public static int high = 2;
    }

    public bool hasDate()
    {
        return txt_day.gameObject.activeSelf;
    }

    public void Initialize(int day, int? value, bool disable, TMP_FontAsset font)
    {
        //Debug.Log($"day : {day}, input value : {value} disable : {disable}");
        if (disable)
        {
            // 없는 날짜
            txt_day.color = ColorChart.gray_350;
            txt_day.text = day.ToString();
            img_bg.gameObject.SetActive(false);
        }
        else
        {
            txt_day.gameObject.SetActive(true);
            txt_day.text = day.ToString();
            txt_day.font = font;

            if (value == null)
            {
                // 아직 기록 안됨
                txt_day.color = ColorChart.gray_500;
                img_bg.gameObject.SetActive(false);
            }
            else
            {
                // 기록됨
                if (value == MonthlyCellState.high)
                    img_bg.color = ColorChart.primary_300;
                else if (value == MonthlyCellState.medium)
                    img_bg.color = ColorChart.primary_200;
                else
                    img_bg.color = ColorChart.gray_250;

                txt_day.color = ColorChart.gray_700;
                img_bg.gameObject.SetActive(true);
            }
        }
    }
}
