using Festa.Client;
using Festa.Client.Module;
using Festa.Client.RefData;

using UnityEngine;

namespace DRun.Client
{
    public class UICircleLineFillAnimator : MonoBehaviour
    {
        [SerializeField]
        private UICircleLine _targetUICircleLine;

        [field: SerializeField]
        public float FallbackInterpolationTimeInSeconds { get; set; } = 0.5f;

        private FloatSmoothDamper _damper;

        public void init()
        {
            // ref string 에서 보간 시간 가져오기.
            int time = GlobalRefDataContainer.getConfigInteger(
                RefConfig.Key.UI.gauge_fill_time,
                (int)(FallbackInterpolationTimeInSeconds * 100)
            );

            _damper = FloatSmoothDamper.create(0, (float)time / 100);
            applyValue();
        }

        public void transtion(float fillAmount)
        {
            _damper.setTarget(fillAmount);
        }

        private void Update()
        {
            if (_damper == null)
                return;
            
            if (_damper.update())
            {
                applyValue();
            }
        }

        private void applyValue()
        {
			float damperValue = _damper.getCurrent();
			damperValue = Mathf.Max(0, Mathf.Min(1, damperValue));
			_targetUICircleLine.setFillAmount(damperValue, true);
		}
	}
}