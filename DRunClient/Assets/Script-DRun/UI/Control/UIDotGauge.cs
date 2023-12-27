using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIDotGauge : MonoBehaviour
	{
		public RectTransform dotRoot;
		public RectTransform indicator;

		public Color colorOn;
		public Color colorOff;
		public Color colorIndicatorOverlapped = new Color(1, 1, 1, 0);

		[Range(0, 1)]
		public float gauge; 

		private List<Image> _dotList;

		public void setGauge(float gauge)
		{
			prepare();

			applyDots(applyIndicatorPosition(gauge));
		}

		private void prepare()
		{
			if( _dotList != null)
			{
				return;
			}

			_dotList = new List<Image>();
			dotRoot.GetComponentsInChildren<Image>(true, _dotList);
		}

		private Vector2 applyIndicatorPosition(float gauge)
		{
			Rect parentRect = (indicator.parent as RectTransform).rect;
			Rect indicatorRect = indicator.rect;

			Vector2 position = Vector2.zero;
			position.x = (parentRect.width - indicatorRect.width) * gauge;

			indicator.anchoredPosition = position;

			return new Vector2(position.x, position.x + indicatorRect.width);
		}

		private void applyDots(Vector2 indicatorPos)
		{
			// 2022.12.06 indicator랑 겹치는 애는 않보이게 해주자

			for(int i = 0; i < _dotList.Count; ++i)
			{
				Image image = _dotList[i];

				Vector2 imagePosition = image.rectTransform.anchoredPosition;

				if(imagePosition.x < indicatorPos.x )
				{
					image.color = colorOn;
				}
				else if( imagePosition.x > indicatorPos.y)
				{
					image.color = colorOff;
				}
				else
				{
					image.color = colorIndicatorOverlapped;
				}
			}
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			_dotList = null;
			if( dotRoot == null || indicator == null)
			{
				return;
			}

			setGauge(gauge);
		}
#endif
	}
}
