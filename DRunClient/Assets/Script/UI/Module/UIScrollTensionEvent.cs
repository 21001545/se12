using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class UIScrollTensionEvent : MonoBehaviour
{
    public UnityEvent onTension;

    [SerializeField]
    private EnhancedScroller _targetScroller;

    [SerializeField]
    private GameObject go_tensionLoading;

    private bool _moreRequest = false;

    private void Start()
    {
        go_tensionLoading.SetActive(false);

        _targetScroller.ScrollRect.onValueChanged.AddListener(onScrollScrolled);
        var eventTrigger = _targetScroller.GetComponent<EventTrigger>();
        if (eventTrigger == null )
        {
            eventTrigger = _targetScroller.gameObject.AddComponent<EventTrigger>();
        }

        var entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.EndDrag;
        entry.callback.AddListener(onScrollEndDrag);
        eventTrigger.triggers.Add(entry);
    }

    //private Tweener _cachedTween = null;
    private void onScrollScrolled(Vector2 position)
    {
        if (position.y > 1.0f)
        {
            if (go_tensionLoading.activeSelf == false)
                go_tensionLoading.SetActive(true);

            float delta = (position.y - 1.0f) * _targetScroller.ScrollSize;
/*            float alpha = delta / img_indicator.rectTransform.rect.height;

            var color = img_indicator.color;
            color.a = alpha;
            img_indicator.color = color;

            // alpha에 따라 회전 속도를 다르게 해야해?
            if (_cachedTween == null )
                _cachedTween = img_indicator.rectTransform.DOLocalRotate(new Vector3(0, 0, 360), 0.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);

            _cachedTween.timeScale = Mathf.Min(2.0f, Mathf.Max(0.1f, alpha));*/

            if (delta >= 32f)
            {
                _moreRequest = true;
            }
            else
            {
                _moreRequest = false;
            }
        }
        else
        {
            if (go_tensionLoading.activeSelf)
                go_tensionLoading.SetActive(false);
        }
    }

    public void onScrollEndDrag(BaseEventData e)
    {
        if (_moreRequest)
        {
            onTension?.Invoke();
            _moreRequest = false;
        }
    }

}
