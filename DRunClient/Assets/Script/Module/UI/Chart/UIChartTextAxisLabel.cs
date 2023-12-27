using AwesomeCharts;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIChartTextAxisLabel : AxisLabel
{
    public TMP_Text textLabel;

    public override void SetLabelColor(Color color)
    {
        textLabel.color = color;
    }

    public override void SetLabelText(string text)
    {
        textLabel.text = text;
    }

    public override float GetTextPreferredWidth()
    {
        return textLabel.preferredWidth;
    }

    public override void SetLabelTextAlignment(TextAnchor anchor)
    {
        if (anchor == TextAnchor.UpperLeft) {
            textLabel.alignment = TextAlignmentOptions.TopLeft;
        }
        else if (anchor == TextAnchor.UpperCenter)
        {
            textLabel.alignment = TextAlignmentOptions.Top;
        }
        else if (anchor == TextAnchor.UpperRight)
        {
            textLabel.alignment = TextAlignmentOptions.TopRight;
        }
        else if (anchor == TextAnchor.MiddleLeft)
        {
            textLabel.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else if (anchor == TextAnchor.MiddleCenter)
        {
            textLabel.alignment = TextAlignmentOptions.Midline;
        }
        else if (anchor == TextAnchor.MiddleRight)
        {
            textLabel.alignment = TextAlignmentOptions.MidlineRight;
        }
        else if (anchor == TextAnchor.LowerLeft)
        {
            textLabel.alignment = TextAlignmentOptions.BottomLeft;
        }
        else if (anchor == TextAnchor.LowerCenter)
        {
            textLabel.alignment = TextAlignmentOptions.Bottom;
        }
        else if (anchor == TextAnchor.LowerRight)
        {
            textLabel.alignment = TextAlignmentOptions.BottomRight;
        }
    }

    public override void SetLabelTextSize(int size)
    {
        textLabel.fontSize = size;
    }
}
