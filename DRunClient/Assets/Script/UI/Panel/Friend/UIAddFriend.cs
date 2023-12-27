using EnhancedUI.EnhancedScroller;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class UIAddFriend : UISingletonPanel<UIAddFriend>, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;
		public UIAddFriend_CellView cellViewItemSource;
		public GameObject loading;

		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
			loading.SetActive(false);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();

			base.open(param, transitionType, closeType);

			// 스크롤 오류나는 문제로,, onTransitionEvent 으로 이동
			//loadData();
		}

		public override void onTransitionEvent(int type)
        {
            base.onTransitionEvent(type);
			if(type == TransitionEventType.end_open)
				loadData();
        }

        public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
		}

		private void loadData()
		{
			ClientFollowSuggestionConfig config = ViewModel.Social.SuggestionConfig;
			if( DateTime.UtcNow < config.next_update_time)
			{
				if(scroller.Container != null)
					scroller.ReloadData();
				return;
			}

			loading.SetActive(true);

			GetFollowSuggestionProcessor processor = GetFollowSuggestionProcessor.create();
			processor.run(() => {
				if (scroller.Container != null)
					scroller.ReloadData();

				loading.SetActive(false);				
			});
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			SocialViewModel vm = ViewModel.Social;
			//_bindingManager.makeBinding(vm, nameof(vm.SuggestionList), updateSuggestionList);
		}

		//private void updateSuggestionList(object obj)
		//{
		//	scroller.ReloadData();
		//}

		public void onClickSearch()
        {
            UIFindFriend.getInstance().open();

            ClientMain.instance.getPanelNavigationStack().push(this, UIFindFriend.getInstance());
        }

		public void onClickContact()
        {
            UIContactFriend.getInstance().open();

            ClientMain.instance.getPanelNavigationStack().push(this, UIContactFriend.getInstance());
        }

		private void onClickFollow(UIAddFriend_CellView cellView)
		{
			ClientFollowSuggestion suggestion = cellView.getData();

			UIBlockingInput.getInstance().open();

			MapPacket req = Network.createReq(CSMessageID.Social.FollowReq);
			req.put("id", suggestion.suggestion_id);

			Network.call(req, ack => {

				UIBlockingInput.getInstance().close();

				if( ack.getResult() == ResultCode.ok)
				{
					ViewModel.updateFromPacket(ack);

					ClientFollow newFollow = (ClientFollow)ack.get(MapPacketKey.ClientAck.follow);

					ViewModel.Social.SuggestionList.Remove(suggestion);
					scroller.ReloadData();
				}

			});
		}

		
		#region scroller_delegate

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			UIAddFriend_CellView item = scroller.GetCellView(cellViewItemSource) as UIAddFriend_CellView;

			ClientFollowSuggestion suggestion = ViewModel.Social.SuggestionList[dataIndex];
			item.setup(suggestion, onClickFollow);
			
			return item;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return 72f;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return ViewModel.Social.SuggestionList.Count;
		}

		#endregion
	}
}
