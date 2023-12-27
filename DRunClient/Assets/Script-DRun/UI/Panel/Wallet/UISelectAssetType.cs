using DRun.Client.NetData;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace DRun.Client
{
	public class UISelectAssetType : UISingletonPanel<UISelectAssetType>
	{
		public TMP_Text text_title;
		public UISelectAssetType_Item itemSource;
		public Transform optionRoot;

		private int _currentAssetType;
		private UnityAction<SelectResult> _callback;
		private List<UISelectAssetType_Item> _itemList;

		public class SelectResult
		{
			public int assetType;
			public int tokenID;

			public static SelectResult create(int asset_type)
			{
				SelectResult result = new SelectResult();
				result.assetType = asset_type;
				result.tokenID = 0;
				return result;
			}

			public static SelectResult create(int asset_type,int token_id)
			{
				SelectResult result = new SelectResult();
				result.assetType = asset_type;
				result.tokenID = token_id;
				return result;
			}
		}

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);
			itemSource.gameObject.SetActive(false);
			_itemList = new List<UISelectAssetType_Item>();
		}

		public void open(string title,int currentAssetType,List<int> assetList,UnityAction<SelectResult> callback)
		{
			base.open();

			text_title.text = title;
			_callback = callback;
			_currentAssetType = currentAssetType;
			buildOptionList(assetList);
			updateSelect();
		}

		private void buildOptionList(List<int> assetList)
		{
			GameObjectCache.getInstance().delete(_itemList);

			foreach(int assetType in assetList)
			{
				UISelectAssetType_Item item = GameObjectCache.getInstance().make(itemSource, optionRoot, GameObjectCacheType.ui);
				item.setup(assetType);

				_itemList.Add(item);
			}
		}

		private void updateSelect()
		{
			foreach(UISelectAssetType_Item item in _itemList)
			{
				item.setSelect(item.getAssetType() == _currentAssetType);
			}
		}

		public void onClickItem(int assetType)
		{
			_currentAssetType = assetType;

			if( assetType == ClientWalletBalance.AssetType.NFT)
			{
				UISelectWithdrawNFTItem.getInstance().open(_callback);
			}
			else
			{
				_callback( SelectResult.create( _currentAssetType));
				base.close();
			}
		}

		public void onClick_Cancel()
		{
			base.close();
		}
	}
}
