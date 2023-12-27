using System;
using System.Collections;
using System.Linq;

using DG.Tweening;
using DRun.Client;
using UnityEngine;
using UnityEngine.UI;

using Vector2 = UnityEngine.Vector2;

namespace Festa.Client
{
	public enum OpeningState
	{
		FromAppStart,
		FromMap,
		FromStart
	}

	public class UIRunningStatusTransitionAnimation : MonoBehaviour
	{
		[SerializeField] private Button _btn_pause;

		[SerializeField]
		private Image _img_btnPause;

		[SerializeField]
		private Image _img_btnPauseIcon;

		[Space(10)]
		[SerializeField] private UIStopButton _btn_stop;
		[SerializeField]
		private Image _img_btnStop;

		[SerializeField]
		private Image _img_btnStopIcon;

		[Space(10)]
		[SerializeField] private Button _btn_resume;
		[SerializeField]
		private Image _img_btnResume;

		[Space(10)]
		[SerializeField]
		private RectTransform _btnPauseRectTransform;

		[SerializeField]
		private RectTransform _btnStopRectTransform;

		[SerializeField]
		private RectTransform _btnResumeRectTransform;

		private Vector2 _origStopBtnPos;
		private Vector2 _origResumeBtnPos;
		private Vector2 _origPauseBtnPos;

		public const float DefaultTransitionDurationInSeconds = 0.35f;

		public bool isPausing = false;

		private Sequence _pauseSeq;
		private Sequence _stopSeq;

		private void Awake()
		{
			_origPauseBtnPos = _btnPauseRectTransform.anchoredPosition;
			_origStopBtnPos = _btnStopRectTransform.anchoredPosition;
			_origResumeBtnPos = _btnResumeRectTransform.anchoredPosition;
		}

		public void update(OpeningState openingState)
		{
			_img_btnPause.color = new(_img_btnPause.color.r, _img_btnPause.color.g, _img_btnPause.color.b, 1);
			_img_btnPauseIcon.color = new(_img_btnPauseIcon.color.r, _img_btnPauseIcon.color.g, _img_btnPauseIcon.color.b, 1);

			_img_btnStop.color = new(_img_btnStop.color.r, _img_btnStop.color.g, _img_btnStop.color.b, 1);
			_img_btnStopIcon.color = new(_img_btnStopIcon.color.r, _img_btnStopIcon.color.g, _img_btnStopIcon.color.b, 1);

			_img_btnResume.color = new(_img_btnResume.color.r, _img_btnResume.color.g, _img_btnResume.color.b, 1);

			switch (openingState)
			{
				case OpeningState.FromAppStart:
					if (isPausing)
					{
						pause();
						break;
					}

					resume();
					break;

				case OpeningState.FromMap:
					if (isPausing)
					{
						pause();
						break;
					}

					resume();
					break;

				case OpeningState.FromStart:
					resume();
					break;
			}
		}

		public void pause()
		{
			_btnStopRectTransform.anchoredPosition = _origStopBtnPos;
			_btnResumeRectTransform.anchoredPosition = _origResumeBtnPos;
			_btnPauseRectTransform.anchoredPosition = _origPauseBtnPos;

			_btn_stop.enabled = true;
			_btn_resume.interactable = true;
			_btn_pause.interactable = false;
		}

		public void resume()
		{
			_btnStopRectTransform.anchoredPosition = _origPauseBtnPos;
			_btnResumeRectTransform.anchoredPosition = _origPauseBtnPos;
			_btnPauseRectTransform.anchoredPosition = _origPauseBtnPos;

			_btn_stop.enabled = false;
			_btn_resume.interactable = false;
			_btn_pause.interactable = true;
		}

		// caching 하면 자꾸 뚝뚝 끊킴...
		public void pauseToStopAndResume()
		{
			StartCoroutine(fromPauseToStop());
		}

		public void stopAndResumeToPause()
		{
			StartCoroutine(fromStopToPause());
		}

		IEnumerator fromPauseToStop()
		{
			_btn_stop.enabled = false;
			_btn_resume.interactable = false;

			if (_stopSeq != null && _stopSeq.active && _stopSeq.IsPlaying())
				yield return _stopSeq.WaitForCompletion();

			_pauseSeq = DOTween.Sequence().SetAutoKill(true);

			// pauseToStopAndResume button 내부 icon fade out.
			// stopAndResumeToPause button 내부 icon fade in.
			// stop button fades in.
			// resume button fades in.
			// pauseToStopAndResume button 이 눌림 -> stopAndResumeToPause button 쪽으로 이동. (왼쪽으로).
			var tweens = new Tween[] {
				_img_btnPauseIcon.DOFade(0, DefaultTransitionDurationInSeconds),
				_img_btnStopIcon.DOFade(1, DefaultTransitionDurationInSeconds),
				_img_btnStop.DOFade(1, DefaultTransitionDurationInSeconds),
				_img_btnResume.DOFade(1, DefaultTransitionDurationInSeconds),

				_btnStopRectTransform.DOAnchorPos(_origStopBtnPos, DefaultTransitionDurationInSeconds),
				_btnPauseRectTransform.DOAnchorPos(_origStopBtnPos, DefaultTransitionDurationInSeconds),
				_btnResumeRectTransform.DOAnchorPos(_origResumeBtnPos, DefaultTransitionDurationInSeconds)
			};

			foreach (Tween t in tweens)
				_pauseSeq.Insert(0, t);

			yield return _pauseSeq.WaitForCompletion();

			_btn_stop.enabled = true;
			_btn_resume.interactable = true;
		}

		IEnumerator fromStopToPause()
		{
			_btn_pause.interactable = false;

			if (_pauseSeq != null && _pauseSeq.active && _pauseSeq.IsPlaying())
				yield return _pauseSeq.WaitForCompletion();

			_stopSeq = DOTween.Sequence().SetAutoKill(true);

			// 1. pauseToStopAndResume 버튼 내부 icon fade in.
			// 2. stopAndResumeToPause 버튼 내부 icon fade out.
			// 3. stopAndResumeToPause button 이 길게 눌림 -> pauseToStopAndResume button 쪽으로 이동. (오른쪽으로).
			var tweens = new Tween[] {
				_img_btnPauseIcon.DOFade(1, DefaultTransitionDurationInSeconds),
				_img_btnStopIcon.DOFade(0, DefaultTransitionDurationInSeconds),
				_img_btnStop.DOFade(0, DefaultTransitionDurationInSeconds),
				_img_btnResume.DOFade(0, DefaultTransitionDurationInSeconds),

				_btnStopRectTransform.DOAnchorPos(_origPauseBtnPos, DefaultTransitionDurationInSeconds),
				_btnPauseRectTransform.DOAnchorPos(_origPauseBtnPos, DefaultTransitionDurationInSeconds),
				_btnResumeRectTransform.DOAnchorPos(_origPauseBtnPos, DefaultTransitionDurationInSeconds)
			};

			foreach (Tween t in tweens)
				_stopSeq.Insert(0, t);

			yield return _stopSeq.WaitForCompletion();

			_btn_pause.interactable = true;
		}
	}
}
