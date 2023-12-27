using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class HealthTodayFriendRankViewModel : AbstractViewModel
	{
		// 내것도 포함되어 있음
		private int _myIndex;
		private int _maxValue;
		private List<ClientFriendHealthData> _friendList;

		public List<ClientFriendHealthData> FriendList
		{
			get
			{
				return _friendList;
			}
			set
			{
				Set(ref _friendList, value);
			}
		}

		public int MyIndex
		{
			get
			{
				return _myIndex;
			}
			set
			{
				Set(ref _myIndex, value);
			}
		}

		public int MaxValue
		{
			get
			{
				return _maxValue;
			}
			set
			{
				Set(ref _maxValue, value);
			}

		}

		public static HealthTodayFriendRankViewModel create()
		{
			HealthTodayFriendRankViewModel model = new HealthTodayFriendRankViewModel();
			model.init();
			return model;
		}

		protected override void init()
		{
			base.init();
			_myIndex = -1;
		}
	}
}
