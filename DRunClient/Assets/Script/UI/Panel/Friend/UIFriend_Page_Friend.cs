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
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIFriend_Page_Friend : MonoBehaviour, IEnhancedScrollerDelegate
	{
		public EnhancedScroller scroller;
		public UIFriend_FriendCellView cellViewItemSource;
		public UIFriend_listTopCellView listTopCellView;
		public UIFriend_addSlotCellView addSlotCellView;
		public UIFriend_searchFriendCellView searchFriendCellView;

		public GameObject loading;
		public CanvasGroup load_more_top;
		public CanvasGroup load_more_bottom;

		[SerializeField] 
		private TMP_Text txt_friendCount;
        [SerializeField]
        private Animator animator;
		[SerializeField]
		private Sprite[] img_crowns;
		[SerializeField]
		private GameObject go_blankPage;

		private float _cellSize = 72f;

		private SocialViewModel ViewModel => UIFriend.getInstance().SocialViewModel;
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
		private bool _dragging;
		private bool _loadingMore;
		private int _loadingMoreType;

		class CellType
		{
			public static readonly int topCell = 0;
			public static readonly int friendCell = 1;
			public static readonly int addSlot = 2;
			public static readonly int searchFriend = 3;
		}

		class CellBase
		{
			public float height;
			public int type;
			public CellBase(float height, int type)
			{
				this.height = height;
				this.type = type;
			}
		};
		class topCell : CellBase
		{
			public int friendCount;
			public int maxCount;

			public topCell(int count, int max) : base(44f, CellType.topCell)
			{
				friendCount = count;
				maxCount = max;
			}
		};

		class friendCell : CellBase
		{
			public ClientFollow data;
			public Sprite crown;
			public friendCell(ClientFollow data, float height, Sprite crown) : base(height, CellType.friendCell)
			{
				this.data = data;
				this.crown = crown;
			}
		};

		class addSlot : CellBase
        {
			public addSlot() : base(165f, CellType.addSlot)
            {
            }
        }

		class searchFriend : CellBase
		{
			public searchFriend() : base(165f, CellType.searchFriend)
			{
			}
		}

		private List<CellBase> _data = new List<CellBase>();

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
			ViewModel.FollowList.Clear();
			loadData(false);
		}

		private void loadData(bool is_more)
		{
			loadData_FollowList(is_more);
		}

		private void loadData_FollowList(bool is_more)
		{
			if( is_more == false && ViewModel.FollowList.Count > 0)
			{
				reloadScrollerData(is_more);
				return;
			}

			loading.SetActive(true);
			Vector2Int range = ViewModel.getFollowNextPage();
			QueryFollowProcessor processor = QueryFollowProcessor.create(UIFriend.getInstance().getTargetAccountID(),range, ViewModel);
			processor.run(result => {
				_loadingMore = false;

				loading.SetActive(false);
				reloadScrollerData(is_more);
			});
		}

		private void reloadScrollerData(bool keepScrollPosition)
		{
			float prev_pos = scroller.ScrollPosition;
			setupCellViewList();
			if(scroller.Container != null)
				scroller.ReloadData();

			if( keepScrollPosition)
			{
				scroller.ScrollPosition = prev_pos;
			}

			setupBlankPage();
		}

		private void setupCellViewList()
        {
			_data.Clear();

			// 현재 친구수 / 최대 수는 여기서 임시로 셋팅하자.
			int maxCount = ViewModel.getMaxFriendCount();
			_data.Add(new topCell(ViewModel.FollowCumulation.follow_count, maxCount));

			for (int i = 0; i < ViewModel.FollowList.Count; ++i)
            {
				if (i < 3)
					_data.Add(new friendCell(ViewModel.FollowList[i], _cellSize, img_crowns[i]));
				else
					_data.Add(new friendCell(ViewModel.FollowList[i], _cellSize, null));
			}

			if(UIFriend.getInstance().isMyAccount())
            {
				if (ViewModel.FollowList.Count > 0)
				{
                    // 일단 테스트용으로,, 최대 친구 수 - 2 보다 크면 add slot; 아니면 search friend
                    if (maxCount - 2 < ViewModel.FollowList.Count)
                        _data.Add(new addSlot());
                    else
                        _data.Add(new searchFriend());
                }
			}
        }

		private void setupBlankPage()
		{
			go_blankPage.gameObject.SetActive(ViewModel.FollowList.Count == 0 ? true : false);
		}

		public void onClickSendChat(UIFriend_FriendCellView cellView)
		{
			//int target_account_id = cellView.getFollowData().follow_id;
			//string message = "test" + Time.realtimeSinceStartup.ToString();

			//SendDirectMessageProcessor processor = SendDirectMessageProcessor.create(target_account_id, message);
			//processor.run(result => { });
		}

		public void onClickMore(UIFriend_FriendCellView cellView)
		{
			ClientFollow follow = cellView.getFollowData();
			string message = StringCollection.getFormat("friend.unfollow.popup.message", 0, follow._profileCache?.Profile.name);

			UIPopup.spawnYesNo(message, () => {
				unfollow(follow);
			});
		}

		private void unfollow(ClientFollow follow)
		{
			if( UIFriend.getInstance().isMyAccount() == false)
			{
				return;
			}

			MapPacket req = Network.createReq(CSMessageID.Social.UnfollowReq);
			req.put("id", follow.follow_id);

			UIBlockingInput.getInstance().open();

			Network.call(req, ack => {

				UIBlockingInput.getInstance().close();

				if (ack.getResult() == ResultCode.ok)
				{
					ClientMain.instance.getViewModel().updateFromPacket(ack);

					ViewModel.FollowList.Remove(follow);
					reloadScrollerData(true);
				}

			});
		}

		#region scroller_delegate

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller,int dataIndex,int cellIndex)
		{
			var cellData = _data[dataIndex];

			if (cellData.type == CellType.topCell)
            {
				var topData = cellData as topCell;
				UIFriend_listTopCellView item = scroller.GetCellView(listTopCellView) as UIFriend_listTopCellView;
				item.setup(topData.friendCount, topData.maxCount, true, GlobalRefDataContainer.getStringCollection().get("friends.order.socialscore", 0));
				return item;
            }
			else if (cellData.type == CellType.friendCell)
            {
				var friendData = cellData as friendCell;
				UIFriend_FriendCellView item = scroller.GetCellView(cellViewItemSource) as UIFriend_FriendCellView;
				item.setup(friendData.data, friendData.crown, friendData.data.score);

				return item;
			}
			else if(cellData.type == CellType.addSlot)
            {
				UIFriend_addSlotCellView item = scroller.GetCellView(addSlotCellView) as UIFriend_addSlotCellView;
				return item;
            }
			else if(cellData.type == CellType.searchFriend)
            {
				UIFriend_searchFriendCellView item = scroller.GetCellView(searchFriendCellView) as UIFriend_searchFriendCellView;
				return item;
			}

			return null;
		}


		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			return _data[dataIndex].height;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return Math.Max(0, _data.Count);
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
