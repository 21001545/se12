using System.Collections;
using Drun.Client;
using TMPro;

using UnityEngine;

namespace DRun.Client
{
	[ExecuteInEditMode]
	public class UIEntryProfileNotification : MonoBehaviour
	{
		public TMP_Text text_notification_contents;

		[SerializeField]
		private Animator _anim_entry_profile_notification;

		private static readonly int Close = Animator.StringToHash("close");

		public void notify(string contents, float closeDelayInSeconds = 3.0f)
		{
//#if UNITY_EDITOR
//			Debug.Log($"<color=teal>엔트리 프로필 notify!! {contents}</color>", this);
//#endif
			gameObject.SetActive(true);

			// play animator automatically on activate self.
			text_notification_contents.text = contents;

			if (isActiveAndEnabled)
				StartCoroutine(hideSelfAfterDelay(closeDelayInSeconds));
			else
				Yielder.SharedMono.StartCoroutine(hideSelfAfterDelay(closeDelayInSeconds));
		}

		IEnumerator hideSelfAfterDelay(float closeDelayInSeconds)
		{
			yield return new WaitForSeconds(closeDelayInSeconds);

			gameObject.SetActive(false);
			if( _anim_entry_profile_notification.gameObject.activeInHierarchy )
				_anim_entry_profile_notification.SetTrigger(Close);
		}
	}
}