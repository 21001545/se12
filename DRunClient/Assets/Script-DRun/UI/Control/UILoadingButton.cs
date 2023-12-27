using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UILoadingButton : Button
	{
		private static int _key_loading = Animator.StringToHash("loading");

		public void beginLoading()
		{
			animator.SetFloat(_key_loading, 1.0f);
			interactable = false;
		}

		public void endLoading()
		{
			animator.SetFloat(_key_loading, 0.0f);
			interactable = true;
		}

		//public override void OnPointerDown(PointerEventData eventData)
		//{
		//	base.OnPointerDown(eventData);

		//	Debug.Log($"OnPointerDown:ID[{gameObject.GetInstanceID()}]");
		//}

		//public override void OnPointerClick(PointerEventData eventData)
		//{
		//	base.OnPointerClick(eventData);

		//	Debug.Log($"OnPointerClick:ID[{gameObject.GetInstanceID()}]");
		//}

		//public override void OnPointerUp(PointerEventData eventData)
		//{
		//	base.OnPointerUp(eventData);

		//	Debug.Log($"OnPointerUp:ID[{gameObject.GetInstanceID()}]");
		//}
	}
}
