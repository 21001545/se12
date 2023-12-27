using Festa.Client.Module.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Festa.Client.Module.UI
{
	public class UIPanelOpenParam : MapPacket
	{
		public bool hasBackNavigation
		{
			get
			{
				return (bool)get("backNavigation");
			}
			set
			{
				put("backNavigation", value);
			}
		}

		public int accountID
		{
			get
			{
				return (int)get("accountID");
			}
			set
			{
				put("accountID", value);
			}
		}

		///
		public static new UIPanelOpenParam create()
		{
			UIPanelOpenParam param = new UIPanelOpenParam();
			param.init();
			return param;
		}

		public static T create<T>() where T : UIPanelOpenParam , new()
		{
			T t = new T();
			t.init();
			return t;
		}

		public static UIPanelOpenParam createForBackNavigation()
		{
			UIPanelOpenParam param = create();
			param.hasBackNavigation = true;
			return param;
		}

		protected virtual void init()
		{
			_map = new Dictionary<int, object>();
		}
	}
}
