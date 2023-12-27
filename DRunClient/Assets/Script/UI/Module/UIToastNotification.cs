using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIToastNotification : UIPanel
{
    [SerializeField]
    private TMP_Text txt_text;
    [SerializeField]
    private RectTransform rect_bg;
    [SerializeField]
    private RectTransform rect_panel;

    [SerializeField]
    private float screenHeight = 812f;
    private float _offset = 4f;
    private float _desY = 52f;      // 팝업 위치 y 값
    private TouchScreenKeyboardUtil KeyboardUtil => TouchScreenKeyboardUtil.getInstance();

    public static UIToastNotification spawn(string message)
    {
        return spawn(message, ColorChart.gray_650);
    }

    public static UIToastNotification spawn(string message, Color backgroundColor)
    {
        UIToastNotification popup = UIManager.getInstance().spawnInstantPanel<UIToastNotification>();

        popup.setup(message, backgroundColor);
        return popup;
    }

    public void setup(string message, Color backgroundColor)
    {
        txt_text.text = message;

        // 배경색을 변경해보자
        var image = rect_bg.GetComponent<Image>();
        if ( image != null )
        {
            image.color = backgroundColor;
        }

        if (txt_text.textInfo.lineCount > 1)
        {
            // 두 줄 이상인 경우 높이 수정
            rect_bg.sizeDelta = new Vector2(302f, 96f);
        }
        else
        {
            rect_bg.sizeDelta = new Vector2(302f, 64f);
        }

        Invoke("close", 1.0f);
    }

    private void close()
    {
        base.close(TransitionEventType.start_close);
    }

    public override void update()
    {
        base.update();

        // 걍 시작할 때 크기를 잡아버려~
        float newDesY = 812f - ((KeyboardUtil.getHeight() / Screen.height) * screenHeight - (_desY - _offset));

        if (_desY != newDesY)
        {
            // 키보드 없는 경우 그냥 꽉 채워요
            _desY = Mathf.Clamp(newDesY, 0f, 812f);
            Vector2 desPos = new Vector2(375f, _desY);
            rect_panel.sizeDelta = desPos;
        }
    }
}
