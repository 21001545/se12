using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
    public class UIAlphaBlender : MonoBehaviour
    {
        [SerializeField]
        private Graphic[] _blendTargets;

        [SerializeField]
        private float _blendDurationInSeconds = 0.6f;

        public IEnumerable<TweenerCore<Color, Color, ColorOptions>> blendToEnableOnlyOneTweener(
            int enableOnlyIndex,
            (float from, float to)? range,
            float? blendDurationInSecondsOverride = null)
        {
            var (from, to) = range ?? (0, 1);
            return _blendTargets.Select((graphic, i) =>
                i == enableOnlyIndex
                    ? graphic.DOFade(to, blendDurationInSecondsOverride ?? _blendDurationInSeconds)
                    : graphic.DOFade(from, blendDurationInSecondsOverride ?? _blendDurationInSeconds)
            );
        }
    }
}