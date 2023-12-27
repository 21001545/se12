using System;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.RefData;

using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
    /// <summary>
    /// Gauge 끝에 달린 동글뱅이
    /// </summary>
    public class UICircleLinePointMover : MonoBehaviour
    {
        private RectTransform _pointRectTransform;

        [SerializeField]
        private UICircleLine _parentCircleLine;

        /// <summary>
        /// true: start() 에서 자동 움직임.
        /// </summary>
        [SerializeField]
        private bool _startAutomatically = false;

        private Rect _parentRect;

        /// <summary>
        /// true: Damper 사용
        /// </summary>
        [SerializeField]
        private bool _useSmoothDamp = true;

        private FloatSmoothDamper _damper;

        [field: SerializeField]
        public float FallbackInterpolationTimeInSeconds { get; set; } = 0.5f;

        [field: SerializeField]
        public UICircleFill pointCircle { get; private set; }

        public void init()
        {
            _pointRectTransform = GetComponent<RectTransform>();
            _parentRect = _parentCircleLine.GetComponent<RectTransform>().rect;

            if (!_useSmoothDamp)
                return;

            // ref string 에서 보간 시간 가져오기.
            int time = GlobalRefDataContainer.getConfigInteger(
                RefConfig.Key.UI.gauge_fill_time,
                (int)(FallbackInterpolationTimeInSeconds * 100)
            );

            _damper = FloatSmoothDamper.create(0, (float)time / 100);
            applyValue();
        }

        private void Start()
        {
            if (_startAutomatically)
            {
                if (_damper == null)
                    init();

                if (_useSmoothDamp)
                {
                    moveWithTransition(_parentCircleLine.fillAmount);
                    return;
                }

                moveInstantly();
            }
        }

        public void moveInstantly()
        {
            if (_useSmoothDamp)
                return;

            _pointRectTransform.anchoredPosition = CalcNextPosition(_parentCircleLine.fillAmount);
        }

        public void moveWithTransition(float fillAmount)
        {
            if (!_useSmoothDamp)
                return;

            //_damper.setTarget(_parentCircleLine.fillAmount);
            _damper.setTarget(fillAmount);
        }

        private void Update()
        {
            if (_damper == null)
                return;
            
            if (!_useSmoothDamp)
                return;

            if (_damper.update())
            {
                applyValue();
            }
        }

        private void applyValue()
        {
            var damperValue = _damper.getCurrent();
            damperValue = Mathf.Max(0, Mathf.Min(1, damperValue));
            _pointRectTransform.anchoredPosition = CalcNextPosition(damperValue);
        }

        private Vector2 CalcNextPosition(float fillAmount)
        {
            float angle = (_parentCircleLine.beginAngle * Mathf.Deg2Rad) -
                          (fillAmount * _parentCircleLine.fillScale * Mathf.PI * 2);

            return new(
                Mathf.Cos(angle) * _parentRect.width * 0.5f,
                Mathf.Sin(angle) * _parentRect.height * 0.5f
            );
        }
    }
}