using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using Festa.Client.RefData;
using Festa.Client.Module.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace Festa.Client
{
	public class UIActivityItem : EnhancedScrollerCellView
	{
        [SerializeField]
        private GameObject layout_moment;
        [SerializeField]
        private GameObject layout_reward;

		[Header("Layout Moment")]
		[SerializeField]
		private TMP_Text txt_moment_name;
		[SerializeField]
		private TMP_Text txt_moment_desc;
		[SerializeField]
		private RectTransform rect_moment_desc;
		[SerializeField]
		private TMP_Text txt_moment_time;
		[SerializeField]
		private UIPhotoThumbnail img_moment_profile;
		[SerializeField]
		private UIPhotoThumbnail img_moment_photo;
		[SerializeField]
		private float textAreaWidth;

		[Header("Layout Reward")]
		[SerializeField]
		private TMP_Text txt_reward_title;
        [SerializeField]
        private TMP_Text txt_reward_desc;
		[SerializeField]
		private TMP_Text txt_claimButton;
		[SerializeField]
		private TMP_Text txt_claimButtonFixed;
        [SerializeField]
        private GameObject go_reward_claim;
		[SerializeField]
		private Animator animator;

		private ClientActivity _data;
		private ClientMoment _moment;
		private ClientProfile _agentProfile;
		private RefStringCollection StringCollection => GlobalRefDataContainer.getStringCollection();
		private string PhotoBaseURL => GlobalConfig.fileserver_url;

		public void setup(ClientActivity activity)
		{
			_data = activity;
			_moment = _data._moment;
			_agentProfile = _data._agentProfile;

			setupUI();
		}

		public override void RefreshCellView() {
			setupUI();
		}

		private void setupUI()
		{
			clearPhotos();
			layout_moment.SetActive(false);
			layout_reward.SetActive(false);

			if (_data.event_type == ClientActivity.Type.moment_like || _data.event_type == ClientActivity.Type.moment_comment)
			{
				layout_moment.SetActive(true);

				if (_moment != null)
				{
					img_moment_photo.setImageFromCDN(_moment.getRepresentativePhotoURL(PhotoBaseURL));
				}

				if (_agentProfile != null)
				{
					img_moment_profile.setImageFromCDN(_agentProfile.getPicktureURL(PhotoBaseURL));
					txt_moment_name.text = _agentProfile.name;

					rect_moment_desc.sizeDelta = new Vector2(textAreaWidth - txt_moment_name.preferredWidth - 2f, 20f);     // 2f 는 패딩
					txt_moment_desc.text = StringCollection.get("activity.desc", _data.event_type);
				}

				txt_moment_time.text = UIMomentComment.formatTime(DateTime.UtcNow - _data.event_time);
			}
			else if (_data.event_type == ClientActivity.Type.reward_moment_like)
			{
				layout_reward.SetActive(true);

				txt_reward_title.text = StringCollection.get("activity.title", _data.event_type);
				txt_reward_desc.text = StringCollection.getFormat("activity.desc", _data.event_type, _data.param2);

				if (_data.claim_status == ClientActivity.ClaimStatus.wait_claim)
				{
					go_reward_claim.SetActive(true);

					if (_data.reward_type == ItemType.coin)
					{
					}
					else if (_data.reward_type == ItemType.star)
					{
						// 220519 소현 : 코인 관련 기획이 나오기 전까지는,, 일단 스타만 고쳐보자구
						// 코인에 대해서는 리원님이 아라님과 얘기해 보고 말씀 준다고 하셨당!!
						txt_claimButton.text = $"+{_data.reward_amount}";
						txt_claimButtonFixed.text = $"+{_data.reward_amount}";
					}
				}
				else
				{
					go_reward_claim.SetActive(false);
				}

			}
		}

		public void onClickProfile()
        {
            UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
			param.accountID = _data.agent_account_id;

            UIProfile.getInstance().open(param);
			//UIActivity.getInstance().close(TransitionEventType.openImmediately);
            ClientMain.instance.getPanelNavigationStack().push(UIActivity.getInstance(), UIProfile.getInstance());
		}

		public void onClickActivityItem()
        {
			// 페이지 만들어지면 연결..!!
/*            UIPanelOpenParam param = UIPanelOpenParam.createForBackNavigation();
            param.accountID = _data.agent_account_id;

            UIProfile.getInstance().open(param);
            ClientMain.instance.getPanelNavigationStack().push(UIActivity.getInstance(), UIProfile.getInstance());*/
        }

		private void clearPhotos()
		{
			img_moment_profile.setEmpty();
			img_moment_photo.setEmpty();
		}

		// 220519 소현 : 트렌지션 부분
		#region 리워드 받기 버튼

		[Header("get reward button")]
		[SerializeField]
		private RectTransform rect_itemPannel;
        [SerializeField]
        private RectTransform rect_star;
        [SerializeField]
        private Image img_star;

        [SerializeField]
        private RectTransform rect_text;
        [SerializeField]
        private TMP_Text txt_text;

        private float _starDesPos;
        private float _textDesPos;
        private float _padding = 2f;
        private float _pivotHeight = -1.1f;     // 눌렀을 때 살짝 뒤로 빠지는 y 값
        private float _desHeight = 24f;         // 최종 안착해야 하는 y 값

        public void onClickButtonDown()
        {
            calculateButtonUpDesPosition();

            float starX = rect_star.anchoredPosition.x;
            float textX = rect_text.anchoredPosition.x;

            float duration = 0.1f;

            DOTween.To(() => rect_star.anchoredPosition, x => rect_star.anchoredPosition = x, calculateButtonDownDesPosition(starX, _starDesPos), duration);
            DOTween.To(() => rect_text.anchoredPosition, x => rect_text.anchoredPosition = x, calculateButtonDownDesPosition(textX, _textDesPos), duration);
        }

        public void onClickButtonUp()
        {
			// 이동 모션
			float duration = 0.2f;
            DOTween.To(() => rect_star.anchoredPosition, x => rect_star.anchoredPosition = x, new Vector2(_starDesPos, _desHeight), duration);
            DOTween.To(() => rect_text.anchoredPosition, x => rect_text.anchoredPosition = x, new Vector2(_textDesPos, _desHeight), duration);

			// 나머지 연출은 전부 얘가 한당
			animator.Play("fadeout");

			// 리워드를 얻자
			Invoke("claimReward", 0.65f);
        }

		private void claimReward()
		{
			UIActivity.getInstance().claimReward(_data, dataIndex);
		}

		private void calculateButtonUpDesPosition()
        {
            float halfSize = (rect_text.rect.width + rect_star.rect.width + _padding) * 0.5f;
            _starDesPos = -halfSize + rect_star.rect.width * 0.5f;
            _textDesPos = halfSize - rect_text.rect.width * 0.5f;
        }

        private Vector2 calculateButtonDownDesPosition(float origX, float desPos)
        {
			// up 했을 때의 포지션과 리니어하게 빠져 볼게
            float pivotX = (origX + (origX - desPos) / _desHeight) * -_pivotHeight;
            return new Vector2(pivotX, _pivotHeight);
        }
    }

    #endregion
}
