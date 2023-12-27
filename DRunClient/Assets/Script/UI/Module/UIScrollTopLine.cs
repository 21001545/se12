using EnhancedUI.EnhancedScroller;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIScrollTopLine : MonoBehaviour
{
    [SerializeField]
    private EnhancedScroller _targetScroller;

    [SerializeField]
    private Image img_line;

    private void Start()
    {
        _targetScroller.ScrollRect.onValueChanged.AddListener(onScrollScrolled);
    }
    private void onScrollScrolled(Vector2 position)
    {
        float delta = (position.y - 1.0f) * _targetScroller.ScrollSize;
        img_line.gameObject.SetActive(delta != 0.0f);
    }
}
