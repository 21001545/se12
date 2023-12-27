using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using Festa.Client.Module;

public class SwipeDownPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum Direction
    {
        downToUp,
        upToDown
    }

    [SerializeField]
    private Image img_bg;
    [SerializeField]
    private CanvasGroup can_bg;
    [SerializeField]
    private RectTransform rect_targetPanel;
    [SerializeField]
    private float targetPanelHeight;
    [SerializeField]
    private float panelCloseRateThreshold = 0.7f;      // 비율! 0 ~ 1 사이로; 기본 70%
    [SerializeField]
    private Direction _direction;
    [SerializeField]
    private bool _dragClose = true;

    public bool _isDragging = false;
    private bool _isOpen = false;

#if UNITY_EDITOR
    private InputModule_PC _inputModule;
#else
    private InputModule_Mobile _inputModule;
#endif

    public RectTransform getTargetPanelRect()
    {
        return rect_targetPanel;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 패널이 열려 있을 때만 드래그 먹도록
        if (_isOpen && _dragClose && eventData.pressPosition.y > targetPanelHeight - 20)
        {
            _isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(_dragClose && _isDragging)
        {
            Vector2 position = rect_targetPanel.anchoredPosition;
            position.y = eventData.position.y;

            if (position.y < 0)
                position.y = 0;
            else if (position.y > targetPanelHeight)
                position.y = targetPanelHeight;

            rect_targetPanel.anchoredPosition = position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(_dragClose && _isDragging)
        {
            _isDragging = false;

            if (eventData.position.y < targetPanelHeight * panelCloseRateThreshold)
            {
                // 닫아요
                swipePanel(false);
            }
            else
            {
                // 열어요
                swipePanel(true);
            }
        }
    }

    public void showHideImmediately(bool show)
    {
        _isOpen = show;

        float targetHeight = targetPanelHeight;

        if (_direction == Direction.upToDown)
        {
            targetHeight *= -1;
        }

        if (show)
            rect_targetPanel.anchoredPosition = new Vector2(0f, targetHeight);
        else
            rect_targetPanel.anchoredPosition = new Vector2(0f, 0f);
    }

    public void swipePanel(bool open)
    {
        _isOpen = open;

        float targetHeight = targetPanelHeight;

        if(_direction == Direction.upToDown)
        {
            targetHeight *= -1;
        }

        if(open)
            DOTween.To(() => rect_targetPanel.anchoredPosition, x => rect_targetPanel.anchoredPosition = x, new Vector2(0f, targetHeight), 0.3f);
        else
            DOTween.To(() => rect_targetPanel.anchoredPosition, x => rect_targetPanel.anchoredPosition = x, Vector2.zero, 0.3f);
    }

    private void Start()
    {
#if UNITY_EDITOR
        _inputModule = InputModule_PC.create();
#else
        _inputModule = InputModule_Mobile.create();
#endif
    }

    private void Update()
    {
        if(img_bg != null)
        {
            var tempColor = img_bg.color;
            float newAlpha = (rect_targetPanel.anchoredPosition.y / targetPanelHeight) * 0.5f;      // 기본 오퍼시티가 0.5 라서~!

            if (newAlpha <= 0f)
                img_bg.gameObject.SetActive(false);
            else
            {
                img_bg.gameObject.SetActive(true);
                tempColor.a = newAlpha;
                img_bg.color = tempColor;
            }
        }

        if(can_bg != null)
        {
            float newAlpha = (rect_targetPanel.anchoredPosition.y / targetPanelHeight);

            if (newAlpha <= 0f)
                can_bg.gameObject.SetActive(false);
            else
            {
                can_bg.gameObject.SetActive(true);
                can_bg.alpha = newAlpha;
            }
        }
    }
}