using Festa.Client.Module;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Festa.Client
{
    /// <summary>
    /// 다음 스텝으로 넘어가는 유아이에서 주로 쓰이는 버튼
    /// 
    /// 1. 활성/비활성 상태에 따라 색이 바뀌고
    ///     220519 but... 디자인 수정되면서 굳이 이걸 할 필요가 없어짐!!
    /// 2. 키보드가 올라가면 버튼도 올라가도록 되어 있어요
    /// </summary>

    public class SetBottomButton : MonoBehaviour
    {
        [SerializeField]
        RectTransform rect;
        [SerializeField]
        private Button btn;

        [SerializeField]
        private bool isReactiveToKeyBoard;

        [SerializeField]
        private float screenHeight = 812f;
        private float _offset = 16f;
        private float _desY = 48f;          // 목표 위치 y 값
        private TouchScreenKeyboardUtil KeyboardUtil => TouchScreenKeyboardUtil.getInstance();

        void Update()
        {
            if(isReactiveToKeyBoard)
            {
                float newDesY = (KeyboardUtil.getHeight() / Screen.height) * screenHeight;
                Vector2 desPos = Vector2.zero;

                if (_desY != newDesY)
                {
                    _desY = newDesY;
                    if (_desY <= 1f)
                        // 키보드 없는 경우 : 바닥에 챡 붙지 않도록~!
                        desPos.y = 48f;
                    else
                    {
                        rect.anchoredPosition = Vector2.zero;
                        desPos.y = _desY + _offset;
                    }

                    DOTween.To(() => rect.anchoredPosition, x => rect.anchoredPosition = x, desPos, 0.4f).SetEase(Ease.OutCubic);
                }
            }
        }
    }
}