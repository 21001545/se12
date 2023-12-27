using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.NetData;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO 요거..스크립트 정리 하자!
public class UIMessengerBubble : EnhancedScrollerCellView
{
    [SerializeField]
    protected TMP_InputField txt_message;

    [SerializeField]
    protected VerticalLayoutGroup rt_bg;

    [SerializeField]
    protected RectTransform rt_showTotal;

    private string _message;

    // 해당 text를 선택한다.
    public virtual void select(string text)
    {
        var index = txt_message.text.IndexOf(text);
        if (index >= 0)
        {
            txt_message.selectionStringAnchorPosition = index;
            txt_message.selectionStringFocusPosition = index + text.Length;
            txt_message.Select();
        }
    }

    public Bounds getTextBounds()
    {
        return txt_message.textComponent.textBounds;
    }

    public Vector2 getComponentSize()
    {
        var size = getTextBounds().size;
        size.x += rt_bg.padding.horizontal;
        size.y += rt_bg.padding.vertical;
        size.y += 8.0f;

        if (rt_showTotal != null && rt_showTotal.gameObject.activeSelf)
        {
            size.y += rt_showTotal.sizeDelta.y;
        }

        return size;
    }

    public virtual void setup(ClientChatRoomLog log)
    {
        string msg = log.payload.getString("msg");
        _message = msg;

        var layoutElement = txt_message.GetComponent<LayoutElement>();        
        layoutElement.minWidth = 250;
        const int limit = 500;
        rt_showTotal?.gameObject.SetActive(false);
        if (limit < msg.Length)
        {
            msg = msg.Substring(0, limit - 3) + "...";
            rt_showTotal?.gameObject.SetActive(true);
        }
        txt_message.text = msg;

        Canvas.ForceUpdateCanvases();

        var bound = getTextBounds();
        layoutElement.minWidth = Mathf.Min(bound.size.x, 250);
    }

    public void onClick()
    {
        UIMessengerDetailMessage.getInstance().setup(_message);
        UIMessengerDetailMessage.getInstance().open();

        ClientMain.instance.getPanelNavigationStack().push( UIMessenger.getInstance(), UIMessengerDetailMessage.getInstance());
    }
}


public class UIMessengerBubbleMyCell : UIMessengerBubble
{
}
