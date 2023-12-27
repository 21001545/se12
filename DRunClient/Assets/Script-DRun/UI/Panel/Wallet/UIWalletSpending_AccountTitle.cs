using UnityEngine;

namespace DRun.Client
{
	public class UIWalletSpending_AccountTitle : UIWalletScrollerCellView
	{
		public Animator tooltipAnimator;

		public void onClick_Tooltip()
		{
			tooltipAnimator.SetTrigger("show");
		}
	}
}
