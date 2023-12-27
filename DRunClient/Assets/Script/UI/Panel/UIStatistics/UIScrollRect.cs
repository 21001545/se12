using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScrollRect : UnityEngine.UI.ScrollRect

{
    private DragListener parentDragListener;
    private bool _enable = true;

    protected override void Awake()
    {
        base.Awake();

        parentDragListener = GetComponentInParent<DragListener>();
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (_enable == false)
        {
            parentDragListener?.OnDrag(eventData);
            return;
        }

        var prev = content.anchoredPosition;

        base.OnDrag(eventData);

        if (eventData.delta.y < 0.0f)
        {
            if (content.anchoredPosition.y <= 0.1f)
            {
                content.anchoredPosition = new Vector2(0.0f, 0.0f);
                parentDragListener?.OnBeginDrag(eventData);
                parentDragListener?.OnDrag(eventData);
            }
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (_enable == false)
        {
            parentDragListener?.OnBeginDrag(eventData);
            return;
        }

        base.OnBeginDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (_enable == false)
        {
            parentDragListener?.OnEndDrag(eventData);
            return;
        }

        base.OnEndDrag(eventData);
    }

    public override void OnScroll(PointerEventData data)
    {
        if (_enable == false)
        {
            return;
        }

        base.OnScroll(data);
    }

    public void enableScroll(bool enable)
    {
        _enable = enable;
    }

    public bool isTop()
    {
        return content.anchoredPosition.y <= 0.1f;
    }
}
