using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class JobProgressViewModel : AbstractViewModel
	{
		// 일단 목록은 하드코딩
		private ObservableList<JobProgressItemViewModel> _listMoment;

		public ObservableList<JobProgressItemViewModel> ListMoment => _listMoment;

		public static JobProgressViewModel create()
		{
			JobProgressViewModel vm = new JobProgressViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			_listMoment = new ObservableList<JobProgressItemViewModel>();
		}
	}
}
