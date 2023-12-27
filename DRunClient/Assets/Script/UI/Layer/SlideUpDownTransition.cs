using System;
using System.Collections;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Festa.Client
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class SlideUpDownTransition : AbstractPanelTransition
    {
        public enum SlideDirection
        {
            DownToUp,
            RightToLeft,
        }
		
        public enum TransitionType
        {
            FadeOutCloseOnly,
            FadeOutWithSlide,
            SlideOnly
        }

        private RectTransform _rt;
        private CanvasGroup _canvas_group;
        private Vector2SmoothDamper _positionDamper;
        private FloatSmoothDamper _alpha_damper;

        private int _step;
        private bool _active;
        private bool _isClosingNow;
        private float _closeWaitTime;

        private ITransitionEventHandler _eventHandler;

        [SerializeField]
        private float _positionDuration = 0.08f;
        
        [field: SerializeField]
        public float alphaDuration { get; private set; }= 0.3f;

        [SerializeField]
        private RectTransform targetTransform;
        [SerializeField]
        private Image _dissolveImage;
        private float _dissolveMax;

        [field: SerializeField]
        public SlideDirection slideDirection { get; set; } = SlideDirection.DownToUp;

        public static class Step
        {
            public const int open = 1;
            public const int close = 2;
        }

        [field: SerializeField] 
        public TransitionType transitionType { get; set; } = TransitionType.SlideOnly;

        public bool useOnlyOpen;

        private Vector2 _initPosition;
        private Vector2 _hidePosition;

        public override float getDuration()
        {
            return _positionDuration;
        }

        public override void init(ITransitionEventHandler eventHandler)
        {
            _canvas_group = GetComponent<CanvasGroup>();
            if(_dissolveImage != null)
            {
                _dissolveMax = _dissolveImage.color.a;
            }

            if (targetTransform != null)
            {
                _rt = targetTransform;
            }
            else
            {
                _rt = transform as RectTransform;
            }

            RectTransform rtParent = _rt.parent as RectTransform;

            Debug.Log($"[{gameObject.name}] : {rtParent.rect}, {_rt.rect}, {_rt.anchoredPosition}", gameObject);

            _hidePosition = _initPosition = _rt.anchoredPosition;
            if (slideDirection == SlideDirection.DownToUp)
            {
                // 넓이로 하면 엄청 멀리 가야 해서 엄청 빨리 움직이게 되는구나!!
                //_hidePosition = _initPosition + new Vector2(0, -(_initPosition.y + _rt.rect.height));
                //_hidePosition = _initPosition + new Vector2(0, -Mathf.Clamp(_rt.rect.height, 0, rtParent.rect.height));

                //
                //_hidePosition.y = -Screen.height + _initPosition.y;
                _hidePosition.y = -rtParent.rect.size.y;
            }
            else
            {
                //_hidePosition = _initPosition + new Vector2((_initPosition.x + _rt.rect.width), 0);
                _hidePosition = _initPosition + new Vector2(Mathf.Clamp(_rt.rect.width, 0, rtParent.rect.width), 0);
            }

            _positionDamper = Vector2SmoothDamper.create(_hidePosition, _positionDuration);
            _alpha_damper = FloatSmoothDamper.create(1.0f, alphaDuration);
            //_backgroundDamper = FloatSmoothDamper.create(0f, _duration);

            _rt.anchoredPosition = _positionDamper.getCurrent();

            _canvas_group.interactable = false;
            _active = false;
            _eventHandler = eventHandler;
        }

        public override float startOpen()
        {
            _isClosingNow = false;
            _step = Step.open;
            _active = true;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            //_positionDamper.init( new Vector2( 0, _rt.rect.height), duration);
            _positionDamper.resetVelocity();
            _positionDamper.setTarget(_initPosition);
            _alpha_damper.reset(1.0f);
            _canvas_group.alpha = 1.0f;

            if (_dissolveImage != null)
            {
                Invoke("dissolveShowImage", _positionDuration * 0.5f);

            }

            _rt.anchoredPosition = _positionDamper.getCurrent();
            _eventHandler.onTransitionEvent(TransitionEventType.start_open);

            //Debug.Log($"{Time.realtimeSinceStartup}:startOpen");

            return _positionDuration;
        }

        private void dissolveShowImage()
        {
            Color c = _dissolveImage.color;
            c.a = _dissolveMax;
            DOTween.To(() => _dissolveImage.color, x => _dissolveImage.color = x, c, _positionDuration * 0.5f);
        }

        public override float openImmediately()
        {
            _isClosingNow = false;
            transform.SetAsFirstSibling();
            _step = Step.open;
            _positionDamper.setTarget(_initPosition);
            _positionDamper.reset(_initPosition);
            _alpha_damper.reset(1.0f);
            _canvas_group.alpha = 1.0f;

            if (_dissolveImage != null)
            {
                Color c = _dissolveImage.color;
                c.a = _dissolveMax;
                _dissolveImage.color = c;
            }

            _rt.anchoredPosition = _positionDamper.getCurrent();
            gameObject.SetActive(true);
            _canvas_group.interactable = true;

            _eventHandler.onTransitionEvent(TransitionEventType.start_open);
            _eventHandler.onTransitionEvent(TransitionEventType.end_open);
            return 0f;
        }

        public override float startClose()
        {
transform.SetAsLastSibling();
            _isClosingNow = false;
            _step = Step.close;
            _active = true;
            gameObject.SetActive(true);
            _canvas_group.interactable = false;

            if (this.useOnlyOpen)
            {
                _positionDamper.resetVelocity();
                _positionDamper.reset(_hidePosition);
_eventHandler.onTransitionEvent(TransitionEventType.start_close);

return _positionDuration;
            }

            // 2022/12/26 윤상: fade out + slide 동시동작 필요해서 변경
            switch (transitionType)
            {
                case TransitionType.FadeOutCloseOnly:
                    _alpha_damper.setTarget(0.0f);

                    // alpha 가 0되면 바로 원상 이동
                    StartCoroutine(resetPosAfterFadeOutOnClosing());

                    IEnumerator resetPosAfterFadeOutOnClosing()
                    {
                        yield return new WaitUntil(() =>
                            UnityEngine.Mathf.Approximately(_alpha_damper.getCurrent(), 0));
                        
                        _positionDamper.resetVelocity();
                        _positionDamper.reset(_hidePosition);
                        _rt.anchoredPosition = _hidePosition;
                    }
break;
                
                case TransitionType.FadeOutWithSlide:
                    _alpha_damper.setTarget(0.0f);
                    _positionDamper.resetVelocity();
                    _positionDamper.setTarget(_hidePosition);

                    if (_dissolveImage != null)
                    {
                        Color c = _dissolveImage.color;
                        c.a = 0f;
                        DOTween.To(() => _dissolveImage.color, x => _dissolveImage.color = x, c, _positionDuration * 0.5f);
                    }

                    _rt.anchoredPosition = _positionDamper.getCurrent();
                    break;
                
                case TransitionType.SlideOnly:
                    _positionDamper.resetVelocity();
                    _positionDamper.setTarget(_hidePosition);

                    if (_dissolveImage != null)
                    {
                        Color c = _dissolveImage.color;
                        c.a = 0f;
                        DOTween.To(() => _dissolveImage.color, x => _dissolveImage.color = x, c, _positionDuration * 0.5f);
                    }

                    _rt.anchoredPosition = _positionDamper.getCurrent();
                    break;
}
            
_eventHandler.onTransitionEvent(TransitionEventType.start_close);

            //Debug.Log($"{Time.realtimeSinceStartup}:startClose");
            return _positionDuration;
        }

        public override float closeImmediately(float duration)
        {
            transform.SetAsFirstSibling();
            _step = Step.close;
            _isClosingNow = true;
            _closeWaitTime = duration;
            return 0f;
        }

        private void closePanel()
        {
            _active = true;
            _positionDamper.setTarget(_hidePosition);
            //_positionDamper.reset(_hidePosition);
            _rt.anchoredPosition = _positionDamper.getCurrent();
            gameObject.SetActive(false);
            _canvas_group.interactable = false;
            _alpha_damper.reset(1.0f);

            if(_dissolveImage != null)
            {
                Color c = _dissolveImage.color;
                c.a = 0f;
                _dissolveImage.color = c;
            }

            _eventHandler.onTransitionEvent(TransitionEventType.start_close);
            _eventHandler.onTransitionEvent(TransitionEventType.end_close);

            _isClosingNow = false;
        }

        public override bool isActive()
        {
            return _active;
        }

        public override void update()
        {
            if (_isClosingNow)
            {
                if (_closeWaitTime <= 0f)
                    closePanel();
                else
                    _closeWaitTime -= Time.deltaTime;
            }

            if (_active == false)
            {
                return;
            }

            _active = false;

            if (_positionDamper.update())
            {
                _active = true;
                _rt.anchoredPosition = _positionDamper.getCurrent();
            }

            if( _alpha_damper.update())
            {
                _active = true;
                _canvas_group.alpha = _alpha_damper.getCurrent();
            }

            if (_active == false)
            {
                if (_step == Step.open)
                {
                    _canvas_group.interactable = true;
                    _eventHandler.onTransitionEvent(TransitionEventType.end_open);
                    //Debug.Log($"{Time.realtimeSinceStartup}:endOpen");
                }
                else if (_step == Step.close)
                {
                    _eventHandler.onTransitionEvent(TransitionEventType.end_close);
                    gameObject.SetActive(false);
                    //Debug.Log($"{Time.realtimeSinceStartup}:endClose");
                }
            }
        }
    }
}