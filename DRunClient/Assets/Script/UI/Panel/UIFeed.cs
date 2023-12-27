using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	public class UIFeed : UISingletonPanel<UIFeed>, IEnhancedScrollerDelegate
	{
		public class MomentType
        {
            public static readonly int moment = 1;
            public static readonly int momentPosting = 2;
        }

		public class MomentDataBase
        {
			public MomentDataBase(int type, float height, bool alreadyCalculate)
            {
				this.type = type;
				this.height = height;
				this.alreadyCalculate = alreadyCalculate;
            }

			public int type;
			public float height;
			public bool alreadyCalculate = false;
        }

		public class MomentData : MomentDataBase
		{
			public ClientFeed feed;
			public MomentData(ClientFeed feed) : base(MomentType.moment, 0.0f, false)
			{
				this.feed = feed;
			}
		}

		public class MomentPostingData : MomentDataBase
        {
			public JobProgressItemViewModel model;
			public MomentPostingData(JobProgressItemViewModel model) : base(MomentType.momentPosting, 52.0f, true)
            {
				this.model = model;
            }
        }
		public GameObject go_loading;

		public EnhancedScroller _scroller;
		public UIMomentCell _monentCellPrefab;
		public UIMomentPostingCell _momentPostingCellPrefab;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private bool _calcuateLayout;
		private bool _lastPadderActive;
		private float _lastPadderSize;
		
        [SerializeField]
        private GameObject go_notify;

		[SerializeField]
		private UIToggleButton btn_notify;

        [SerializeField]
        private TMP_Text txt_notify_like;

        [SerializeField]
        private TMP_Text txt_notify_comment;

        [SerializeField]
        private TMP_Text txt_notify_star;

        private List<MomentDataBase> _data = new List<MomentDataBase>();

        private bool _needResizeScroller = false;

        public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			_scroller.Delegate = this;
			_scroller.cellViewWillRecycle = onCellViewWillRecyle;

		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			resetBindings();

			base.open(param, transitionType, closeType);

			if ( checkBuildList())
			{
				buildList();
			}
			//else
			//{
			//	resizeScroller();
			//}

			// 이전 open때 go_notify가 켜진채로 꺼질 수 있다.
			// open시 명시적으로 꺼준다.
			go_notify.gameObject.SetActive(false);
			btn_notify.IsOn = false;

			// 임시 코드
			loadNewActivityCount();
		}

        public override void onTransitionEvent(int type)
        {
            base.onTransitionEvent(type);
			if ( type == TransitionEventType.start_open)
            {
                if (_needResizeScroller)
                {
					foreach(var data in _data)
                    {
						data.alreadyCalculate = false;
                    }

                    resizeScroller();
                    _needResizeScroller = false;

                }
            }
        }

        private bool checkBuildList()
		{
			if( ViewModel.Feed.LoadedFeedList.Count == 0)
			{
				return true;
			}

			TimeSpan diff = DateTime.UtcNow - ViewModel.Feed.LastFeedQueryTime;
			if (diff.TotalMinutes > 30)
			{
				return true;
			}

			return false;
		}

		private void resetBindings()
		{
			if( _bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			//2022.4.25 
			_bindingManager.makeBinding(ViewModel.JobProgress.ListMoment, updateJobProgressList);
			
			//ActivityViewModel activityVM = ViewModel.Activity;
			//_bindingManager.makeBinding(activityVM, nameof(activityVM.NewActicityCount), updateNewActivityCount);

			//WalletViewModel wallet_vm = ViewModel.Wallet;
			//_bindingManager.makeBinding(wallet_vm, nameof(wallet_vm.FestaCoin), text_coin, nameof(text_coin.text), UIUtil.convertFestaCoinString);
		}

		private void updateJobProgressList(int event_type,object obj)
		{
			if( event_type == CollectionEventType.add)
			{
				JobProgressItemViewModel item = (JobProgressItemViewModel)obj;
                Debug.Log($"moment job added : type[{item.Type}]");

                if (item.Type == JobProgressItemViewModel.JobType.make_moment)
                {

                    _data.Insert(0, new MomentPostingData(item));

                    resizeScroller();
                }

            }
			else if( event_type == CollectionEventType.remove)
			{
				JobProgressItemViewModel item = (JobProgressItemViewModel)obj;

				Debug.Log($"moment job removed : type[{item.Type}]");

				// swpark 수정은 딱히 뭘 하지 말자.
                if (item.Type == JobProgressItemViewModel.JobType.modify_moment)
                {
					return;
                }

                // 남아 있는 작업이 더이상 없으면 feed를 한번 갱신해줌
                if ( ViewModel.JobProgress.ListMoment.size() == 0)
				{
					buildList();

					// 2022.07.04 이강희, 프로필 모먼트 목록 갱신을 위해
					ViewModel.Moment.MomentList.Clear();
				}
			}
		}

        private void updateNewActivityCount(object obj)
		{
			ClientNewActivityCount notiCount = ViewModel.Activity.NewActicityCount;

			int like_count = notiCount.getCount(ClientNewActivityCount.CountType.moment_like);
			int comment_count = notiCount.getCount(ClientNewActivityCount.CountType.moment_comment);
			int star_reward_count = notiCount.getCount(ClientNewActivityCount.CountType.reward_moment_like);
			int star_reward_unclaimed_count = notiCount.getCount(ClientNewActivityCount.CountType.reward_moment_like_unclaimed);

			Debug.Log($"like[{like_count}] comment[{comment_count}] star_reward[{star_reward_count}] star_reward_unclaimed[{star_reward_unclaimed_count}]"); ;

			// 읽지 않은게 있으면 noti 보드 노출
			if( like_count > 0 || comment_count > 0 || star_reward_count > 0 )
            {
				go_notify.SetActive(true);
				txt_notify_like.gameObject.SetActive(like_count > 0);
				txt_notify_comment.gameObject.SetActive(comment_count > 0);
				txt_notify_star.gameObject.SetActive(star_reward_count > 0);

                txt_notify_like.text = like_count.ToString("N0");
				txt_notify_comment.text = comment_count.ToString("N0");
                txt_notify_star.text = star_reward_count.ToString("N0");

				btn_notify.IsOn = true;
			}
			else
			{
				go_notify.SetActive(false);

				// 보상이 남아 있으면 버튼 활성화
				if( star_reward_unclaimed_count > 0)
				{
					btn_notify.IsOn = true;
				}
				else
				{
					btn_notify.IsOn = false;
				}
			}
		}

		private void loadNewActivityCount()
		{
			MapPacket req = Network.createReq(CSMessageID.Social.GetNewActivityCountReq);
			req.put("slot_id", ViewModel.Activity.NextReadSlotID);

			Network.call(req, ack => { 
				if( ack.getResult() == ResultCode.ok)
				{
					ViewModel.Activity.NewActicityCount = (ClientNewActivityCount)ack.get("data");

					updateNewActivityCount(null);
				}
			});
		}

		public void buildList()
		{
			go_loading.gameObject.SetActive(true);

			float start_time = Time.realtimeSinceStartup;

			QueryFeedProcessor processor = QueryFeedProcessor.create();
			processor.run(result => {
				Debug.Log($"query feed takes {Time.realtimeSinceStartup - start_time}s");

				go_loading.gameObject.SetActive(false);

				if( result.succeeded())
				{
					// 새롭게 갱신하자.
					_data.Clear();
					foreach(var feed in ViewModel.Feed.LoadedFeedList)
                    {
						_data.Add(new MomentData(feed));
                    }

					resizeScroller();
				}
			});
		}

		private void resizeScroller()
		{
            if (_scroller.Container == null )
            {
                _needResizeScroller = true;
				return;
            }

            var rectTransform = _scroller.transform as RectTransform;
			var size = rectTransform.sizeDelta;

			rectTransform.sizeDelta = new Vector2(size.x, float.MaxValue);

            _calcuateLayout = true;
			_scroller.ReloadData();

			rectTransform.sizeDelta = size;

			_calcuateLayout = false;
			_scroller.ReloadData();

			_scroller.Container.anchoredPosition = Vector2.zero;

			// 네트워크 로딩 중에, UIFeed가 닫히면 스크롤러가 제대로 갱신되지 못한 채.. 끝나버림. 
			// 다음 오픈 시, 올바르게 갱신하기 위하여.. 
			if ( gameObject.activeSelf == false )
            {
				_needResizeScroller = true;
			}
        }

		// 상단.. 텐션 이벤트..
		public void onTensionRefresh()
        {
			// swpark 그냥 마구마구 업데이트 해보자.
			//if (checkBuildList())
				buildList();
        }

		public void gotoTop()
        {
			// 탑으로 스크롤을 올리자.
			_scroller.JumpToDataIndex(0, tweenType:EnhancedScroller.TweenType.easeInCubic, tweenTime:0.3f);
        }

		public void onClickNewMoment()
        {
			ViewModel.MakeMoment.reset(MakeMomentViewModel.EditMode.make,null, ViewModel.Location.createCurrentPlaceData());

			UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.moment);
            UIPhotoTake.getInstance().open();

            UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoTake.getInstance());
            stack.addPrev(UIMainTab.getInstance());
        }

		public void onClickAcitivity()
		{
			UIActivity.getInstance().open();
			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIActivity.getInstance());
			stack.addPrev(UIMainTab.getInstance());

			// 일단 가리자. 
			go_notify.SetActive(false);
		}

		public void onClickFriend()
		{
			UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
			param.accountID = Network.getAccountID();
			
			UIFriend.getInstance().open(param);
			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIFriend.getInstance());
			stack.addPrev(UIMainTab.getInstance());
		}


		#region scroller_delegate

		public void onCellViewWillRecyle(EnhancedScrollerCellView cellView)
		{
			if( cellView is UIMomentCell)
			{
				UIMomentCell momentCell = (UIMomentCell)cellView;
				momentCell.onRecycle();
			}
		}

		public void onCellHeightChanged(int dataIndex)
        {
			_data[dataIndex].alreadyCalculate = false;
        }

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller,int dataIndex,int cellIndex)
		{
			var data = _data[dataIndex];
			if ( data.type == MomentType.moment)
            {
                UIMomentCell cellView = scroller.GetCellView(_monentCellPrefab) as UIMomentCell;
				var feedData = ((MomentData)data).feed;

				if (_calcuateLayout)
                {
                    if (data.alreadyCalculate == false)
                    {
                        cellView.setup(feedData, _calcuateLayout);
                        data.height = cellView.Height;
						data.alreadyCalculate = true;
                    }
                }
                else
                {
                    cellView.setup(feedData, _calcuateLayout);
                }

                return cellView;
            }
			else if (data.type == MomentType.momentPosting)
            {
                UIMomentPostingCell cellView = scroller.GetCellView(_momentPostingCellPrefab) as UIMomentPostingCell;
                cellView.setup(((MomentPostingData)data).model);
                return cellView;
            }

            return null;
		}

		public float GetCellViewSize(EnhancedScroller scroller,int dataIndex)
		{
			return _data[dataIndex].height;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return _data.Count;
		}

		public void initializeCellTween(int dataIndex,int cellViewIndex)
		{
			// get the cell's position (using the cell view index in case of looping)
			var cellPosition = _scroller.GetScrollPositionForCellViewIndex(cellViewIndex, EnhancedScroller.CellViewPositionEnum.Before);

			// get the offset of the cell from the top of the scroll rect
			var tweenCellOffset = cellPosition - _scroller.ScrollPosition;

			// turn off loop jumping so that the scroller will not try to jump to a new location as the cell is expanding / collapsing
			_scroller.IgnoreLoopJump(true);

            _calcuateLayout = true;
            _scroller.ReloadData();

            _calcuateLayout = false;
            _scroller.ReloadData();

            // get the new position of the cell (using the cell view index in case of looping)
            cellPosition = _scroller.GetScrollPositionForCellViewIndex(cellViewIndex, EnhancedScroller.CellViewPositionEnum.Before);

			// set the scroller's position to focus on the cell, using the offset calculated above
			_scroller.SetScrollPositionImmediately(cellPosition - tweenCellOffset);

			// turn loop jumping back on
			_scroller.IgnoreLoopJump(false);

			//// if this cell has an immediate tween type, then we can just exit the
			//// method and not worry about adjusting padder sizes or calling
			//// the tweening on the cell
			//if (_data[dataIndex].tweenType == Tween.TweenType.immediate)
			//{
			//	return;
			//}

			//// cache the last padder's active state and size for after the tween
			//_lastPadderActive = scroller.LastPadder.IsActive();
			//_lastPadderSize = scroller.LastPadder.minHeight;

			//// manually set the last padder's size so that we can tween the cell
			//// size without distorting all the cells' sizes
			//if (data._expaneded)
			//{
			//	scroller.LastPadder.minHeight += _data[dataIndex].SizeDifference;
			//}
			//else
			//{
			//	scroller.LastPadder.minHeight -= _data[dataIndex].SizeDifference;
			//}

			//// make sure the last padder is active so that we can tween its size
			//scroller.LastPadder.gameObject.SetActive(true);

			//// grab the cell that was clicked so that we can start tweening.
			//// note that we cannot just pass in the cell to this method since we
			//// are calling ReloadData, which destroys that cell. Grabbing it
			//// here is the only way to get an active cell after the reload.
			//var cellViewTween = scroller.GetCellViewAtDataIndex(dataIndex) as UIFeed_MomentItem;

			//// start the cell's tweening process
			//cellViewTween.BeginTween();
		}

		public void CellTweenUpdated(int dataIndex,int cellViewIndex,float newValue,float delta)
		{
			_scroller.LastPadder.minHeight -= delta;
		}

		public void CellTweenEnd(int dataIndex, int cellViewIndex)
		{
			// set the last padder's active state back to what we captured before the tween
			_scroller.LastPadder.gameObject.SetActive(_lastPadderActive);

			// set the last padder's size back to what we captured before the tween
			_scroller.LastPadder.minHeight = _lastPadderSize;
		}

		#endregion
	}
}
