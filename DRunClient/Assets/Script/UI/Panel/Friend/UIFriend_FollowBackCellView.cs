using EnhancedUI.EnhancedScroller;
using Festa.Client.Module.Net;
using Festa.Client.NetData;
using Festa.Client.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Festa.Client
{
	public class UIFriend_FollowBackCellView : EnhancedScrollerCellView
	{
		public UIPhotoThumbnail photo;
		//public TMP_Text txt_rank;
		public TMP_Text txt_name;
		public TMP_Text txt_message;

		private ClientFollowBack _followBackData;
        private SocialViewModel ViewModel => UIFriend.getInstance().SocialViewModel;
        private ClientNetwork Network => ClientMain.instance.getNetwork();

        public ClientFollowBack getFollowBackData()
		{
			return _followBackData;
		}

		public void setup(/*int rank,*/ClientFollowBack follow_back)
		{
			_followBackData = follow_back;

			if( _followBackData._profileCache != null)
			{
				setupProfile(_followBackData._profileCache.Profile);
			}

			//txt_rank.text = rank.ToString();
			//txt_rank.color = rank <= 3 ? Color.white : Color.black;
		}

		private void setupProfile(ClientProfile profile)
		{
			txt_name.text = profile.name;
			txt_message.text = profile.message;
			photo.setImageFromCDN(profile.getPicktureURL(GlobalConfig.fileserver_url));
		}

		public void onClickFollow()
        {
            if (UIFriend.getInstance().isMyAccount() == false)
            {
                return;
            }

            ClientFollowBack follow_back = getFollowBackData();
            MapPacket req = Network.createReq(CSMessageID.Social.FollowReq);
            req.put("id", follow_back.follow_back_id);

            UIBlockingInput.getInstance().open();

            Network.call(req, ack =>
            {
                UIBlockingInput.getInstance().close();

                if (ack.getResult() == ResultCode.ok)
                {
                    ClientMain.instance.getViewModel().updateFromPacket(ack);

                    ViewModel.FollowBackList.Clear();
                    //loadData(false);
                }
            });
        }

		public void onClickProfile()
		{
			UIFriend.getInstance().moveToProfile(_followBackData.follow_back_id);
		}
	}
}
