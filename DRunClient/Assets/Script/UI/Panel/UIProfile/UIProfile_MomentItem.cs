using Festa.Client.NetData;
using PolyAndCode.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Festa.Client
{
	public class UIProfile_MomentItem : MonoBehaviour
	{
		public GameObject pivot;
		public UIPhotoThumbnail thumbnail;
		public GameObject go_location;

		private ClientMoment _moment;

		public void onClicked()
		{

		}

		public void setup(ClientMoment moment)
		{
			pivot.SetActive(true);

			_moment = moment;

			if(_moment.photo_list.Count > 0)
			{
				thumbnail.setImageFromCDN(_moment.makePhotoURL(GlobalConfig.fileserver_url, 0));
			}
			else
			{
				thumbnail.setEmpty();
			}

			go_location.SetActive(_moment.trip_log != 0);
		}

		public void setupEmpty()
		{
			pivot.SetActive(false);

			_moment = null;
			thumbnail.setEmpty();
		}
		
	}
}
