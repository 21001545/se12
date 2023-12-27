using Festa.Client.Module.UI;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	public class UIFriend_TopFriendItem : MonoBehaviour
	{
		public UIPhotoThumbnail photo;
		public TMP_Text txt_name;
		public TMP_Text txt_message;

		private ClientFollow _followData;

		public void setup(ClientFollow data)
		{
			_followData = data;

			if( _followData == null)
			{
				clearUI();
			}
			else
			{
				setupProfile(_followData._profileCache.Profile);
			}
		}

		private void clearUI()
		{
			txt_name.text = "";
			txt_message.text = "";
			photo.setEmpty();

			gameObject.SetActive(false);
		}

		private void setupProfile(ClientProfile profile)
		{
			txt_name.text = profile.name;
			txt_message.text = profile.message;
			photo.setImageFromCDN(profile.getPicktureURL(GlobalConfig.fileserver_url));
            gameObject.SetActive(true);
        }

		public void onClickProfile()
		{
			if( _followData == null)
			{
				return;
			}

			UIFriend.getInstance().moveToProfile(_followData.follow_id);
		}
	}
}
