using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DRun.Client
{
	public class UIInputField : TMP_InputField
	{
		[Header("UIInputField")]
		[SerializeField]
		private RectTransform btn_clear;

		[SerializeField]
		private Gradient caret_gradient;

		public void eventOnValueChanged(string value)
		{
			if( btn_clear != null)
			{
				btn_clear.gameObject.SetActive(String.IsNullOrEmpty(value) == false);
			}
		}

		public void hideClearButton()
		{
			if( btn_clear != null)
			{
				btn_clear.gameObject.SetActive(false);
			}
		}

		public override void OnPointerClick(PointerEventData eventData)
		{
			base.OnPointerClick(eventData);

			if( btn_clear != null && RectTransformUtility.RectangleContainsScreenPoint(btn_clear,eventData.position,Camera.main))
			{
				this.text = "";
			}
		}

		//// 2022.09.14 이강희
		//protected override GameObject createCaret()
		//{
		//	GameObject go = base.createCaret();

		//	if( caret_gradient != null)
		//	{
		//		UIGradientEffect effect = go.AddComponent<UIGradientEffect>();
		//		effect.gradient = caret_gradient;
		//	}

		//	return go;
		//}

		protected override void setupCaretCursorVertsColor()
		{
			if( caret_gradient != null)
			{
				m_CursorVerts[1].color = caret_gradient.Evaluate(0);
				m_CursorVerts[2].color = caret_gradient.Evaluate(0);
				m_CursorVerts[0].color = caret_gradient.Evaluate(1);
				m_CursorVerts[3].color = caret_gradient.Evaluate(1);
			}
			else
			{
				base.setupCaretCursorVertsColor();
			}
		}



	}
}
