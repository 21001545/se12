using DRun.Client.NetData;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.RefData;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UISelectAssetType_Item : ReusableMonoBehaviour
	{
		public Image image_selected;
		public Image image_icon;
		public TMP_Text text_name;

		public Sprite[] iconSprites;

		private int _assetType;
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();

		public int getAssetType()
		{
			return _assetType;
		}

		public void setup(int assetType)
		{
			_assetType = assetType;
			if (assetType == ClientWalletBalance.AssetType.ETH)
			{
				text_name.text = StringCollection.get("wallet.asset.eth", 0);
				image_icon.sprite = iconSprites[0];
			}
			else if (assetType == ClientWalletBalance.AssetType.DRNT)
			{
				text_name.text = StringCollection.get("wallet.asset.drnt", 0);
				image_icon.sprite = iconSprites[1];
			}
			else if(assetType == ClientWalletBalance.AssetType.NFT)
			{
				text_name.text = StringCollection.get("wallet.asset.nft", 0);
				image_icon.sprite = iconSprites[2];
			}
		}

		public void setSelect(bool b)
		{
			image_selected.gameObject.SetActive(b);
		}

		public void onClick()
		{
			UISelectAssetType.getInstance().onClickItem(_assetType);
		}
	}
}
