using System.Linq;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class UIAlphaFader : MonoBehaviour
    {
        [SerializeField]
        private float _fadeSpeed = 0.5f;

        [SerializeField]
        private Graphic[] _fadeTargetGraphics;

        public void fade(
            FadeDirection fadeDirection = FadeDirection.fadeIn,
            float? fadeSpeedOverride = null)
        {
            if (_fadeTargetGraphics == null || _fadeTargetGraphics.Length == 0)
                return;

            var alphaTweeners = _fadeTargetGraphics.Select(graphic =>
                DOTween.ToAlpha(
                    getter: () => graphic.color,
                    setter: set => graphic.color = set,
                    endValue: fadeDirection switch
					{
						FadeDirection.fadeIn => 1,
						FadeDirection.fadeOut => 0,
						_ => throw new System.NotImplementedException(),
					},
                    duration: fadeSpeedOverride ?? _fadeSpeed
                )
            );

            var seq = DOTween.Sequence();
            foreach (var tw in alphaTweeners)
                seq.Insert(0, tw);
        }
    }
}