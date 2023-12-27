using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIStopButton : UIBehaviour, IPointerDownHandler, IPointerUpHandler
	{
		public UICircleLine gauge;
		public float clickDuration;

		[Serializable]
		public class ClickEvent : UnityEvent<bool>{	}

		[SerializeField]
		private ClickEvent onClick = new ClickEvent();

		private bool _isDown = false;
		private float _downTime = 0;

		public void OnPointerDown(PointerEventData eventData)
		{
			_isDown = true;
			_downTime = 0;
			gauge.setFillAmount(0);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			RectTransform rt = transform as RectTransform;
			if( _isDown && RectTransformUtility.RectangleContainsScreenPoint(rt, eventData.position, Camera.main))
			{
				float ratio = _downTime / clickDuration;

				onClick.Invoke(ratio >= 1.0f);
			}

			_isDown = false;
			gauge.setFillAmount(0);
		}

		private void LateUpdate()
		{
			if( _isDown)
			{
				_downTime += Time.deltaTime;

				float ratio = _downTime / clickDuration;
				ratio = Mathf.Clamp(ratio, 0, 1);
				gauge.setFillAmount(ratio);
			}
		}
	}
}
