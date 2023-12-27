using DRun.Client.Module;
using DRun.Client.NetData;
using TMPro;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIWalletSpending_Balance : UIWalletScrollerCellView
	{
		public Image icon;
		public TMP_Text assetName;
		public TMP_Text balance;

		public void setup(ClientDRNBalance drn_balance)
		{
			balance.text = StringUtil.toDRNStringDefault(drn_balance.balance);
		}
	}
}
