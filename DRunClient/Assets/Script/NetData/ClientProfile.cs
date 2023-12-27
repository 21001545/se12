using Festa.Client.Module.MsgPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.NetData
{
	public class ClientProfile
	{
		public string name;
		public string picture_url;
		public string message;

		[SerializeOption(SerializeOption.NONE)]
		public DateTime cached_time;

		// 2022.05.03 이강희
		[SerializeOption(SerializeOption.NONE)]
		public bool _isFollow;

		[SerializeOption(SerializeOption.NONE)]
		public bool _isFollowBack;

		[SerializeOption(SerializeOption.NONE)]
		public int _socialScore;

		public bool isFollowEachOther()
		{
			return _isFollow && _isFollowBack;
		}

		public string getPicktureURL(string baseURL)
		{
			if( string.IsNullOrEmpty(picture_url))
			{
				return null;
			}

			return string.Format("{0}/{1}/{2}", baseURL, "profilephoto", picture_url);
		}
	}
}
