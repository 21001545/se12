using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class DailyBoostViewModel : AbstractViewModel
	{
		private ClientDailyBoost _dailyBoost;

		public ClientDailyBoost DailyBoost
		{
			get
			{
				return _dailyBoost;
			}
			set
			{
				Set(ref _dailyBoost, value);
			}
		}

		public static DailyBoostViewModel create()
		{
			DailyBoostViewModel vm = new DailyBoostViewModel();
			vm.init();
			return vm;
		}
	}
}
