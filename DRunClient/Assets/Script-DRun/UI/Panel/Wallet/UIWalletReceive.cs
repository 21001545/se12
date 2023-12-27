using Festa.Client;
using Festa.Client.Module.UI;
using TMPro;
using UnityEngine;

namespace DRun.Client
{
	public class UIWalletReceive : UISingletonPanel<UIWalletReceive>
	{
		public TMP_Text text_address;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			base.open(param, transitionType, closeType);

			setupUI();
		}

		private void setupUI()
		{
			string address = ViewModel.Wallet.Wallet.address;
			if (address.Length > 16)
			{
				string first = address.Substring(0, 8);
				string second = address.Substring(address.Length - 8, 8);
				address = $"{first}...{second}";
			}

			text_address.text = address;
		}

		public void onClick_OK()
		{
			string refStr = GlobalRefDataContainer.getStringCollection().get("wallet.octet.address_copy", 0) ?? "## Successfully Copied!";

			close();
			GUIUtility.systemCopyBuffer = ViewModel.Wallet.Wallet.address;

			UIToast.spawn(refStr, new(20, -704))
				.autoClose(3)
				.setType(UIToastType.check)
				.useBackdrop(false)
				.toggleTextWrap(false)
				.withTransition<SlideUpDownTransition>(t =>
					t.slideDirection = SlideUpDownTransition.SlideDirection.DownToUp);
		}

		public void onClick_copyAddress()
		{
			GUIUtility.systemCopyBuffer = ViewModel.Wallet.Wallet.address;
		}
	}
}
