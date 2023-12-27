using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UIMap_tripTypeCellView : MonoBehaviour
{
    [SerializeField]
    private RectTransform rect_panel;
    [SerializeField]
    private GameObject go_highlight;

    private bool _highlightOn = false;
    private float _animationDuration = 0.3f;
    private Vector2 _normalSize = new Vector2(184f, 232f);
    private Vector2 _pointedSize = new Vector2(202f, 256f);

    public void pointOn()
    {
        _highlightOn = true;
        DOTween.To(() => rect_panel.sizeDelta, x => rect_panel.sizeDelta = x, _pointedSize, _animationDuration);
        Invoke("highlightOn", _animationDuration);
    }

    private void highlightOn()
    {
        if(_highlightOn)
            go_highlight.SetActive(true);
    }

    public void pointOnImmediately()
    {
        _highlightOn = true;
        go_highlight.SetActive(true);
        rect_panel.sizeDelta = _pointedSize;
    }

    public void pointOff()
    {
        _highlightOn = false;
        go_highlight.SetActive(false);
        DOTween.To(() => rect_panel.sizeDelta, x => rect_panel.sizeDelta = x, _normalSize, _animationDuration);
    }

    public void pointOffImmediately()
    {
        _highlightOn = false;
        go_highlight.SetActive(false);
        rect_panel.sizeDelta =_normalSize;
    }

}
