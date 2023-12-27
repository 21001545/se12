using DRun.Client.Module;
using EnhancedUI.EnhancedScroller;
using Festa.Client;
using Festa.Client.Module;
using Festa.Client.Module.UI;
using Festa.Client.ViewModel;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

namespace DRun.Client
{
	public class UIEvent : UISingletonPanel<UIEvent>, IEnhancedScrollerDelegate
	{
		public TMP_Text txt_drnAmount;
		public Button btn_profile;

		public EnhancedScroller scroller;
		public UIEvent_Item cellItem;
		
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public class EventData
		{
			public int event_id;
		}

		private List<EventData> _eventList;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = 2)
		{
			resetBindings();
			base.open(param, transitionType, closeType);
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			WalletViewModel walletVM = ViewModel.Wallet;
			_bindingManager.makeBinding(walletVM, nameof(walletVM.DRN_Balance), onUpdateDRNBalance);
		}

		private void onUpdateDRNBalance(object obj)
		{
			txt_drnAmount.text = StringUtil.toDRN(ViewModel.Wallet.DRN_Balance.balance).ToString("F2");
		}
		
		public void onClick_Setting()
		{
			UISetting.getInstance().open();
		}

		public override void onTransitionEvent(int type)
		{
			if( type == TransitionEventType.start_open)
			{
				buildEventList();
			}
		}

		private void buildEventList()
		{
			EventData e = new EventData();
			e.event_id = 1;

			_eventList = new List<EventData>();
			_eventList.Add( e);
		}

		#region scroller

		EnhancedScrollerCellView IEnhancedScrollerDelegate.GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UIEvent_Item cell = (UIEvent_Item)scroller.GetCellView(cellItem);
			cell.setup(_eventList[dataIndex]);
			return cell;
		}

		float IEnhancedScrollerDelegate.GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return cellItem.height;
		}

		int IEnhancedScrollerDelegate.GetNumberOfCells(EnhancedScroller scroller)
		{
			return _eventList.Count;
		}

		#endregion
	}
}
