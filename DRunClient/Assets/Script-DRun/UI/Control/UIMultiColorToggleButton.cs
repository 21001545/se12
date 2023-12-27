using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIMultiColorToggleButton : Button
	{
		[Serializable]
		public class StateData
		{
			public Image image;
			public TMP_Text text;
			public Color[] statColors;
		}

		public StateData[] dataList;
		public bool status;

		private void applyStatus()
		{
			foreach (StateData data in dataList)
			{
				if (data == null || data.statColors == null || data.statColors.Length == 0)
				{
					continue;
				}
			
				Color color = data.statColors[status ? 1 : 0];

				if( data.image != null)
				{
					data.image.color = color;
				}

				if (data.text != null)
				{
					data.text.color = color;
				}
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
			applyStatus();
		}
#endif

	}
}
