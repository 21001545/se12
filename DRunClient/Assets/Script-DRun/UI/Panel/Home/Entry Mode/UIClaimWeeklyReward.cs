using DRun.Client.Module;
using DRun.Client.RefData;
using DRun.Client.ViewModel;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIClaimWeeklyReward : UISingletonPanel<UIClaimWeeklyReward>
	{
		[SerializeField]
		private Gradient _particle_gradient;

		public ParticleSystem particle;
		public TMP_Text text_title;
		public TMP_Text text_desc;

		[SerializeField]
		private TMP_Text _text_bonus;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public void open(ClaimedWeeklyRewardData data)
		{
			base.open();

			setupUI(data);
		}

		private void setupUI(ClaimedWeeklyRewardData data)
		{
			RefEntryLevel refLevel = GlobalRefDataContainer.getInstance().get<RefEntryLevel>(ViewModel.BasicMode.LevelData.level);

			string bonus_text = refLevel.bonus_percent.ToString();
			string reward_text = StringUtil.toDRNStringAll(data.reward_amount);

			text_title.text = StringCollection.getFormat("basic.weeklyrewards.claim.title", 0, reward_text);
			text_desc.text = StringCollection.get("basic.weeklyrewards.claim.desc", 0);
			
			_text_bonus.text =
				StringCollection.getFormat("basic.weeklyrewards.claim.desc", 1, refLevel.level, bonus_text);
			

			// 파티클 Gradient StartColor 설정.
			var particleMain = particle.main;
			particleMain.startColor = new ParticleSystem.MinMaxGradient(_particle_gradient);
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.end_open)
			{
				particle.Play();
			}
		}

		public void onClick_Back()
		{
			close();
		}

		public void onClick_Confirm()
		{
			close();
		}
	}
}
