using Festa.Client.Module;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UITripStatus_TypeItem : ReusableMonoBehaviour
	{
		public Image	image_icon;
		public TMP_Text txt_label;

		private UnityAction _callback;

		public override void onCreated(ReusableMonoBehaviour source)
		{
			
		}

		public override void onReused()
		{
			
		}

		public void setup(Sprite sprite,string label,UnityAction callback)
		{
			image_icon.sprite = sprite;
			txt_label.text = label;
			_callback = callback;
		}

		public void onClick()
		{
			_callback();
		}
	}
}
