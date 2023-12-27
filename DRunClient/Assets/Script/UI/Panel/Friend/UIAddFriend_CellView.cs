using EnhancedUI.EnhancedScroller;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Festa.Client
{
	public class UIAddFriend_CellView : EnhancedScrollerCellView
	{
		public UIPhotoThumbnail photo;
		public TMP_Text txt_name;
		public TMP_Text txt_message;
		public Button btn_follow;

		private ClientFollowSuggestion _data;
		private ClientProfileCache _profile;
		private UnityAction<UIAddFriend_CellView> _clickEvent;

		public ClientFollowSuggestion getData()
		{
			return _data;
		}

		public ClientProfileCache getProfileCache()
        {
			return _profile;
        }

		public void setup(ClientFollowSuggestion data,UnityAction<UIAddFriend_CellView> clickEvent)
		{
			_data = data;
			_profile = data._profile;
			_clickEvent = clickEvent;

			if( _data._profile != null)
			{
				txt_name.text = _data._profile.Profile.name;
				txt_message.text = _data._profile.Profile.message;
                photo.setImageFromCDN(_data._profile.Profile.getPicktureURL(GlobalConfig.fileserver_url));
            }
            updateFollow();
        }

        public void setup(ClientProfileCache profile, UnityAction<UIAddFriend_CellView> clickEvent)
        {
			_profile = profile;
			_clickEvent = clickEvent;

            if (_profile.Profile != null)
            {
                txt_name.text = _profile.Profile.name;
                txt_message.text = _profile.Profile.message;
				photo.setImageFromCDN(_profile.Profile.getPicktureURL(GlobalConfig.fileserver_url));
            }
            updateFollow();
        }

		public void updateFollow()
        {
            btn_follow.gameObject.SetActive(!_profile.Profile._isFollow);
        }

        public void onClickFollow()
		{
			_clickEvent(this);
		}

	}
}
