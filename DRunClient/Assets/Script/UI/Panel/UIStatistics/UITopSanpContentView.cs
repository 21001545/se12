using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UITopSanpContentView : DragListener, IScrollHandler
{
    [SerializeField]
    private RectTransform _content;

    [SerializeField]
    private RectTransform _topContent;

    [SerializeField]
    private RectTransform _bottomContent;

    //[SerializeField]
    //private float _snapSpeed = 0.33f;

    [SerializeField]
    private bool _enableSnap = true;

    private float _originY = 0.0f;
    private List<UIScrollRect> _childScrollRect = new List<UIScrollRect>();
    protected override void Awake()
    {
        base.Awake();
    }

    protected virtual void Start()
    {
        Vector3[] corners = new Vector3[4];
        _topContent.GetWorldCorners(corners);
        _originY = _topContent.rect.height;

        var scrollRect = GetComponentsInChildren<UIScrollRect>();

        foreach (var scroll in scrollRect)
        {
            _childScrollRect.Add(scroll);
        }

        enableChildScrollRect(false, true);
    }

    private bool _enableChildScrollRect = true;
    public void enableChildScrollRect(bool enable, bool force = false)
    {
        if (_enableChildScrollRect == enable && force == false )
        {
            return;
        }

        _enableChildScrollRect = enable;

        Debug.Log($"enableChildScrollRect : {enable}");
        foreach (var scroll in _childScrollRect)
        {
            scroll.enableScroll(enable);
        }
    }

    public void enableSnap(bool enable, bool reset = true)
    {
        _enableSnap = enable;
        
        if ( reset )
        {
            _content.offsetMin = new Vector2(0, 0);
            _content.offsetMax = new Vector2(0, 0);
        }
    }

    private Vector2 _contentStartPosition;
    private Vector2 _cursorStartPosition;

   // private bool _drag = false;
    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);

        _cursorStartPosition = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out _cursorStartPosition);
        _contentStartPosition = _content.anchoredPosition;
        //_drag = true;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);


        //_drag = false;
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);

        if ( _enableSnap == false )
        {
            return;
        }

        foreach(var scroll in _childScrollRect)
        {
            if (scroll.isTop() == false)
                return;
        }

        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localCursor))
            return;

        var pointerDelta = localCursor - _cursorStartPosition;
        pointerDelta.x = 0.0f;
        Vector2 position = _contentStartPosition + pointerDelta;

        if (eventData.delta.y > 0.0f)
        {
            // 위로..
            if (position.y >= _originY)
            {
                position.y = _originY;
                enableChildScrollRect(true);
                foreach (var scroll in _childScrollRect)
                {
                    scroll.OnBeginDrag(eventData);
                }
            }
        }
        else
        {
            enableChildScrollRect(false);
        }
        if (position.y <= 0.0f)
            position.y = 0.0f;

        _content.anchoredPosition = position;

        _bottomContent.offsetMin = new Vector3(_bottomContent.offsetMin.x, -position.y);
    }

    public void OnScroll(PointerEventData eventData)
    {
        Debug.Log("OnScroll");
    }

}
