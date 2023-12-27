using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Festa.Client.Module;
using UnityEngine.Events;

namespace Festa.Client
{
    public class UIInnerPanelTransition : MonoBehaviour
    {
        [SerializeField]
        private float duration = 0.1f;

        private FloatSmoothDamper _floatDamper;
        private RectTransform rect_former;
        private RectTransform rect_target;
        private bool _isAnimating = false;
        private bool _isClosing;
        private UnityAction _callback;

        public float getDuration()
        {
            return duration;
        }

        public void reset()
        {
            _callback = () =>
            {
                rect_former = null;
                rect_target = null;
            };
        }

        // pivot 0.5, 0.5 일 때 잘 작동해요
        public void slideLeftRight(RectTransform targetRect, float from, float to, bool isClosing, RectTransform realTarget = null)
        {
            if(_floatDamper == null)
                _floatDamper = FloatSmoothDamper.create(0f, duration);

            if (isClosing)
            {
                if(targetRect == null)
                {
                    rect_former.gameObject.SetActive(false);
                }
                else
                {
                    rect_former = targetRect;

                    // 이전 패널 원상복귀
                    rect_former.anchoredPosition = new Vector2(from, rect_former.anchoredPosition.y);
                    rect_former.gameObject.SetActive(true);
                    rect_former.gameObject.transform.SetAsFirstSibling();
                }
            }
            else
            {
                openImmediately(targetRect);
            }

            _isClosing = isClosing;
            _floatDamper.reset(from);
            _floatDamper.setTarget(to);

            _isAnimating = true;
        }

        public void openImmediately(RectTransform targetRect)
        {
            if (rect_target != null)
            {
                rect_former = rect_target;
                _callback = () =>
                {
                    rect_former.gameObject.SetActive(false);
                };
            }

            rect_target = targetRect;
            rect_target.gameObject.SetActive(true);
            rect_target.gameObject.transform.SetAsLastSibling();
        }

        public void Awake()
        {
            _floatDamper = FloatSmoothDamper.create(0f, duration);
        }

        public void Update()
        {
            if(_isAnimating)
            {
                if (_floatDamper.update())
                {
                    rect_target.anchoredPosition = new Vector2(_floatDamper.getCurrent(), rect_target.anchoredPosition.y);
                }
                else
                {
                    if (_isClosing)
                    {
                        rect_target.gameObject.SetActive(false);
                        rect_target = rect_former;
                    }

                    if(_callback != null)
                    {
                        _callback.Invoke();
                        _callback = null;
                    }

                    _isAnimating = false;
                }
            }
        }
    }
}
