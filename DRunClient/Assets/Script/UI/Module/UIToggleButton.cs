using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Festa.Client
{
	public class UIToggleButton : UnityEngine.UI.Button
	{
		public Sprite[] stateSprites;
		public Color[] stateTextColors;

		private bool _isOn;

		public bool IsOn
		{
			get
			{
				return _isOn;
			}
			set
			{
				_isOn = value;
				setupToggle();
			}
		}

		private void setupToggle()
		{
			if (stateSprites == null || stateSprites.Length <= 0)
			{
				Debug.LogWarning("stateSprites is empty", gameObject);
				return;
			}

			// 이미지 종류
			if (targetGraphic is Image)
			{
				Image image = targetGraphic as Image;
				image.sprite = stateSprites[_isOn ? 1 : 0];
			}

			// 글씨색
			if (stateTextColors != null && stateTextColors.Length > 1)
			{
				TMP_Text text = gameObject.GetComponentInChildren<TextMeshPro>();

				if (text != null)
					text.color = stateTextColors[_isOn ? 1 : 0];
			}
		}

	}
}
