using Festa.Client.MapBox;
using Festa.Client.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class MapRevealViewModel : UMBMapRevealViewModel
	{
		public static MapRevealViewModel create()
		{
			MapRevealViewModel vm = new MapRevealViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();
		}

	}
}
