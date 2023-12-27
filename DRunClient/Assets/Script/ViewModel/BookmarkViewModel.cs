using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client
{
	public class BookmarkViewModel : AbstractViewModel
	{
		private List<ClientBookmark> _bookmarkList;

		public List<ClientBookmark> BookmarkList
		{
			get
			{
				return _bookmarkList;
			}
			set
			{
				Set(ref _bookmarkList, value);
			}
		}

		public static BookmarkViewModel create()
		{
			BookmarkViewModel vm = new BookmarkViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_bookmarkList = new List<ClientBookmark>();
		}
	}
}
