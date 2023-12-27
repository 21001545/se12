using EnhancedUI.EnhancedScroller;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	public class UIShop : UISingletonPanel<UIShop>, IEnhancedScrollerDelegate
	{
		[SerializeField]
		private TMP_Text txt_coin;

        [SerializeField]
        private TMP_Text txt_star;

        public EnhancedScroller scroller;

		public UIShopItem_Tree shopItemSource;

		private List<RefShopItem> _shopItemList;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();
			base.open(param, transitionType, closeType);
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				setupShopItems();
			}
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			var model = ClientMain.instance.getViewModel();
			TreeViewModel tree_vm = model.Tree;

			_bindingManager.makeBinding(tree_vm, nameof(tree_vm.TreeMap), onTreeMapUpdate);
			
			_bindingManager.makeBinding(model.Wallet, nameof(model.Wallet.FestaCoin),
                                                        txt_coin, nameof(txt_coin.text), UIUtil.convertCurrencyString);
            _bindingManager.makeBinding(model.Stature, nameof(model.Stature.Star),
											txt_star, nameof(txt_star.text), UIUtil.convertCurrencyString);

        }

        public void onClickBackNavigation()
        {
            ClientMain.instance.getPanelNavigationStack().pop();
        }

		private void onTreeMapUpdate(object obj)
		{
			scroller.RefreshActiveCellViews();
		}

		private void setupShopItems()
		{
			List<RefShopBoard> boardItemList = new List<RefShopBoard>();
			foreach(KeyValuePair<int,RefData.RefData> item in GlobalRefDataContainer.getInstance().getMap<RefShopBoard>())
			{
				RefShopBoard refBoardBoard = item.Value as RefShopBoard;
				boardItemList.Add(refBoardBoard);
			}

			boardItemList.Sort((a, b) => { 
				if( a.order < b.order)
				{
					return -1;
				}
				else if( a.order > b.order)
				{
					return 1;
				}

				return 0;
			});

			//
			_shopItemList = new List<RefShopItem>();
			foreach(RefShopBoard refShopBoard in boardItemList)
			{
				_shopItemList.Add(GlobalRefDataContainer.getInstance().get<RefShopItem>(refShopBoard.shop_item_id));
			}

			scroller.ReloadData();
		}

		#region scroller_delegate
		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller,int dataIndex,int cellIndex)
		{
			UIShopItem_Tree treeShopItem = scroller.GetCellView(shopItemSource) as UIShopItem_Tree;
			treeShopItem.setup(_shopItemList[dataIndex]);

			return treeShopItem;
		}

		public float GetCellViewSize(EnhancedScroller scroller,int dataIndex)
		{
			return 322;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _shopItemList.Count;
		}

		#endregion
	}
}
