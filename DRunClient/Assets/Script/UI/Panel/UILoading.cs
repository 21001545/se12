using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	public class UILoading : UISingletonPanel<UILoading>
	{
		public RectTransform rtProgress;
		public TMP_Text txtProgress;

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);
			setProgress("", 0);
		}

		public void setProgressText(string text)
		{
			txtProgress.text = text;
		}

		public void setProgress(int progress)
		{
			//imgProgress.fillAmount = (float)progress / 100.0f;
			//rect_fox.anchoredPosition = new Vector2(rect_fox.anchoredPosition.x, rect_progressBg.rect.width * imgProgress.fillAmount);

			RectTransform rtParent = rtProgress.parent as RectTransform;
			Vector2 offsetMax = rtProgress.offsetMax;

			float ratio = (float)progress / 100.0f;

			offsetMax.x = -(1.0f - ratio) * rtParent.rect.width;

			rtProgress.offsetMax = offsetMax;
		}

		public void setProgress(string text,int progress)
		{
			setProgressText(text);
			setProgress(progress);
		}
	}
}
