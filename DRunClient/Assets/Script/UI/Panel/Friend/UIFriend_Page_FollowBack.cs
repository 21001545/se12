using EnhancedUI.EnhancedScroller;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Festa.Client
{
	public class UIFriend_Page_FollowBack : MonoBehaviour, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;
		public UIFriend_FollowBackCellView cellViewItemSource;
		public UIFriend_listTopCellView listTopCellView;
		public GameObject loading;
		public CanvasGroup load_more_top;
		public CanvasGroup load_more_bottom;

		[SerializeField]
		private GameObject go_blankPage;
		[SerializeField]
        private GameObject go_search;
        [SerializeField]
        private GameObject go_addSlot;

		private float _cellSize = 72f;

		private SocialViewModel ViewModel => UIFriend.getInstance().SocialViewModel;
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
		private bool _dragging;
		private bool _loadingMore;
		private int _loadingMoreType;

		void Start()
        {

            scroller.Delegate = this;
            scroller.scrollerScrolled = ScrollerScrolled;
            _dragging = false;
            _loadingMore = false;
            _loadingMoreType = 0;

            load_more_top.alpha = 0;
            load_more_bottom.alpha = 0;
        }

		public void open()
		{
			ViewModel.FollowBackList.Clear();
            loadData(false);
		}

		private void loadData(bool is_more)
		{
			loadData_FollowBackList(is_more);
		}

		private void loadData_FollowBackList(bool is_more)
		{
			if (is_more == false && ViewModel.FollowBackList.Count > 0)
			{
				reloadScrollerData(is_more);
				return;
			}

			loading.SetActive(true);
			Vector2Int range = ViewModel.getFollowBackNextPage();

			QueryFollowBackProcessor processor = QueryFollowBackProcessor.create( UIFriend.getInstance().getTargetAccountID(), range, ViewModel);
			processor.run(result => {
				_loadingMore = false;

				loading.SetActive(false);
				reloadScrollerData(is_more);
			});
		}

		private void reloadScrollerData(bool keepScrollPosition)
		{
			//2022.02.07 이강희 Crash버그 수정
			if ( scroller.Container == null)
			{
				return;
			}

			float prev_pos = scroller.ScrollPosition;
			scroller.ReloadData();

			if (keepScrollPosition)
			{
				scroller.ScrollPosition = prev_pos;
			}

			setupBlankPage();
            updateFriendCount();
        }

        public void updateFriendCount()
        {
            // 현재 친구수 / 최대 수는 여기서 임시로 셋팅하자.
            int maxCount = ViewModel.getMaxFriendCount();
			listTopCellView.setup(ViewModel.FollowCumulation.follow_back_count, maxCount, false, GlobalRefDataContainer.getStringCollection().get("friends.order.default", 0));
        }

		private void setupBlankPage()
		{
			go_blankPage.gameObject.SetActive(ViewModel.FollowBackList.Count == 0 ? true : false);
		}

		#region scroller_delegate
		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			if(dataIndex == 0)
            {
				return scroller.GetCellView(listTopCellView) as UIFriend_listTopCellView;
            }
            else
            {
				--dataIndex;
				ClientFollowBack data = ViewModel.FollowBackList[dataIndex];
				UIFriend_FollowBackCellView item = scroller.GetCellView(cellViewItemSource) as UIFriend_FollowBackCellView;
				item.setup(data);

				return item;
			}
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			if (dataIndex == 0)
				return 44f;
			else
				return _cellSize;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return Math.Max(1, ViewModel.FollowBackList.Count + 1);
		}

		private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
		{
			_loadingMoreType = 0;

			if ((scroller.NormalizedScrollPosition <= 0.0f ||
						scroller.NormalizedScrollPosition >= 1.0f) && _dragging)
			{
				float pos_y = scroller.ScrollRect.content.anchoredPosition.y;
				float alpha = 0;

				if (pos_y < 0)
				{
					alpha = Mathf.Clamp(Mathf.Abs(pos_y) / _cellSize, 0, 1);
					if (alpha >= 1.0f)
					{
						_loadingMoreType = 1;
					}

					load_more_bottom.alpha = 0.0f;
					load_more_top.alpha = alpha;
				}
				else
				{
					float delta = pos_y - scroller.ScrollSize;
					alpha = Mathf.Clamp(delta / _cellSize, 0, 1);
					if (alpha >= 1.0f)
					{
						_loadingMoreType = 2;
					}

					load_more_bottom.alpha = alpha;
					load_more_top.alpha = 0.0f;
				}
			}
			else
			{
				load_more_bottom.alpha = 0.0f;
				load_more_top.alpha = 0.0f;
			}

			//if(scroller.NormalizedScrollPosition >= 1.0f && !_loadingNew)
			//{
			//	_loadingNew = true;

			//	loadMoreData();
			//}
		}

		public void OnBeginDrag(BaseEventData e)
		{
			_dragging = true;
		}

		public void OnEndDrag(BaseEventData e)
		{
			_dragging = false;

			if (_loadingMoreType == 2 && _loadingMore == false)
			{
				_loadingMore = true;
				loadData(true);
			}
		}
		#endregion
	}
}
