using Assets.Script_DRun.Logic.Account;
using DRun.Client.Module;
using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using System.Collections.Generic;
using TMPro;

namespace DRun.Client
{
	public class UIConfirmInvitationReward : UISingletonPanel<UIConfirmInvitationReward>
	{
		public TMP_Text txtDesc;
		public TMP_Text txtClaim;
		public UILoadingButton btnConfirm;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		private List<ClientDRNTransaction> _incompleteList;

		public void open(List<ClientDRNTransaction> incompleteList)
		{
			base.open();

			_incompleteList = incompleteList;
			setupUI();
		}

		private void setupUI()
		{
			txtDesc.text = StringCollection.getFormat("invitation.confirm.claim.desc", 0, _incompleteList.Count);

			double send_reward = GlobalRefDataContainer.getConfigDouble(RefConfig.Key.DRun.invitation_send_reward, 3);

			txtClaim.text = StringCollection.getFormat("invitation.confirm.claim.button", 0, send_reward * _incompleteList.Count);
		}

		public void onClickConfirm()
		{
			btnConfirm.beginLoading();

			ProcessIncopmleteInvitationRewardProcessor step = ProcessIncopmleteInvitationRewardProcessor.create(_incompleteList);
			step.run(result =>
			{
				btnConfirm.endLoading();
				close();

				ClientMain.instance.getViewModel().Wallet.NotifyAccumulatedDRNChange();
			});
		}

	}
}
