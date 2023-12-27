using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIActivityDateCell : EnhancedScrollerCellView
{
    [SerializeField]
    private TMP_Text txt_text;

    [SerializeField]
    private Image img_line;

    public void setup(string text, bool showLine)
    {
        txt_text.text = text;
        img_line.gameObject.SetActive(showLine);
    }
}
