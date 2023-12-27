using Festa.Client.Module;
using Festa.Client.NetData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.ViewModel
{
	public class FeedViewModel : AbstractViewModel
	{
		private ClientFeedConfig _feedConfig;
		private List<ClientFeed> _feedList;
		private List<ClientFeed> _loadedFeedList;
		private DateTime _lastFeedQueryTime;

		public ClientFeedConfig FeedConfig
		{
			get
			{
				return _feedConfig;
			}
			set
			{
				Set(ref _feedConfig, value);
			}
		}

		public List<ClientFeed> FeedList
		{
			get
			{
				return _feedList;
			}
			set
			{
				Set(ref _feedList, value);
			}
		}

		public List<ClientFeed> LoadedFeedList
		{
			get
			{
				return _loadedFeedList;
			}
			set
			{
				Set(ref _loadedFeedList, value);
			}
		}

		public DateTime LastFeedQueryTime
		{
			get
			{
				return _lastFeedQueryTime;
			}
			set
			{
				Set(ref _lastFeedQueryTime, value);
			}
		}

		public static FeedViewModel create()
		{
			FeedViewModel vm = new FeedViewModel();
			vm.init();
			return vm;
		}

		protected override void init()
		{
			base.init();

			_feedList = new List<ClientFeed>();
			_loadedFeedList = new List<ClientFeed>();
			_lastFeedQueryTime = DateTime.UtcNow.AddDays(1);
		}
	}
}
