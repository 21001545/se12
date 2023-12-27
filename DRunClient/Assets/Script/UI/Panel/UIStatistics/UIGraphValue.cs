using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	public class UIGraphValue : ReusableMonoBehaviour
	{
		public TMP_Text			txt_label;
		public TMP_Text			txt_value;
		public RectTransform	rt_bar;
		public float			bar_max_height;

		public override void onCreated(ReusableMonoBehaviour source)
		{

		}

		public void set(string label,int value,int max_value)
		{
			txt_label.text = label;
			txt_value.text = value.ToString("N0");

			if( max_value == 0)
			{
				max_value = 1;
			}

			float bar_height = (float)value * bar_max_height / (float)max_value;
			
			rt_bar.sizeDelta = new Vector2(rt_bar.sizeDelta.x, bar_height);
		}
	}
}
