using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIColorPicker : MonoBehaviour
{
    [SerializeField]
    private Slider _slider = null;

    [SerializeField]
    private Image _sliderBackground = null;
    private Material _sliderBackgroundMaterial = null;

    [SerializeField]
    private Image _pallete = null;
    private Material _palleteMaterial = null;

    [SerializeField]
    private RectTransform _palleteCursorTransform = null;

    [SerializeField]
    private Color32 _currentColor = new Color32();

    // 음.. 퍼블릭 이벤트도... 소문자로?
    public UnityEvent<Color> OnChangedColor = new UnityEvent<Color>();

    private void Start()
    {
        // Material copy를 하자.
        if (_pallete?.material != null )
        {
            _palleteMaterial= _pallete.material = new Material(_pallete.material);
        }

        if (_sliderBackground?.material != null )
        {
            _sliderBackgroundMaterial = _sliderBackground.material = new Material(_sliderBackground.material);
        }

        setColor(_currentColor);

        DragListener dragListener = _pallete.GetComponent<DragListener>();
        if (dragListener == null)
        {
            dragListener = _pallete.gameObject.AddComponent<DragListener>();
        }

        dragListener.OnDragEvent.AddListener(onPalleteDrag);

    }
    
    public void setColor(Color color)
    {
        _currentColor = color;
        onColorChanged();
    }


    // 팔레트의 커서가 이동 되었다. 
    private void onPalleteDrag(PointerEventData eventData)
    {
        if (_pallete == null)
            return;

        if (_palleteCursorTransform == null)
            return; 

        RectTransform rect = _pallete.transform as RectTransform;

        Vector2 cursorPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out cursorPosition);

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, 0, rect.rect.size.x);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, 0, rect.rect.size.y);

        _palleteCursorTransform.localPosition = cursorPosition;

        onValueChanged();
    }

    void onValueChanged()
    {
        RectTransform rect = _pallete.transform as RectTransform;

        _currentColor.r = (byte)_slider.value;
        _currentColor.g = (byte)((_palleteCursorTransform.localPosition.x / rect.rect.size.x) * 255);
        _currentColor.b = (byte)(((rect.rect.size.y - _palleteCursorTransform.localPosition.y) / rect.rect.size.y) * 255);

        OnChangedColor?.Invoke(_currentColor);

        updatePallete();
    }

    void onColorChanged()
    {
        RectTransform rect = _pallete.transform as RectTransform;

        // y축만 반전 시켜야 함.
        Vector2 cursorPosition = new Vector2((_currentColor.g / 255.0f) * rect.rect.size.x, rect.rect.size.y - ((_currentColor.b / 255.0f) * rect.rect.size.y));
        _palleteCursorTransform.localPosition = cursorPosition;

        _slider.value = _currentColor.r;

        updatePallete();
    }

    void updatePallete()
    {
        if (_palleteMaterial != null )
        {
            _palleteMaterial.SetColor("LeftColor", new Color32(_currentColor.r, 0, 0, 255));
            _palleteMaterial.SetColor("RightColor", new Color32(_currentColor.r, 255, 0, 255));
            _palleteMaterial.SetColor("BottomColor", new Color32(_currentColor.r, 0, 255, 255));
            _palleteMaterial.SetColor("TopColor", new Color32(_currentColor.r, 0, 0, 255));
        }

        if (_sliderBackgroundMaterial != null)
        {
            _sliderBackgroundMaterial.SetColor("BottomColor", new Color32(0, _currentColor.g, _currentColor.b, 255));
            _sliderBackgroundMaterial.SetColor("TopColor", new Color32(255, _currentColor.g, _currentColor.b, 255));
        }
    }

    public void onSliderValueChanged()
    {
        onValueChanged();
    }
}
