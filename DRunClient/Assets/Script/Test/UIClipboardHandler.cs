using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.Module;
using Festa.Client;
using TMPro;
using UnityEngine.EventSystems;

public class UIClipboardHandler : MonoBehaviour
{
    public enum ClipboardType
    {
        copyOnly,
        copyPaste
    }
    [SerializeField]
    private ClipboardType _clipboardType = ClipboardType.copyPaste;

    [SerializeField]
    private RectTransform rect_inputArea;
    [SerializeField]
    private TMP_InputField _inputField;

#if UNITY_EDITOR
    private InputModule_PC _inputModule;
#else
    private InputModule_Mobile _inputModule;
#endif
    private bool _isTouching;
    private float _accumTime;
    private float _longTouchThreshold = 0.8f;       // 일단 대충 8초로

    public bool isInsideRect(RectTransform rect, Vector2 pos)
    {
        bool isInside = RectTransformUtility.RectangleContainsScreenPoint(rect, pos, Camera.main);

        if (isInside)
            return true;
        else
            return false;
    }

    public void Start()
    {
#if UNITY_EDITOR
        _inputModule = InputModule_PC.create();
#else
        _inputModule = InputModule_Mobile.create();
#endif
    }

    public void Update()
    {
        if (_inputModule.isTouchDown())
        {
            if(isInsideRect(rect_inputArea, _inputModule.getTouchPosition()))
            {
                if (!_inputField.isFocused)
                    _isTouching = true;
                else
                {
                    if (!UIClipboard.getInstance().isActiveAndEnabled)
                        createClipboardPopup();
                }
            }
            else if (UIClipboard.getInstance().isActiveAndEnabled && !isInsideRect(UIClipboard.getInstance().getPopupRect(), _inputModule.getTouchPosition()))
            {
                UIClipboard.getInstance().close();
            }
        }

        if (_isTouching && _inputModule.isTouchUp())
        {
            if (_accumTime >= _longTouchThreshold)
            {
                Debug.Log("long touch");
                createClipboardPopup();
            }
            else
            {
                int selectedLetters = Mathf.Abs(_inputField.caretPosition - _inputField.selectionStringAnchorPosition);
                if (selectedLetters == 0)
                    UIClipboard.getInstance().close();
                else
                {
                    if(UIClipboard.getInstance().isActiveAndEnabled)
                        UIClipboard.getInstance().close();
                    else
                    {
                        Debug.Log("selected");
                        createClipboardPopup();
                    }
                }
            }
            _isTouching = false;
            _accumTime = 0f;
        }

        if (_isTouching)
        {
            _accumTime += Time.deltaTime;
        }
    }

    private Vector2 getPopupPosition()
    {
        Vector2 popupPos = RectTransformUtility.WorldToScreenPoint(Camera.main, rect_inputArea.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(UIClipboard.getInstance().getRefRect(), popupPos, Camera.main, out popupPos);

        // x 위치 조정! 현재 입력된 텍스트의 중앙에 올릴 거야
        popupPos.x -= rect_inputArea.rect.width * rect_inputArea.pivot.x;
        popupPos.x += 375f * 0.5f;

        var bound = _inputField.textComponent.textBounds;
        if(bound.size.x > 0f)
            popupPos.x += bound.size.x * 0.5f;
/*        if (bound.size.x > rect_inputArea.rect.width)
            popupPos.x += rect_inputArea.rect.width * 0.5f;
        else
            popupPos.x += _inputField.preferredWidth * 0.5f;*/
        // 팝업 위치,, 일단은 인풋필드 x 축 중앙에 오도록
        // popupPos.x += rect_inputArea.rect.width * 0.5f;

        // y 위치 조정!
        popupPos.y -= rect_inputArea.rect.height * rect_inputArea.pivot.y;
        popupPos.y += 812f;
        popupPos.y += rect_inputArea.rect.height;

        // 양극단보정
        if (popupPos.x > 375f - 65f)
            popupPos.x = 375f - 65f;
        else if (popupPos.x < 65f)
            popupPos.x = 65f;

        return popupPos;
    }

    public void createClipboardPopup()
    {
        UIClipboard.getInstance().init(getPopupPosition(), _inputField, _clipboardType);
    }
}