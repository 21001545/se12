using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public static class ColorChart
{
    // 민트색
    public static readonly Color primary_50 = new Color(241.0f / 255.0f, 250.0f / 255.0f, 250.0f / 255.0f);
    public static readonly Color primary_100 = new Color(0.898f, 0.965f, 0.969f);
    public static readonly Color primary_200 = new Color(0.741f, 0.918f, 0.930f);
    public static readonly Color primary_300 = new Color(0.318f, 0.784f, 0.812f);
    public static readonly Color primary_400 = new Color(0.196f, 0.675f, 0.706f);
    public static readonly Color primary_500 = new Color(0.161f, 0.561f, 0.584f);

    // 빨간색
    public static readonly Color secondary_100 = new Color(0.988f, 0.894f, 0.890f);
    public static readonly Color secondary_200 = new Color(0.953f, 0.627f, 0.612f);
    public static readonly Color secondary_300 = new Color(0.922f, 0.361f, 0.337f);

    // highlighted/pressed
    public static readonly Color primary_300_highlighted = new Color(0.161f, 0.561f, 0.584f);

    // 회색 ~ 남색
    public static readonly Color gray_150 = new Color(0.969f, 0.969f, 0.984f);      // 인풋필드 디폴트 배경색 등
    public static readonly Color gray_200 = new Color(0.933f, 0.933f, 0.965f);      // 비활버튼 배경색 등
    public static readonly Color gray_250 = new Color(0.902f, 0.902f, 0.949f);
    public static readonly Color gray_300 = new Color(219.0f/255.0f, 219.0f/255.0f, 232.0f/255.0f);
    public static readonly Color gray_350 = new Color(0.784f, 0.784f, 0.867f);
    public static readonly Color gray_400 = new Color(0.718f, 0.718f, 0.816f);      // 비활텍스트 등
    public static readonly Color gray_500 = new Color(0.576f, 0.576f, 0.729f);
    public static readonly Color gray_600 = new Color(114.0f / 255.0f, 114.0f / 255.0f, 157.0f / 255.0f);
    public static readonly Color gray_650 = new Color(96/255.0f, 96.0f/255.0f, 141.0f/255.0f);
    public static readonly Color gray_700 = new Color(0.329f, 0.329f, 0.486f);      // 일반텍스트 등
    public static readonly Color gray_750 = new Color(0.271f, 0.271f, 0.467f);
    public static readonly Color gray_900 = new Color(0.031f, 0.031f, 0.275f);

    // 흰색
    public static readonly Color white = Color.white;

    // 에러색
    public static readonly Color error_100 = new Color(0.988f, 0.894f, 0.890f);
    public static readonly Color error_300 = new Color(0.922f, 0.361f, 0.337f);

    // 성공색
    public static readonly Color success_100 = new Color(0.671f, 0.918f, 0.678f);
    public static readonly Color success_300 = new Color(0.302f, 0.824f, 0.318f);

    public static readonly Color input_checklightBg = new Color(0.678f, 0.898f, 0.910f);    // 입력 확인만 할 때 쓰는 색(인증코드 등)

    public static Color isConfirmed_input(bool isConfirmed)
    {
        if (isConfirmed)
            return success_100;
        else
            return error_100;
    }
}
