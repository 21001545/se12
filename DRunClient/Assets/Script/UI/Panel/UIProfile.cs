using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnhancedUI.EnhancedScroller;
using Festa.Client.Logic;
using Festa.Client.RefData;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using PolyAndCode.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Festa.Client
{
	public class UIProfile : UISingletonPanel<UIProfile>, IEnhancedScrollerDelegate
	{
		[Header("stickers")]
		[SerializeField]
		private RectTransform rect_stickerRoot;
		[SerializeField]
		private GameObject stickerPrefab;

		[Header("profiles")]
		public UIPhotoThumbnail image_photo;
		public TMP_Text text_name;
		public TMP_Text text_mention;
		public TMP_Text text_coin;
		public TMP_Text text_star;
		public TMP_Text text_friend_count;
		public TMP_Text text_followback_count;

		public GameObject go_momentCount;
		public GameObject go_bookmarkCount;
		public TMP_Text text_momentCount;
		public TMP_Text text_bookmarkCount;

		[SerializeField]
		private Button btn_follow;
		[SerializeField]
		private Button btn_followAdded;

		public EnhancedScroller scroller;
		public UIProfile_MomentRowItem itemSource;

		public Image img_moment;
		public Image img_bookmark;
		public RectTransform rect_highlightBar;

		// 2022.08.10 소현 : 디자인 수정에 의해 로딩 제거!
		//public GameObject go_loading;
		public Button btnBack;

		[SerializeField]
		private GameObject go_confirmPopup;

		[SerializeField]
		private UIProfileScroll scroll;
		private bool _isEmpty;
		public bool isEmpty { get { return _isEmpty; } }

		[SerializeField]
		private GameObject go_lockAssets;
		[SerializeField]
		private GameObject[] emptyPages;    // 0 : empty, 1 : no moment on my feed, 2 : not my friend
		[SerializeField]
		private TMP_Text txt_emptyPageMsg;

		[SerializeField]
		// 친구 프로필 일 경우, 표시될 내용들.
		private GameObject[] friend_context = new GameObject[2];

		[SerializeField]
		// 내 프로필 일 경우 표시될 내용들.
		private GameObject[] mine_context = new GameObject[2];

		public Animator anim_panel;

		//
		private int _targetAccountID;
		private ClientProfileCache _profileCache;
		private Action _confirmOpenChatRoomCallback;

		private class TabMode
		{
			public const int moment = 0;
			public const int bookmark = 1;
		}

		private int _tabMode;

		#region view_model
		//// 이거 쓰지 말고 여기서 별도 viewmodel을 구성해보자.

		private ClientProfile _profileViewModel = null;
		public ClientProfile ProfileViewModel
		{
			get
			{
				return _profileViewModel;
			}
			set
			{
				Set(ref _profileViewModel, value);
			}
		}

		private StatureViewModel _statureViewModel = null;
		public StatureViewModel StatureViewModel
		{
			get { return _statureViewModel; }
			set { Set(ref _statureViewModel, value); }
		}

		private WalletViewModel _walletViewModel = null;
		public WalletViewModel WalletViewModel
		{
			get
			{
				return _walletViewModel;
			}
			set
			{
				Set(ref _walletViewModel, value);
			}
		}


		private SocialViewModel _socialViewModel = null;
		public SocialViewModel SocialViewModel
		{
			get
			{
				return _socialViewModel;
			}
			set
			{
				Set(ref _socialViewModel, value);
			}
		}

		private MomentViewModel _momentViewModel = null;
		public MomentViewModel MomentViewModel
		{
			get
			{
				return _momentViewModel;
			}
			set
			{
				Set(ref _momentViewModel, value);
			}
		}

		private BookmarkViewModel _bookmarkViewModel = null;
		public BookmarkViewModel BookmarkViewModel
		{
			get
			{
				return _bookmarkViewModel;
			}
			set
			{
				Set(ref _bookmarkViewModel, value);
			}
		}

		#endregion

		public bool isMyAccount()
		{
			return _targetAccountID == Network.getAccountID();
		}

		//private ClientViewModel ViewModel => ClientMain.instance.getViewModel();
		private ClientNetwork Network => ClientMain.instance.getNetwork();
		//private UIScrollBehaviour _scroller;

		private int _id_time = Animator.StringToHash("time");
		private float _lastQueryTime;

		public override void initSingleton(SingletonInitializer initializer)
		{
			base.initSingleton(initializer);

			scroller.Delegate = this;
			_lastQueryTime = 0;
			//go_loading.SetActive(false);

			//Vector2 ratioConvert = new Vector2( 10.0f / 60.0f, 50.0f / 60.0f);
			//_scroller = UIScrollBehaviour.create(ratioConvert, onScroll);
		}

		public override void open(UIPanelOpenParam param = null, int transitionType = 0, int closeType = TransitionEventType.start_close)
		{
			base.open(param, transitionType, closeType);
			scroll.setProfileUI(0f);
			scroll.change_scrollInteractable(true);

			resetBindings();

			setupBackNavigation(param);

			if (param == null)
			{
				_targetAccountID = Network.getAccountID();
			}
			else
			{
				_targetAccountID = param.accountID;
			}

			setTabMenu(true);
			rect_highlightBar.anchoredPosition = new Vector2(-85f, 0f);

			ClientMain.instance.getProfileCache().getProfileCache(_targetAccountID, result => {
				if (result.succeeded())
				{
					_profileCache = result.result();

					ProfileViewModel = _profileCache.Profile;
					MomentViewModel = _profileCache.Moment;
					SocialViewModel = _profileCache.Social;
					WalletViewModel = _profileCache.Wallet;
					StatureViewModel = _profileCache.Stature;
					BookmarkViewModel = _profileCache.Bookmark;

					if (isMyAccount())
					{
						for (int i = 0; i < mine_context.Length; ++i)
							mine_context[i].SetActive(true);

						for (int i = 0; i < friend_context.Length; ++i)
							friend_context[i].SetActive(false);
					}
					else
					{
						for (int i = 0; i < mine_context.Length; ++i)
							mine_context[i].SetActive(false);

						for (int i = 0; i < friend_context.Length; ++i)
							friend_context[i].SetActive(true);

						checkFollow();
					}

					buildMomentList();
					buildStickerBoard();
				}
			});
		}

		private void buildStickerBoard()
		{
			var jsonData = _profileCache.Sticker.StickerBoard.getJsonData();

			int version = jsonData.getInteger("version");
			JsonArray stickerList = jsonData.getJsonArray("list");

			int currentStickers = rect_stickerRoot.transform.childCount;

			for (int i = 0; i < stickerList.size(); ++i)
			{
				JsonObject sticker = stickerList.getJsonObject(i);

				int type = sticker.getInteger("type");
				int id = sticker.getInteger("id");

				JsonArray vertices = sticker.getJsonArray("vertices");
				float position_x = (float)vertices.getDouble(0);
				float position_y = (float)vertices.getDouble(1);
				float rightBottomVertex_x = (float)vertices.getDouble(2);
				float rightBottomVertex_y = (float)vertices.getDouble(3);

				//// 만들기!!
				//StickerSizeController controller;
				//if (i < currentStickers)
				//{
				//	// 재활용~~
				//	rect_stickerRoot.GetChild(i).gameObject.SetActive(true);
				//	controller = rect_stickerRoot.GetChild(i).GetComponent<StickerSizeController>();
				//}
				//else
				//{
				//	GameObject stickerObj = Instantiate(stickerPrefab, rect_stickerRoot);
				//	controller = stickerObj.GetComponent<StickerSizeController>();
				//}

				//controller.setup(StickerState.OnBoard, id, i, position_x, position_y, rightBottomVertex_x, rightBottomVertex_y, rect_stickerRoot);

				//if(UIProfileBoard.getInstance().testIDs.Contains(id))
    //            {
				//	int index = Array.IndexOf(UIProfileBoard.getInstance().testIDs, id);
				//	controller.setImage(UIProfileBoard.getInstance().testStickers[index].texture);
				//}
			}

			for (int i = stickerList.size(); i < currentStickers; ++i)
			{
				// 지우지 말고 꺼놓자~ 언제 다시 쓸지 몰라
				rect_stickerRoot.GetChild(i).gameObject.SetActive(false);
			}
		}

		private void setupBackNavigation(UIPanelOpenParam param)
		{
			if (param != null && param.hasBackNavigation)
			{
				btnBack.gameObject.SetActive(true);
			}
			else
			{
				btnBack.gameObject.SetActive(false);
			}
		}

		public override void close(int transitionType = 0)
		{
			base.close(transitionType);
		}

		public override void onTransitionEvent(int type)
		{
			//Debug.Log($"onTransitionEvent:{type}");

			if (TransitionEventType.start_open == type)
			{
				// animator가 리셋된다
				//onScroll(_scroller.getNormalizedScrollPos());
			}
			else if (TransitionEventType.end_open == type)
			{
				//buildMomentList();

				//// 서버 테스트
				//buildBookmarkList();
			}
		}

		private void resetBindings()
		{
			if (_bindingManager.getBindingList().Count > 0)
			{
				return;
			}

			_bindingManager.makeBinding(this, nameof(WalletViewModel), onUpdateFestaCoin);
			_bindingManager.makeBinding(this, nameof(StatureViewModel), onUpdateFestaStar);
			_bindingManager.makeBinding(this, nameof(ProfileViewModel), onUpdateProfile);

			_bindingManager.makeBinding(this, nameof(MomentViewModel), onMomentListUpdate);
			_bindingManager.makeBinding(this, nameof(SocialViewModel), onUpdateFollowCumulation);

		}

		public void onClickEditName()
		{
			if (isMyAccount())
			{
				//UIBackNavigation.getInstance().setup(this, UIEditProfile.getInstance());
				//UIBackNavigation.getInstance().open();

				UIEditProfile.getInstance().resetForEdit();
				UIEditProfile.getInstance().open();

				UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIEditProfile.getInstance());
				stack.addPrev(UIMainTab.getInstance());
			}
		}

		public void onClickEditPhoto()
		{
			if (isMyAccount())
			{
				//UIBackNavigation.getInstance().setup(this, UIEditProfile.getInstance());
				//UIBackNavigation.getInstance().open();

				UIEditProfile.getInstance().resetForEdit();
				UIEditProfile.getInstance().open();

				ClientMain.instance.getPanelNavigationStack().push(this, UIEditProfile.getInstance());
			}
		}

		public void onClickFollow()
		{
            // following중인지 체크해야 될것 같은뎅
            MapPacket req = Network.createReq(CSMessageID.Social.FollowReq);
			req.put("id", _targetAccountID);

			Network.call(req, ack => {
				if (ack.getResult() == ResultCode.ok)
				{
					// 내꺼 업데이트
					ClientMain.instance.getViewModel().updateFromPacket(ack);
					_profileCache.Profile._isFollow = true;
					switchFollow(true);

					if(_tabMode == TabMode.moment)
						buildMomentList();
					else if(_tabMode == TabMode.bookmark)
						buildBookmarkList();
				}
			});
		}

		public void onClickUnFollow()
        {
            MapPacket req = Network.createReq(CSMessageID.Social.UnfollowReq);
			req.put("id", _targetAccountID);

			Network.call(req, ack => {
				if (ack.getResult() == ResultCode.ok)
				{
					// 내꺼 업데이트
					ClientMain.instance.getViewModel().updateFromPacket(ack);
					_profileCache.Profile._isFollow = false;
					switchFollow(false);

					if (_tabMode == TabMode.moment)
						buildMomentList();
					else if (_tabMode == TabMode.bookmark)
						buildBookmarkList();
				}
			});
		}

		public void checkFollow()
		{
			switchFollow(_profileCache.Profile._isFollow);
		}

		private void switchFollow(bool canFollow)
		{
			if (!canFollow)
            {
				btn_follow.gameObject.SetActive(true);
				btn_followAdded.gameObject.SetActive(false);
            }
			else
            {
				btn_follow.gameObject.SetActive(false);
				btn_followAdded.gameObject.SetActive(true);
			}
		}

		private void setTabMenu(bool isMoment)
		{
			go_momentCount.SetActive(isMoment);
			go_bookmarkCount.SetActive(!isMoment);

			if (isMoment)
			{
				_tabMode = TabMode.moment;
				img_moment.color = ColorChart.gray_900;
				img_bookmark.color = ColorChart.gray_400;
			}
			else
			{
				_tabMode = TabMode.bookmark;
				img_bookmark.color = ColorChart.gray_900;
				img_moment.color = ColorChart.gray_400;
			}

		}

		public void onClickEditBoard()
		{
			//// 2022.07.27 이강희
			//// 나만 Edit창을 열 수 있다

			////if (isMyAccount())
			////{
			//// TODO 뭔가 해줘야 함,, 친구 보드도 열어야 하는데!!
			//UIProfileBoard.getInstance().setClientProfile(_profileCache, isMyAccount());
			//UIProfileBoard.getInstance().open();
			//UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIProfileBoard.getInstance());
			//stack.addPrev(UIMainTab.getInstance());
			////}
		}

		public void onClickTabMoment()
		{
			setTabMenu(true);
			DOTween.To(() => rect_highlightBar.anchoredPosition, x => rect_highlightBar.anchoredPosition = x, new Vector2(-85f, 0f), 0.15f);

			// 임시코드
			buildMomentList();
		}

		public void onClickTabBookmark()
		{
			setTabMenu(false);
			DOTween.To(() => rect_highlightBar.anchoredPosition, x => rect_highlightBar.anchoredPosition = x, new Vector2(85f, 0f), 0.15f);

			// 임시코드
			buildBookmarkList();
		}

		public void onClickBackNavigation()
		{
			ClientMain.instance.getPanelNavigationStack().pop();
		}

		#region message

		// UIMessengerSelectFriend 에서 가져왔는데,, 이거 이렇게 해도 되는지 모르겠네!! 여기랑 저기서밖에 안 쓰는 거 맞나??
		// 확인하고 맞으면 마저 만들기

		public void onClickConfirm()
		{
			if (_confirmOpenChatRoomCallback != null)
			{
				_confirmOpenChatRoomCallback();
			}
			go_confirmPopup.SetActive(false);

			// TODO, 돈내고 룸 열어!
		}

		public void onClickConfirmClose()
		{
			go_confirmPopup.SetActive(false);
		}

		public void onClickSendMessage()
		{
			if (isMyAccount())
				return;

			if (ProfileViewModel == null)
			{
				return;
			}

			bool room_opended = ClientMain.instance.getViewModel().Chat.findDirectMessageRoom(_profileCache._accountID) != null;

			// 맞팔이 아니면 돈내야됨
			if (room_opended == false && ProfileViewModel.isFollowEachOther() == false)
			{
				_confirmOpenChatRoomCallback = () =>
				{
					startOpenChatRoom(_profileCache);
				};
				go_confirmPopup.SetActive(true);
			}
			else
			{
				startOpenChatRoom(_profileCache);
			}
		}

		private void startOpenChatRoom(ClientProfileCache profile)
		{
			if (profile.Profile.isFollowEachOther() == false)
			{
				var cost = GlobalRefDataContainer.getConfigInteger(RefConfig.Key.Chat.dm_open_price, 400);
				// 돈계산을 해야한다.
				if (ClientMain.instance.getViewModel().Wallet.FestaCoin < cost)
				{
					var sc = GlobalRefDataContainer.getStringCollection();
					UIPopup.spawnOK(sc.get("tree.purchase.popup.notenough.coin", 0));
					return;
				}
			}
			OpenChatRoomProcessor step = OpenChatRoomProcessor.create(profile._accountID);
			step.run(result =>
			{
				if (result.succeeded())
				{
					UIMessenger.getInstance().setup(step.getRoomViewModel());
					UIMessenger.getInstance().open();
					UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIMessenger.getInstance());
					stack.addPrev(UIMainTab.getInstance());
				}
				else
				{
					var sc = GlobalRefDataContainer.getStringCollection();
					UIPopup.spawnOK(sc.get("messageopen.popup.title", 0), sc.get("messageopen.popup.error", 0), () =>
					{

					});
				}
			});
		}

		#endregion

		public void onClickFollowList()
		{
			UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
			param.accountID = _targetAccountID;

			UIFriend.getInstance().open(param);

			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIFriend.getInstance());
			stack.addPrev(UIMainTab.getInstance());
		}

		public void onClickSettingMenu()
		{
			var sc = GlobalRefDataContainer.getStringCollection();

			UIPopupMenu.spawnThreeMenu(sc.get("setting.account.title", 0), sc.get("setting.push.title", 0), sc.get("setting.customerInfo.title", 0), onClickSettings, onClickPushSettings, null);
		}

		private void onClickSettings()
		{
			UISettings.getInstance().open();
			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UISettings.getInstance());
			stack.addPrev(UIMainTab.getInstance());
		}

		public void onClickPushSettings()
		{
			UIPushSettings.getInstance().open();
			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIPushSettings.getInstance());
			stack.addPrev(UIMainTab.getInstance());
		}

		private void onUpdateFestaCoin(object obj)
		{
			//string text = string.Format("<sprite name=\"coin\">{0}", WalletViewModel.FestaCoin.ToString("N0"));
			text_coin.text = WalletViewModel.FestaCoin.ToString("N0");
		}

		private void onUpdateFestaStar(object obj)
		{
			text_star.text = StatureViewModel.Star.ToString("N0");
		}

		private void onUpdateProfile(object obj)
		{
			if (ProfileViewModel == null)
			{
				// 아직.. 셋팅안됨~
				// TODO 기본값을 보여주도록 하자.
				return;
			}

			text_name.text = ProfileViewModel.name;
			string message = ProfileViewModel.message;

			if (string.IsNullOrEmpty(message))
			{
				message = "...";
			}

			text_mention.text = message;

			var pictureURL = ProfileViewModel.getPicktureURL(GlobalConfig.fileserver_url);
			if (string.IsNullOrEmpty(pictureURL))
				image_photo.setEmpty();
			else
				image_photo.setImageFromCDN(pictureURL);
		}

		private void onUpdateFollowCumulation(object o)
		{
			if (SocialViewModel == null)
			{
				// 아직.. 셋팅안됨~
				// TODO 기본값을 보여주도록 하자.
				return;
			}

			ClientFollowCumulation c = SocialViewModel.FollowCumulation;

			text_friend_count.text = c.follow_count.ToString("N0");
			text_followback_count.text = c.follow_back_count.ToString("N0");
		}

		private void onMomentListUpdate(object o)
		{
			if (MomentViewModel == null)
			{
				// 아직.. 셋팅안됨~
				// TODO 기본값을 보여주도록 하자.
				return;
			}

			//            if ( type == CollectionEventType.sort/* || type == CollectionEventType.clear*/)
			//			{
			//Debug.Log("reload profile moment list");

			//				scrollRect.ReloadData();
			//			}
		}

		private void buildMomentList()
		{
			//if(_lastQueryTime != 0 && Time.realtimeSinceStartup - _lastQueryTime < 10.0f)
			//{
			//	scroller.ReloadData();
			//	return;
			//         }

			if (MomentViewModel == null)
			{
				scroller.ReloadData();
				return;
			}

			_lastQueryTime = Time.realtimeSinceStartup;

			//go_loading.SetActive(true);

			int begin = MomentViewModel.MomentList.Count;
			int count = 20;

			MapPacket req = Network.createReq(CSMessageID.Moment.QueryListReq);
			req.put("id", _targetAccountID);    // 다른 사람 모먼트 리스트를 얻을려고 할때는 이부분을 수정
			req.put("begin", begin);
			req.put("count", count);
			Network.call(req, ack => {
				if (ack.getResult() == ResultCode.ok)
				{
					MomentViewModel.updateFromAck(ack);
					text_momentCount.text = MomentViewModel.MomentList.Count.ToString("N0");

					if (_tabMode == TabMode.moment)
					{
						setEmptyPage(-1);

						if (isMyAccount())
						{
							if (MomentViewModel.MomentList.Count == 0)
                            {
								setEmptyPage(1);
								return;
							}
						}
						else
						{
							if (ProfileViewModel.isFollowEachOther() == false)
                            {
								setEmptyPage(2);
								return;
							}

							else if (MomentViewModel.MomentList.Count == 0)
                            {
								setEmptyPage(0);
								return;
							}
						}

						scroller.ReloadData();
					}
				}
			});
		}

		// bookmark얻어오기 추가 폴리싱 필요
		private void buildBookmarkList()
		{
			//if(BookmarkViewModel.BookmarkList.Count > 0)
			//{
			//	scroller.ReloadData();
			//	return;
			//}

			int target_account_id = _targetAccountID;
			int begin = 0;
			int count = 20;

			//go_loading.SetActive(true);

			QueryBookmarkProcessor p = QueryBookmarkProcessor.create(target_account_id, begin, count);
			p.run(result => {
				//go_loading.SetActive(false);

				if (result.succeeded())
				{
					List<ClientBookmark> bookmarkList = p.getBookmarkList();

					BookmarkViewModel.BookmarkList = bookmarkList;
					text_bookmarkCount.text = BookmarkViewModel.BookmarkList.Count.ToString("N0");

					setEmptyPage(-1);

					if (isMyAccount())
					{
						if (bookmarkList.Count == 0)
                        {
							setEmptyPage(0);
							return;
						}
					}
					else
					{
						if (ProfileViewModel.isFollowEachOther() == false)
						{
							setEmptyPage(2);
							return;
						}

						if (bookmarkList.Count == 0)
                        {
							setEmptyPage(0);
							return;
						}
					}

					scroller.ReloadData();
				}
			});
		}

		private void setEmptyPage(int index)
		{
			_isEmpty = true;

            if (index == 2)
                // 잠긴친구
                go_lockAssets.SetActive(true);
            else
                go_lockAssets.SetActive(false);

            if (index == -1)
			{
				for (int i = 0; i < emptyPages.Length; ++i)
				{
					emptyPages[i].SetActive(false);
				}
				_isEmpty = false;

				return;
			}

			for (int i = 0; i < emptyPages.Length; ++i)
			{
				emptyPages[i].SetActive(i == index ? true : false);
			}
		}

		public void onClickNewMoment()
		{
			ClientMain.instance.getViewModel().MakeMoment.reset(MakeMomentViewModel.EditMode.make, null, ClientMain.instance.getViewModel().Location.createCurrentPlaceData());

			UIPhotoTake.getInstance().setMode(UIPhotoTake.Mode.moment);
			UIPhotoTake.getInstance().open();

			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(this, UIPhotoTake.getInstance());
			stack.addPrev(UIMainTab.getInstance());
		}

		public void onTensionRefresh()
		{
			if (_tabMode == TabMode.moment)
				buildMomentList();
			else if (_tabMode == TabMode.bookmark)
				buildBookmarkList();

			buildStickerBoard();
		}

		public override void update()
		{
			base.update();

            if (scroll.isUp)
            {
                if (scroller.NormalizedScrollPosition <= 0f && scroller.Velocity.y < 0f)
                {
                    scroll.change_scrollInteractable(true);
					scroll.setProfileUI(0f, isMyAccount());
                }
/*                else
                {
                    scroll.change_scrollInteractable(false);
                }*/
            }
        }

		#region scroller_delegate

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller,int dataIndex,int cellIndex)
		{
			UIProfile_MomentRowItem rowItem = scroller.GetCellView(itemSource) as UIProfile_MomentRowItem;

			List<ClientMoment> momentList = new List<ClientMoment>();

			// pick 3 items
			int startIndex = dataIndex * 3;

			if( _tabMode == TabMode.moment)
			{
				for(int i = 0; i < 3; ++i)
				{
					if( startIndex + i < MomentViewModel.MomentList.Count)
					{
						momentList.Add(MomentViewModel.MomentList[startIndex + i]);
					}
				}
			}
			else if( _tabMode == TabMode.bookmark)
			{
				for(int i = 0; i < 3; ++i)
				{
					if( startIndex + i < BookmarkViewModel.BookmarkList.Count)
					{
						momentList.Add(BookmarkViewModel.BookmarkList[startIndex + i]._moment);
					}
				}
			}

			rowItem.setup(momentList);

			return rowItem;
		}

		public float GetCellViewSize(EnhancedScroller scrolle,int dataIndex)
		{
			return 166;
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			if (_tabMode == TabMode.moment)
			{
				return Mathf.CeilToInt((float)MomentViewModel.MomentList.Count / 3.0f);
			}
			else if (_tabMode == TabMode.bookmark)
			{
				return Mathf.CeilToInt((float)BookmarkViewModel.BookmarkList.Count / 3.0f);
			}

			return 0;
		}
		
		//public int GetItemCount()
		//{
		//	if( MomentViewModel == null)
		//	{
		//		return 0;
		//	}

		//	return MomentViewModel.MomentList.Count;
		//}

		//public void SetCell(ICell cell,int index)
		//{
		//	UIProfile_MomentItem item = cell as UIProfile_MomentItem;
		//	ClientMoment moment = MomentViewModel.MomentList[index];

		//	item.setup(moment);
		//}
		#endregion

		//#region PanelScroll
		//public void onBeginDrag(BaseEventData e)
		//{
		//	_scroller.onBeginDrag(e);
		//}

		//public void onDrag(BaseEventData e)
		//{
		//	_scroller.onDrag(e);
		//}

		//public void onEndDrag(BaseEventData e)
		//{
		//	_scroller.onEndDrag(e);
		//}

		//private void onScroll(float ratio)
		//{
		//	anim_panel.SetFloat(_id_time, ratio);
		//}

		//#endregion
	}
}
