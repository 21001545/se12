using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace DRun.Client
{
	public class UIOffsetMaxGauge : MonoBehaviour
	{
		public RectTransform gauge;

		[Range(0,1)]
		[SerializeField]
		private float _gauge;

		private bool _updated;

		public float Gauge
		{
			get
			{
				return _gauge;
			}
			set
			{
				_gauge = value;
				_updated = true;
			}
		}

		private void applyGauge()
		{
			RectTransform rtParent = gauge.parent as RectTransform;
			Vector2 offsetMax = gauge.offsetMax;
			offsetMax.x = -(1.0f - _gauge) * rtParent.rect.width;// + 3;
			gauge.offsetMax = offsetMax;
		}

		private void LateUpdate()
		{
			if( _updated)
			{
				_updated = false;
				applyGauge();
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			if (gauge == null)
				return;

			_updated = true;
		}
#endif

	}
}
