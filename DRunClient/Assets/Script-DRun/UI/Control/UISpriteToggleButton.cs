using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	[RequireComponent(typeof(Image))]
	public class UISpriteToggleButton : Button
	{
		[Header("SpriteToggle")]
		public Sprite spriteOn;
		public Sprite spriteOff;

		public bool status;

		private Image _image;

		private void prepare()
		{
			if( _image == null)
			{
				_image = GetComponent<Image>();
			}
		}

		private void applyStatus()
		{
			prepare();
			_image.sprite = status ? spriteOn : spriteOff;
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

			_image = null;
			applyStatus();
		}
#endif
	}
}
