using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DRun.Client
{
	[RequireComponent(typeof(Animator))]
	public class UISwitchButton : Selectable, IPointerClickHandler
	{
		public bool status;
		public RectTransform rtLeft;
		public RectTransform rtRight;

		[Serializable]
		public class StatusChangeEvent : UnityEvent<bool> { }

		[SerializeField]
		private StatusChangeEvent onStatusChanged = new StatusChangeEvent();

		private static int anim_left = Animator.StringToHash("anim_left");
		private static int anim_right = Animator.StringToHash("anim_right");

		public virtual void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			if( RectTransformUtility.RectangleContainsScreenPoint(rtLeft, eventData.position, Camera.main))
			{
				setState(false, false);
			}
			else if( RectTransformUtility.RectangleContainsScreenPoint(rtRight, eventData.position, Camera.main))
			{
				setState(true, false);
			}
		}

		public void setState(bool newStatus,bool immediate)
		{
			if( status == newStatus)
			{
				return;
			}

			status = newStatus;
			applyStatus(immediate);
			onStatusChanged.Invoke(status);
		}

		private void applyStatus(bool immediate)
		{
			if( status == false)
			{
				animator.Play(anim_left, -1, immediate ? 1 : 0);
			}
			else
			{
				animator.Play(anim_right, -1, immediate ? 1 : 0);
			}
		}
	}
}
