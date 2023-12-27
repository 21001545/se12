using DRun.Client.Module;
using Festa.Client;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIClaimDailyReward : UISingletonPanel<UIClaimDailyReward>
	{
		public TMP_Text text_title;
		public ParticleSystem particle;
		private System.Action _onClose_ClaimReward;
		
		public void open(long boxReward, System.Action onClose_ClaimReward)
		{
			base.open();
			
			_onClose_ClaimReward = onClose_ClaimReward;
			text_title.text = GlobalRefDataContainer.getStringCollection().getFormat("basic.dailyrewards.claim.title", 0, StringUtil.toDRNStringDefault(boxReward));
		}

		public override void onTransitionEvent(int type)
		{
			if (type == TransitionEventType.end_open)
			{
				particle.Play();
			}
		}

		public void onClick_Back()
		{
			close();
			_onClose_ClaimReward?.Invoke();
		}

		public void onClick_Confirm()
		{
			close();
			_onClose_ClaimReward?.Invoke();
		}
	}
}
