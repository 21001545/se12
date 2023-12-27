using DRun.Client.ViewModel;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIInvitationCode_MyCode_Control : UIInvitationCode_MyCode_CellBase
	{
		public TMP_Text txtTitle;
		public TMP_Text txtDesc;
		public TMP_Text txtCode;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public override void setup(UIInvitationCode.CellItem cellItem)
		{
			EventViewModel vm = ClientMain.instance.getViewModel().Event;

			double send_reward = GlobalRefDataContainer.getConfigDouble(RefConfig.Key.DRun.invitation_send_reward, 3);

			txtTitle.text = StringCollection.getFormat("invitation.mycode.title", 0, send_reward);
			txtDesc.text = StringCollection.getFormat("invitation.mycode.desc", 0, send_reward);

			txtCode.text = vm.InvitationCode.code;
		}

		public void onClick_CopyCode()
		{
			GUIUtility.systemCopyBuffer = txtCode.text;
			// 좀 더 길게 눌러야 정지 됩니다: 팝업 띄우기.
			UIToast.spawn(
					StringCollection.get("invitation.input.copy_complete", 0),
					new(20, -606))
				.setType(UIToastType.normal)
				.withTransition<FadePanelTransition>()
				.autoClose(3.0f);
		}
	}
}
