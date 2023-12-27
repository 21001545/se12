using DG.Tweening;
using UnityEngine;

namespace Festa.Client
{
    public static class UIPanelTransitionHelper
    {
        private const float DefaultTransitionDurationInSeconds = 0.2f;
        //private static MonoBehaviour SharedMono => Yielder.SharedMono;
        //private static readonly WaitForEndOfFrame Eof = new WaitForEndOfFrame();

        #region method extension

        public static void moveInDirection(this RectTransform self,
            (float from, float to) delta,
            float? transitionDurationInSeconds = null,
            WhichDirection whichDirection = WhichDirection.Horizontal,
            Ease easeFn = Ease.Unset)
        {
            var (from, to) = delta;
            self.anchoredPosition = whichDirection switch
			{
				WhichDirection.Horizontal => new(from, 0),
				WhichDirection.Vertical => new(0, from),
				_ => throw new System.NotImplementedException()
			};

            self
                .DOAnchorPos(
                    endValue: whichDirection switch
					{
						WhichDirection.Horizontal => new(to, 0),
						WhichDirection.Vertical => new(0, to),
						_ => throw new System.NotImplementedException()
					},
                    duration: transitionDurationInSeconds ?? DefaultTransitionDurationInSeconds
                )
                .SetEase(easeFn);
        }

        public static void moveInDirectionWithCallback(this RectTransform self,
            (float from, float to) delta,
            float? transitionDurationInSeconds = null,
            WhichDirection whichDirection = WhichDirection.Horizontal,
            Ease easeFn = Ease.Unset,
            TweenCallback onBefore = null,
            TweenCallback<float> onProgress = null,
            TweenCallback onComplete = null)
        {
            var (from, to) = delta;

            onBefore?.Invoke();
            self.anchoredPosition = whichDirection switch
			{
				WhichDirection.Horizontal => new(from, 0),
				WhichDirection.Vertical => new(0, from),
				_ => throw new System.NotImplementedException()
			};

            self
                .DOAnchorPos(
                    endValue: whichDirection switch
					{
						WhichDirection.Horizontal => new(to, 0),
						WhichDirection.Vertical => new(0, to),
						_ => throw new System.NotImplementedException()
					},
                    duration: transitionDurationInSeconds ?? DefaultTransitionDurationInSeconds
                )
                .SetEase(easeFn)
                .OnUpdate(() =>
                    {
                        float progress = whichDirection switch
						{
							WhichDirection.Horizontal => self.anchoredPosition.x,
							WhichDirection.Vertical => self.anchoredPosition.y,
							_ => throw new System.NotImplementedException(),
						};
                        onProgress?.Invoke(progress / to);
                    }
                )
                .OnComplete(onComplete);
        }

        #endregion method extension

        #region impl with damper

        //public enum Direction
        //{
        //    L2R,
        //    R2L,
        //};

        //public static Func<Direction, (float from, float to)> Out() => (Direction direction) => direction switch
        //{
        //    Direction.L2R => (0, Screen.width),
        //    Direction.R2L => (0, -Screen.width),
        //};

        //public static Func<Direction, (float from, float to)> In() => (Direction direction) => direction switch
        //{
        //    Direction.L2R => (Screen.width, 0),
        //    Direction.R2L => (-Screen.width, 0)
        //};

        /// <summary>
        /// 한 방향으로 rect transform 을 이동.
        /// </summary>
        /// <param name="delta">이동 량</param>
        /// <param name="targetRectTransform">이동 타겟</param>
        /// <param name="transitionDurationInSeconds">이동 시간</param>
        /// <param name="whichDirection">이동 방향</param>
        //public static void moveInDirection(
        //	(float from, float to) delta,
        //	RectTransform targetRectTransform,
        //	float? transitionDurationInSeconds = null,
        //	WhichDirection whichDirection = WhichDirection.Horizontal)
        //{
        //	float transitionDuration = transitionDurationInSeconds ?? DefaultTransitionDurationInSeconds;
        //	var (from, to) = delta;

        //	// Transition 할 필요 없이 즉시 발동.
        //	if (Mathf.Approximately(transitionDuration, 0))
        //	{
        //		targetRectTransform.anchoredPosition += whichDirection switch
        //		{
        //			WhichDirection.Horizontal => new(to, 0),
        //			WhichDirection.Vertical => new(0, to),
        //		};

        //		return;
        //	}

        //	var damper = FloatSmoothDamper.create(from, transitionDuration);
        //	damper.setTarget(to);

        //	SharedMono.StartCoroutine(Transition());

        //	IEnumerator Transition()
        //	{

        //		while (damper.update())
        //		{
        //			float val = damper.getCurrent();
        //			Vector2 nextPos = whichDirection switch
        //			{
        //				WhichDirection.Horizontal => new(val, 0),
        //				WhichDirection.Vertical => new(0, val),
        //			};

        //			targetRectTransform.anchoredPosition = nextPos;

        //			yield return Eof;
        //		}
        //	}
        //}


        ///// <summary>
        ///// 한 방향으로 rect transform 을 이동.
        ///// </summary>
        ///// <param name="delta">이동 량</param>
        ///// <param name="targetRectTransform">이동 타겟</param>
        ///// <param name="transitionDurationInSeconds">이동 시간</param>
        ///// <param name="whichDirection">이동 방향</param>
        ///// <param name="onBefore"></param>
        ///// <param name="onProgress"></param>
        ///// <param name="onComplete"></param>
        //public static void moveInDirectionWithCallback(
        //	(float from, float to) delta,
        //	RectTransform targetRectTransform,
        //	float? transitionDurationInSeconds = null,
        //	WhichDirection whichDirection = WhichDirection.Horizontal,
        //	Action onBefore = null,
        //	Action<float> onProgress = null,
        //	Action onComplete = null)
        //{
        //	onBefore?.Invoke();

        //	float transitionDuration = transitionDurationInSeconds ?? DefaultTransitionDurationInSeconds;

        //	var (from, to) = delta;

        //	// Transition 할 필요 없이 즉시 발동.
        //	if (Mathf.Approximately(transitionDuration, 0))
        //	{
        //		targetRectTransform.anchoredPosition += whichDirection switch
        //		{
        //			WhichDirection.Horizontal => new(to, 0),
        //			WhichDirection.Vertical => new(0, to),
        //		};
        //		onComplete?.Invoke();

        //		return;
        //	}

        //	FloatSmoothDamper damper = FloatSmoothDamper.create(from, transitionDuration);
        //	damper.setTarget(to);

        //	SharedMono.StartCoroutine(Transition());

        //	IEnumerator Transition()
        //	{
        //		while (damper.update())
        //		{
        //			float val = damper.getCurrent();
        //			Vector2 nextPos = whichDirection switch
        //			{
        //				WhichDirection.Horizontal => new(val, 0),
        //				WhichDirection.Vertical => new(0, val),
        //			};

        //			targetRectTransform.anchoredPosition = nextPos;
        //			onProgress?.Invoke(val / to);

        //			yield return Eof;
        //		}

        //		onComplete?.Invoke();
        //	}

        //}

        #endregion impl with damper
    }
}