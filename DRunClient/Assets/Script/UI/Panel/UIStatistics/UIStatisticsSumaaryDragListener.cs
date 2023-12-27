using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIStatisticsSumaaryDragListener : DragListener
{
    private bool? _isCallParentDragListener;
    private Vector2 _cursorStartPosition;

    public override void OnDrag(PointerEventData eventData)
    {
        // x축으로 한번이라도 이동을 했다면 움직이지 말자.

        if (_isCallParentDragListener == null)
        {
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            var pointerDelta = localCursor - _cursorStartPosition;
            if ( pointerDelta.sqrMagnitude > 0 )
                _isCallParentDragListener = Math.Abs(pointerDelta.x) < Math.Abs(pointerDelta.y);
        }
        if (_isCallParentDragListener.HasValue && _isCallParentDragListener.Value)
            parentDragListener?.OnDrag(eventData);
        else
            OnDragEvent.Invoke(eventData);
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        _isCallParentDragListener = null;

        _cursorStartPosition = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out _cursorStartPosition);
    }
}
