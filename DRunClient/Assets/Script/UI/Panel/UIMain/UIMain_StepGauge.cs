using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIMain_StepGauge : MonoBehaviour
	{
		public Image	image_gauge;
		public TMP_Text txt_value;

		public Vector2 gauge_range = new Vector2( 0, 1);

		private RectTransform _rt_gauge;
		private RectTransform _rt_value;
		private FloatSmoothDamper _damper;

		public void init()
		{
			_rt_gauge = image_gauge.rectTransform;
			_rt_value = txt_value.rectTransform;
			_damper = FloatSmoothDamper.create(0, 1.0f, 0.005f);
		}

		public void setGauge(float ratio)
		{
			_damper.setTarget(ratio);

		}

		public void update()
		{
			if( _damper.update())
			{
				float value = _damper.getCurrent();

				float length = gauge_range.y - gauge_range.x;
				float real_ratio = gauge_range.x + value * length;

				image_gauge.fillAmount = real_ratio;
				txt_value.text = string.Format("{0}%", (int)(value * 100));

				Vector2 pos = _rt_value.anchoredPosition;
				pos.y = _rt_gauge.rect.height * real_ratio;
				_rt_value.anchoredPosition = pos;
			}
		}
	}
}
