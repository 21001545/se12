using Festa.Client.Logic;
using Festa.Client.Module.UI;
using Festa.Client.NetData;
using Festa.Client.RefData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

namespace Festa.Client
{
	public class UIMomentCommentCell : EnhancedUI.EnhancedScroller.EnhancedScrollerCellView
    {
        [SerializeField]
        private RectTransform rt_root;

        [SerializeField]
		private UIPhotoThumbnail _thumbnail;

		[SerializeField]
        private TMP_Text txt_message;

		[SerializeField]
        private TMP_Text txt_time;

		[SerializeField]
        private TMP_Text txt_likeCount;

		[SerializeField]
        private Image img_background;

        [SerializeField]
        private RectTransform rt_indent;

        [SerializeField]
        private UIToggleButton btn_like;

        [SerializeField]
        private Button btn_comment;

		[SerializeField]
        private Button btn_modify;

		[SerializeField]
        private Button btn_delete;

        private int my_account_id => ClientMain.instance.getNetwork().getAccountID();
		private RefStringCollection sc => GlobalRefDataContainer.getStringCollection();
		private ClientMomentComment _comment;

		public ClientMomentComment getComment()
		{
			return _comment;
		}

        public Vector2 getComponentSize()
        {
            var size = txt_message.textBounds.size;

            // top 13
            size.y += 13.0f;

            // time
            size.y += 18.0f;

            //bottom 14
            size.y += 14.0f;
            return size;
        }

        public void setup(ClientMomentComment comment)
		{
			_comment = comment;

            rt_indent.anchoredPosition = new Vector2(0, 0);

            img_background.rectTransform.offsetMin = new Vector2(comment.sub_id > 1 ? 33.0f : 0, 0);

            if (comment._profile != null )
                _thumbnail.setImageFromCDN(comment._profile.getPicktureURL(GlobalConfig.fileserver_url));
            txt_message.text = sc.getFormat("moment.comment", 0, comment._profile.name, comment.message);


            var size = txt_message.GetPreferredValues();
            Debug.Log($"size : {size.y}");

            btn_modify.gameObject.SetActive(true);
            (btn_delete.transform as RectTransform).anchoredPosition = new Vector2(0, 0);

            var thumbnailRT = _thumbnail.transform as RectTransform;
            thumbnailRT.sizeDelta = new Vector2(comment.sub_id > 1 ? 24.0f : 32.0f, comment.sub_id > 1 ? 24.0f : 32.0f);

            Canvas.ForceUpdateCanvases();

            if ( comment._isPosting )
            {
                img_background.color = ColorChart.primary_50;
                // 업로드 중..
                txt_likeCount.text = "";
                btn_like.gameObject.SetActive(false);
                btn_comment.gameObject.SetActive(false);
                txt_time.text = "posting...";
            }
            else
            {
                img_background.color = ColorChart.white;

                txt_likeCount.text = sc.getFormat("moment.comment.like", comment.like_count > 0 ? 1 : 0, comment.like_count);

                txt_time.text = UIMomentComment.formatTime(DateTime.UtcNow - comment.update_time);


                btn_like.gameObject.SetActive(true);
                btn_comment.gameObject.SetActive(true);

                btn_like.IsOn = comment._isLiked;
            }
        }

        private void doForParents<T>(Action<T> action) where T : IEventSystemHandler
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                foreach (var component in parent.GetComponents<Component>())
                {
                    if (component is T)
                        action((T)(IEventSystemHandler)component);
                }
                parent = parent.parent;
            }
        }

        private bool _isDrag = false;
        private bool _routeDragToParent = false;
        private Vector2 _dragStartPosition;

        public void onDrag(BaseEventData eventData)
        {
            PointerEventData data = eventData as PointerEventData;
            if (_routeDragToParent)
                doForParents<IDragHandler>((parent) => { parent.OnDrag(data); });
            else if (_isDrag)
            {
                Vector2 delta = data.position - _dragStartPosition;

                // 버튼 보다는 크게 움직여보자
                if (delta.x < -16.0f)
                {
                    // 좌로 땡겨
                    rt_indent.DOAnchorPosX(-116.0f, 0.1f).SetEase(Ease.InCubic);
                    _isDrag = false;
                }
                else if ( delta.x > 16.0f)
                {
                    rt_indent.DOAnchorPosX(0.0f, 0.1f).SetEase(Ease.InCubic);
                    _isDrag = false;
                }
            }
        }

        public void onBeginDrag(BaseEventData eventData)
        {
            PointerEventData data = eventData as PointerEventData;

            if (Math.Abs(data.delta.x) < Math.Abs(data.delta.y))
                _routeDragToParent = true;
            else
                _routeDragToParent = false;

            _routeDragToParent = _routeDragToParent || my_account_id != _comment.comment_account_id;

            if (_routeDragToParent)
                doForParents<IBeginDragHandler>((parent) => { parent.OnBeginDrag(data); });
            else
            {
                _isDrag = true;
                _dragStartPosition = data.position;
            }
        }

        public void onEndDrag(BaseEventData eventData)
        {
            PointerEventData data = eventData as PointerEventData;
            if (_routeDragToParent)
                doForParents<IEndDragHandler>((parent) => { parent.OnEndDrag(data); });

            _isDrag = false;
        }

		public void onClick()
		{
			UIMomentComment.getInstance().setInputMode(UIMomentComment.InputMode.add_comment_to_target, _comment);
			//UIMomentComment.getInstance().input_addcomment.ActivateInputField();
		}

		public void onClickModify()
        {
            UIMomentComment.getInstance().setInputMode(UIMomentComment.InputMode.modify_comment, _comment);
            rt_indent.DOAnchorPosX(0.0f, 0.2f).SetEase(Ease.InCubic).OnComplete(() =>
            {
            });
		}

		public void onClickDelete()
        {
            btn_modify.gameObject.SetActive(false);
            (btn_delete.transform as RectTransform).DOAnchorPosX(-375.0f / 2.0f, 0.2f).SetEase(Ease.InCubic);
            rt_indent.DOAnchorPosX(-375.0f, 0.2f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                UIMomentComment.getInstance().deleteComment(this);
            });
        }

		public void onClickLike()
		{
			UIMomentComment.getInstance().likeComment(this);
		}

		public void onClickPhoto()
		{
			UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
			param.accountID = _comment.comment_account_id;

			UIMomentComment.getInstance().close();
			UIProfile.getInstance().open(param);

			ClientMain.instance.getPanelNavigationStack().push(UIMomentComment.getInstance(), UIProfile.getInstance());
		}
	}
}
