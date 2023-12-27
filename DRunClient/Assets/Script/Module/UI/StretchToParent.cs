using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	[RequireComponent(typeof(RectTransform))]
	public class StretchToParent : MonoBehaviour
	{
		void Awake()
		{
			stretchNow();
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			stretchNow();
		}
#endif

		void stretchNow()
		{
			RectTransform rt = transform as RectTransform;
			RectTransform rtParent = transform.parent as RectTransform;

			rt.sizeDelta = rtParent.rect.size;
		}
	}
}
