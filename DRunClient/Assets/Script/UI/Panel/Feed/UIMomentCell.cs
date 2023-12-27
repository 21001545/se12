using EnhancedUI.EnhancedScroller;
using Festa.Client.Logic;
using Festa.Client.Module;
using Festa.Client.Module.Net;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIMomentCell : EnhancedScrollerCellView, IEnhancedScrollerDelegate
	{
		[SerializeField]
		private RectTransform rt;

        [SerializeField]
        private TMP_Text text_name;


        [SerializeField]
        private TMP_Text txt_story;

        [SerializeField]
        private TMP_Text text_like;

        [SerializeField]
        private TMP_Text txt_commentCount;

        [SerializeField]
        private UIPhotoThumbnail image_profile;

        [SerializeField]
        private UIToggleButton btn_like;

        [SerializeField]
        private UIToggleButton btn_bookmark;

        [SerializeField]
        private GameObject btn_follow;

        [SerializeField]
        private GameObject btn_followAdded;

        [SerializeField]
        private UIMomentPhoto _photoPrefab;

        [SerializeField]
        private EnhancedScroller _photoScoller;

        [SerializeField]
        private GameObject go_comments;

        [SerializeField]
        private TMP_Text txt_previewComment;

        [SerializeField]
        private TMP_Text txt_viewComment;

        [SerializeField]
        private TMP_Text txt_time;

		[SerializeField]
		private RectTransform rt_pageDotRoot;
		
		[SerializeField]
        private Image[] rt_pageDots;

        [SerializeField]
        private TMP_Text txt_more;

        private ClientFeed _feed;
		private ClientMoment _moment;
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
		private LayoutElement _layoutElement;
		private bool _calculateLayout;

		private ClientNetwork Network => ClientMain.instance.getNetwork();
		private ClientViewModel ViewModel => ClientMain.instance.getViewModel();

		public float Height
		{
			get
			{
				return rt.sizeDelta.y;
			}
		}

		public ClientFeed getFeed()
		{
			return _feed;
		}

		public void setup(ClientFeed feed,bool isCalculateLayout )
		{
			_calculateLayout = isCalculateLayout;
			_feed = feed;
			_moment = _feed._moment;

			text_name.text = _moment._profile.name;

			if (_moment._shortStory == null)
			{
				_moment.makeShortStory(StringCollection.get("feed.morething", 0));
			}

			_photoScoller.Delegate = this;
			_photoScoller.scrollerSnapped = (EnhancedScroller scroller, int cellIndex, int dataIndex, EnhancedScrollerCellView cellView) => 
			{
				setupPageDot(dataIndex);
			};
			// 사진개수에 따라 높이가 다르네,
			var layoutElement = _photoScoller.GetComponent<LayoutElement>();

			if (_feed._moment.photo_list.Count == 1)
            {
				layoutElement.preferredHeight = 456.0f;
				_photoScoller.padding.bottom = 0;
			}
			else
            {
                layoutElement.preferredHeight = 428.0f;
				//_photoScoller.padding.bottom = 9; // 하단 스크롤바 때문에... 
			}

			btn_follow.gameObject.SetActive(!feed._moment._profile._isFollow && feed.object_owner_account_id != Network.getAccountID());
			btn_followAdded.gameObject.SetActive(false);

			setupLikeComment();
            setupBookmark();
            setupThumnail();
            setupProfileImage();
            setupTime();
			setupComment();

			if ( _calculateLayout == false )
            {
				var visible = _feed._moment.photo_list.Count > 1;
				rt_pageDotRoot.gameObject.SetActive(visible);
				if ( visible )
                {
					setupPageDot(0);					
                }
            }

            if (_feed._expaneded)
            {
                txt_story.text = _moment.story;
                txt_story.overflowMode = TextOverflowModes.Overflow;
				var size = txt_story.GetPreferredValues();
                var textLayoutElement = txt_story.GetComponent<LayoutElement>();
                textLayoutElement.preferredHeight = textLayoutElement.minHeight = size.y;
				txt_story.margin = new Vector4(16, 0, 16, 0);

				txt_more.gameObject.SetActive(false);

			}
            else
            {
				var textInfo= txt_story.GetTextInfo(_moment.story);
                txt_story.overflowMode = TextOverflowModes.Ellipsis;
                var textLayoutElement = txt_story.GetComponent<LayoutElement>();
				textLayoutElement.preferredHeight = textLayoutElement.minHeight = 40.0f;
                var size = txt_story.GetPreferredValues();
				textLayoutElement.preferredHeight = textLayoutElement.minHeight = Math.Min(40.0f, size.y);
				if ( textInfo.characterCount > 0 && textInfo.characterCount - 1 < textInfo.characterInfo.Length && 
					textInfo.characterInfo[textInfo.characterCount-1].isVisible == false )
                {
                    txt_more.gameObject.SetActive(true);
                    txt_story.margin = new Vector4(16, 0, 60, 0);
                }
				else
                {
                    txt_more.gameObject.SetActive(false);
                    txt_story.margin = new Vector4(16, 0, 16, 0);
                }
            }

            Canvas.ForceUpdateCanvases();
        }

		private void setupPageDot(int index)
        {
			if (rt_pageDotRoot.gameObject.activeSelf == false)
				return;

			var count = _feed._moment.photo_list.Count;

            bool[] visible = new bool[5] { count >= 2, count >= 2, count >= 3, count >= 4, count >= 5 };
			// 0 : 큰거, 1 : 중간, 2 : 작은거
			int[] size;
			
			if ( count > 5 )
            {
                size = new int[5] {
                    index == 0 ? 0 : ( index >= 2 ? 2 : 1),
                    index == 1 ? 0 : 1,
                    index >=2 && index < count-2 ? 0 : 1,
                    (count - 1) - index == 1  ? 0 : 1,
					(count - 1) == index ? 0 : ( count - index == 2 ? 1 : 2),
                };
            }
			else
            {
                size = new int[5] {
                    index == 0 ? 0 : 1,
                    index == 1 ? 0 : 1,
                    index == 2 ? 0 : 1,
                    index == 3 ? 0 : 1,
                    index == 4 ? 0 : 1,
                };
            }
			// 선택된것 gray_600
			// 아닌거 gary_300
			// 선택된거 크기 10,5
			// 중간 크기 5,5
			// 작은거 3,3

			var largeSize = new Vector2(10, 5);
			var middleSize = new Vector2(5, 5);
            var smallSize = new Vector2(3, 3);
            for ( int i =0; i <5; ++i )
            {
				if ( visible[i])
                {
                    rt_pageDots[i].gameObject.SetActive(true);
                    rt_pageDots[i].rectTransform.sizeDelta = size[i] == 0 ? largeSize : size[i] == 1 ? middleSize : smallSize;
                    rt_pageDots[i].color = size[i] == 0 ? ColorChart.gray_600 : ColorChart.gray_300;
                }
				else
                {
					rt_pageDots[i].gameObject.SetActive(false);
					continue;
                }
            }
        }

        public void onRecycle()
		{
			// 사진같은거 release 시켜줄까?
			int cellCount = GetNumberOfCells(_photoScoller);
			for(int i = 0; i < cellCount; ++i)
			{
				EnhancedScrollerCellView cellView = _photoScoller.GetCellViewAtDataIndex(i);
				if( cellView == null)
				{
					continue;
				}

				UIMomentPhoto momentPhoto = cellView as UIMomentPhoto;
				momentPhoto.setEmpty();
			}

			// 프로필 사진
			image_profile.setEmpty();
		}

		public void onClickTab()
        {
            UIFeedOptionPopup.spawn(_feed);
        }

        public void onClickMoreThing()
		{
			if (_feed._expaneded)
				return;

            if (txt_story.textInfo.characterCount < txt_story.text.Length)
            {
				// 두줄 이상이네, 더보기를 출력 해야한다.
				// 더보기 위치는 어떻게 맞추니?
				_feed._expaneded = true;

				var layoutElement = txt_story.GetComponent<LayoutElement>();
				txt_story.overflowMode = TextOverflowModes.Overflow;
				txt_story.GetPreferredValues();
				layoutElement.preferredHeight = txt_story.renderedHeight;

                txt_story.margin = new Vector4(16, 0, 16, 0);

                UIFeed.getInstance().onCellHeightChanged(dataIndex);
                UIFeed.getInstance().initializeCellTween(dataIndex, cellIndex);
            }
        }

		public void onClickFollow()
        {
			btn_follow.gameObject.SetActive(false);
			btn_followAdded.gameObject.SetActive(true);

			MapPacket req = Network.createReq(CSMessageID.Social.FollowReq);
            req.put("id", _feed.object_owner_account_id);

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    // 내꺼 업데이트
                    ClientMain.instance.getViewModel().updateFromPacket(ack);
					_feed._moment._profile._isFollow = true;
				}
            });
        }

		public void onClickUnfollow()
        {
            btn_follow.gameObject.SetActive(true);
            btn_followAdded.gameObject.SetActive(false);

            MapPacket req = Network.createReq(CSMessageID.Social.UnfollowReq);
            req.put("id", _feed.object_owner_account_id);

            Network.call(req, ack =>
            {
                if (ack.getResult() == ResultCode.ok)
                {
                    // 내꺼 업데이트
                    ClientMain.instance.getViewModel().updateFromPacket(ack);
                    _feed._moment._profile._isFollow = false;
                }
            });
        }

		public void onClickLike()
		{
			btn_like.interactable = false;

			int msg_id = _moment._isLiked ? CSMessageID.Moment.UnlikeReq : CSMessageID.Moment.LikeReq;
			MapPacket req = Network.createReq( msg_id);
			req.put("moment_account_id", _moment.account_id);
			req.put("moment_id", _moment.id);

			Network.call(req, ack => {
				btn_like.interactable = true;

				if( ack.getResult() == ResultCode.ok)
				{
					ClientMoment moment = (ClientMoment)ack.get("moment");
					_moment.like_count = moment.like_count;
					_moment._isLiked = !_moment._isLiked;

					setupLikeComment();
				}
			});
		}

		public void onClickComment()
		{
			//UIBackNavigation.getInstance().setup(UIMainTab.getInstance(), UIMomentComment.getInstance());
			//UIBackNavigation.getInstance().open();
			//UIMomentComment.getInstance().setup(_moment);

			UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
			param.put("moment", _moment);
			UIMomentComment.getInstance().open(param);

			//UIFeed.getInstance().close();

			UIPanelNavigationStackItem stack = ClientMain.instance.getPanelNavigationStack().push(UIMainTab.getInstance(), UIMomentComment.getInstance());
			stack.addPrev(UIFeed.getInstance());
		}

		public void onClickBookmark()
		{
			btn_bookmark.interactable = false;

			int msg_id = _feed._checkBookmark ? CSMessageID.Feed.RemoveBookmarkReq : CSMessageID.Feed.AddBookmarkReq;
			MapPacket req = Network.createReq(msg_id);

			//_object_datasource_id = _req.get(MapPacketKey.object_datasource_id);
			//_object_owner_account_id = _req.get(MapPacketKey.object_owner_account_id);
			//_object_type = _req.get(MapPacketKey.object_type);
			//_object_id = _req.get(MapPacketKey.object_id);

			req.put("object_datasource_id", _feed.object_datasource_id);
			req.put("object_owner_account_id", _feed.object_owner_account_id);
			req.put("object_type", _feed.object_type);
			req.put("object_id", _feed.object_id);

			Network.call(req, ack => {
				btn_bookmark.interactable = true;

				if ( ack.getResult() == ResultCode.ok)
				{
					if( msg_id == CSMessageID.Feed.RemoveBookmarkReq)
					{
						_feed._checkBookmark = false;
						setupBookmark();
					}
					else if( msg_id == CSMessageID.Feed.AddBookmarkReq)
					{
						_feed._checkBookmark = true;
						setupBookmark();
					}
				}
			});
		}

		public void onClickProfile()
		{
            // 2022.05.20 이강희, 모먼트 지우기 테스트
            //deleteMoment();

            // 2022.05.20 이강희 모먼트 수정하기 테스트
            //modifyMoment();

            UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
            param.accountID = _feed.object_owner_account_id;

            UIProfile.getInstance().open(param);
            ClientMain.instance.getPanelNavigationStack().push(UIFeed.getInstance(), UIProfile.getInstance());
        }

		//private void deleteMoment()
		//{
		//	// 내꺼가 아니면 지울 수 없음
		//	if( _feed.object_owner_account_id != Network.getAccountID())
		//	{
		//		return;
		//	}

		//	DeleteMomentProcessor step = DeleteMomentProcessor.create(_feed._moment.id);
		//	step.run(result => { });
		//}

		//private void modifyMoment()
		//{
		//	// 내꺼가 아니면 편집 할 수 없음
		//	if( _feed.object_owner_account_id != Network.getAccountID())
		//	{
		//		return;
		//	}

		//	//ViewModel.MakeMoment.setupModify(_moment);

		//	ViewModel.MakeMoment.reset(MakeMomentViewModel.EditMode.modify, _moment);

		//	UIMakeMomentCommit.getInstance().open();
		//	ClientMain.instance.getPanelNavigationStack().push(UIFeed.getInstance(), UIMakeMomentCommit.getInstance());
		//}

		private void setupThumnail()
		{
			// 음..
			// 2022.02.28 이강희 cell height가 0이 되는 이슈가 있어서
			if (GetNumberOfCells(_photoScoller) == 1 )
            {
				_photoScoller.ScrollRect.movementType = ScrollRect.MovementType.Clamped;
				//_photoScoller.ScrollRect.enabled = false;
			}
			else
            {
				_photoScoller.ScrollRect.movementType = ScrollRect.MovementType.Elastic;
				//_photoScoller.ScrollRect.enabled = true;
            }

			_photoScoller.ReloadData();			
		}

		private void setupLikeComment()
        {
            text_like.text = _moment.like_count.ToString("N0");
			txt_commentCount.text = _moment.comment_count.ToString("N0");
			btn_like.IsOn = _moment._isLiked;
		}

		private void setupBookmark()
		{
			btn_bookmark.IsOn = _feed._checkBookmark;
		}

		private void setupProfileImage()
		{
			if (_moment._profile != null && String.IsNullOrEmpty(_moment._profile.picture_url) == false && _calculateLayout == false)
			{
				image_profile.setImageFromCDN(_moment._profile.getPicktureURL(GlobalConfig.fileserver_url));
			}
			else
			{
				image_profile.setEmpty();
			}
		}

		private void setupComment()
        {
            if ( _moment.comment_count > 0 )
            {
                go_comments.gameObject.SetActive(true);
				if (_moment.recent_comment.Count > 0)
                {
                    txt_previewComment.gameObject.SetActive(true);
                    txt_previewComment.text = StringCollection.getFormat("moment.comment", 0, _moment.recent_comment[0]._profile?.name, _moment.recent_comment[0].message);
                }
				else
                {
					txt_previewComment.gameObject.SetActive(false);
				}
				
				if (_moment.comment_count > 1)
                {
					txt_viewComment.gameObject.SetActive(true);
                    txt_viewComment.text = StringCollection.getFormat("moment.comment.more", 0, _moment.comment_count);
                }
				else
                {
                    txt_viewComment.gameObject.SetActive(false);
                }
            }
			else
            {
				go_comments.gameObject.SetActive(false);
            }
        }

        private void setupTime()
        {
			txt_time.text = UIMomentComment.formatTime(DateTime.UtcNow - _moment.create_time);
        }

        private string pickTagIcon(string tag_string)
		{
			int index = tag_string.IndexOf('>');
			if( index == -1)
			{
				return "";
			}

			return tag_string.Substring(0, index + 1);
		}

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
			return (_feed != null && _feed._moment != null ) ?  _feed._moment.photo_list.Count : 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
			return _feed._moment.photo_list.Count == 1 ? 343.0f : 320.0f ;
		}

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
			var photo = _photoScoller.GetCellView(_photoPrefab) as UIMomentPhoto;

			if (_calculateLayout)
			{
				photo.setEmpty();
			}
			else
			{
				if (dataIndex == 0)
				{
					photo.setup(_moment.makePhotoURL(GlobalConfig.fileserver_url, dataIndex), _moment.getPlaceData(), null, _moment.account_id == Network.getAccountID());
				}
				else
				{
					photo.setup(_moment.makePhotoURL(GlobalConfig.fileserver_url, dataIndex), _moment.getPhotoPlaceData(dataIndex), null, _moment.account_id == Network.getAccountID());
                }
                photo.setAddLocationCallback(() =>
                {
                    if (_feed == null)
                        return;

                    if (_feed.object_owner_account_id != Network.getAccountID())
                    {
                        return;
                    }

                    ViewModel.MakeMoment.reset(MakeMomentViewModel.EditMode.modify, _feed._moment);
                    UIMakeMomentLocation.spawn((PlaceData placeData) =>
                    {
                        if (placeData != null)
                        {
							if (dataIndex == 0 )
								ViewModel.MakeMoment.Place = placeData;
							else
                                ViewModel.MakeMoment.PhotoList[dataIndex].placeData = placeData;

                            UIBlockingInput.getInstance().open();
                            ModifyMomentProcessor step = ModifyMomentProcessor.create();
                            step.run(result =>
                            {
                                UIBlockingInput.getInstance().close();
								_moment.setPlaceData(dataIndex, placeData);

								if (dataIndex == 0)
                                {
                                    photo.setup(_moment.makePhotoURL(GlobalConfig.fileserver_url, dataIndex), _moment.getPlaceData(), null, _moment.account_id == Network.getAccountID());
                                }
                                else
                                {
                                    photo.setup(_moment.makePhotoURL(GlobalConfig.fileserver_url, dataIndex), _moment.getPhotoPlaceData(dataIndex), null, _moment.account_id == Network.getAccountID());
                                }
                            });
                        }
                    });
                });
            }

			return photo;
        }
    }
}
