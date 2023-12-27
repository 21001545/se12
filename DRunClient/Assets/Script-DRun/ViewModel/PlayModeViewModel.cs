using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DRun.Client.ViewModel
{
	public class PlayModeViewModel : AbstractViewModel
	{
		public static class PlayMode
		{
			public const int Basic = 1;
			public const int Pro = 2;
            public const int Marathon = 3;
        }

		private int _mode;

		public int Mode
		{
			get
			{
				return _mode;
			}
			set
			{
				Set(ref _mode, value);
			}
		}

		public static PlayModeViewModel create()
		{
			PlayModeViewModel vm = new PlayModeViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();
			_mode = PlayMode.Pro;
		}
	}
}
