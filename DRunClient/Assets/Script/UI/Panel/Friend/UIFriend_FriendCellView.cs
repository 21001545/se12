using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIFriend_FriendCellView : EnhancedScrollerCellView
	{
		public UIPhotoThumbnail photo;
		public TMP_Text txt_name;
		public TMP_Text txt_message;
		[SerializeField]
		private Image img_crown;
		[SerializeField]
		private TMP_Text txt_score;

		private ClientFollow _followData;

		public ClientFollow getFollowData()
		{
			return _followData;
		}

		public void setup(/*int rank,*/ClientFollow data, Sprite sprite, int score)
		{
			_followData = data;

			if( _followData._profileCache != null)
			{
				setupProfile(_followData._profileCache.Profile);
			}

			if (sprite == null)
				img_crown.gameObject.SetActive(false);
            else
            {
				img_crown.gameObject.SetActive(true);
				img_crown.sprite = sprite;
			}

			txt_score.text = score.ToString();
		}

		private void setupProfile(ClientProfile profile)
		{
			txt_name.text = profile.name;
			txt_message.text = profile.message;
			photo.setImageFromCDN(profile.getPicktureURL(GlobalConfig.fileserver_url));
		}

		public void onClickSendChat()
		{
			//UIFriend_Page_Friend.getInstance().onClickSendChat(this);
		}

		public void onClickMore()
		{
//			UIFriend_Page_Friend.getInstance().onClickMore(this);
		}

		public void onClickProfile()
		{
			UIFriend.getInstance().moveToProfile(_followData.follow_id);
		}
	}
}
