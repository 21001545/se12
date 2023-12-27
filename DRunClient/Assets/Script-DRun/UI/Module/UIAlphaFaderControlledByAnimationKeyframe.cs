using Festa.Client.Module;

using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
    public sealed class UIAlphaFaderControlledByAnimationKeyframe : MonoBehaviour
    {
        [SerializeField]
        private Graphic[] _fadeTargetGraphics;

        [SerializeField]
        [ReadOnly]
        private float _alphaStepControlledByAnimation;

        public void applyAlphaStep()
        {
            foreach (var graphic in _fadeTargetGraphics)
            {
                Color origColor = graphic.color;
                origColor.a = _alphaStepControlledByAnimation;
                graphic.color = origColor;
            }
        }

        public void completeAlphaStep(float completeAlpha = 1.0f)
        {
            _alphaStepControlledByAnimation = completeAlpha;
        }

    }
}