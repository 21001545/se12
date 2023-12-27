using System.Collections;

using Festa.Client;

using UnityEngine;

namespace DRun.Client
{
	public class UIEntryProfile : MonoBehaviour
	{
		[SerializeField]
		private Animator _anim_profileExpand;

		/// <summary>
		/// 0 - profile
		/// 1 - profile extended 
		/// </summary>
		[SerializeField]
		private UISwitcher _profileSwitcher;

		[Space(20)]
		[Header("닫히는 데 걸리는 시간 (기본: 3초)")]
		[SerializeField]
		private float _closeDelayInSeconds = 3.0f;

		[SerializeField]
		// 현재 keyframe 프로필 닫기 애니메이션 길이 -> 0.4x 초
		private float _closeAnimationLengthInSeconds = 0.5f;

		[SerializeField] private UIEntryProfileExt _profileExt;

		private static readonly int Open = Animator.StringToHash("open");
		private static readonly int Close = Animator.StringToHash("close");

		private Coroutine _collapseAutoHandle;
		private Coroutine _collapseManuallyHandle;

		private static ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		private void stopCoroutineHandleSafe(Coroutine handle)
		{
			if (handle == null)
				return;

			StopCoroutine(handle);
			handle = null;
		}

		private void Start()
		{
			_profileExt.init();
			_profileExt.gameObject.SetActive(false);
		}

		public void onClick_expandProfile()
		{
			// switch to profile extend
			_profileSwitcher.@switch(1);

			_anim_profileExpand.enabled = true;
			_anim_profileExpand.SetTrigger(Open);

			stopCoroutineHandleSafe(_collapseAutoHandle);
			_collapseAutoHandle = StartCoroutine(collapseProfileAutomaticallyAfterOpen());

			if (ViewModel.BasicMode.IsSubscribedEntryExpDeferrer)
			{
				_profileExt.gameObject.SetActive(true);
			}
		}

		public void onClick_collapseProfile()
		{
			_anim_profileExpand.enabled = true;
			stopCoroutineHandleSafe(_collapseManuallyHandle);
			_collapseManuallyHandle = StartCoroutine(collapseProfileManually());
		}

		IEnumerator collapseProfileAutomaticallyAfterOpen()
		{
			stopCoroutineHandleSafe(_collapseManuallyHandle);

			yield return new WaitForSeconds(_closeDelayInSeconds);

			yield return collapseImpl(_collapseAutoHandle);
		}

		IEnumerator collapseProfileManually()
		{
			stopCoroutineHandleSafe(_collapseAutoHandle);

			yield return collapseImpl(_collapseManuallyHandle);
		}

		IEnumerator collapseImpl(Coroutine handle)
		{
			_anim_profileExpand.SetTrigger(Close);

			// 현재 keyframe 프로필 닫기 애니메이션 길이 -> 0.7x 초
			// 고정 시간 후 닫기 로직 실행.
			yield return new WaitForSeconds(_closeAnimationLengthInSeconds);

			// switch to profile basic.
			_profileSwitcher.@switch(0);
			_anim_profileExpand.enabled = false;
		}
	}
}