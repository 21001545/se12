using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using DG.Tweening;
using Festa.Client;
using UnityEngine.UI;

public class UIProfileScroll : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField]
    private GameObject go_loadingSpinner;
    [SerializeField]
    private Image img_targetArea;
    [SerializeField]
    private RectTransform rect_target;
    [SerializeField]
    private float _refreshThreshold;
    [SerializeField]
    private float _scrollUpDownThreshold;

    [Header("scroll up changes")]
    [SerializeField]
    private CanvasGroup can_itemCounter;
    [SerializeField]
    private CanvasGroup can_followCounter;
    [SerializeField]
    private RectTransform rect_profileUpperText;
    [SerializeField]
    private GameObject go_profileLowerText;
    [SerializeField]
    private Image img_profileBack;
    [SerializeField]
    private RectTransform rect_profileThumbnail;
    [SerializeField]
    private CanvasGroup can_settingButton;
    [SerializeField]
    private RectTransform rect_editButton;

    public UnityEvent onTension;

    private bool _isDragging = false;
    public bool _scrollInteractable = true;
    public bool isUp = false;

    private float _targetOrigHeight;
    private float _dragstartPosY;
    private float _maxHeight;
    private Vector2 _profileOrigPos;
    private float _profileOrigWidth;
    private float _editButtonOrigPosY;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_isDragging && eventData.position.y > 812f - 225f)
            UIProfile.getInstance().onClickEditBoard();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
        _dragstartPosY = eventData.position.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        float deltaY = eventData.position.y - _dragstartPosY;

        bool loading = false;

        if (eventData.delta.y < 0)
        {
            // 아래로 스크롤 중

            /*            if (!_scrollInteractable)
                        {
                            // 확대상태 + 스크롤 맨 위까지 갔는데도 더 가고 싶음
                            if (isUp && UIProfile.getInstance().scroller.NormalizedScrollPosition <= 0f)
                            {
                                change_scrollInteractable(true);
                            }

                            return;
                        }*/

            if (rect_target.rect.height <= _targetOrigHeight)
            {
                loading = true;
                float target = Mathf.Clamp(_targetOrigHeight + deltaY, _targetOrigHeight - _refreshThreshold, _targetOrigHeight);
                // 적당히 따라와
                changeSizeDelta(target);
            }
            else
            {
                deltaY = Mathf.Clamp(deltaY, _targetOrigHeight - _maxHeight, 0f);
                if (_maxHeight + deltaY > _scrollUpDownThreshold)
                {
                    // 적당히 따라와
                    changeSizeDelta(_maxHeight + deltaY);
                }
                else
                {
                    // 자동스크롤
                    setProfileUI(0f, UIProfile.getInstance().isMyAccount());
                }
            }
        }

        else if(eventData.delta.y > 0)
        {
            // 위로 스크롤 중

            if (isUp && !UIProfile.getInstance().isEmpty)
            {
                change_scrollInteractable(false);
                return;
            }

            //deltaY = Mathf.Clamp(deltaY, _targetOrigHeight, _targetOrigHeight + _scrollUpDownThreshold);
            deltaY = Mathf.Clamp(deltaY, 0f, _maxHeight - _targetOrigHeight);

            if (deltaY + _targetOrigHeight < _scrollUpDownThreshold)
            {
                // 적당히 따라와
                changeSizeDelta(_targetOrigHeight + deltaY);
            }
            else
            {
                // 자동스크롤
                setProfileUI(1f, UIProfile.getInstance().isMyAccount());
            }
        }

        go_loadingSpinner.SetActive(loading);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
        go_loadingSpinner.SetActive(false);

        if (_targetOrigHeight - rect_target.rect.height >= _refreshThreshold)
        {
            Vector2 targetPos = new Vector2(375f, _targetOrigHeight);
            DOTween.To(() => rect_target.sizeDelta, x => rect_target.sizeDelta = x, targetPos, 0.2f);
            //rect_targetArea.sizeDelta = targetPos;
            onTension?.Invoke();
        }
    }

    private void changeSizeDelta(float height)
    {
        rect_target.sizeDelta = new Vector2(375f, height);
        //rect_targetArea.sizeDelta = new Vector2(375f, height);
    }

    public void change_scrollInteractable(bool interact)
    {
        if(img_targetArea != null)
            img_targetArea.raycastTarget = interact;
        _scrollInteractable = interact;
    }

    // 0 이면 아래 1 이면 위
    public void setProfileUI(float ratio, bool isMe = true)
    {
        if (_targetOrigHeight == 0f)
            return;

        float duration = 0.2f;

        // 전체
        Vector2 targetPos = new Vector2(375f, ratio == 1f ? _maxHeight : _targetOrigHeight);
        DOTween.To(() => rect_target.sizeDelta, x => rect_target.sizeDelta = x, targetPos, duration);
        //rect_targetArea.sizeDelta = targetPos;

        // 설정 / 수정 버튼
        DOTween.To(() => can_settingButton.alpha, x => can_settingButton.alpha = x, 1 - ratio, duration);
        can_settingButton.interactable = ratio == 1f ? false : true;
        DOTween.To(() => rect_editButton.anchoredPosition, x => rect_editButton.anchoredPosition = x, new Vector2(rect_editButton.anchoredPosition.x, _editButtonOrigPosY - ratio * 76f), duration);

        // 카운터 알파
        DOTween.To(() => can_followCounter.alpha, x => can_followCounter.alpha = x, 1 - ratio, duration);
        DOTween.To(() => can_itemCounter.alpha, x => can_itemCounter.alpha = x, 1 - ratio, duration);

        // 프로필 텍스트
        float textTarget = 343f - ratio * 32f;
        if (!isMe)
            textTarget = 343f - ratio * 56f;

        DOTween.To(() => rect_profileUpperText.sizeDelta, x => rect_profileUpperText.sizeDelta = x, new Vector2(textTarget, rect_profileUpperText.rect.height), duration);
        go_profileLowerText.SetActive(ratio == 1f ? false : true);

        // 프로필 이미지
        float imageTarget = _profileOrigPos.x + 2f * ratio;
        if (!isMe)
            imageTarget = _profileOrigPos.x + 26f * ratio;

        DOTween.To(() => rect_profileThumbnail.anchoredPosition, x => rect_profileThumbnail.anchoredPosition = x, new Vector2(imageTarget, _profileOrigPos.y - 115f * ratio), duration);
        DOTween.To(() => rect_profileThumbnail.sizeDelta, x => rect_profileThumbnail.sizeDelta = x, new Vector2(_profileOrigWidth - 48f * ratio, _profileOrigWidth - 48f * ratio), duration);

        if(ratio == 1f)
        {
            Color color = img_profileBack.color;
            color.a = 0f;
            img_profileBack.color = color;
        }
        else
        {
            Invoke("setProfileBack", duration);
        }

        isUp = ratio == 1f ? true : false;
    }

    private void setProfileBack()
    {
        Color color = img_profileBack.color;
        color.a = 1f;
        img_profileBack.color = color;
    }

    public void Start()
    {
        go_loadingSpinner.SetActive(false);
        _targetOrigHeight = rect_target.rect.height;
        _maxHeight = 763.5f;

        _profileOrigPos = rect_profileThumbnail.anchoredPosition;
        _profileOrigWidth = rect_profileThumbnail.rect.width;
        _editButtonOrigPosY = rect_editButton.anchoredPosition.y;
    }
}
