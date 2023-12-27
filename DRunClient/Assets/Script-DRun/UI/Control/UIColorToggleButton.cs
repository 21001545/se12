using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIColorToggleButton : Button
	{
		[Header("ColorToggle")]
		public Color colorOn;
		public Color colorOff;

		public bool status;

		public Transform targetRoot;

		private List<TMP_Text> _textList;
		private List<Image> _imageList;

		private void prepare()
		{
			Transform root = targetRoot == null ? transform : targetRoot;

			if ( _textList == null)
			{
				_textList = new List<TMP_Text>();

				root.GetComponentsInChildren<TMP_Text>(true, _textList);
			}

			if( _imageList == null)
			{
				_imageList = new List<Image>();
				root.GetComponentsInChildren<Image>(true, _imageList);
			}
		}

		private void applyStatus()
		{
			prepare();

			foreach(TMP_Text text in _textList)
			{
				text.color = status ? colorOn : colorOff;
			}

			foreach(Image image in _imageList)
			{
				image.color = status ? colorOn : colorOff;
			}
		}

		public void setStatus(bool s)
		{
			status = s;
			applyStatus();
		}

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			base.OnValidate();

			_textList = null;
			_imageList = null;
			applyStatus();
		}
#endif
	}
}
