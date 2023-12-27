using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using Festa.Client;

public class ClaimRewardTransition : MonoBehaviour
{
    [SerializeField]
    private RectTransform rect_star;
    [SerializeField]
    private Image img_star;

    [SerializeField]
    private RectTransform rect_text;
    [SerializeField]
    private TMP_Text txt_text;

    [Header("button body")]
    [SerializeField]
    private Image img_buttonBack;
    [SerializeField]
    private Image img_star_const;
    [SerializeField]
    private TMP_Text txt_text_const;

    private float _starDesPos;
    private float _textDesPos;
    private float _padding = 2f;
    private float _pivotHeight = -1.1f;     // 눌렀을 때 살짝 움직이는 y 값
    private float _desHeight = 24f;         // 최종 안착해야 하는 y 값

    public void onClickButtonDown()
    {
        calculateButtonUpDesPosition();

        float starX = rect_star.anchoredPosition.x;
        float textX = rect_text.anchoredPosition.x;

        float duration = 0.1f;

        DOTween.To(() => rect_star.anchoredPosition, x => rect_star.anchoredPosition = x, calculateButtonDownDesPosition(starX, _starDesPos), duration);
        DOTween.To(() => rect_text.anchoredPosition, x => rect_text.anchoredPosition = x, calculateButtonDownDesPosition(textX, _textDesPos), duration);
    }

    public void onClickButtonUp()
    {
        float duration = 0.3f;

        // 위치 보정
        DOTween.To(() => rect_star.anchoredPosition, x => rect_star.anchoredPosition = x, new Vector2(_starDesPos, _desHeight), duration);
        DOTween.To(() => rect_text.anchoredPosition, x => rect_text.anchoredPosition = x, new Vector2(_textDesPos, _desHeight), duration);

        // 색 보정
        // 움직이는 글씨
        DOTween.To(() => img_star.color, x => img_star.color = x, ColorChart.primary_500, duration);
        DOTween.To(() => txt_text.color, x => txt_text.color = x, ColorChart.primary_500, duration);

        // 배경 날리기
        Color mint_trans = ColorChart.primary_300;
        mint_trans.a = 0f;
        Color white_trans = ColorChart.white;
        white_trans.a = 0f;

        DOTween.To(() => img_buttonBack.color, x => img_buttonBack.color = x, mint_trans, duration);
        DOTween.To(() => img_star_const.color, x => img_star_const.color = x, white_trans, duration);
        DOTween.To(() => txt_text_const.color, x => txt_text_const.color = x, white_trans, duration);

        Invoke("claimRewards", 0.55f);
    }

    // 위치
    private void calculateButtonUpDesPosition()
    {
        float halfSize = (rect_text.rect.width + rect_star.rect.width + _padding) * 0.5f;
        _starDesPos = -halfSize + rect_star.rect.width * 0.5f;
        _textDesPos = halfSize - rect_text.rect.width * 0.5f;
    }

    private Vector2 calculateButtonDownDesPosition(float origX, float desPos)
    {
        float pivotX = (origX + (origX - desPos) / _desHeight) * -_pivotHeight;
        return new Vector2(pivotX, _pivotHeight);
    }

    // 색
    private void claimRewards()
    {
        // 일단 글씨 지우고,,
        float duration = 0.2f;

        Color mint_trans = ColorChart.primary_500;
        mint_trans.a = 0f;

        DOTween.To(() => img_star.color, x => img_star.color = x, mint_trans, duration);
        DOTween.To(() => txt_text.color, x => txt_text.color = x, mint_trans, duration);
    }
}