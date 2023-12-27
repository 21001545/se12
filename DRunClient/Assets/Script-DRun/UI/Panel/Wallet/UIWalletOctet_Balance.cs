using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIWalletOctet_Balance : UIWalletScrollerCellView
	{
		public Image icon;
		public TMP_Text assetName;
		public TMP_Text balance;

		public Sprite[] assetSprites;

		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public void setup(ClientWalletBalance data)
		{
			if (data.asset_type == ClientWalletBalance.AssetType.ETH)
			{
				icon.sprite = assetSprites[0];
				assetName.text = StringCollection.get("wallet.asset.eth", 0);
			}
			else
			{
				icon.sprite = assetSprites[1];
				assetName.text = StringCollection.get("wallet.asset.drnt", 0);
			}

			balance.text = data.getDisplayLiquid();
		}
	}
}
