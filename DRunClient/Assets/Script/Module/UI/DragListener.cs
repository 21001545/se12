using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DragListener : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    // 퍼블릭인 친구들도, _으로 시작해야하나..?
    public UnityEvent<PointerEventData> OnDragEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> OnDragStartEvent = new UnityEvent<PointerEventData>();
    public UnityEvent<PointerEventData> OnDragEndEvent = new UnityEvent<PointerEventData>();

    public RectTransform rectTransform = null;
    protected DragListener parentDragListener = null;

    protected virtual void Awake()
    {
        Debug.Log("DragListener");
        rectTransform = GetComponent<RectTransform>();

        parentDragListener = transform.parent?.GetComponentInParent<DragListener>();
    }

    public virtual void OnDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnDragEvent.Invoke(eventData);
        parentDragListener?.OnDrag(eventData);
    }
     
    public virtual void OnBeginDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnDragStartEvent.Invoke(eventData);

        parentDragListener?.OnBeginDrag(eventData);
    }

    public virtual void OnEndDrag(UnityEngine.EventSystems.PointerEventData eventData)
    {
        OnDragEndEvent.Invoke(eventData);

        parentDragListener?.OnEndDrag(eventData);
    }
}
