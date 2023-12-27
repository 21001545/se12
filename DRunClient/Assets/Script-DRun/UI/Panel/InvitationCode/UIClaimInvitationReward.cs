using DRun.Client.Module;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIClaimInvitationReward : UISingletonPanel<UIClaimInvitationReward>
	{
		public Gradient particle_gradient;
		public ParticleSystem particle;
		public TMP_Text txtTitle;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public void open(long rewardAmount)
		{
			base.open();

			setupUI(rewardAmount);
		}

		private void setupUI(long rewardAmount)
		{
			txtTitle.text = StringCollection.getFormat("invitation.claim.title", 0, StringUtil.toDRN(rewardAmount));
			var particleMain = particle.main;
			particleMain.startColor = new ParticleSystem.MinMaxGradient(particle_gradient);
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.end_open)
			{
				particle.Play();
			}
		}

		public void onClick_Confirm()
		{
			close();
		}
	}
}
