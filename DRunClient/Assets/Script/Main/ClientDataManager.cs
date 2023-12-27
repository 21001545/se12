using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Festa.Client.Data;

namespace Festa.Client
{
	public class ClientDataManager
	{
		private StartupContext _startupContext;

		public StartupContext getStartupContext()
		{
			return _startupContext;
		}

		public static ClientDataManager create()
		{
			ClientDataManager manager = new ClientDataManager();
			manager.init();
			return manager;
		}

		private void init()
		{
			_startupContext = StartupContext.create();
		}
	}
}
